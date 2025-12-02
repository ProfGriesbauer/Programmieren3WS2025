using System;using System.Windows.Input;

namespace OOPGames
{
    public class A4_ShellStrike_HumanPlayer : IHumanGamePlayer
    {
        public string Name => "A4 ShellStrike Human";
        public int PlayerNumber { get; private set; }
        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;
        public bool CanBeRuledBy(IGameRules rules) => rules is A4_ShellStrike_Rules;
        public IGamePlayer Clone() { var c = new A4_ShellStrike_HumanPlayer(); c.SetPlayerNumber(PlayerNumber); return c; }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is not A4_ShellStrike_Field) return null;
            if (selection is IKeySelection ks)
            {
                switch (ks.Key)
                {
                    case Key.A: return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.MoveLeft, -5);
                    case Key.D: return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.MoveRight, 5);
                    case Key.W: return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.TurretUp, 3);
                    case Key.S: return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.TurretDown, 3);
                }
            }
            if (selection is IClickSelection cs && cs.ChangedButton == 0)
            {
                return new A4_ShellStrike_Move(PlayerNumber, ShellStrikeAction.Fire, 0);
            }
            return null;
        }
    }
}
