using System;

namespace OOPGames
{
    /// <summary>
    /// Konkrete Klasse: Represents a health pack pickup that spawns in the air.
    /// Heals the tank that collects it by shooting it.
    /// Appears as a red box with white cross.
    /// 
    /// Inheritance Hierarchy:
    /// - B5_Shellshock_GameEntity (abstrakte Oberklasse/Superklasse)
    ///   - B5_Shellshock_CollidableEntity (abstrakte Unterklasse mit ICollidable)
    ///     - B5_Shellshock_HealthPack (konkrete Implementierung)
    /// 
    /// OOP Concepts demonstrated:
    /// - Vererbung: Extends B5_Shellshock_CollidableEntity
    /// - Polymorphie: Different collision shape than Tank
    /// - Ersetzbarkeit (Liskov): Can be used wherever ICollidable is expected
    /// 
    /// Invarianten:
    /// - PackSize is constant (30 pixels)
    /// - HealAmount is constant (40 HP)
    /// </summary>
    public class B5_Shellshock_HealthPack : B5_Shellshock_CollidableEntity
    {
        // Health pack dimensions and properties
        public const double PackSize = 30; // Width/Height in pixels
        public const int HealAmount = 40;

        #region Properties

        /// <summary>
        /// Entity type identifier for debugging.
        /// Implements abstract property from B5_Shellshock_GameEntity.
        /// </summary>
        public override string EntityType => "HealthPack";

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a health pack at the specified position.
        /// 
        /// Vorbedingung: x, y should be valid game coordinates
        /// Nachbedingung: HealthPack is active and ready to be collected
        /// </summary>
        public B5_Shellshock_HealthPack(double x, double y) 
            : base(x, y)  // Call base constructor (Vererbung)
        {
            // Base constructor sets IsActive = true
        }

        #endregion

        #region Abstract Method Implementations

        /// <summary>
        /// Implements abstract method from B5_Shellshock_CollidableEntity.
        /// Defines the health pack's collision bounding box.
        /// 
        /// Polymorphie: Different collision shape than Tank - 
        /// square-shaped collision area centered on position.
        /// </summary>
        protected override (double minX, double maxX, double minY, double maxY) GetCollisionBounds()
        {
            double halfSize = PackSize / 2;
            // Convert pixel size to normalized Y coordinates for consistency
            double normalizedSize = PackSize * 0.001;
            
            return (
                _x - halfSize,
                _x + halfSize,
                _y - normalizedSize,
                _y + normalizedSize
            );
        }

        #endregion
    }
}
