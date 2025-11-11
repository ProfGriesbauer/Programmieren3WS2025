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
        private const double SPEED = 10.0; // Pixels per tick for smooth movement
        private const int TIMER_INTERVAL_MS = 16; // ~60 FPS

        // Game state
        public List<PixelPosition> Snake { get; private set; }
        public PixelPosition Direction { get; private set; }
        public PixelPosition Food { get; private set; }
        public int CountdownSeconds { get; private set; }
        public bool IsCountingDown { get; private set; }

        private readonly DispatcherTimer _gameTimer;
        private readonly DispatcherTimer _countdownTimer;
        private readonly Random _random;
        private int _initialTailLength = 3;

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
            double startX = FIELD_WIDTH / 2.0;
            double startY = FIELD_HEIGHT / 2.0;

            // Head at start position
            Snake.Add(new PixelPosition(startX, startY));
            Direction = new PixelPosition(1, 0);

            // Seed history with points behind head along -X axis
            // This ensures initial tail segments appear correctly spaced
            int neededHistory = (_initialTailLength - 1) * StepGap + StepGap * 2;
            for (int i = 0; i <= neededHistory; i++)
            {
                _history.Add(new PixelPosition(startX - (i * SPEED), startY));
            }

            // Create additional segments positioned from history
            for (int i = 1; i < _initialTailLength; i++)
            {
                Snake.Add(new PixelPosition(startX - (i * SEGMENT_GAP), startY));
            }
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
                Direction = newDirection;
            }
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