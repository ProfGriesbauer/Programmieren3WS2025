using System;

namespace OOPGames
{
    /// <summary>
    /// Konkrete Klasse: Represents a projectile (artillery shell) in flight.
    /// Implements realistic ballistic physics with gravity and wind effects.
    /// Power affects gravity scaling rather than launch speed for consistent visual trajectory.
    /// 
    /// Inheritance Hierarchy:
    /// - B5_Shellshock_GameEntity (abstrakte Oberklasse/Superklasse)
    ///   - B5_Shellshock_Projectile (konkrete Implementierung)
    /// 
    /// Note: Does NOT inherit from CollidableEntity because projectiles are
    /// the "colliders" that hit other objects, not the "collidees" that get hit.
    /// This follows the Single Responsibility Principle.
    /// 
    /// OOP Concepts demonstrated:
    /// - Vererbung: Extends B5_Shellshock_GameEntity for position/active state
    /// - Kapselung: Physics constants are private, behavior is public
    /// - Polymorphie: Overrides EntityType from base class
    /// 
    /// Invarianten:
    /// - _powerNorm is always in range [0, 1]
    /// - Physics constants are immutable
    /// - IsActive becomes false when projectile hits something
    /// </summary>
    public class B5_Shellshock_Projectile : B5_Shellshock_GameEntity
    {
        // Velocity components
        private double _velocityX;
        private double _velocityY;
        
        // Ownership
        private readonly int _playerNumber;
        
        // Physics parameters
        private readonly double _powerNorm; // Normalized power [0, 1] for gravity scaling

        #region Physics Constants
        
        /// <summary>
        /// Base gravity multiplier. Lower value = longer range projectiles.
        /// Tuned for balanced gameplay across different power levels.
        /// </summary>
        private const double BaseGravityFactor = 0.025;
        
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

        #region Overridden Properties

        /// <summary>
        /// Entity type identifier for debugging.
        /// Implements abstract property from B5_Shellshock_GameEntity.
        /// </summary>
        public override string EntityType => $"Projectile (P{_playerNumber})";

        #endregion

        #region Properties

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

        public int PlayerNumber => _playerNumber;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a projectile at the specified position with given angle and power.
        /// 
        /// Vorbedingung: 
        /// - angle should be in [0, 180]
        /// - power should be in [0, 100]
        /// - playerNumber should be 1 or 2
        /// 
        /// Nachbedingung:
        /// - Projectile is active
        /// - Velocity calculated from angle
        /// - Power normalized to [0, 1]
        /// </summary>
        public B5_Shellshock_Projectile(double x, double y, double angle, double power, int playerNumber)
            : base(x, y)  // Call base constructor (Vererbung)
        {
            _playerNumber = playerNumber;
            // Base constructor sets _isActive = true
            _powerNorm = Math.Max(0.0, Math.Min(1.0, power / 100.0));

            // Calculate initial velocity from firing angle
            // Launch speed is constant; angle determines direction
            double angleRad = angle * Math.PI / 180.0;
            _velocityX = LaunchSpeed * Math.Cos(angleRad);
            // Y velocity scaled down for normalized coordinate system
            _velocityY = -LaunchSpeed * Math.Sin(angleRad) / 500.0;
        }

        #endregion

        #region Physics Methods

        /// <summary>
        /// Updates projectile position based on ballistic physics simulation.
        /// Implements gravity and wind effects with power-based gravity scaling.
        /// Higher power = reduced gravity = flatter, longer trajectory.
        /// 
        /// Vorbedingung: deltaTime > 0
        /// Nachbedingung: Position updated according to physics equations
        /// Invariante: Only modifies position/velocity if IsActive
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

        #endregion
    }
}
