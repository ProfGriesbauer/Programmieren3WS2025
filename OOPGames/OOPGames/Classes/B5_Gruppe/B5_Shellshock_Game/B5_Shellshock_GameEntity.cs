using System;

namespace OOPGames
{
    /// <summary>
    /// Interface for objects that can collide with projectiles.
    /// Enables polymorphic collision detection in the game engine.
    /// 
    /// Vertrag (Contract):
    /// - Vorbedingung (Precondition): projectile must not be null
    /// - Nachbedingung (Postcondition): returns true if collision occurred, false otherwise
    /// - Invariante: collision detection is stateless and does not modify either object
    /// </summary>
    public interface ICollidable
    {
        /// <summary>
        /// Checks if this object collides with the given projectile.
        /// </summary>
        /// <param name="projectile">The projectile to check collision against</param>
        /// <returns>True if collision detected, false otherwise</returns>
        bool CollidesWith(B5_Shellshock_Projectile projectile);
    }

    /// <summary>
    /// Interface for objects that can be rendered on the game canvas.
    /// Enables polymorphic rendering in the painter.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>X position in game coordinates (pixels)</summary>
        double X { get; }
        
        /// <summary>Y position in normalized coordinates (0-1)</summary>
        double Y { get; }
        
        /// <summary>Whether this entity should be rendered</summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Abstrakte Basisklasse (Abstract Base Class) for all game entities.
    /// Provides common functionality for position, activation state.
    /// 
    /// Demonstrates:
    /// - Abstrakte Klasse: Cannot be instantiated directly
    /// - Vererbung (Inheritance): Tank, Projectile, HealthPack inherit from this
    /// - Polymorphie: Subclasses can override virtual methods
    /// - Kapselung (Encapsulation): Protected fields, public properties
    /// 
    /// Invarianten (Class Invariants):
    /// - X is always a valid pixel coordinate (may be negative for off-screen)
    /// - Y is in normalized range (0.0 = top, 1.0 = bottom)
    /// </summary>
    public abstract class B5_Shellshock_GameEntity : IRenderable
    {
        #region Protected Fields (for subclass access)

        protected double _x;
        protected double _y;
        protected bool _isActive;

        #endregion

        #region Properties

        /// <summary>
        /// X position in game world (pixels).
        /// Subclasses may override to add validation.
        /// </summary>
        public virtual double X
        {
            get => _x;
            set => _x = value;
        }

        /// <summary>
        /// Y position in normalized coordinates.
        /// 0.0 = top of screen, 1.0 = bottom of screen.
        /// </summary>
        public virtual double Y
        {
            get => _y;
            set => _y = value;
        }

        /// <summary>
        /// Whether this entity is active/visible in the game.
        /// Inactive entities are not rendered or processed.
        /// </summary>
        public virtual bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new game entity at the specified position.
        /// </summary>
        /// <param name="x">Initial X position in pixels</param>
        /// <param name="y">Initial Y position in normalized coordinates</param>
        protected B5_Shellshock_GameEntity(double x, double y)
        {
            _x = x;
            _y = y;
            _isActive = true;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the display name of this entity type for debugging.
        /// Must be implemented by all concrete subclasses.
        /// </summary>
        public abstract string EntityType { get; }

        #endregion

        #region Virtual Methods (can be overridden)

        /// <summary>
        /// Calculates the distance to another entity.
        /// Virtual method allows subclasses to customize distance calculation.
        /// </summary>
        public virtual double DistanceTo(B5_Shellshock_GameEntity other)
        {
            double dx = _x - other._x;
            double dy = _y - other._y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Deactivates this entity, removing it from gameplay.
        /// Virtual to allow subclasses to perform cleanup.
        /// </summary>
        public virtual void Deactivate()
        {
            _isActive = false;
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            return $"{EntityType} at ({_x:F1}, {_y:F3}) Active={_isActive}";
        }

        #endregion
    }

    /// <summary>
    /// Abstrakte Basisklasse für Spielobjekte, die kollidieren können.
    /// Kombiniert GameEntity mit ICollidable Interface.
    /// 
    /// Demonstriert:
    /// - Interface-Implementierung in abstrakter Klasse
    /// - Template Method Pattern: CollidesWith definiert Rahmen, 
    ///   Subklassen implementieren GetCollisionBounds
    /// </summary>
    public abstract class B5_Shellshock_CollidableEntity : B5_Shellshock_GameEntity, ICollidable
    {
        protected B5_Shellshock_CollidableEntity(double x, double y) : base(x, y)
        {
        }

        /// <summary>
        /// Gets the collision bounds for this entity.
        /// Subclasses define their own collision shape.
        /// Returns (minX, maxX, minY, maxY).
        /// </summary>
        protected abstract (double minX, double maxX, double minY, double maxY) GetCollisionBounds();

        /// <summary>
        /// Template method for collision detection.
        /// Uses GetCollisionBounds() from subclasses for actual shape.
        /// 
        /// Vorbedingung: projectile != null
        /// Nachbedingung: returns collision result without modifying state
        /// </summary>
        public virtual bool CollidesWith(B5_Shellshock_Projectile projectile)
        {
            if (projectile == null) return false;
            if (!_isActive) return false;

            var bounds = GetCollisionBounds();
            return projectile.X >= bounds.minX &&
                   projectile.X <= bounds.maxX &&
                   projectile.Y >= bounds.minY &&
                   projectile.Y <= bounds.maxY;
        }
    }
}
