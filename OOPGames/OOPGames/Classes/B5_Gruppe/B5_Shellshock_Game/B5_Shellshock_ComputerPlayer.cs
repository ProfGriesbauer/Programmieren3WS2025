using System;

namespace OOPGames
{
    // AI player that calculates trajectory to hit opponent, considers wind and terrain, 
    // adjusts for difficulty level
    public class B5_Shellshock_ComputerPlayer : IComputerGamePlayer
    {
        private int _playerNumber;
        private B5_Shellshock_AIDifficulty _difficulty;
        private Random _random;

        public string Name => $"B5_Shellshock_AI_Player ({_difficulty})";

        public int PlayerNumber => _playerNumber;

        public B5_Shellshock_ComputerPlayer(B5_Shellshock_AIDifficulty difficulty = B5_Shellshock_AIDifficulty.Medium)
        {
            _playerNumber = 2;
            _difficulty = difficulty;
            _random = new Random();
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
            var clone = new B5_Shellshock_ComputerPlayer(_difficulty);
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is not B5_Shellshock_Field shellField)
                return null;

            B5_Shellshock_Tank myTank = shellField.GetTankByPlayer(_playerNumber);
            B5_Shellshock_Tank opponentTank = shellField.GetOpponentTank(_playerNumber);

            // Random chance to adjust position, angle, or power before shooting
            int action = _random.Next(0, 10);

            if (action < 2) // 20% chance to adjust angle
            {
                double optimalAngle = CalculateOptimalAngle(shellField);
                if (Math.Abs(myTank.Angle - optimalAngle) > 5)
                {
                    return new B5_Shellshock_Move(_playerNumber, 
                        myTank.Angle < optimalAngle ? B5_Shellshock_ActionType.IncreaseAngle : B5_Shellshock_ActionType.DecreaseAngle);
                }
            }
            else if (action < 4) // 20% chance to adjust power
            {
                double optimalPower = CalculateOptimalPower(shellField);
                if (Math.Abs(myTank.Power - optimalPower) > 5)
                {
                    return new B5_Shellshock_Move(_playerNumber, 
                        myTank.Power < optimalPower ? B5_Shellshock_ActionType.IncreasePower : B5_Shellshock_ActionType.DecreasePower);
                }
            }

            // Most of the time, shoot
            return new B5_Shellshock_Move(_playerNumber, B5_Shellshock_ActionType.Shoot);
        }

        private double CalculateOptimalAngle(B5_Shellshock_Field field)
        {
            B5_Shellshock_Tank myTank = field.GetTankByPlayer(_playerNumber);
            B5_Shellshock_Tank opponentTank = field.GetOpponentTank(_playerNumber);

            // Calculate distance to opponent
            double distance = Math.Abs(opponentTank.X - myTank.X);
            double heightDiff = opponentTank.Y - myTank.Y;

            // Simple angle calculation based on distance
            // Higher angle for longer distances
            double baseAngle = 45 + (distance / 800.0) * 30; // 45-75 degrees

            // Adjust for height difference
            if (heightDiff < 0) // Shooting upward
                baseAngle += 10;
            else if (heightDiff > 0) // Shooting downward
                baseAngle -= 10;

            // Add random error based on difficulty
            double error = AddRandomError();
            return Math.Max(10, Math.Min(170, baseAngle + error));
        }

        private double CalculateOptimalPower(B5_Shellshock_Field field)
        {
            B5_Shellshock_Tank myTank = field.GetTankByPlayer(_playerNumber);
            B5_Shellshock_Tank opponentTank = field.GetOpponentTank(_playerNumber);

            // Calculate distance to opponent
            double distance = Math.Abs(opponentTank.X - myTank.X);

            // Power scales with distance
            double basePower = (distance / 800.0) * 80 + 20; // 20-100 power

            // Add random error based on difficulty
            double error = AddRandomError();
            return Math.Max(10, Math.Min(100, basePower + error));
        }

        private bool SimulateShot(double angle, double power, B5_Shellshock_Field field)
        {
            // Simple simulation to check if shot would hit
            // This is a simplified version - full physics simulation would be more accurate
            B5_Shellshock_Tank myTank = field.GetTankByPlayer(_playerNumber);
            B5_Shellshock_Tank opponentTank = field.GetOpponentTank(_playerNumber);

            double distance = Math.Abs(opponentTank.X - myTank.X);
            double estimatedRange = power * Math.Cos(angle * Math.PI / 180.0) * 10;

            return Math.Abs(estimatedRange - distance) < 50;
        }

        private double AddRandomError()
        {
            // Add random error based on difficulty
            double maxError = _difficulty switch
            {
                B5_Shellshock_AIDifficulty.Easy => 20.0,
                B5_Shellshock_AIDifficulty.Medium => 10.0,
                B5_Shellshock_AIDifficulty.Hard => 5.0,
                _ => 10.0
            };

            return (_random.NextDouble() * 2 - 1) * maxError; // Random between -maxError and +maxError
        }
    }
}
