namespace OOPGames
{
    // Represents a player action: movement, angle/power adjustment, or shooting.
    public class B5_Shellshock_Move : IPlayMove
    {
        private int _playerNumber;
        private B5_Shellshock_ActionType _actionType;
        private double _value;

        public int PlayerNumber => _playerNumber;
        public B5_Shellshock_ActionType ActionType => _actionType;
        public double Value => _value;

        public B5_Shellshock_Move(int playerNumber, B5_Shellshock_ActionType actionType, double value = 0)
        {
            _playerNumber = playerNumber;
            _actionType = actionType;
            _value = value;
        }
    }
}
