using System;

namespace OOPGames
{
    public class A4_ShellStrike_ComputerPlayer : IComputerGamePlayer
    {
        public string Name => "A4 ShellStrike AI";
        public int PlayerNumber { get; private set; }
        private int _step = 0;
        private Random _rnd = new Random();

        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;
        public bool CanBeRuledBy(IGameRules rules) => rules is A4_ShellStrike_Rules;
        public IGamePlayer Clone() { var c = new A4_ShellStrike_ComputerPlayer(); c.SetPlayerNumber(PlayerNumber); return c; }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is not A4_ShellStrike_Field shell) return null;
            _step++;
            // Simple behavior: move oscillating, adjust turret gradually, fire every 30 ticks
            if (_step % 50 < 25)
                return new A4_ShellStrike_Move(PlayerNumber, PlayerNumber==1?ShellStrikeAction.MoveRight:ShellStrikeAction.MoveLeft, 5);
            if (_step % 50 < 40)
                return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.TurretUp, 2);
            if (_step % 50 < 49)
                return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.TurretDown, 2);
            return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.Fire, 0);
        }
    }
}
