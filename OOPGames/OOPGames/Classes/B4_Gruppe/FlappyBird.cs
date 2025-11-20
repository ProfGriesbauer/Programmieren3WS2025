using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    public abstract class PipePart
    {
        public abstract void Draw(Canvas canvas, double x, double y, double width, double height, bool isTop);
    }

    public class PipeHead : PipePart
    {
        private ImageSource _image;
        public PipeHead(ImageSource image) => _image = image;
        public override void Draw(Canvas canvas, double x, double y, double width, double height, bool isTop)
        {
            var img = new Image()
            {
                Source = _image,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill
            };
            if (isTop)
            {
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                img.RenderTransform = new RotateTransform(180);
            }
            Canvas.SetLeft(img, x);
            Canvas.SetTop(img, y);
            canvas.Children.Add(img);
        }
    }

    public class PipeBody : PipePart
    {
        private ImageSource _image;
        public PipeBody(ImageSource image) => _image = image;
        public override void Draw(Canvas canvas, double x, double y, double width, double height, bool isTop)
        {
            var img = new Image()
            {
                Source = _image,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill
            };
            if (isTop)
            {
                img.RenderTransformOrigin = new Point(0.5, 0.5);
                img.RenderTransform = new RotateTransform(180);
            }
            Canvas.SetLeft(img, x);
            Canvas.SetTop(img, y);
            canvas.Children.Add(img);
        }
    }

    public class SoundManager
    {
        private MediaPlayer jumpSound;
        private MediaPlayer hitSound;
        private MediaPlayer scoreSound;
        private MediaPlayer backgroundMusic;

        private string backgroundMusicPath;
        private double _volume = 0.3;
        private bool isBackgroundPlaying = false;

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                jumpSound.Volume = _volume;
                hitSound.Volume = _volume;
                scoreSound.Volume = _volume;
                if (backgroundMusic != null)
                    backgroundMusic.Volume = _volume * 0.5;
            }
        }

        public SoundManager(string basePath)
        {
            jumpSound = CreateMediaPlayer(System.IO.Path.Combine(basePath, "jump.wav"));
            hitSound = CreateMediaPlayer(System.IO.Path.Combine(basePath, "hit.wav"));
            scoreSound = CreateMediaPlayer(System.IO.Path.Combine(basePath, "score.wav"));

            backgroundMusicPath = System.IO.Path.Combine(basePath, "background.mp3");
            backgroundMusic = null; // Lade erst beim Start
        }

        private MediaPlayer CreateMediaPlayer(string file)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(file, UriKind.Absolute));
            player.Volume = _volume;
            return player;
        }

        public void PlayJump()
        {
            jumpSound.Stop();
            jumpSound.Position = TimeSpan.Zero;
            jumpSound.Play();
        }

        public void PlayHit()
        {
            hitSound.Stop();
            hitSound.Position = TimeSpan.Zero;
            hitSound.Play();
        }

        public void PlayScore()
        {
            scoreSound.Stop();
            scoreSound.Position = TimeSpan.Zero;
            scoreSound.Play();
        }

        public void StartBackgroundMusic()
        {
            if (isBackgroundPlaying) return;
            if (backgroundMusic == null)
            {
                backgroundMusic = new MediaPlayer();
                backgroundMusic.Open(new Uri(backgroundMusicPath, UriKind.Absolute));
                backgroundMusic.Volume = _volume * 0.5;
                backgroundMusic.MediaEnded += (s, e) =>
                {
                    backgroundMusic.Position = TimeSpan.Zero;
                    backgroundMusic.Play();
                };
            }
            backgroundMusic.Position = TimeSpan.Zero;
            backgroundMusic.Play();
            isBackgroundPlaying = true;
        }

        public void StopMusic()
        {
            if (backgroundMusic != null)
                backgroundMusic.Stop();
            isBackgroundPlaying = false;
        }
    }

    public class FlappyBirdJumpMove : IPlayMove
    {
        public int PlayerNumber { get; private set; }
        public FlappyBirdJumpMove(int playerNumber) => PlayerNumber = playerNumber;
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

        public bool CanBePaintedBy(IPaintGame painter) => painter is FlappyBirdPainter;
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
        private ImageSource _pipeHeadImage;
        private ImageSource _pipeBodyImage;
        private ImageSource _birdImage;
        private ImageSource _backgroundImage;

        private PipePart _pipeHead;
        private PipePart _pipeBody;

        private const double PipeHeadHeight = 24;

        public FlappyBirdPainter()
        {
            string projectRoot = FlappyBirdRules.GetProjectFolderPath();
            string graphicsPath = System.IO.Path.Combine(projectRoot, "Classes", "B4_Gruppe", "Grafics");

            _pipeHeadImage = LoadImage(System.IO.Path.Combine(graphicsPath, "pipe_head.png"));
            _pipeBodyImage = LoadImage(System.IO.Path.Combine(graphicsPath, "pipe_body.png"));
            _birdImage = LoadImage(System.IO.Path.Combine(graphicsPath, "bird.png"));
            _backgroundImage = LoadImage(System.IO.Path.Combine(graphicsPath, "background.png"));

            _pipeHead = new PipeHead(_pipeHeadImage);
            _pipeBody = new PipeBody(_pipeBodyImage);
        }

        private ImageSource LoadImage(string fullPath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public void PaintGameField(Canvas canvas, IGameField field)
        {
            canvas.Children.Clear();
            if (field is FlappyBirdField f)
            {
                f.FieldHeight = canvas.ActualHeight;

                var bgImage = new Image()
                {
                    Source = _backgroundImage,
                    Width = canvas.ActualWidth,
                    Height = canvas.ActualHeight,
                    Stretch = Stretch.Fill
                };
                canvas.Children.Add(bgImage);

                double birdDrawFactor = 1.35;
                double drawBirdSize = f.BirdSize * birdDrawFactor;

                var bird = new Image()
                {
                    Source = _birdImage,
                    Width = drawBirdSize,
                    Height = drawBirdSize,
                    Stretch = Stretch.Fill
                };
                Canvas.SetLeft(bird, f.BirdX + (f.BirdSize - drawBirdSize) / 2);
                Canvas.SetTop(bird, f.BirdY + (f.BirdSize - drawBirdSize) / 2);
                canvas.Children.Add(bird);

                foreach (var obs in f.Obstacles)
                {
                    if (obs.Height > PipeHeadHeight)
                    {
                        if (obs.IsTop)
                        {
                            _pipeBody.Draw(canvas, obs.X, obs.Y, obs.Width, obs.Height - PipeHeadHeight, true);
                            _pipeHead.Draw(canvas, obs.X, obs.Y + (obs.Height - PipeHeadHeight), obs.Width, PipeHeadHeight, true);
                        }
                        else
                        {
                            _pipeHead.Draw(canvas, obs.X, obs.Y, obs.Width, PipeHeadHeight, false);
                            _pipeBody.Draw(canvas, obs.X, obs.Y + PipeHeadHeight, obs.Width, obs.Height - PipeHeadHeight, false);
                        }
                    }
                    else
                    {
                        _pipeHead.Draw(canvas, obs.X, obs.Y, obs.Width, obs.Height, obs.IsTop);
                    }
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

            if (currentField is FlappyBirdField f)
            {
                // Musik nur starten, wenn Spiel begonnen - sonst Musik stoppen
                if (!FlappyBirdRules.GameStarted)
                {
                    FlappyBirdRules.SoundManagerInstance?.StopMusic();
                    DrawStartMessage(canvas, f);
                }
                else if (FlappyBirdRules.GameOver)
                {
                    DrawGameOver(canvas);
                    DrawHighscore(canvas);
                }
            }
        }

        private void DrawStartMessage(Canvas canvas, FlappyBirdField field)
        {
            var msg = new TextBlock()
            {
                Text = "Tap to Start",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            double textX = field.BirdX + field.BirdSize + 20;
            double textY = field.BirdY;

            Canvas.SetLeft(msg, textX);
            Canvas.SetTop(msg, textY);

            canvas.Children.Add(msg);
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

        public static bool GameStarted = false;
        public static int ActivePlayer = 1;

        private FlappyBirdField _field;

        public static SoundManager SoundManagerInstance;

        private const double Gravity = 2.9;
        private const double JumpForce = -18;
        private double _birdVelocity = 0;

        public IGameField CurrentField => _field;
        public bool MovesPossible { get; private set; } = true;
        public static readonly List<int> Highscores = new List<int>();
        public static bool GameOver = false;

        private readonly SoundManager soundManager;

        public FlappyBirdRules()
        {
            string basePath = GetProjectFolderPath() + @"\Classes\B4_Gruppe\Grafics";
            soundManager = new SoundManager(basePath);
            SoundManagerInstance = soundManager;

            LoadHighscores();
            ClearField();
        }

        public static string GetProjectFolderPath()
        {
            string exePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(exePath, @"..\..\.."));
        }

        public void DoMove(IPlayMove move)
        {
            if (!MovesPossible) return;

            if (move is FlappyBirdJumpMove jm)
            {
                if (jm.PlayerNumber != ActivePlayer) return;

                if (!GameStarted)
                {
                    GameStarted = true;
                    soundManager.StartBackgroundMusic();
                }

                _birdVelocity = JumpForce;
                soundManager.PlayJump();
            }
        }

        public void TickGameCall()
        {
            if (!MovesPossible || !GameStarted) return;

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

            int oldScore = _field.Score;
            _field.UpdateScoreIfPassed();
            if (_field.Score > oldScore)
            {
                soundManager.PlayScore();
            }

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
            soundManager.PlayHit();
            soundManager.StopMusic();

            Highscores.Add(_field.Score);
            Highscores.Sort((a, b) => b.CompareTo(a));
            if (Highscores.Count > 10)
                Highscores.RemoveAt(Highscores.Count - 1);

            SaveHighscores();

            ActivePlayer = (ActivePlayer == 1 ? 2 : 1);
        }

        public void LoadHighscores()
        {
            string file = System.IO.Path.Combine(GetProjectFolderPath(), "Classes", "B4_Gruppe", "FlappyBirdHighscore.json");
            if (File.Exists(file))
            {
                try
                {
                    string json = File.ReadAllText(file);
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

        public void SaveHighscores()
        {
            string file = System.IO.Path.Combine(GetProjectFolderPath(), "Classes", "B4_Gruppe", "FlappyBirdHighscore.json");
            try
            {
                string directory = System.IO.Path.GetDirectoryName(file);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string json = JsonSerializer.Serialize(Highscores);
                File.WriteAllText(file, json);
            }
            catch { }
        }

        public void ClearField()
        {
            _field = new FlappyBirdField();
            _birdVelocity = 0;
            MovesPossible = true;
            GameOver = false;
            GameStarted = false;
        }

        public int CheckIfPLayerWon() => MovesPossible ? -1 : 1;

        public void StartedGameCall() => soundManager.StartBackgroundMusic();
    }

    public class FlappyBirdHumanPlayer : IHumanGamePlayer
    {
        public string Name => "Flappy Bird Human Player";

        private int _playerNumber;

        public int PlayerNumber => _playerNumber;

        public void SetPlayerNumber(int number) => _playerNumber = number;

        public IGamePlayer Clone() => new FlappyBirdHumanPlayer();

        public bool CanBeRuledBy(IGameRules rules) => rules is FlappyBirdRules;

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IClickSelection) return new FlappyBirdJumpMove(FlappyBirdRules.ActivePlayer);

            if (selection is IKeySelection keySel && keySel.Key == Key.Space) return new FlappyBirdJumpMove(FlappyBirdRules.ActivePlayer);

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
