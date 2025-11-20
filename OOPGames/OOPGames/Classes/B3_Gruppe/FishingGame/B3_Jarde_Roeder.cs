using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;
using Line = System.Windows.Shapes.Line;

namespace OOPGames
{
    public class B3_Jarde_Roeder : IPaintGame2, IGameRules2
    {
        private readonly FishingField _field = new();
        public IGameField CurrentField => _field;
        public string Name => "B3_Fishermans_Friend_Aaron_Mika";
        public bool MovesPossible => true;

        public void TickGameCall() => _field.Update();

        public void StartedGameCall()
        {
            _field.Reset();
            AudioEngine.Init();
        }

        public void ClearField() => _field.Reset();
        public int CheckIfPLayerWon() => 0;
        public void DoMove(IPlayMove move) { }
        public void PaintGameField(Canvas canvas, IGameField field) => _field.Draw(canvas);
        public void TickPaintGameField(Canvas canvas, IGameField field) => _field.Draw(canvas);
    }

    public class FishingField : IGameField
    {
        private readonly List<Fish> _fishes = new();
        private readonly List<PopupScore> _popups = new();
        private readonly Random _rng = new();
        private Hook _hook = new();

        private int _spawnCounter = 0;
        private const int SPAWN_INTERVAL = 60;

        private bool _gameStarted = false, _showStartText = true, _gameOver = false;
        private double _remainingTime = 35.0;
        private DateTime _lastUpdateTime = DateTime.UtcNow;
        private int _score = 0;

        private double _lastW = 800, _lastH = 600;

        public FishingField() { Assets.EnsureLoaded(); }
        public bool CanBePaintedBy(IPaintGame painter) => painter is B3_Jarde_Roeder;

        public void Reset()
        {
            AudioEngine.StopMusic();
            AudioEngine.StopHook();

            _fishes.Clear();
            _popups.Clear();
            _hook = new Hook();
            _spawnCounter = 0;
            _gameStarted = false;
            _showStartText = true;
            _gameOver = false;
            _remainingTime = 35.0;
            _score = 0;
            _lastUpdateTime = DateTime.UtcNow;
        }

        public void Update()
        {
            double deltaTime = (DateTime.UtcNow - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = DateTime.UtcNow;

            if (Application.Current?.MainWindow is Window win)
                if (LogicalTreeHelper.FindLogicalNode(win, "GameCanvas") is Canvas canvas)
                { _lastW = canvas.ActualWidth; _lastH = canvas.ActualHeight; }

            if (_gameOver && Keyboard.IsKeyDown(Key.Tab)) { Reset(); return; }
            if (_gameOver) return;

            if (!_gameStarted)
            {
                if (Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down))
                {
                    _gameStarted = true;
                    _showStartText = false;
                    _lastUpdateTime = DateTime.UtcNow;

                    AudioEngine.PlayStart();
                    AudioEngine.PlayMusic();
                }
                return;
            }

            _hook.Update();

            _remainingTime -= deltaTime;
            if (_remainingTime <= 0)
            {
                _remainingTime = 0;
                _gameOver = true;
                AudioEngine.StopMusic();
                AudioEngine.StopHook();
                return;
            }

            if (++_spawnCounter >= SPAWN_INTERVAL)
            {
                _spawnCounter = 0;
                double roll = _rng.NextDouble();
                Fish f = roll switch
                {
                    <= 0.30 => new Barsch(_rng, _lastW, _lastH),
                    <= 0.60 => new Hecht(_rng, _lastW, _lastH),
                    <= 0.75 => new Saibling(_rng, _lastW, _lastH),
                    <= 0.90 => new Zander(_rng, _lastW, _lastH),
                    _ => new Wels(_rng, _lastW, _lastH)
                };
                _fishes.Add(f);
            }

            foreach (var f in _fishes) f.Update();
            foreach (var p in _popups) p.Update(deltaTime);

            Rect hookBox = _hook.GetHitbox();
            Point hookCenter = new Point(hookBox.X + hookBox.Width / 2 + 4, hookBox.Y + hookBox.Height * 0.75);
            double hookRadius = _hook.SpriteWidth * 0.25;

            List<Fish> caught = new();
            foreach (var f in _fishes)
            {
                if (f.CollidesWithHeadCircle(hookCenter, hookRadius))
                {
                    _remainingTime = Math.Min(90, _remainingTime + f.BonusSeconds);
                    _score += f.Points;
                    caught.Add(f);
                    _popups.Add(new PopupScore($"+{f.Points}", f.Center));

                    AudioEngine.PlaySplash();
                }
            }

            _fishes.RemoveAll(f => f.IsOffScreen() || caught.Contains(f));
            _popups.RemoveAll(p => p.IsExpired);
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();
            double w = canvas.ActualWidth, h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;
            _lastW = w; _lastH = h;

            if (Assets.Background != null)
                canvas.Children.Add(new Rectangle
                {
                    Width = w,
                    Height = h,
                    Fill = new ImageBrush(Assets.Background) { Stretch = Stretch.UniformToFill }
                });

            _hook.SetCanvasSize(w, h, Assets.Hook);
            _hook.SetSprite(Assets.Hook);

            foreach (var f in _fishes) f.Draw(canvas);
            _hook.Draw(canvas);

            AddText(canvas, $"⏱ {(int)Math.Ceiling(_remainingTime)} s", w - 140, 10, 24, Brushes.White, FontWeights.Bold);
            AddText(canvas, $"🎣 {_score} pts", w - 140, 40, 22, Brushes.Gold, FontWeights.Bold);

            foreach (var p in _popups) p.Draw(canvas);

            if (_showStartText)
            {
                AddText(canvas, "Press ↑ or ↓ to start", w / 2 - 130, h / 2 - 20, 28, Brushes.White, FontWeights.Bold);
            }
            else if (_gameOver)
            {
                AddTextCentered(canvas, $"⏰ TIME UP!\n🎯 Score: {_score}\n\nPress TAB to restart",
                                w / 2, h / 2 - 40, 38, Brushes.OrangeRed, FontWeights.Bold);
            }
        }

