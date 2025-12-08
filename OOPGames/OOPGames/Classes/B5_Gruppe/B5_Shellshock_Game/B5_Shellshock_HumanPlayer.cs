using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Human player controller for Shellshock game.
    /// Implements IHumanGamePlayer to receive keyboard and mouse input.
    /// 
    /// Controls:
    /// - A/Left, D/Right: Move tank (limited per turn)
    /// - W/Up, S/Down: Adjust firing angle
    /// - Q, E: Adjust shot power
    /// - Space: Fire / Start game / Restart
    /// - Mouse Click: Fire
    /// 
    /// Following framework pattern: only creates IPlayMove objects.
    /// All state changes are handled by B5_Shellshock_Rules.DoMove().
    /// </summary>
    public class B5_Shellshock_HumanPlayer : IHumanGamePlayer
    {
        #region Fields

        private int _playerNumber;

        #endregion

        #region IGamePlayer Implementation

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

        #endregion

        #region IHumanGamePlayer Implementation

        /// <summary>
        /// Processes player input and returns corresponding move.
        /// Does NOT modify game state directly - only creates move objects.
        /// </summary>
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || field is not B5_Shellshock_Field shellField)
                return null;

            if (selection is IKeySelection keySelection)
                return GetMoveFromKey(keySelection.Key, shellField);

            if (selection is IClickSelection)
                return GetMoveFromClick(shellField);

            return null;
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Maps keyboard input to game actions.
        /// Creates appropriate B5_Shellshock_Move for the active player.
        /// </summary>
        private IPlayMove GetMoveFromKey(Key key, B5_Shellshock_Field field)
        {
            // Block input during projectile flight
            if (field.ProjectileInFlight) 
                return null;

            int activePlayer = field.ActiveTankNumber;

            switch (key)
            {
                // Movement controls
                case Key.A:
                case Key.Left:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.MoveLeft);

                case Key.D:
                case Key.Right:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.MoveRight);

                // Angle controls (Player 2 inverted for intuitive left-facing aim)
                case Key.W:
                case Key.Up:
                    return new B5_Shellshock_Move(activePlayer, 
                        activePlayer == 2 ? B5_Shellshock_ActionType.DecreaseAngle : B5_Shellshock_ActionType.IncreaseAngle);

                case Key.S:
                case Key.Down:
                    return new B5_Shellshock_Move(activePlayer, 
                        activePlayer == 2 ? B5_Shellshock_ActionType.IncreaseAngle : B5_Shellshock_ActionType.DecreaseAngle);

                // Power controls
                case Key.Q:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.DecreasePower);

                case Key.E:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.IncreasePower);

                // Action controls
                case Key.Space:
                    return GetSpaceAction(field, activePlayer);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Handles Space key which has different effects based on game phase.
        /// </summary>
        private IPlayMove GetSpaceAction(B5_Shellshock_Field field, int activePlayer)
        {
            switch (field.GamePhase)
            {
                case B5_Shellshock_GamePhase.Setup:
                case B5_Shellshock_GamePhase.GameOver:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.StartGame);

                case B5_Shellshock_GamePhase.PlayerTurn:
                    return new B5_Shellshock_Move(activePlayer, B5_Shellshock_ActionType.Shoot);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Maps mouse click to shoot action.
        /// </summary>
        private IPlayMove GetMoveFromClick(B5_Shellshock_Field field)
        {
            if (field.ProjectileInFlight) 
                return null;

            if (field.GamePhase == B5_Shellshock_GamePhase.PlayerTurn)
                return new B5_Shellshock_Move(field.ActiveTankNumber, B5_Shellshock_ActionType.Shoot);

            return null;
        }

        #endregion
    }
}
