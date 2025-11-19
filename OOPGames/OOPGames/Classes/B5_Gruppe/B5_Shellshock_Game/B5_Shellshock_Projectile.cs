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
        private double _powerNorm; // 0..1 for gravity scaling
        // Tunable parameters for trajectory feel
        private const double BaseGravityFactor = 0.03; // lower gravity factor to extend range
        private const double PowerExponent = 1.6;      // gentler curve so mid power travels further

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
            _powerNorm = Math.Max(0.0, Math.Min(1.0, power / 100.0));

            // Convert angle to radians and calculate initial velocity
            double angleRad = angle * Math.PI / 180.0;

            // Constant launch speed; direction equals barrel angle
            // This keeps draw speed visually constant and aligns shot direction with barrel
            double launchSpeedPxPerSec = 300.0; // constant speed magnitude
            _velocityX = launchSpeedPxPerSec * Math.Cos(angleRad);
            _velocityY = -launchSpeedPxPerSec * Math.Sin(angleRad) / 500.0; // stronger vertical to ensure reasonable arc at power 50
        }

        public void UpdatePosition(double gravity, double wind, double deltaTime)
        {
            if (!_isActive) return;

            // Apply physics: velocity changes due to gravity and wind
            // Gravity scaled for normalized Y coordinates; power reduces effective gravity (higher power -> longer range)
            // Nonlinear mapping for stronger scaling by power
            double denom = 0.15 + 0.85 * Math.Pow(_powerNorm, PowerExponent); // 0.15..1.0 as power goes 0..1
            double powerGravityScale = 1.0 / denom;                           // ~6.7 at very low power down to 1.0 at max
            _velocityY += gravity * deltaTime * BaseGravityFactor * powerGravityScale;

            // Strengthen wind effect so it is noticeable
            _velocityX += wind * deltaTime * 1.5;

            // Update position based on velocity
            _x += _velocityX * deltaTime;
            _y += _velocityY * deltaTime;
        }
    }
}
