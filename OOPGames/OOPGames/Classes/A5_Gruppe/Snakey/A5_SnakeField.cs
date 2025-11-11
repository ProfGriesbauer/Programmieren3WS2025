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
    // Abstand zwischen Kopf- und Schwanzsegmenten in Pixeln = Bildgröße
    private const double SEGMENT_GAP = SNAKE_SIZE;
        
    private const double SPEED = 10.0; // Pixel pro Tick (flüssige Bewegung)
        private const int TIMER_INTERVAL_MS = 16; // ~60 FPS

        // Spielzustand
        public List<PixelPosition> Snake { get; private set; }
        public PixelPosition Direction { get; private set; }
        public PixelPosition Food { get; private set; }
        
    private readonly DispatcherTimer _gameTimer;
        private readonly Random _random;
        private int _initialTailLength = 3; // Startlänge des Schwanzes
    // Verlauf der Kopfpositionen (für kontinuierlichen Abstand der Segmente)
    private readonly List<PixelPosition> _history = new List<PixelPosition>();

    // Anzahl History-Schritte, die einem Segmentabstand entsprechen
    private int StepGap => Math.Max(1, (int)Math.Round(SEGMENT_GAP / SPEED));

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField()
        {
            _random = new Random();
            Snake = new List<PixelPosition>();
            InitializeSnake();
            SpawnFood();
            
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
            _history.Clear();
            double startX = FIELD_WIDTH / 2.0;
            double startY = FIELD_HEIGHT / 2.0;

            // Kopf an Startposition
            Snake.Add(new PixelPosition(startX, startY));
            Direction = new PixelPosition(1, 0);

            // Seed der History: Punkte hinter dem Kopf entlang -X, so dass
            // die ersten Schwanzsegmente sofort korrekt mit Abstand erscheinen
            int neededHistory = (_initialTailLength - 1) * StepGap + StepGap * 2; // etwas Puffer
            for (int i = 0; i <= neededHistory; i++)
            {
                // Lege Punkte in 1-Schritt-Abständen von SPEED entlang der negativen X-Achse an
                _history.Add(new PixelPosition(startX - (i * SPEED), startY));
            }

            // Erzeuge weitere Segmente; sie werden später aus der History positioniert
            for (int i = 1; i < _initialTailLength; i++)
            {
                Snake.Add(new PixelPosition(startX - (i * SEGMENT_GAP), startY));
            }
        }

        private void SpawnFood()
        {
            // Spawn food at random grid-aligned position
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
                InitializeSnake();
                SpawnFood();
                return;
            }

            // Kopf aktualisieren und History erweitern
            Snake[0] = newHead;
            _history.Add(new PixelPosition(newHead.X, newHead.Y));

            // Positioniere die Schwanzsegmente anhand des Verlaufs so, dass
            // jedes Segment in SEGMENT_GAP-Abstand dem Kopf folgt
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

            // History begrenzen: ausreichend für aktuelle Segmentanzahl + Puffer
            int maxHistory = (Snake.Count + 5) * StepGap;
            int toTrim = _history.Count - maxHistory;
            if (toTrim > 0)
            {
                _history.RemoveRange(0, toTrim);
            }

            // Check if food is eaten
            if (IsFoodEaten(newHead))
            {
                // Schlange wächst: füge ein neues Segment am Ende hinzu
                var last = Snake[Snake.Count - 1];
                Snake.Add(new PixelPosition(last.X, last.Y));
                SpawnFood();
            }
            // Kein automatisches Entfernen des letzten Segments nötig, da
            // die Positionen über den Verlauf gesteuert werden
        }

        private bool IsFoodEaten(PixelPosition head)
        {
            // Check if head is close enough to food (within SNAKE_SIZE distance)
            double dx = Math.Abs(head.X - Food.X);
            double dy = Math.Abs(head.Y - Food.Y);
            return dx < SNAKE_SIZE && dy < SNAKE_SIZE;
        }

        private bool CheckSelfCollision(PixelPosition head)
        {
            // Check collision with own body (skip first few segments)
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
            MoveSnake();
        }

        public int GetPosition(int x, int y)
        {
            return 0; // Legacy-Methode für IGameField-Kompatibilität
        }
    }
}