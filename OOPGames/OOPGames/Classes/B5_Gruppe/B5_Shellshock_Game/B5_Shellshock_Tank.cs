using System;

namespace OOPGames
{
    /// <summary>
    /// Represents a combat tank in the Shellshock game.
    /// Encapsulates all tank-related state (position, angle, power, health) and behavior (movement, aiming, firing).
    /// Implements value clamping to ensure valid game state.
    /// </summary>
    public class B5_Shellshock_Tank
    {
        // Position in game world
        private double _x;
        private double _y;
        
        // Combat parameters
        private double _angle;  // Firing angle in degrees [0-180]
        private double _power;  // Shot power percentage [0-100]
        private int _health;     // Health points [0-100]
        
        // Visual identification
        private readonly B5_Shellshock_TankColor _color;

        // Tank dimensions for collision detection
        public const double CollisionWidth = 28;      // Width in pixels
        public const double CollisionHeight = 0.03;   // Height in normalized coordinates

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

        /// <summary>
        /// Firing angle in degrees. Automatically clamped to [0, 180] range.
        /// 0° = horizontal right, 90° = vertical up, 180° = horizontal left.
        /// </summary>
        public double Angle
        {
            get => _angle;
            set => _angle = Math.Max(0, Math.Min(180, value));
        }

        /// <summary>
        /// Shot power as percentage. Automatically clamped to [0, 100] range.
        /// Affects gravity scaling: higher power = flatter trajectory.
        /// </summary>
        public double Power
        {
            get => _power;
            set => _power = Math.Max(0, Math.Min(100, value));
        }

        /// <summary>
        /// Health points. Automatically clamped to minimum 0.
        /// Tank is destroyed when health reaches 0.
        /// </summary>
        public int Health
        {
            get => _health;
            set => _health = Math.Max(0, value);
        }

        public B5_Shellshock_TankColor Color => _color;

        public bool IsAlive => _health > 0;

        #endregion

        public B5_Shellshock_Tank(double x, double y, B5_Shellshock_TankColor color)
        {
            _x = x;
            _y = y;
            _color = color;
            _angle = 45;    // Default 45° angle for balanced shots
            _power = 50;    // Default mid-range power
            _health = 100;  // Start at full health
        }

        #region Combat Methods

        /// <summary>
        /// Reduces tank health by specified damage amount.
        /// Health is clamped to minimum 0.
        /// </summary>
        public void TakeDamage(int damage)
        {
            _health = Math.Max(0, _health - damage);
        }

        /// <summary>
        /// Creates and returns a projectile fired from this tank's current position and settings.
        /// </summary>
        public B5_Shellshock_Projectile Fire(int playerNumber)
        {
            return new B5_Shellshock_Projectile(_x, _y, _angle, _power, playerNumber);
        }

        #endregion

        #region Movement Methods

        public void MoveLeft(double distance)
        {
            _x -= distance;
        }

        public void MoveRight(double distance)
        {
            _x += distance;
        }

        #endregion

        #region Aiming Methods

        /// <summary>
        /// Adjusts firing angle by specified delta.
        /// Result is automatically clamped to valid range.
        /// </summary>
        public void AdjustAngle(double delta)
        {
            Angle = _angle + delta;
        }

        /// <summary>
        /// Adjusts firing power by specified delta.
        /// Result is automatically clamped to valid range.
        /// </summary>
        public void AdjustPower(double delta)
        {
            Power = _power + delta;
        }

        #endregion

        #region Collision Detection

        /// <summary>
        /// Checks if this tank collides with a projectile using bounding box collision.
        /// </summary>
        public bool CollidesWith(B5_Shellshock_Projectile projectile)
        {
            return projectile.X >= _x - CollisionWidth / 2 &&
                   projectile.X <= _x + CollisionWidth / 2 &&
                   projectile.Y >= _y - CollisionHeight &&
                   projectile.Y <= _y;
        }

        #endregion
    }
}
