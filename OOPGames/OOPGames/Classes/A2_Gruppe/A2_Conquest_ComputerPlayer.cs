using System;
using System.Linq;

namespace OOPGames
{
    public class A2_ConquestComputerPlayer : IComputerGamePlayer
    {
        public string Name => "A2_Conquest_Computer";
        public int PlayerNumber { get; private set; } = 2;

        public A2_ConquestComputerPlayer(int playerNumber = 2) => PlayerNumber = playerNumber;

        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;
        public bool CanBeRuledBy(IGameRules rules) => rules is A2_ConquestRules;
        public IGamePlayer Clone() => new A2_ConquestComputerPlayer(PlayerNumber);

        public IPlayMove GetMove(IGameField field)
        {
            if (field is not A2_ConquestGameField gf) return null;

            var game = gf.Game;
            int myId = PlayerNumber - 1;
            if (game.CurrentPlayerId != myId) return null;

            // Enemy TargetBase finden (die TargetBase, die NICHT mir gehört)
            var enemyTarget = game.Field.AllTiles().FirstOrDefault(t => t.IsTargetBase && t.OwnerID != myId);
            if (enemyTarget == null) return new A2_ConquestPassMove(PlayerNumber);

            // Troops holen
            var troops = game.GetTroopsOf(myId);

            // Capturing-Troops NICHT bewegen
            bool IsCapturing(Troop tr)
            {
                var tile = game.Field.GetTile(tr.X, tr.Y);
                return tile.IsBeingContested && tile.CapturingPlayerID == myId;
            }

            // 1) Wenn TargetBase "unlockt" ist und ein legaler King-Move drauf möglich ist -> machen
            if (game.MeetsTargetBasePreconditions(myId, enemyTarget))
            {
                foreach (var tr in troops.Where(t => !IsCapturing(t)))
                {
                    if (game.IsLegalTroopMove(myId, tr.LocalIndex, enemyTarget.X, enemyTarget.Y))
                        return new A2_ConquestTroopMove(PlayerNumber, tr.LocalIndex, enemyTarget.Y, enemyTarget.X);
                }
            }

            // 2) Sonst: Front aufbauen (connected expand) -> bewege eine nicht-capturing Troop Richtung Target,
            // bevorzugt auf Felder, die neben eigenen Tiles liegen (damit Capture connected wächst)
            int BestDist(int x, int y) => Math.Abs(x - enemyTarget.X) + Math.Abs(y - enemyTarget.Y);

            IPlayMove  bestMove = null;
            int bestScore = int.MinValue;

            foreach (var tr in troops.Where(t => !IsCapturing(t)))
            {
                for (int ny = tr.Y - 1; ny <= tr.Y + 1; ny++)
                for (int nx = tr.X - 1; nx <= tr.X + 1; nx++)
                {
                    if (!game.IsLegalTroopMove(myId, tr.LocalIndex, nx, ny)) continue;

                    var tile = game.Field.GetTile(nx, ny);

                    // nicht auf Homebases (unnötig)
                    if (tile.IsHomeBase) continue;

                    int score = 0;
                    score -= BestDist(nx, ny) * 5; // näher ist besser

                    bool isNew = tile.OwnerID != myId;
                    bool adjOwned = game.Field.GetNeighbours4(tile).Any(n => n.OwnerID == myId);

                    if (isNew) score += 30;        // lieber neue Felder
                    if (isNew && adjOwned) score += 60;  // connected expansion

                    // TargetBase nicht betreten solange gelockt (sonst verschwendete Züge)
                    if (tile.IsTargetBase && !game.MeetsTargetBasePreconditions(myId, tile))
                        score -= 10000;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new A2_ConquestTroopMove(PlayerNumber, tr.LocalIndex, ny, nx);
                    }
                }
            }

            // 3) Wenn nix sinnvolles -> Pass (wichtig, damit Captures weiterlaufen & Ressourcen steigen)
            return bestMove ?? new A2_ConquestPassMove(PlayerNumber);
        }
    }
}
