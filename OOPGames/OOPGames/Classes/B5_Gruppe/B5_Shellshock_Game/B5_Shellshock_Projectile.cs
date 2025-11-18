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
            // Velocity scaled for normalized coordinates: X in pixel space (0-800), Y in 0-1 space
            double initialVelocityX = power * 2.5; // X velocity in pixels per second (increased from 0.8)
            double initialVelocityY = power * 0.003; // Y velocity in normalized units per second (increased from 0.001)

            _velocityX = initialVelocityX * Math.Cos(angleRad);
            _velocityY = -initialVelocityY * Math.Sin(angleRad); // Negative because Y increases downward
        }

        public void UpdatePosition(double gravity, double wind, double deltaTime)
        {
            if (!_isActive) return;

            // Apply physics: velocity changes due to gravity and wind
            // Gravity scaled for normalized Y coordinates (0-1 range) - increased to flatten arc
            _velocityY += gravity * deltaTime * 0.000005; // Increased from 0.0001 for faster, flatter trajectory
            _velocityX += wind * deltaTime * 0.1; // Wind has less effect

            // Update position based on velocity
            _x += _velocityX * deltaTime;
            _y += _velocityY * deltaTime;
        }
    }
}
