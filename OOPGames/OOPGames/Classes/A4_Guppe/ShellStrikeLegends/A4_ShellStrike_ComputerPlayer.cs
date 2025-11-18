using System;

namespace OOPGames
{
    public class A4_ShellStrike_ComputerPlayer : IComputerGamePlayer
    {
        public string Name => "A4 ShellStrikeLegends Computer";

        public int PlayerNumber { get; private set; }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A4_ShellStrike_Rules;
        }

        public IGamePlayer Clone()
        {
            var c = new A4_ShellStrike_ComputerPlayer();
            c.SetPlayerNumber(PlayerNumber);
            return c;
        }

        public IPlayMove GetMove(IGameField field)
        {
            // No moves in static preview mode
            return null;
        }
    }
}