        private static void AddText(Canvas c, string t, double x, double y, double size, Brush color, FontWeight weight)
        {
            var tb = new TextBlock { Text = t, Foreground = color, FontSize = size, FontWeight = weight };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y); c.Children.Add(tb);
        }

        private static void AddTextCentered(Canvas c, string t, double cx, double cy, double size, Brush color, FontWeight weight)
        {
            var tb = new TextBlock { Text = t, Foreground = color, FontSize = size, FontWeight = weight, TextAlignment = TextAlignment.Center };
            c.Children.Add(tb);
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(tb, cx - tb.DesiredSize.Width / 2);
            Canvas.SetTop(tb, cy - tb.DesiredSize.Height / 2);
        }
    }

    // ---------------- Fische ----------------

    public abstract class Fish
    {
        protected double X, Y, Width, Height, SpeedX;
        protected ImageSource Sprite;
        protected bool FacingLeft;
        protected double CanvasW, CanvasH;

        public abstract int Points { get; }
        public abstract double BonusSeconds { get; }
        protected abstract double HeadOffset { get; }
        protected abstract double HeadRadius { get; }

        protected Fish(Random rng, ImageSource sprite, double baseScale, double canvasW, double canvasH)
        {
            Sprite = sprite;
            CanvasW = canvasW; CanvasH = canvasH;

            double scale = baseScale * 1.15;
            Width = 125 * scale;
            Height = 60 * scale;

            bool fromLeft = rng.NextDouble() < 0.5;
            FacingLeft = !fromLeft;

            if (fromLeft) { X = -Width * 0.25; SpeedX = rng.Next(3, 6); }
            else { X = canvasW * 0.78 + Width * 0.25; SpeedX = -rng.Next(3, 6); }

            double uiZoneHeight = 100;
            double spawnTop = uiZoneHeight + 30;
            double spawnBottom = CanvasH - Height * 1.2;
            Y = rng.NextDouble() * (spawnBottom - spawnTop) + spawnTop;
        }

        public void Update() => X += SpeedX;

        public bool IsOffScreen()
        {
            double rightLimit = CanvasW * 0.78;
            return SpeedX > 0 ? X > rightLimit + Width : X < -Width;
        }

        public Point Center => new Point(X + Width / 2, Y + Height / 2);

        public bool CollidesWithHeadCircle(Point hookCenter, double hookRadius)
        {
            double centerX = X + Width / 2 + (FacingLeft ? -HeadOffset : HeadOffset);
            double centerY = Y + Height / 2;
            double dx = hookCenter.X - centerX;
            double dy = hookCenter.Y - centerY;
            return Math.Sqrt(dx * dx + dy * dy) < (hookRadius + HeadRadius);
        }

        public void Draw(Canvas canvas)
        {
            if (Sprite == null) return;
            var img = new Image { Source = Sprite, Width = Width, Height = Height, Stretch = Stretch.Uniform };
            if (FacingLeft) img.LayoutTransform = new ScaleTransform(-1, 1);
            Canvas.SetLeft(img, X);
            Canvas.SetTop(img, Y);
            canvas.Children.Add(img);
        }
    }

    public class Hecht : Fish
    {
        public Hecht(Random r, double cw, double ch) : base(r, Assets.Hecht, 1.9, cw, ch) { }
        public override int Points => 5;
        public override double BonusSeconds => 2.0;
        protected override double HeadOffset => 26.0;
        protected override double HeadRadius => 7.0;
    }

    public class Barsch : Fish
    {
        public Barsch(Random r, double cw, double ch) : base(r, Assets.Barsch, 1.55, cw, ch) { }
        public override int Points => 5;
        public override double BonusSeconds => 2.0;
        protected override double HeadOffset => 20.0;
        protected override double HeadRadius => 7.0;
    }

    public class Saibling : Fish
    {
        public Saibling(Random r, double cw, double ch) : base(r, Assets.Saibling, 1.25, cw, ch) { }
        public override int Points => 10;
        public override double BonusSeconds => 3.0;
        protected override double HeadOffset => 18.0;
        protected override double HeadRadius => 7.0;
    }

    public class Zander : Fish
    {
        public Zander(Random r, double cw, double ch) : base(r, Assets.Zander, 1.7, cw, ch) { }
        public override int Points => 10;
        public override double BonusSeconds => 3.0;
        protected override double HeadOffset => 22.0;
        protected override double HeadRadius => 7.0;
    }

    public class Wels : Fish
    {
        public Wels(Random r, double cw, double ch) : base(r, Assets.Wels, 2.2, cw, ch) { }
        public override int Points => 20;
        public override double BonusSeconds => 5.0;
        protected override double HeadOffset => 28.0;
        protected override double HeadRadius => 11.0;
    }

    // ---------------- Hook ----------------

    public class Hook
    {
        private double _x;
        public double Y = 200;
        private const double Speed = 5;
        private double _canvasWidth = 800, _canvasHeight = 600;
        private ImageSource _sprite;
        private double _spriteWidth = 30, _spriteHeight = 58;
        public double SpriteWidth => _spriteWidth;

        public void SetCanvasSize(double w, double h, ImageSource sprite)
        {
            _canvasWidth = w;
            _canvasHeight = h;
            if (sprite != null) _sprite = sprite;
            _x = (_canvasWidth - _spriteWidth) / 2.0;
            Y = Math.Clamp(Y, 60, _canvasHeight - _spriteHeight);
        }

        public void SetSprite(ImageSource sprite) => _sprite = sprite ?? _sprite;

        public void Update()
        {
            bool moved = false;

            if (Keyboard.IsKeyDown(Key.Up))
            {
                Y = Math.Max(60, Y - Speed);
                moved = true;
            }
            else if (Keyboard.IsKeyDown(Key.Down))
            {
                Y = Math.Min(_canvasHeight - _spriteHeight, Y + Speed);
                moved = true;
            }

            if (moved)
                AudioEngine.StartHook();
            else
                AudioEngine.StopHook();
        }

        public void Draw(Canvas canvas)
        {
            var line = new Line
            { X1 = _x + _spriteWidth / 2, Y1 = 0, X2 = _x + _spriteWidth / 2, Y2 = Y + 4, Stroke = Brushes.Black, StrokeThickness = 2 };
            canvas.Children.Add(line);

            if (_sprite != null)
            {
                var img = new Image { Source = _sprite, Width = _spriteWidth, Height = _spriteHeight, Stretch = Stretch.Uniform };
                Canvas.SetLeft(img, _x);
                Canvas.SetTop(img, Y);
                canvas.Children.Add(img);
            }
        }

        public Rect GetHitbox() => new Rect(_x, Y, _spriteWidth, _spriteHeight);
    }

    // ---------------- Popup ----------------

    public class PopupScore
    {
        public Point Position;
        private double _lifetime = 0.6;
        private readonly string _text;
        private double _opacity = 1.0;
        public bool IsExpired => _lifetime <= 0;

        public PopupScore(string text, Point position) { _text = text; Position = position; }

        public void Update(double deltaTime)
        {
            _lifetime -= deltaTime;
            Position = new Point(Position.X, Position.Y - 25 * deltaTime);
            _opacity = Math.Max(0, _lifetime / 0.6);
        }

        public void Draw(Canvas canvas)
        {
            TextBlock txt = new TextBlock
            {
                Text = _text,
                Foreground = new SolidColorBrush(Color.FromArgb((byte)(_opacity * 255), 255, 215, 0)),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(txt, Position.X - 15);
            Canvas.SetTop(txt, Position.Y - 20);
            canvas.Children.Add(txt);
        }
    }

    // ---------------- Assets ----------------

    public static class Assets
    {
        private static bool _loaded = false;
        private static string Dir =>
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishingGame");

        public static ImageSource Background { get; private set; }
        public static ImageSource Hook { get; private set; }
        public static ImageSource Hecht { get; private set; }
        public static ImageSource Barsch { get; private set; }
        public static ImageSource Saibling { get; private set; }
        public static ImageSource Zander { get; private set; }
        public static ImageSource Wels { get; private set; }

        public static void EnsureLoaded()
        {
            if (_loaded) return;
            Background = Load("background.png");
            Hook = Load("hook.png");
            Hecht = Load("hecht.png");
            Barsch = Load("barsch.png");
            Saibling = Load("saibling.png");
            Zander = Load("zander.png");
            Wels = Load("wels.png");
            _loaded = true;
        }

        private static ImageSource Load(string file)
        {
            try
            {
                string p = System.IO.Path.Combine(Dir, file);
                if (!System.IO.File.Exists(p)) return null;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(p, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }
    }

    // ---------------- AUDIO ENGINE ----------------

    public static class AudioEngine
    {
        private static string Dir =>
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishingGame", "Audio");

        private static MediaPlayer _music = new MediaPlayer();
        private static MediaPlayer _start = new MediaPlayer();
        private static MediaPlayer _hook = new MediaPlayer();
        private static bool _hookActive = false;

        public static void Init()
        {
            _music.Open(new Uri(System.IO.Path.Combine(Dir, "background.mp3")));
            _music.Volume = 0.4;
            _music.MediaEnded += (s, e) =>
            {
                _music.Position = TimeSpan.Zero;
                _music.Play();
            };

            _start.Open(new Uri(System.IO.Path.Combine(Dir, "start.mp3")));
            _start.Volume = 0.3;

            _hook.Open(new Uri(System.IO.Path.Combine(Dir, "hook.mp3")));
            _hook.Volume = 0.7;
            _hook.MediaEnded += (s, e) =>
            {
                if (_hookActive)
                {
                    _hook.Position = TimeSpan.Zero;
                    _hook.Play();
                }
            };
        }

        public static void PlayMusic() => _music.Play();
        public static void StopMusic() => _music.Stop();

        public static void PlayStart()
        {
            _start.Position = TimeSpan.Zero;
            _start.Play();
        }

        public static void StartHook()
        {
            if (_hookActive) return;
            _hookActive = true;
            _hook.Position = TimeSpan.Zero;
            _hook.Play();
        }

        public static void StopHook()
        {
            _hookActive = false;
            _hook.Stop();
        }

        public static void PlaySplash()
        {
            var p = new MediaPlayer();
            p.Open(new Uri(System.IO.Path.Combine(Dir, "splash.mp3")));
            p.Volume = 0.3;

            p.Play();
            p.MediaEnded += (s, e) => p.Close();
        }
    }
}
