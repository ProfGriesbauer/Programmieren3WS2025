using System;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet das Essen und dessen Position
    /// </summary>
    public class FoodManager
    {
        private readonly SnakeGameConfig _config;
        private readonly Random _random;

        public PixelPosition CurrentFood { get; private set; }

        public FoodManager(SnakeGameConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        public void SpawnFood()
        {
            int gridX = _random.Next(0, _config.FieldWidth / _config.CellSize);
            int gridY = _random.Next(0, _config.FieldHeight / _config.CellSize);
            CurrentFood = new PixelPosition(gridX * _config.CellSize, gridY * _config.CellSize);
        }
    }
}
