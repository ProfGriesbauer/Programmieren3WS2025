using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace OOPGames
{
    public class A5_SnakeField : IGameField
    {
        // Game field configuration constants
        public const int FIELD_WIDTH = 480;
        public const int FIELD_HEIGHT = 480;
        public const int SNAKE_SIZE = 32;

        // Segment spacing in pixels equals image size
        private const double SEGMENT_GAP = SNAKE_SIZE;
        private const double SPEED = 8; // Pixels per tick for smooth movement
        private const int TIMER_INTERVAL_MS = 16; // ~60 FPS

        // Game state
        public List<PixelPosition> Snake { get; private set; }
        public PixelPosition Direction { get; private set; }
        public PixelPosition Food { get; private set; }
        public int CountdownSeconds { get; private set; }
        public bool IsCountingDown { get; private set; }

        // Pending direction change for grid-aligned movement
        private PixelPosition _pendingDirection;
        private PixelPosition _targetPosition; // Target position where direction change should happen

        private readonly DispatcherTimer _gameTimer;
        private readonly DispatcherTimer _countdownTimer;
        private readonly Random _random;

        // Head position history for continuous segment spacing
        private readonly List<PixelPosition> _history = new List<PixelPosition>();

        // Number of history steps equal to one segment gap
        private int StepGap => Math.Max(1, (int)Math.Round(SEGMENT_GAP / SPEED));

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField()
        {
            _random = new Random();
            Snake = new List<PixelPosition>();
            
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL_MS)
            };
            _gameTimer.Tick += OnTimerTick;

            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += OnCountdownTick;

            InitializeSnake();
            SpawnFood();
            StartCountdown();
        }

        private void InitializeSnake()
        {
            Snake.Clear();
            _history.Clear();
            
            // Start in the center of the middle grid cell
            // Field is 480x480 with 32x32 cells = 15x15 grid
            // Center should be at (240-16, 240-16) = (224, 224)
            double startX = FIELD_WIDTH / 2.0 - 16; // 224
            double startY = FIELD_HEIGHT / 2.0 - 16; // 224

            // Head at start position - only head, no tail initially
            Snake.Add(new PixelPosition(startX, startY));
            Direction = new PixelPosition(1, 0);
            _pendingDirection = null;
            _targetPosition = null;

            // Minimal history - just the starting position
            _history.Add(new PixelPosition(startX, startY));
        }

        private void SpawnFood()
        {
            int gridX = _random.Next(0, FIELD_WIDTH / SNAKE_SIZE);
            int gridY = _random.Next(0, FIELD_HEIGHT / SNAKE_SIZE);
            Food = new PixelPosition(gridX * SNAKE_SIZE, gridY * SNAKE_SIZE);
        }

        public void MoveSnake()
        {
            if (Snake.Count == 0) return;

            var head = Snake[0];
            
            // Check if we have reached the target position for direction change
            if (_targetPosition != null && _pendingDirection != null)
            {
                // Check if we have reached or passed the target position
                bool reachedTarget = false;
                
                if (Direction.X > 0) // Moving right
                    reachedTarget = head.X >= _targetPosition.X;
                else if (Direction.X < 0) // Moving left
                    reachedTarget = head.X <= _targetPosition.X;
                else if (Direction.Y > 0) // Moving down
                    reachedTarget = head.Y >= _targetPosition.Y;
                else if (Direction.Y < 0) // Moving up
                    reachedTarget = head.Y <= _targetPosition.Y;
                
                if (reachedTarget)
                {
                    // Move to exact target position and change direction
                    Snake[0].X = _targetPosition.X;
                    Snake[0].Y = _targetPosition.Y;
                    Direction = _pendingDirection;
                    _pendingDirection = null;
                    _targetPosition = null;
                    
                    // Update history with the target position
                    _history.Add(new PixelPosition(Snake[0].X, Snake[0].Y));
                }
            }
            
            var newHead = new PixelPosition(
                head.X + (Direction.X * SPEED),
                head.Y + (Direction.Y * SPEED)
            );

            if (IsOutOfBounds(newHead) || CheckSelfCollision(newHead))
            {
                _gameTimer.Stop();
                InitializeSnake();
                SpawnFood();
                StartCountdown();
                return;
            }

            // Update head and extend history
            Snake[0] = newHead;
            _history.Add(new PixelPosition(newHead.X, newHead.Y));

            // Position tail segments from history with SEGMENT_GAP spacing
            for (int i = 1; i < Snake.Count; i++)
            {
                int idx = _history.Count - 1 - (i * StepGap);
                if (idx >= 0)
                {
                    var p = _history[idx];
                    Snake[i].X = p.X;
                    Snake[i].Y = p.Y;
                }
            }

            // Trim history to keep only necessary data
            int maxHistory = (Snake.Count + 5) * StepGap;
            int toTrim = _history.Count - maxHistory;
            if (toTrim > 0)
            {
                _history.RemoveRange(0, toTrim);
            }

            // Check if food is eaten
            if (IsFoodEaten(newHead))
            {
                var last = Snake[Snake.Count - 1];
                Snake.Add(new PixelPosition(last.X, last.Y));
                SpawnFood();
            }
        }

        private bool IsFoodEaten(PixelPosition head)
        {
            double dx = Math.Abs(head.X - Food.X);
            double dy = Math.Abs(head.Y - Food.Y);
            return dx < SNAKE_SIZE && dy < SNAKE_SIZE;
        }

        private bool CheckSelfCollision(PixelPosition head)
        {
            // Skip first few segments to avoid false collision
            for (int i = 3; i < Snake.Count; i++)
            {
                double dx = Math.Abs(head.X - Snake[i].X);
                double dy = Math.Abs(head.Y - Snake[i].Y);
                if (dx < SNAKE_SIZE / 2 && dy < SNAKE_SIZE / 2)
                {
                    return true;
                }
            }
            return false;
        }

        public void ChangeDirection(double dx, double dy)
        {
            var newDirection = new PixelPosition(dx, dy);
            if (!newDirection.IsOpposite(Direction))
            {
                var head = Snake[0];
                
                // Calculate which grid cell the head is currently in
                int gridX = (int)(head.X / SNAKE_SIZE);
                int gridY = (int)(head.Y / SNAKE_SIZE);
                
                // Calculate center of current grid cell (corrected by -16 pixels)
                double centerX = gridX * SNAKE_SIZE + SNAKE_SIZE / 2.0 - 16;
                double centerY = gridY * SNAKE_SIZE + SNAKE_SIZE / 2.0 - 16;
                
                // Calculate 3/4 threshold point (24 pixels from start of cell, corrected by -16)
                double threeQuarterX = gridX * SNAKE_SIZE + (SNAKE_SIZE * 3.0 / 4.0) - 16;
                double threeQuarterY = gridY * SNAKE_SIZE + (SNAKE_SIZE * 3.0 / 4.0) - 16;
                
                // Determine target position based on current position relative to 3/4 point
                PixelPosition targetPos;
                
                // Check if we're before or after the 3/4 point in the direction of movement
                bool beforeThreeQuarterX = (Direction.X > 0 && head.X < threeQuarterX) || (Direction.X < 0 && head.X > threeQuarterX);
                bool beforeThreeQuarterY = (Direction.Y > 0 && head.Y < threeQuarterY) || (Direction.Y < 0 && head.Y > threeQuarterY);
                bool atThreeQuarterX = Direction.X == 0 || Math.Abs(head.X - threeQuarterX) < 1;
                bool atThreeQuarterY = Direction.Y == 0 || Math.Abs(head.Y - threeQuarterY) < 1;
                
                if ((beforeThreeQuarterX || atThreeQuarterX) && (beforeThreeQuarterY || atThreeQuarterY))
                {
                    // Haven't reached 3/4 point yet, target is center of current cell
                    targetPos = new PixelPosition(centerX, centerY);
                }
                else
                {
                    // Past 3/4 point, target is center of next cell in current direction
                    double nextCenterX = (gridX + Direction.X) * SNAKE_SIZE + SNAKE_SIZE / 2.0 - 16;
                    double nextCenterY = (gridY + Direction.Y) * SNAKE_SIZE + SNAKE_SIZE / 2.0 - 16;
                    targetPos = new PixelPosition(nextCenterX, nextCenterY);
                }
                
                _pendingDirection = newDirection;
                _targetPosition = targetPos;
            }
        }

        private bool IsAtGridPosition(PixelPosition position)
        {
            // Check if position is aligned to grid (multiple of SNAKE_SIZE)
            const double tolerance = 0.1; // Small tolerance for floating point precision
            double gridX = position.X / SNAKE_SIZE;
            double gridY = position.Y / SNAKE_SIZE;
            
            return Math.Abs(gridX - Math.Round(gridX)) < tolerance &&
                   Math.Abs(gridY - Math.Round(gridY)) < tolerance;
        }

        private bool IsOutOfBounds(PixelPosition position)
        {
            return position.X < 0 || position.X >= FIELD_WIDTH ||
                   position.Y < 0 || position.Y >= FIELD_HEIGHT;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (!IsCountingDown)
            {
                MoveSnake();
            }
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            CountdownSeconds--;
            if (CountdownSeconds <= 0)
            {
                _countdownTimer.Stop();
                IsCountingDown = false;
                _gameTimer.Start();
            }
        }

        private void StartCountdown()
        {
            CountdownSeconds = 3;
            IsCountingDown = true;
            _countdownTimer.Start();
        }

        public int GetPosition(int x, int y)
        {
            return 0; // IGameField interface compatibility
        }
    }
}