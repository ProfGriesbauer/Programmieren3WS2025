using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
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
                return last.X < 200;
            }
        }

        public int Score { get; private set; } = 0;

        public void CreateObstacle()
        {
            double gap = 140;
            double gapY = _rand.Next(50, (int)(FieldHeight - gap - 50));
            Obstacles.Add(new Obstacle(480, 0, 75, gapY, true));
            double yBottom = gapY + gap;
            double heightBottom = FieldHeight - yBottom;
            Obstacles.Add(new Obstacle(480, yBottom, 75, heightBottom, false));
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
        public bool IsTop;

        public Obstacle(double x, double y, double width, double height, bool isTop)
        {
            X = x; Y = y; Width = width; Height = height; IsTop = isTop;
        }

        public Rect Rectangle => new Rect(X, Y, Width, Height);
    }

    public class FlappyBirdPainter : IPaintGame2
    {
        public string Name => "Flappy Bird";

        private static readonly Brush Bronze = new SolidColorBrush(Color.FromRgb(205, 127, 50));

        public void PaintGameField(Canvas canvas, IGameField field)
        {
            canvas.Children.Clear();
            if (field is FlappyBirdField f)
            {
                f.FieldHeight = canvas.ActualHeight;

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

            if (currentField is FlappyBirdField flappyField && FlappyBirdRules.GameOver)
            {
                DrawGameOver(canvas);
                DrawHighscore(canvas);
            }
        }

        private void DrawGameOver(Canvas canvas)
        {
            var gameOverText = new TextBlock()
            {
                Text = "GAME OVER",
                FontSize = 48,
                FontWeight = FontWeights.ExtraBold,
                Foreground = Brushes.Red,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(gameOverText, (canvas.ActualWidth / 2) - 150);
            Canvas.SetTop(gameOverText, (canvas.ActualHeight / 2) - 100);
            canvas.Children.Add(gameOverText);
        }

        private void DrawHighscore(Canvas canvas)
        {
            var hs = FlappyBirdRules.Highscores;
            var yStart = (canvas.ActualHeight / 2) - 40;

            var title = new TextBlock()
            {
                Text = "Highscores",
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(title, (canvas.ActualWidth / 2) - 60);
            Canvas.SetTop(title, yStart);
            canvas.Children.Add(title);

            yStart += 40;

            for (int i = 0; i < hs.Count; i++)
            {
                int score = hs[i];
                Brush medalColor;
                if (score >= 50) medalColor = Brushes.Gold;
                else if (score >= 20) medalColor = Brushes.Silver;
                else if (score >= 10) medalColor = Bronze;
                else medalColor = Brushes.Black;

                var medal = new Ellipse()
                {
                    Width = 20,
                    Height = 20,
                    Fill = medalColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(medal, (canvas.ActualWidth / 2) - 70);
                Canvas.SetTop(medal, yStart + i * 30 + 3);
                canvas.Children.Add(medal);

                var tb = new TextBlock()
                {
                    Text = $"{i + 1}. {score}",
                    FontSize = 20,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(tb, (canvas.ActualWidth / 2) - 40);
                Canvas.SetTop(tb, yStart + i * 30 - 2);
                canvas.Children.Add(tb);
            }
        }
    }

    public class FlappyBirdRules : IGameRules2
    {
        public string Name => "Flappy Bird Rules";

        private FlappyBirdField _field;

        private const double Gravity = 2.9;
        private const double JumpForce = -18;
        private double _birdVelocity = 0;

        public IGameField CurrentField => _field;
        public bool MovesPossible { get; private set; } = true;

        public static readonly List<int> Highscores = new List<int>();

        public static bool GameOver = false;

        private static readonly string HighscoreFile = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
            "highscores.json");

        public FlappyBirdRules()
        {
            LoadHighscores();
            ClearField();
        }

        private void LoadHighscores()
        {
            if (File.Exists(HighscoreFile))
            {
                try
                {
                    string json = File.ReadAllText(HighscoreFile);
                    var loaded = JsonSerializer.Deserialize<List<int>>(json);
                    if (loaded != null)
                    {
                        Highscores.Clear();
                        Highscores.AddRange(loaded);
                    }
                }
                catch { }
            }
        }

        private void SaveHighscores()
        {
            try
            {
                string json = JsonSerializer.Serialize(Highscores);
                File.WriteAllText(HighscoreFile, json);
            }
            catch { }
        }

        public void ClearField()
        {
            _field = new FlappyBirdField();
            _birdVelocity = 0;
            MovesPossible = true;
            GameOver = false;
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

            _field.UpdateScoreIfPassed();

            if (_field.BirdY < 0 || _field.BirdY + _field.BirdSize > _field.FieldHeight)
            {
                EndGame();
                return;
            }

            var birdRect = _field.BirdRectangle;
            foreach (var obs in _field.Obstacles)
            {
                if (birdRect.IntersectsWith(obs.Rectangle))
                {
                    EndGame();
                    return;
                }
            }
        }

        private void EndGame()
        {
            MovesPossible = false;
            GameOver = true;

            Highscores.Add(_field.Score);
            Highscores.Sort((a, b) => b.CompareTo(a));
            if (Highscores.Count > 10)
                Highscores.RemoveAt(Highscores.Count - 1);

            SaveHighscores();
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
