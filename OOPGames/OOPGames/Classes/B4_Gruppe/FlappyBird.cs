using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Effects;

namespace OOPGames
{
        public class HighscoreEntry
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public HighscoreEntry() { }
        public HighscoreEntry(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }

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
        private MediaPlayer rocketSound;

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
            rocketSound = CreateMediaPlayer(System.IO.Path.Combine(basePath, "rocket.wav"));
            rocketSound.Volume = _volume * 0.7;
            rocketSound.MediaEnded += (s, e) =>
            {
                rocketSound.Position = TimeSpan.Zero;
                rocketSound.Play();
            };

            backgroundMusicPath = System.IO.Path.Combine(basePath, "background.mp3");
            backgroundMusic = null; // Load on demand
        }

        private MediaPlayer CreateMediaPlayer(string file)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(file, UriKind.Absolute));
            player.Volume = _volume;
            return player;
        }
        public void PlayRocket()
        {
            if (rocketSound == null) return;
            rocketSound.Stop();
            rocketSound.Position = TimeSpan.Zero;
            rocketSound.Play();
        }
        public void StopRocket()
        {
            if (rocketSound == null) return;
            rocketSound.Stop();
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

    public class RocketObstacle : Obstacle
    {
        private ImageSource _rocketImage1;
        private ImageSource _rocketImage2;
        public double Speed { get; set; } = 8;

        public RocketObstacle(double x, double y, double width, double height, ImageSource rocketImage1, ImageSource rocketImage2)
            : base(x, y, width, height, false)
        {
            _rocketImage1 = rocketImage1;
            _rocketImage2 = rocketImage2;
        }
        public void Move()
        {
            X -= Speed;
        }
        public void Draw(Canvas canvas, int frameCount)
        {
            var img = new Image()
            {
                Source = (frameCount % 2 == 0 ? _rocketImage1 : _rocketImage2),
                Width = Width,
                Height = Height,
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(img, X);
            Canvas.SetTop(img, Y);
            canvas.Children.Add(img);
        }
    }

    public class FlappyBirdField : IGameField
    {
        public bool CanBePaintedBy(IPaintGame painter) => painter is FlappyBirdPainter;
        public double BirdX { get; set; } = 100;
        public double BirdY { get; set; } = 200;
        public double BirdSize { get; set; } = 30;
        public double FieldHeight { get; set; } = 600;
        public List<Obstacle> Obstacles { get; private set; } = new List<Obstacle>();
        public List<RocketObstacle> Rockets { get; private set; } = new List<RocketObstacle>();

        private static readonly Random _rand = new Random();
        private int _pipeCount = 0;

        public Rect BirdRectangle => new Rect(BirdX, BirdY, BirdSize, BirdSize);

        public bool NeedsObstacle
        {
            get
            {
                if (Obstacles.Count == 0) return true;
                var last = Obstacles[Obstacles.Count - 1];
                return last.X < 250;  // Obstacle spacing
            }
        }
        public int Score { get; set; } = 0;

        public void CreateObstacle(ImageSource rocketImage1, ImageSource rocketImage2)
        {
            _pipeCount++;
            double gap = 140;
            double gapY = _rand.Next(50, (int)(FieldHeight - gap - 50));  //Obstacle gap up to down pipe
            Obstacles.Add(new Obstacle(480, 0, 75, gapY, true));
            double yBottom = gapY + gap;
            double heightBottom = FieldHeight - yBottom;
            Obstacles.Add(new Obstacle(480, yBottom, 75, heightBottom, false));
            if (_pipeCount % 5 == 0)
            {
                double rocketWidth = 80;
                double rocketHeight = 40;
                double rocketY = _rand.Next(50, (int)(FieldHeight - rocketHeight - 50));
                Rockets.Add(new RocketObstacle(480, rocketY, rocketWidth, rocketHeight, rocketImage1, rocketImage2));
            }
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
    }

    public class FlappyBirdPainter : IPaintGame2
    {
        public string Name => "Flappy Bird";
        private static readonly Brush Bronze = new SolidColorBrush(Color.FromRgb(205, 127, 50));
        private ImageSource _pipeHeadImage;
        private ImageSource _pipeBodyImage;
        private ImageSource _birdImage;
        private ImageSource _backgroundImage;
        private ImageSource _startImage;
        private ImageSource _gameOverImage;
        private ImageSource _highscoreImage;
        private ImageSource _rocketImage1;
        private ImageSource _rocketImage2;

        private PipePart _pipeHead;
        private PipePart _pipeBody;
        private const double PipeHeadHeight = 24;

        public FlappyBirdPainter()
        {
            string projectRoot = FlappyBirdRules.GetProjectFolderPath();
            string graphicsPath = System.IO.Path.Combine(projectRoot, "Classes", "B4_Gruppe", "Graphic");
            _pipeHeadImage = LoadImage(System.IO.Path.Combine(graphicsPath, "pipe_head.png"));
            _pipeBodyImage = LoadImage(System.IO.Path.Combine(graphicsPath, "pipe_body.png"));
            _birdImage = LoadImage(System.IO.Path.Combine(graphicsPath, "bird.png"));
            _backgroundImage = LoadImage(System.IO.Path.Combine(graphicsPath, "background.png"));
            _startImage = LoadImage(System.IO.Path.Combine(graphicsPath, "start.png"));
            _gameOverImage = LoadImage(System.IO.Path.Combine(graphicsPath, "GameOver.png"));
            _highscoreImage = LoadImage(System.IO.Path.Combine(graphicsPath, "highscore.png"));
            _rocketImage1 = LoadImage(System.IO.Path.Combine(graphicsPath, "rocket1.png"));
            _rocketImage2 = LoadImage(System.IO.Path.Combine(graphicsPath, "rocket2.png"));
            _pipeHead = new PipeHead(_pipeHeadImage);
            _pipeBody = new PipeBody(_pipeBodyImage);
        }

        public ImageSource LoadImage(string fullPath)
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
                // Rotation nach Fallbewegung
                double angle = FlappyBirdRules.Instance?.BirdRotationAngle ?? 0;
                bird.RenderTransformOrigin = new Point(0.5, 0.5);
                bird.RenderTransform = new RotateTransform(angle);
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
                foreach (var rocket in f.Rockets)
                {
                    rocket.Draw(canvas, FlappyBirdRules.FrameCount);
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
            var startImg = new Image()
            {
                Source = _startImage,
                Width = 225,
                Height = 225,
                Stretch = Stretch.Fill
            };
            double imgX = field.BirdX + field.BirdSize + 20;
            double imgY = field.BirdY;
            Canvas.SetLeft(startImg, imgX);
            Canvas.SetTop(startImg, imgY);
            canvas.Children.Add(startImg);
        }

        private void DrawGameOver(Canvas canvas)
        {
            var gameOverImg = new Image()
            {
                Source = _gameOverImage,
                Width = 300,
                Height = 100,
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(gameOverImg, (canvas.ActualWidth - gameOverImg.Width) / 2);
            Canvas.SetTop(gameOverImg, (canvas.ActualHeight / 2) - 220);  // Y-Koordination of Highscore
            canvas.Children.Add(gameOverImg);
        }

        private void DrawHighscore(Canvas canvas)
        {
            var hs = FlappyBirdRules.Highscores;
            var yStart = (canvas.ActualHeight / 2) - 40;
            var img = new Image()
            {
                Source = _highscoreImage,
                Width = 250,
                Height = 50,
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(img, (canvas.ActualWidth - img.Width) / 2);
            Canvas.SetTop(img, yStart - 70);  // Y-Koordination of Highscore Writing
            canvas.Children.Add(img);
            yStart += 20;
            for (int i = 0; i < hs.Count; i++)
            {
                string name = hs[i].Name;
                int score = hs[i].Score;
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
                Canvas.SetTop(medal, yStart + i * 30 - 25);  // - ... = Y-Koordination of Highscore Dots
                canvas.Children.Add(medal);
                var tb= new TextBlock()
                {
                    Text = $"{i+1}. {name} - {score}",
                    FontSize = 20,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                tb.Effect = new DropShadowEffect()
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    Direction = 0,
                    ShadowDepth = 0,
                    Opacity = 1.0,
                    RenderingBias = RenderingBias.Performance
                };
                Canvas.SetLeft(tb, (canvas.ActualWidth / 2) - 40);
                Canvas.SetTop(tb, yStart + i * 30 - 30);
                canvas.Children.Add(tb);
            }
        }
    }

    public class FlappyBirdRules : IGameRules2
    {

        
        public static FlappyBirdRules Instance; // Singleton für Zugang
        public string Name => "Flappy Bird Rules";
        public static bool GameStarted = false;
        public static int ActivePlayer = 1;
        public static int FrameCount = 0;
        private FlappyBirdField _field;
        public static SoundManager SoundManagerInstance;
        private const double Gravity = 2.3;  // Gravity effect
        private const double JumpForce = -18;  // Upward force when jumping
        private double _birdVelocity = 0;
        private ImageSource _rocketImage1;
        private ImageSource _rocketImage2;
        public IGameField CurrentField => _field;
        public bool MovesPossible { get; private set; } = true;
        public static readonly List<HighscoreEntry> Highscores = new List<HighscoreEntry>();

        public static bool GameOver = false;
        private readonly SoundManager soundManager;

        public double BirdRotationAngle
        {
            get
            {
                double angle = Math.Max(-30, Math.Min(_birdVelocity * 3, 60));
                return angle;
            }
        }

        public FlappyBirdRules()
        {
            Instance = this;
            string basePath = GetProjectFolderPath() + @"\Classes\B4_Gruppe\Graphic";
            soundManager = new SoundManager(basePath);
            SoundManagerInstance = soundManager;
            var painter = new FlappyBirdPainter();
            _rocketImage1 = painter.LoadImage(System.IO.Path.Combine(basePath, "rocket1.png"));
            _rocketImage2 = painter.LoadImage(System.IO.Path.Combine(basePath, "rocket2.png"));
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
        private bool _rocketSoundPlaying = false;
        public void TickGameCall()
        {
            if (!MovesPossible || !GameStarted) return;
            FrameCount++;
            _birdVelocity += Gravity;
            _field.BirdY += _birdVelocity;
            for (int i = _field.Obstacles.Count - 1; i >= 0; i--)
            {
                var obs = _field.Obstacles[i];
                obs.X -= 5;
                if (obs.X + obs.Width < 0)
                    _field.Obstacles.RemoveAt(i);
            }
            bool rocketsVisible = false;
            for (int i = _field.Rockets.Count - 1; i >= 0; i--)
            {
                var rocket = _field.Rockets[i];
                rocket.Move();
                if (rocket.X + rocket.Width > 0 && rocket.X < 480)
                {
                    rocketsVisible = true;
                }
                var birdRect = _field.BirdRectangle;
                var rocketRect = rocket.Rectangle;
                if (birdRect.IntersectsWith(rocketRect))
                {
                    EndGame();
                    return;
                }
                if (!rocket.Passed && rocket.X + rocket.Width < _field.BirdX)
                {
                    rocket.Passed = true;
                    _field.Score += 2;
                    soundManager.PlayScore();
                }
                if (rocket.X + rocket.Width < 0)
                    _field.Rockets.RemoveAt(i);
            }
            if (rocketsVisible && !_rocketSoundPlaying)
            {
                soundManager.PlayRocket();
                _rocketSoundPlaying = true;
            }
            else if (!rocketsVisible && _rocketSoundPlaying)
            {
                soundManager.StopRocket();
                _rocketSoundPlaying = false;
            }
            if (_field.NeedsObstacle)
            {
                _field.CreateObstacle(_rocketImage1, _rocketImage2);
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
            var birdRectCheck = _field.BirdRectangle;
            foreach (var obs in _field.Obstacles)
            {
                if (birdRectCheck.IntersectsWith(obs.Rectangle))
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
            soundManager.StopRocket();

            int score = _field.Score;

            // Prüfe, ob in Top 10
            if (Highscores.Count < 10 || score > Highscores.Min(e => e.Score))
            {
                    string playerName = PromptForName(); // eigene Methode, z.B. mittels Window, Dialog oder InputBox
                    if (string.IsNullOrWhiteSpace(playerName)) playerName = "Anonymous";
                    Highscores.Add(new HighscoreEntry(playerName, score));
                    Highscores.Sort((a, b) => b.Score.CompareTo(a.Score));
                    if (Highscores.Count > 10)
                        Highscores.RemoveAt(Highscores.Count - 1);
                    SaveHighscores();
            }
            ActivePlayer = (ActivePlayer == 1 ? 2 : 1);
        }

        // Beispiel für eine einfache Prompteingabe (kannst du verbessern)
        private string PromptForName()
        {
            return Microsoft.VisualBasic.Interaction.InputBox(
            "Neuer Highscore! Bitte gib deinen Namen ein:",
            "Highscore-Eintrag",
            ""
        );
    }
        public void LoadHighscores()
        {
            string file = System.IO.Path.Combine(GetProjectFolderPath(), "Classes", "B4_Gruppe", "Graphic", "FlappyBirdHighscore.json");
            if (File.Exists(file))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var loaded = JsonSerializer.Deserialize<List<HighscoreEntry>>(json);
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
            string file = System.IO.Path.Combine(GetProjectFolderPath(), "Classes", "B4_Gruppe", "Graphic", "FlappyBirdHighscore.json");
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
