using System;

namespace OOPGames
{
    /// <summary>
    /// Konkrete Klasse (Concrete Class): Represents a combat tank in the Shellshock game.
    /// 
    /// Inheritance Hierarchy:
    /// - B5_Shellshock_GameEntity (abstrakte Oberklasse/Superklasse)
    ///   - B5_Shellshock_CollidableEntity (abstrakte Unterklasse mit ICollidable)
    ///     - B5_Shellshock_Tank (konkrete Implementierung)
    /// 
    /// OOP Concepts demonstrated:
    /// - Vererbung (Inheritance): Extends B5_Shellshock_CollidableEntity
    /// - Kapselung (Encapsulation): Private fields with public accessors and validation
    /// - Polymorphie: Overrides EntityType, GetCollisionBounds
    /// - Ersetzbarkeit (Liskov Substitution): Can be used wherever ICollidable or GameEntity is expected
    /// 
    /// Invarianten (Class Invariants):
    /// - Angle is always in range [0, 180]
    /// - Power is always in range [0, 100]
    /// - Health is always >= 0
    /// - IsAlive is derived from Health > 0
    /// 
    /// Vorbedingungen (Preconditions) for constructor:
    /// - x and y must be valid terrain positions
    /// - color must be a valid TankColor enum value
    /// 
    /// Nachbedingungen (Postconditions) for constructor:
    /// - Tank is created with full health (100)
    /// - Tank has default angle (45) and power (50)
    /// - Tank is active (IsActive = true from base)
    /// </summary>
    public class B5_Shellshock_Tank : B5_Shellshock_CollidableEntity
    {
        // Combat parameters
        private double _angle;  // Firing angle in degrees [0-180]
        private double _power;  // Shot power percentage [0-100]
        private int _health;     // Health points [0-100]
        
        // Visual identification
        private readonly B5_Shellshock_TankColor _color;

        // Tank dimensions for collision detection
        public const double CollisionWidth = 28;      // Width in pixels
        public const double CollisionHeight = 0.03;   // Height in normalized coordinates

        #region Overridden Properties from Base

        /// <summary>
        /// Entity type identifier for debugging and logging.
        /// Implements abstract property from B5_Shellshock_GameEntity.
        /// </summary>
        public override string EntityType => $"Tank ({_color})";

        #endregion

        #region Properties

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

        /// <summary>
        /// Derived property: Tank is alive if health > 0.
        /// Demonstrates Ersetzbarkeit: overrides base IsActive logic.
        /// </summary>
        public bool IsAlive => _health > 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new tank at the specified position.
        /// Calls base constructor to initialize position and active state.
        /// 
        /// Vorbedingungen:
        /// - x, y should be valid terrain positions
        /// 
        /// Nachbedingungen:
        /// - Tank created with full health, default angle and power
        /// </summary>
        public B5_Shellshock_Tank(double x, double y, B5_Shellshock_TankColor color) 
            : base(x, y)  // Call base class constructor (Vererbung)
        {
            _color = color;
            _angle = 45;    // Default 45° angle for balanced shots
            _power = 50;    // Default mid-range power
            _health = 100;  // Start at full health
        }

        #endregion

        #region Abstract Method Implementations

        /// <summary>
        /// Implements abstract method from B5_Shellshock_CollidableEntity.
        /// Defines the tank's collision bounding box.
        /// 
        /// Polymorphie: This implementation is used when CollidesWith is called
        /// on a Tank instance, even through a base class reference.
        /// </summary>
        protected override (double minX, double maxX, double minY, double maxY) GetCollisionBounds()
        {
            return (
                _x - CollisionWidth / 2,
                _x + CollisionWidth / 2,
                _y - CollisionHeight,
                _y
            );
        }

        #endregion

        #region Combat Methods

        /// <summary>
        /// Reduces tank health by specified damage amount.
        /// Health is clamped to minimum 0 (Invariante preserved).
        /// 
        /// Vorbedingung: damage >= 0
        /// Nachbedingung: health = max(0, old_health - damage)
        /// </summary>
        public void TakeDamage(int damage)
        {
            _health = Math.Max(0, _health - damage);
        }

        /// <summary>
        /// Creates and returns a projectile fired from this tank's current position and settings.
        /// Factory method pattern - tank creates its own projectiles.
        /// 
        /// Vorbedingung: playerNumber must be 1 or 2
        /// Nachbedingung: Returns new active projectile with tank's angle/power
        /// </summary>
        public B5_Shellshock_Projectile Fire(int playerNumber)
        {
            return new B5_Shellshock_Projectile(_x, _y, _angle, _power, playerNumber);
        }

        #endregion

        #region Movement Methods

        /// <summary>
        /// Moves tank left by specified distance.
        /// Position update uses inherited _x field.
        /// </summary>
        public void MoveLeft(double distance)
        {
            _x -= distance;
        }

        /// <summary>
        /// Moves tank right by specified distance.
        /// Position update uses inherited _x field.
        /// </summary>
        public void MoveRight(double distance)
        {
            _x += distance;
        }

        #endregion

        #region Aiming Methods

        /// <summary>
        /// Adjusts firing angle by specified delta.
        /// Result is automatically clamped to valid range (Invariante).
        /// </summary>
        public void AdjustAngle(double delta)
        {
            Angle = _angle + delta;
        }

        /// <summary>
        /// Adjusts firing power by specified delta.
        /// Result is automatically clamped to valid range (Invariante).
        /// </summary>
        public void AdjustPower(double delta)
        {
            Power = _power + delta;
        }

        #endregion
    }
}
