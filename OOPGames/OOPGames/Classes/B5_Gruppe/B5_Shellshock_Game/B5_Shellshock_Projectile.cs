using System;

namespace OOPGames
{
    // Represents a fired shell in flight. Updates position based on physics.
    public class B5_Shellshock_Projectile
    {
        private double _x;
        private double _y;
        private double _velocityX;
        private double _velocityY;
        private bool _isActive;
        private int _playerNumber;

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public double VelocityX
        {
            get => _velocityX;
            set => _velocityX = value;
        }

        public double VelocityY
        {
            get => _velocityY;
            set => _velocityY = value;
        }

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public int PlayerNumber => _playerNumber;

        public B5_Shellshock_Projectile(double x, double y, double angle, double power, int playerNumber)
        {
            _x = x;
            _y = y;
            _playerNumber = playerNumber;
            _isActive = true;

            // Convert angle to radians and calculate initial velocity
            double angleRad = angle * Math.PI / 180.0;
            double initialVelocity = power * 0.8; // Scale power to velocity

            _velocityX = initialVelocity * Math.Cos(angleRad);
            _velocityY = -initialVelocity * Math.Sin(angleRad); // Negative because Y increases downward
        }

        public void UpdatePosition(double gravity, double wind, double deltaTime)
        {
            if (!_isActive) return;

            // Apply physics: velocity changes due to gravity and wind
            _velocityY += gravity * deltaTime;
            _velocityX += wind * deltaTime * 0.1; // Wind has less effect

            // Update position based on velocity
            _x += _velocityX * deltaTime;
            _y += _velocityY * deltaTime;
        }
    }
}
