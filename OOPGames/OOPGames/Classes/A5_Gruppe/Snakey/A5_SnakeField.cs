using System;
using System.Collections.Generic;

namespace OOPGames
{
    /// <summary>
    /// Hauptklasse für das Snake-Spielfeld mit OOP-Architektur
    /// </summary>
    public class A5_SnakeField : IGameField
    {
        // Legacy constants for compatibility
        public const int FIELD_WIDTH = 480;
        public const int FIELD_HEIGHT = 480;
        public const int SNAKE_SIZE = 32;

        // OOP Components
        private readonly SnakeGameConfig _config;
        private readonly SnakeEntity _snake;
        private readonly SnakeMovementController _movementController;
        private readonly CollisionDetector _collisionDetector;
        private readonly FoodManager _foodManager;
        private readonly GameTimerManager _timerManager;

        // Public properties for compatibility
        public List<PixelPosition> Snake => new List<PixelPosition>(_snake.Segments);
        public PixelPosition Direction => _snake.Direction;
        public PixelPosition Food => _foodManager.CurrentFood;
        public int CountdownSeconds => _timerManager.CountdownSeconds;
        public bool IsCountingDown => _timerManager.IsCountingDown;
        public bool IsGameRunning => _timerManager.IsGameRunning;

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField()
        {
            _config = new SnakeGameConfig();
            _snake = new SnakeEntity(_config);
            _movementController = new SnakeMovementController(_config);
            _collisionDetector = new CollisionDetector(_config);
            _foodManager = new FoodManager(_config, new Random());
            _timerManager = new GameTimerManager(_config.TimerIntervalMs, OnGameTick, OnCountdownTick);

            InitializeGame();
        }

        private void InitializeGame()
        {
            double startX = _config.FieldWidth / 2.0 - 16;
            double startY = _config.FieldHeight / 2.0 - 16;

            _snake.Initialize(startX, startY);
            _movementController.Reset();
            _foodManager.SpawnFood();
            // Countdown wird erst durch Space gestartet
        }

        public void ChangeDirection(double dx, double dy)
        {
            var newDirection = new PixelPosition(dx, dy);
            _movementController.QueueDirectionChange(newDirection, _snake.Direction, _snake.Head);
        }

        private void OnGameTick()
        {
            MoveSnake();
        }

        private void OnCountdownTick()
        {
            // Countdown managed by GameTimerManager
        }

        public void MoveSnake()
        {
            if (_snake.Head == null) return;

            // Check for pending turn
            if (_movementController.ShouldExecuteTurn(_snake.Head, _snake.Direction, out var newDirection, out var targetPos))
            {
                _snake.SetHeadPosition(targetPos.X, targetPos.Y);
                _snake.SetDirection(newDirection);

                var histPos = new PixelPosition(_snake.Head.X, _snake.Head.Y);
                histPos.DirX = newDirection.X;
                histPos.DirY = newDirection.Y;
                _snake.AddHistoryPoint(histPos);
            }

            // Move head
            double deltaX = _snake.Direction.X * _config.Speed;
            double deltaY = _snake.Direction.Y * _config.Speed;
            _snake.MoveHead(deltaX, deltaY);

            // Check collisions
            if (_collisionDetector.IsOutOfBounds(_snake.Head) || _snake.CheckSelfCollision())
            {
                _timerManager.StopGameTimer();
                InitializeGame();
                return;
            }

            // Update tail
            _snake.UpdateTailPositions();
            _snake.TrimHistory();

            // Check food
            if (_collisionDetector.IsFoodEaten(_snake.Head, _foodManager.CurrentFood))
            {
                _snake.Grow();
                _foodManager.SpawnFood();
            }
        }

        public int GetPosition(int x, int y)
        {
            return 0; // IGameField interface compatibility
        }

        public void StartGame()
        {
            _timerManager.StartCountdown(_config.CountdownSeconds);
        }
    }
}
