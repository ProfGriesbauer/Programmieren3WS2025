using System.Linq;

namespace OOPGames
{
    public class A2_ConquestComputerPlayer : IComputerGamePlayer
    {
        public string Name => "A2_Conquest_Computer";
        public int PlayerNumber { get; private set; }

        public A2_ConquestComputerPlayer(int playerNumber = 2) => PlayerNumber = playerNumber;

        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public bool CanBeRuledBy(IGameRules rules) => rules is A2_ConquestRules;

        public IGamePlayer Clone() => new A2_ConquestComputerPlayer(PlayerNumber);

        public IPlayMove GetMove(IGameField field)
        {
            if (field is not A2_ConquestGameField gf) return new A2_ConquestPassMove(PlayerNumber);

            // Einfach: erste capturable Tile nehmen, sonst Pass.
            var g = gf.Game;
            var p = g.CurrentPlayer;

            foreach (var t in g.Field.AllTiles())
            {
                if (t.IsBeingContested) continue;
                if (!t.CanBeCapturedBy(p, g.Field)) continue;

                return new A2_ConquestMove(PlayerNumber, t.Y, t.X);
            }

            return new A2_ConquestPassMove(PlayerNumber);
        }
    }
}
