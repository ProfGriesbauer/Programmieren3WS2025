using System;

namespace OOPGames
{
    /// <summary>
    /// Represents a health pack that spawns in the air.
    /// Heals the tank that collects it by shooting it.
    /// Appears as a red box with white cross.
    /// </summary>
    public class B5_Shellshock_HealthPack
    {
        private double _x;
        private double _y;
        private bool _isActive;

        // Health pack dimensions and properties
        public const double PackSize = 30; // Width/Height in pixels
        public const int HealAmount = 40;

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

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public B5_Shellshock_HealthPack(double x, double y)
        {
            _x = x;
            _y = y;
            _isActive = true;
        }

        /// <summary>
        /// Checks if a projectile collides with this health pack.
        /// Uses simple bounding box collision.
        /// </summary>
        public bool CollidesWith(B5_Shellshock_Projectile projectile)
        {
            double halfSize = PackSize / 2;
            return projectile.X >= _x - halfSize &&
                   projectile.X <= _x + halfSize &&
                   projectile.Y >= _y - PackSize * 0.001 && // Convert pixels to normalized
                   projectile.Y <= _y + PackSize * 0.001;
        }
    }
}
