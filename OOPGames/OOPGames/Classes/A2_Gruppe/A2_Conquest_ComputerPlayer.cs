using System;

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

            // Ziel: grob Richtung gegnerische TargetBase laufen
            int targetX = (myId == 0) ? game.Field.Width - 1 - 3 : 3;
            int targetY = (myId == 0) ? game.Field.Height - 1 - 3 : 3;

            // probiere einfach irgendeinen legalen Zug, der Entfernung reduziert
            foreach (var troop in game.GetTroopsOf(myId))
            {
                int bestDx = Math.Abs(troop.X - targetX) + Math.Abs(troop.Y - targetY);

                for (int ny = troop.Y - 1; ny <= troop.Y + 1; ny++)
                for (int nx = troop.X - 1; nx <= troop.X + 1; nx++)
                {
                    if (!game.IsLegalTroopMove(myId, troop.LocalIndex, nx, ny)) continue;

                    int d = Math.Abs(nx - targetX) + Math.Abs(ny - targetY);
                    if (d < bestDx)
                        return new A2_ConquestTroopMove(PlayerNumber, troop.LocalIndex, ny, nx);
                }
            }

            // wenn nichts "besseres" geht: irgendeinen legalen King-Move
            foreach (var troop in game.GetTroopsOf(myId))
            {
                for (int ny = troop.Y - 1; ny <= troop.Y + 1; ny++)
                for (int nx = troop.X - 1; nx <= troop.X + 1; nx++)
                {
                    if (game.IsLegalTroopMove(myId, troop.LocalIndex, nx, ny))
                        return new A2_ConquestTroopMove(PlayerNumber, troop.LocalIndex, ny, nx);
                }
            }

            // sonst: Pass
            return new A2_ConquestPassMove(PlayerNumber);
        }
    }
}
