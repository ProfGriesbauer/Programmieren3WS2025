using System;
using System.Windows.Input;

namespace OOPGames
{
    // Human player for Tron: listens to key presses and returns direction changes
    public class B3_Mika_Roeder_Tron_HumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber = 0;

        public string Name { get { return "B3 Mika Röder Tron Human"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B3_Mika_Roeder_Tron_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B3_Mika_Roeder_Tron_HumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null) return null;

            if (!(selection is IKeySelection keySel)) return null;

            B3TronDirection? dir = keySel.Key switch
            {
                Key.Up => B3TronDirection.Up,
                Key.Down => B3TronDirection.Down,
                Key.Left => B3TronDirection.Left,
                Key.Right => B3TronDirection.Right,
                Key.W => B3TronDirection.Up,
                Key.S => B3TronDirection.Down,
                Key.A => B3TronDirection.Left,
                Key.D => B3TronDirection.Right,
                _ => null
            };

            if (dir.HasValue)
            {
                return new B3_Mika_Roeder_Tron_Move(_playerNumber, dir.Value);
            }

            return null;
        }
    }
}
