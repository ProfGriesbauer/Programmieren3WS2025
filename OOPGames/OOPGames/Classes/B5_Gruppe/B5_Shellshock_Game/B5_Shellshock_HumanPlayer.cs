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
            if (field.ProjectileInFlight) return null; // Block input during projectile flight

            // Determine active tank (Rules keeps field.ActiveTankNumber up-to-date)
            B5_Shellshock_Tank activeTank = field.ActiveTankNumber == 1 ? field.Tank1 : field.Tank2;

            switch (key)
            {
                case Key.A:
                case Key.Left:
                    if (field.MovementsRemaining > 0)
                    {
                        double newX = activeTank.X - 10;
                        if (newX > 20 && newX < field.Terrain.Width - 20)
                        {
                            activeTank.X = newX;
                            activeTank.Y = field.Terrain.GetHeightAt(activeTank.X);
                            field.MovementsRemaining--;
                        }
                    }
                    return null; // Do not end turn

                case Key.D:
                case Key.Right:
                    if (field.MovementsRemaining > 0)
                    {
                        double newX = activeTank.X + 10;
                        if (newX > 20 && newX < field.Terrain.Width - 20)
                        {
                            activeTank.X = newX;
                            activeTank.Y = field.Terrain.GetHeightAt(activeTank.X);
                            field.MovementsRemaining--;
                        }
                    }
                    return null; // Do not end turn

                case Key.W:
                case Key.Up:
                    // For right-side tank (2), invert angle direction for intuitive control
                    activeTank.AdjustAngle(field.ActiveTankNumber == 2 ? -1 : 1);
                    return null;

                case Key.S:
                case Key.Down:
                    activeTank.AdjustAngle(field.ActiveTankNumber == 2 ? 1 : -1);
                    return null;

                case Key.Q:
                    activeTank.AdjustPower(-1);
                    return null;

                case Key.E:
                    activeTank.AdjustPower(1);
                    return null;

                case Key.Space:
                    // Space can start game, restart after win, or shoot during active play
                    if (field.GamePhase == B5_Shellshock_GamePhase.Setup)
                    {
                        return new B5_Shellshock_Move(field.ActiveTankNumber, B5_Shellshock_ActionType.StartGame);
                    }
                    if (field.GamePhase == B5_Shellshock_GamePhase.GameOver)
                    {
                        return new B5_Shellshock_Move(field.ActiveTankNumber, B5_Shellshock_ActionType.StartGame);
                    }
                    if (field.GamePhase == B5_Shellshock_GamePhase.PlayerTurn)
                    {
                        return new B5_Shellshock_Move(field.ActiveTankNumber, B5_Shellshock_ActionType.Shoot);
                    }
                    return null;

                default:
                    return null;
            }
        }

        private IPlayMove GetMoveFromClick(IClickSelection selection, B5_Shellshock_Field field)
        {
            if (field.ProjectileInFlight) return null;
            return new B5_Shellshock_Move(field.ActiveTankNumber, B5_Shellshock_ActionType.Shoot);
        }
    }
}
