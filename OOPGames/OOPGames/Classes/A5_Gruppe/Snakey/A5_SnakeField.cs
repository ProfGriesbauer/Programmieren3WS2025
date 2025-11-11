using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace OOPGames
{
    public class A5_SnakeField : IGameField
    {
        // Konstanten für Spielfeld-Konfiguration
        public const int FIELD_WIDTH = 600;
        public const int FIELD_HEIGHT = 600;
        public const int SNAKE_SIZE = 40;
        
        private const double SPEED = 10.0;
        private const int TIMER_INTERVAL_MS = 16; // ~60 FPS

        // Spielzustand
        public List<PixelPosition> Snake { get; private set; }
        public PixelPosition Direction { get; private set; }
        
        private readonly DispatcherTimer _gameTimer;

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField()
        {
            Snake = new List<PixelPosition>();
            InitializeSnake();
            
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL_MS)
            };
            _gameTimer.Tick += OnTimerTick;
            _gameTimer.Start();
        }

        private void InitializeSnake()
        {
            Snake.Clear();
            Snake.Add(new PixelPosition(FIELD_WIDTH / 2.0, FIELD_HEIGHT / 2.0));
            Direction = new PixelPosition(1, 0);
        }

        public void MoveSnake()
        {
            if (Snake.Count == 0) return;
            
            var head = Snake[0];
            var newHead = new PixelPosition(
                head.X + (Direction.X * SPEED),
                head.Y + (Direction.Y * SPEED)
            );
            
            if (IsOutOfBounds(newHead))
            {
                InitializeSnake();
                return;
            }

            Snake.Insert(0, newHead);
            Snake.RemoveAt(Snake.Count - 1);
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
            MoveSnake();
        }

        public int GetPosition(int x, int y)
        {
            return 0; // Legacy-Methode für IGameField-Kompatibilität
        }
    }
}