using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using OOPGames;
using OOPGames;

namespace OOPGames
{
    // Direction enum for Froggo moves
    public enum FroggoDirection { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }

    // Move implementation for Froggo
    public class FroggoMove : IPlayMove
    {
        public int PlayerNumber { get; }
        public FroggoDirection Direction { get; }

        public FroggoMove(int playerNumber, FroggoDirection direction)
        {
            PlayerNumber = playerNumber;
            Direction = direction;
        }
    }

    // Game field for Froggo
    public class FroggoField : IGameField
    {
        public int Width { get; }
        public int Height { get; }
        
        // Player position: Item1 = x, Item2 = y
        public (int, int) PlayerPosition { get; set; } = (5, 9);
        
        // Obstacles: list of (x, y) tuples
        public List<(int, int)> Obstacles { get; } = new List<(int, int)>();
        
        // Offset for obstacle movement (moves left, so offset increases)
        public int ObstacleOffset { get; set; } = 0;

        public FroggoField(int width, int height)
        {
            Width = width;
            Height = height;
            InitializeObstacles();
        }

        private void InitializeObstacles()
        {
            Random rand = new Random();
            // Add obstacles in rows 3 and 6 with random gap and random obstacle block width
            for (int row = 3; row <= 6; row += 3)
            {
                // Gap: random start, random width (at least 2, up to Width/2)
                int minGap = 2;
                int maxGap = Math.Max(minGap, Width / 2);
                int gapWidth = rand.Next(minGap, maxGap + 1);
                int gapStart = rand.Next(0, Width - gapWidth + 1);
                
                int x = 0;
                while (x < Width) {
                    // If in gap, skip
                    if (x >= gapStart && x < gapStart + gapWidth) {
                        x = gapStart + gapWidth;
                        continue;
                    }
                    // Obstacle block: random width, at least 2
                    int minBlock = 2;
                    int maxBlock = Math.Max(minBlock, Width / 2);
                    int blockWidth = rand.Next(minBlock, maxBlock + 1);
                    for (int bx = 0; bx < blockWidth && x < Width && (x < gapStart || x >= gapStart + gapWidth); bx++, x++) {
                        Obstacles.Add((x, row));
                    }
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is FroggoPainter;
        }
    }

    // Human player for Froggo
    public class FroggoPlayer : IHumanGamePlayer, IHumanGamePlayerWithMouse
    {
        private int _playerNumber = 0;
        public string Name { get; set; } = "Froggo_Player";

        public void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;
        public int PlayerNumber => _playerNumber;

        public bool CanBeRuledBy(IGameRules rules) => rules is FroggoRules;

        public IGamePlayer Clone()
        {
            return new FroggoPlayer() { Name = this.Name };
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(field is FroggoField)) return null;

            if (selection is IKeySelection keySel)
            {
                switch (keySel.Key)
                {
                    case Key.Up:
                    case Key.W:
                        return new FroggoMove(_playerNumber, FroggoDirection.Up);
                    case Key.Down:
                    case Key.S:
                        return new FroggoMove(_playerNumber, FroggoDirection.Down);
                    case Key.Left:
                    case Key.A:
                        return new FroggoMove(_playerNumber, FroggoDirection.Left);
                    case Key.Right:
                    case Key.D:
                        return new FroggoMove(_playerNumber, FroggoDirection.Right);
                    default:
                        return null;
                }
            }
            return null;
        }

        public void OnMouseMoved(MouseEventArgs e) { }
    }

    // Painter for Froggo
    public class FroggoPainter : IPaintGame2
    {
        public string Name => "Froggo_Painter";

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!(currentField is FroggoField field)) return;

            canvas.Children.Clear();
            double cellW = Math.Max(10, canvas.ActualWidth / Math.Max(1, field.Width));
            double cellH = Math.Max(10, canvas.ActualHeight / Math.Max(1, field.Height));

            // Draw grid background
            var background = new Rectangle() { Width = canvas.ActualWidth, Height = canvas.ActualHeight, Fill = Brushes.White };
            canvas.Children.Add(background);

