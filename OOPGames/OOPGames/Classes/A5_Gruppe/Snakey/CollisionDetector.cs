using System;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet Kollisionserkennung und Bounds-Checking
    /// </summary>
    public class CollisionDetector
    {
        private readonly SnakeGameConfig _config;

        public CollisionDetector(SnakeGameConfig config)
        {
            _config = config;
        }

        public bool IsOutOfBounds(PixelPosition position)
        {
            return position.X < -_config.DeathMargin ||
                   position.Y < -_config.DeathMargin ||
                   position.X + _config.CellSize > _config.FieldWidth + _config.DeathMargin ||
                   position.Y + _config.CellSize > _config.FieldHeight + _config.DeathMargin;
        }

        public bool IsFoodEaten(PixelPosition head, PixelPosition food)
        {
            double dx = Math.Abs(head.X - food.X);
            double dy = Math.Abs(head.Y - food.Y);
            return dx < _config.CellSize && dy < _config.CellSize;
        }
    }
}
