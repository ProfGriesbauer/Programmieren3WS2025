using System.Windows.Input;

namespace OOPGames
{
    // Human player controls with keyboard and mouse
    public class B5_Shellshock_HumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name => "B5_Shellshock_Human_Player";

        public int PlayerNumber => _playerNumber;

        public B5_Shellshock_HumanPlayer()
        {
            _playerNumber = 1;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B5_Shellshock_Rules;
        }

        public IGamePlayer Clone()
        {
            var clone = new B5_Shellshock_HumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || field is not B5_Shellshock_Field shellField)
                return null;

            if (selection is IKeySelection keySelection)
            {
                return GetMoveFromKey(keySelection.Key, shellField);
            }

            if (selection is IClickSelection clickSelection)
            {
                return GetMoveFromClick(clickSelection, shellField);
            }

            return null;
        }

        private IPlayMove GetMoveFromKey(Key key, B5_Shellshock_Field field)
        {
            // Don't allow any actions while projectile is in flight
            if (field.ProjectileInFlight)
                return null;

            // For shooting, return a Move object (this will trigger framework player switch)
            // For other actions, return a Move object but Rules will handle player switching
            switch (key)
            {
                case Key.A:
                case Key.Left:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.MoveLeft);

                case Key.D:
                case Key.Right:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.MoveRight);

                case Key.W:
                case Key.Up:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.IncreaseAngle);

                case Key.S:
                case Key.Down:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.DecreaseAngle);

                case Key.Q:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.DecreasePower);

                case Key.E:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.IncreasePower);

                case Key.Space:
                case Key.Enter:
                    return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.Shoot);

                default:
                    return null;
            }
        }

        private IPlayMove GetMoveFromClick(IClickSelection selection, B5_Shellshock_Field field)
        {
            // Mouse click can be used to shoot
            return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.Shoot);
        }
    }
}
