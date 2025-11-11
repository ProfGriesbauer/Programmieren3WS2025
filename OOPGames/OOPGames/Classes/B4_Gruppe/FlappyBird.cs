using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class FlappyBirdJumpMove : IPlayMove
    {
        public int PlayerNumber { get; private set; }
        public FlappyBirdJumpMove(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }
    }

    public class FlappyBirdField : IGameField
    {
        public double BirdX { get; set; } = 100;
        public double BirdY { get; set; } = 200;
        public double BirdSize { get; set; } = 30;
        public double FieldHeight { get; set; } = 600;

        public List<Obstacle> Obstacles { get; private set; } = new List<Obstacle>();

        public Rect BirdRectangle => new Rect(BirdX, BirdY, BirdSize, BirdSize);

        private static readonly Random _rand = new Random();

        public bool NeedsObstacle
        {
            get
            {
                if (Obstacles.Count == 0) return true;
                var last = Obstacles[Obstacles.Count - 1];
                return last.X < 400;
            }
        }

        // Score: zählt nur Überquerte obere Hindernisse
        public int Score { get; private set; } = 0;

        public void CreateObstacle()
        {
            double gap = 250;
            double gapY = _rand.Next(50, (int)(FieldHeight - gap - 50));

            Obstacles.Add(new Obstacle(480, 0, 50, gapY, true));   // oberes Rohr mit IsTop = true
            Obstacles.Add(new Obstacle(480, gapY + gap, 50, FieldHeight - gapY - gap, false)); // unteres Rohr
        }

        public void UpdateScoreIfPassed()
        {
            foreach (var obs in Obstacles)
            {
                if (obs.IsTop && !obs.Passed && obs.X + obs.Width < BirdX)
                {
                    obs.Passed = true;
                    Score++;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is FlappyBirdPainter;
        }
    }

    public class Obstacle
    {
        public double X, Y, Width, Height;
        public bool Passed = false;
        public bool IsTop; // NEU: kennzeichnet oberes Paar

        public Obstacle(double x, double y, double width, double height, bool isTop)
        {
            X = x; Y = y; Width = width; Height = height; IsTop = isTop;
        }

        public Rect Rectangle => new Rect(X, Y, Width, Height);
    }

    public class FlappyBirdPainter : IPaintGame2
    {
        public string Name => "Flappy Bird";

        public void PaintGameField(Canvas canvas, IGameField field)
        {
            canvas.Children.Clear();

            if (field is FlappyBirdField f)
            {
                var bg = new Rectangle()
                {
                    Width = canvas.ActualWidth,
                    Height = canvas.ActualHeight,
                    Fill = Brushes.SkyBlue
                };
                canvas.Children.Add(bg);

                var bird = new Ellipse()
                {
                    Width = f.BirdSize,
                    Height = f.BirdSize,
                    Fill = Brushes.Yellow
                };
                Canvas.SetLeft(bird, f.BirdX);
                Canvas.SetTop(bird, f.BirdY);
                canvas.Children.Add(bird);

                foreach (var obs in f.Obstacles)
                {
                    var rect = new Rectangle()
                    {
                        Width = obs.Width,
                        Height = obs.Height,
                        Fill = Brushes.Green
                    };
                    Canvas.SetLeft(rect, obs.X);
                    Canvas.SetTop(rect, obs.Y);
                    canvas.Children.Add(rect);
                }

                // Score-Anzeige
                var scoreText = new TextBlock()
                {
                    Text = $"Score: {f.Score}",
                    FontSize = 24,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(scoreText, 10);
                Canvas.SetTop(scoreText, 10);
                canvas.Children.Add(scoreText);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }

    public class FlappyBirdRules : IGameRules2
    {
        public string Name => "Flappy Bird Rules";

        private FlappyBirdField _field;

        private const double Gravity = 0.5;
        private const double JumpForce = -8;
        private double _birdVelocity = 0;

        public IGameField CurrentField => _field;
        public bool MovesPossible { get; private set; } = true;

        public FlappyBirdRules()
        {
            ClearField();
        }

        public void ClearField()
        {
            _field = new FlappyBirdField();
            _birdVelocity = 0;
            MovesPossible = true;
        }

        public void DoMove(IPlayMove move)
        {
            if (!MovesPossible) return;

            if (move is FlappyBirdJumpMove jumpMove)
            {
                if (jumpMove.PlayerNumber == 1 || jumpMove.PlayerNumber == 2)
                {
                    _birdVelocity = JumpForce;
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            return MovesPossible ? -1 : 1;
        }

        public void StartedGameCall() { }

        public void TickGameCall()
        {
            if (!MovesPossible) return;

            _birdVelocity += Gravity;
            _field.BirdY += _birdVelocity;

            for (int i = _field.Obstacles.Count - 1; i >= 0; i--)
            {
                var obs = _field.Obstacles[i];
                obs.X -= 5;
                if (obs.X + obs.Width < 0)
                {
                    _field.Obstacles.RemoveAt(i);
                }
            }

            if (_field.NeedsObstacle)
            {
                _field.CreateObstacle();
            }

            // Score aktualisieren (nur für obere Hindernisse)
            _field.UpdateScoreIfPassed();

            if (_field.BirdY < 0 || _field.BirdY + _field.BirdSize > _field.FieldHeight)
            {
                MovesPossible = false;
                return;
            }

            var birdRect = _field.BirdRectangle;
            foreach (var obs in _field.Obstacles)
            {
                if (birdRect.IntersectsWith(obs.Rectangle))
                {
                    MovesPossible = false;
                    return;
                }
            }
        }
    }

    public class FlappyBirdHumanPlayer : IHumanGamePlayer
    {
        public string Name => "Flappy Bird Human Player";

        private int _playerNumber;
        public int PlayerNumber => _playerNumber;

        public void SetPlayerNumber(int number)
        {
            _playerNumber = number;
        }

        public IGamePlayer Clone()
        {
            return new FlappyBirdHumanPlayer();
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is FlappyBirdRules;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IClickSelection)
                return new FlappyBirdJumpMove(PlayerNumber);

            if (selection is IKeySelection keySel && keySel.Key == Key.Space)
                return new FlappyBirdJumpMove(PlayerNumber);

            return null;
        }
    }

    public class FlappyBird
    {
        public void Register()
        {
            OOPGamesManager.Singleton.RegisterPainter(new FlappyBirdPainter());
            OOPGamesManager.Singleton.RegisterRules(new FlappyBirdRules());
            OOPGamesManager.Singleton.RegisterPlayer(new FlappyBirdHumanPlayer());
        }
    }
}
