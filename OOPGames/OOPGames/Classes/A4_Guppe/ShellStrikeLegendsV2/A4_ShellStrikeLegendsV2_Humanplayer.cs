using System.Windows.Input;

namespace OOPGames
{
    // Minimal human player that now produces moves for left/right driving.
    public class A4_ShellStrikeLegendsV2_HumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber = 1;

        public string Name => "A4 ShellStrikeLegends V2 Human";

        public void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;
        public int PlayerNumber => _playerNumber;

        public bool CanBeRuledBy(IGameRules rules) => rules is A4_ShellStrikeLegendsV2_Rules;

        public IGamePlayer Clone()
        {
            var p = new A4_ShellStrikeLegendsV2_HumanPlayer();
            p.SetPlayerNumber(_playerNumber);
            return p;
        }

        // Liefert Moves basierend auf Tastatur-Eingaben (Left/Right+ Up/Down)
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null)
                return null;

            // Wir reagieren nur auf Keyboard-Eingaben
            if (selection.MoveType == MoveType.key && selection is IKeySelection keySel)
            {
                switch (keySel.Key)
                {
                    case Key.Left:
                    case Key.A:
                        return new A4_ShellStrikeLegendsV2_Move(SSLV2Action.MoveLeft, _playerNumber);

                    case Key.Right:
                    case Key.D:
                        return new A4_ShellStrikeLegendsV2_Move(SSLV2Action.MoveRight, _playerNumber);

                    case Key.Up:
                    case Key.W:
                        return new A4_ShellStrikeLegendsV2_Move(SSLV2Action.BarrelUp, _playerNumber);

                    case Key.Down:
                    case Key.S:
                        return new A4_ShellStrikeLegendsV2_Move(SSLV2Action.BarrelDown, _playerNumber);

                    case Key.Space:
                    case Key.Enter:
                        return new A4_ShellStrikeLegendsV2_Move(SSLV2Action.Fire, _playerNumber);

                }
            }

            // Keine relevante Eingabe → kein Move
            return null;
        }

    }
}

