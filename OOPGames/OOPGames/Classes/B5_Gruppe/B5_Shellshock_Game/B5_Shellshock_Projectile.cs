using System;

namespace OOPGames
{
    /// <summary>
    /// Represents a projectile (artillery shell) in flight.
    /// Implements realistic ballistic physics with gravity and wind effects.
    /// Power affects gravity scaling rather than launch speed for consistent visual trajectory.
    /// </summary>
    public class B5_Shellshock_Projectile
    {
        // Position and velocity
        private double _x;
        private double _y;
        private double _velocityX;
        private double _velocityY;
        
        // State
        private bool _isActive;
        private int _playerNumber;
        
        // Physics parameters
        private double _powerNorm; // Normalized power [0, 1] for gravity scaling

        #region Physics Constants
        
        /// <summary>
        /// Base gravity multiplier. Lower value = longer range projectiles.
        /// Tuned for balanced gameplay across different power levels.
        /// </summary>
        private const double BaseGravityFactor = 0.03;
        
        /// <summary>
        /// Exponent for nonlinear power-to-gravity mapping.
        /// Higher value = more dramatic difference between power levels.
        /// </summary>
        private const double PowerExponent = 1.6;
        
        /// <summary>
        /// Constant launch speed in pixels per second.
        /// Power affects trajectory arc (via gravity) rather than launch speed,
        /// ensuring consistent visual draw speed regardless of power setting.
        /// </summary>
        private const double LaunchSpeed = 300.0;
        
        #endregion

        #region Properties

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

        #endregion

        public B5_Shellshock_Projectile(double x, double y, double angle, double power, int playerNumber)
        {
            _x = x;
            _y = y;
            _playerNumber = playerNumber;
            _isActive = true;
            _powerNorm = Math.Max(0.0, Math.Min(1.0, power / 100.0));

            // Calculate initial velocity from firing angle
            // Launch speed is constant; angle determines direction
            double angleRad = angle * Math.PI / 180.0;
            _velocityX = LaunchSpeed * Math.Cos(angleRad);
            // Y velocity scaled down for normalized coordinate system
            _velocityY = -LaunchSpeed * Math.Sin(angleRad) / 500.0;
        }

        /// <summary>
        /// Updates projectile position based on ballistic physics simulation.
        /// Implements gravity and wind effects with power-based gravity scaling.
        /// Higher power = reduced gravity = flatter, longer trajectory.
        /// </summary>
        /// <param name="gravity">Gravity constant (typically 9.8 m/s²)</param>
        /// <param name="wind">Wind force (positive = right, negative = left)</param>
        /// <param name="deltaTime">Time step for physics update (~0.04 for 25 FPS)</param>
        public void UpdatePosition(double gravity, double wind, double deltaTime)
        {
            if (!_isActive) return;

            // Power-scaled gravity calculation
            // Creates nonlinear mapping: low power → high effective gravity → steep arc
            // High power → low effective gravity → flat trajectory
            double denom = 0.15 + 0.85 * Math.Pow(_powerNorm, PowerExponent);
            double powerGravityScale = 1.0 / denom;
            // Scale range: ~6.7 at low power down to 1.0 at max power
            
            // Apply gravity to vertical velocity (scaled by power)
            _velocityY += gravity * deltaTime * BaseGravityFactor * powerGravityScale;

            // Apply wind force to horizontal velocity (multiplied for visibility)
            _velocityX += wind * deltaTime * 1.5;

            // Update position based on current velocity
            _x += _velocityX * deltaTime;
            _y += _velocityY * deltaTime;
        }
    }
}
