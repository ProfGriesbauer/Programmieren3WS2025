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
        private readonly SnakeEntity _snake1;
        private readonly SnakeEntity _snake2;
        private readonly SnakeMovementController _movementController1;
        private readonly SnakeMovementController _movementController2;
        private readonly CollisionDetector _collisionDetector;
        private readonly FoodManager _foodManager;
        private readonly GameTimerManager _timerManager;
        private readonly bool _isTwoPlayerMode;

        // Public properties for compatibility
        public List<PixelPosition> Snake => new List<PixelPosition>(_snake1.Segments);
        public List<PixelPosition> Snake2 => _snake2 != null ? new List<PixelPosition>(_snake2.Segments) : new List<PixelPosition>();
        public PixelPosition Direction => _snake1.Direction;
        public PixelPosition Food => _foodManager.CurrentFood;
        public int CountdownSeconds => _timerManager.CountdownSeconds;
        public bool IsCountingDown => _timerManager.IsCountingDown;
        public bool IsGameRunning => _timerManager.IsGameRunning;
        public bool IsTwoPlayerMode => _isTwoPlayerMode;

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField() : this(false)
        {
        }

        public A5_SnakeField(bool twoPlayerMode)
        {
            _config = new SnakeGameConfig();
            _snake1 = new SnakeEntity(_config, 1);
            _movementController1 = new SnakeMovementController(_config);
            _isTwoPlayerMode = twoPlayerMode;
            
            if (twoPlayerMode)
            {
                _snake2 = new SnakeEntity(_config, 2);
                _movementController2 = new SnakeMovementController(_config);
            }
            
            _collisionDetector = new CollisionDetector(_config);
            _foodManager = new FoodManager(_config, new Random());
            _timerManager = new GameTimerManager(_config.TimerIntervalMs, OnGameTick, OnCountdownTick);

            InitializeGame();
        }

        private void InitializeGame()
        {
            double startX = _config.FieldWidth / 2.0 - 16;
            double startY = _config.FieldHeight / 2.0 - 16;

            _snake1.Initialize(startX, startY);
            _movementController1.Reset();
            
            if (_isTwoPlayerMode && _snake2 != null)
            {
                // Snake 2 startet 2 Kästchen tiefer
                double startY2 = startY + (2 * _config.CellSize);
                _snake2.Initialize(startX, startY2);
                _movementController2.Reset();
            }
            
            _foodManager.SpawnFood();
            // Countdown wird erst durch Space gestartet
        }

        public void ChangeDirection(double dx, double dy, int playerNumber = 1)
        {
            var newDirection = new PixelPosition(dx, dy);
            
            if (playerNumber == 1)
            {
                if (_snake1.IsAlive)
                {
                    _movementController1.QueueDirectionChange(newDirection, _snake1.Direction, _snake1.Head);
                }
            }
            else if (playerNumber == 2 && _isTwoPlayerMode && _snake2 != null)
            {
                if (_snake2.IsAlive)
                {
                    _movementController2.QueueDirectionChange(newDirection, _snake2.Direction, _snake2.Head);
                }
            }
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
            MoveSnakeInternal(_snake1, _movementController1);
            
            if (_isTwoPlayerMode && _snake2 != null && _snake2.IsAlive)
            {
                MoveSnakeInternal(_snake2, _movementController2);
            }
            
            // Check inter-snake collision (nur im 2-Spieler-Modus)
            if (_isTwoPlayerMode && _snake1.IsAlive && _snake2 != null && _snake2.IsAlive)
            {
                if (CheckInterSnakeCollision(_snake1, _snake2))
                {
                    _snake1.Kill();
                    _snake2.Kill();
                    _timerManager.StopGameTimer();
                    System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => 
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => InitializeGame());
                    });
                    return;
                }
            }
            
            // Check food für beide Schlangen
            if (_snake1.IsAlive && _collisionDetector.IsFoodEaten(_snake1.Head, _foodManager.CurrentFood))
            {
                _snake1.Grow();
                _foodManager.SpawnFood();
            }
            
            if (_isTwoPlayerMode && _snake2 != null && _snake2.IsAlive && 
                _collisionDetector.IsFoodEaten(_snake2.Head, _foodManager.CurrentFood))
            {
                _snake2.Grow();
                _foodManager.SpawnFood();
            }
        }

        private void MoveSnakeInternal(SnakeEntity snake, SnakeMovementController controller)
        {
            if (snake.Head == null || !snake.IsAlive) return;

            // Check for pending turn
            if (controller.ShouldExecuteTurn(snake.Head, snake.Direction, out var newDirection, out var targetPos))
            {
                snake.SetHeadPosition(targetPos.X, targetPos.Y);
                snake.SetDirection(newDirection);

                var histPos = new PixelPosition(snake.Head.X, snake.Head.Y);
                histPos.DirX = newDirection.X;
                histPos.DirY = newDirection.Y;
                snake.AddHistoryPoint(histPos);
            }

            // Move head
            double deltaX = snake.Direction.X * _config.Speed;
            double deltaY = snake.Direction.Y * _config.Speed;
            snake.MoveHead(deltaX, deltaY);

            // Check collisions
            if (_collisionDetector.IsOutOfBounds(snake.Head) || snake.CheckSelfCollision())
            {
                snake.Kill();
                _timerManager.StopGameTimer();
                System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => 
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => InitializeGame());
                });
                return;
            }

            // Update tail
            snake.UpdateTailPositions();
            snake.TrimHistory();
        }

        private bool CheckInterSnakeCollision(SnakeEntity snake1, SnakeEntity snake2)
        {
            // Prüfe ob Kopf von Snake1 mit irgendeinem Segment von Snake2 kollidiert
            foreach (var segment in snake2.Segments)
            {
                double dx = Math.Abs(snake1.Head.X - segment.X);
                double dy = Math.Abs(snake1.Head.Y - segment.Y);
                if (dx < _config.CellSize / 2 && dy < _config.CellSize / 2)
                {
                    return true;
                }
            }
            
            // Prüfe ob Kopf von Snake2 mit irgendeinem Segment von Snake1 kollidiert
            foreach (var segment in snake1.Segments)
            {
                double dx = Math.Abs(snake2.Head.X - segment.X);
                double dy = Math.Abs(snake2.Head.Y - segment.Y);
                if (dx < _config.CellSize / 2 && dy < _config.CellSize / 2)
                {
                    return true;
                }
            }
            
            return false;
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