            // Draw obstacles as red rectangles with movement offset
            foreach (var obs in field.Obstacles)
            {
                // Apply offset to x position (wrapping around the field width)
                int offsetX = ((obs.Item1 - field.ObstacleOffset) % field.Width + field.Width) % field.Width;
                var rect = new Rectangle() { Width = cellW * 0.9, Height = cellH * 0.9, Fill = Brushes.Red };
                Canvas.SetLeft(rect, offsetX * cellW + cellW * 0.05);
                Canvas.SetTop(rect, obs.Item2 * cellH + cellH * 0.05);
                canvas.Children.Add(rect);
            }

            // Draw player (frog) as a green ellipse
            var playerX = field.PlayerPosition.Item1;
            var playerY = field.PlayerPosition.Item2;
            var frog = new Ellipse() { Width = cellW * 0.8, Height = cellH * 0.8, Fill = Brushes.Green, Stroke = Brushes.DarkGreen, StrokeThickness = 2 };
            Canvas.SetLeft(frog, playerX * cellW + cellW * 0.1);
            Canvas.SetTop(frog, playerY * cellH + cellH * 0.1);
            canvas.Children.Add(frog);

            // Draw grid lines
            for (int x = 0; x <= field.Width; x++)
            {
                var line = new Line() { X1 = x * cellW, Y1 = 0, X2 = x * cellW, Y2 = field.Height * cellH, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                canvas.Children.Add(line);
            }
            for (int y = 0; y <= field.Height; y++)
            {
                var line = new Line() { X1 = 0, Y1 = y * cellH, X2 = field.Width * cellW, Y2 = y * cellH, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                canvas.Children.Add(line);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }

    // Rules for Froggo
    public class FroggoRules : IGameRules, IGameRules2
    {
        public string Name => "Froggo_Rules";

        private FroggoField _field;

        public FroggoRules(int width = 10, int height = 10)
        {
            _field = new FroggoField(width, height);
        }

        public IGameField CurrentField => _field;

        public bool MovesPossible => _field.PlayerPosition.Item2 >= 0;

        public void DoMove(IPlayMove move)
        {
            if (move is FroggoMove fm)
            {
                var pos = _field.PlayerPosition;
                int nx = pos.Item1, ny = pos.Item2;

                switch (fm.Direction)
                {
                    case FroggoDirection.Up: ny = Math.Max(0, pos.Item2 - 1); break;
                    case FroggoDirection.Down: ny = Math.Min(_field.Height - 1, pos.Item2 + 1); break;
                    case FroggoDirection.Left: nx = Math.Max(0, pos.Item1 - 1); break;
                    case FroggoDirection.Right: nx = Math.Min(_field.Width - 1, pos.Item1 + 1); break;
                }

                // Check for collision with obstacles
                bool collision = _field.Obstacles.Any(obs => obs.Item1 == nx && obs.Item2 == ny);
                if (!collision)
                {
                    _field.PlayerPosition = (nx, ny);
                }
            }
        }

        public void ClearField()
        {
            _field = new FroggoField(_field.Width, _field.Height);
        }

        public int CheckIfPLayerWon()
        {
            // Win if player reaches top row
            if (_field.PlayerPosition.Item2 == 0)
                return 1;
            return -1;
        }

        public void StartedGameCall() { }

        // Add this field to control obstacle speed
        private int _obstacleTickCounter = 0;
        private const int ObstacleMoveInterval = 4; // Move every 4 ticks

        public void TickGameCall()
        {
            // Move obstacles left only every ObstacleMoveInterval ticks
            _obstacleTickCounter++;
            if (_obstacleTickCounter >= ObstacleMoveInterval)
            {
                _field.ObstacleOffset = (_field.ObstacleOffset + 1) % _field.Width;
                _obstacleTickCounter = 0;
            }
        }
    }

    // Main game class integrating with WPF framework
    public class FroggoGame
    {
        public void Register()
        {
            var rules = new FroggoRules(20, 20);
            OOPGamesManager.Singleton.RegisterPainter(new FroggoPainter());
            OOPGamesManager.Singleton.RegisterRules(rules);
            OOPGamesManager.Singleton.RegisterPlayer(new FroggoPlayer());
        }
    }
}

