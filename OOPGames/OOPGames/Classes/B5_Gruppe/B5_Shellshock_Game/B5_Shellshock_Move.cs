namespace OOPGames
{
    /// <summary>
    /// Represents a player action in the Shellshock game.
    /// Implements IPlayMove for framework integration.
    /// 
    /// Move types:
    /// - Movement: MoveLeft, MoveRight (costs movement points)
    /// - Aiming: IncreaseAngle, DecreaseAngle, IncreasePower, DecreasePower (free)
    /// - Action: Shoot (ends turn), StartGame (begins/restarts game)
    /// 
    /// Created by players (Human/Computer), executed by B5_Shellshock_Rules.DoMove().
    /// </summary>
    public class B5_Shellshock_Move : IPlayMove
    {
        #region Fields

        private readonly int _playerNumber;
        private readonly B5_Shellshock_ActionType _actionType;
        private readonly double _value;

        #endregion

        #region Properties

        /// <summary>Player who made this move (1 or 2).</summary>
        public int PlayerNumber => _playerNumber;

        /// <summary>Type of action to perform.</summary>
        public B5_Shellshock_ActionType ActionType => _actionType;

        /// <summary>Optional value for parameterized actions (currently unused).</summary>
        public double Value => _value;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new move for the specified player.
        /// </summary>
        /// <param name="playerNumber">Player making the move (1 or 2)</param>
        /// <param name="actionType">Action to perform</param>
        /// <param name="value">Optional parameter value (default 0)</param>
        public B5_Shellshock_Move(int playerNumber, B5_Shellshock_ActionType actionType, double value = 0)
        {
            _playerNumber = playerNumber;
            _actionType = actionType;
            _value = value;
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            return $"Player {_playerNumber}: {_actionType}";
        }

        #endregion
    }
}
