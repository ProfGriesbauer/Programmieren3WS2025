using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    public class A5_SnakePaint : IPaintGame2
    {
        public string Name => "A5 Paint Snake";

        private static ImageBrush _grassBrush;
        private static ImageBrush _snakeBrush;
        private static ImageBrush _tailBrush;
    private static ImageBrush _foodBrush;
        private static bool _imagesLoaded = false;

        private const string GRASS_IMAGE = "grass.png";
        private const string SNAKE_IMAGE = "snake.png";
        private const string TAIL_IMAGE = "tail.png";
    private const string FOOD_IMAGE = "strawberry.png"; // preferred in Assets/Snake

        private void LoadImages()
        {
            if (_imagesLoaded) return;
            _imagesLoaded = true;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assetsPath = System.IO.Path.Combine(baseDir, "Assets", "Snake");

            _grassBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, GRASS_IMAGE));
            _snakeBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, SNAKE_IMAGE));
            _tailBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, TAIL_IMAGE));
            _foodBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, FOOD_IMAGE));

            // Fallback: try the user's Downloads path if asset isn't in project yet
            if (_foodBrush == null)
            {
                try
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string downloadsPath = System.IO.Path.Combine(userProfile, "Downloads", "Snake_Strawberry_model_32px_1.png");
                    _foodBrush = LoadImageBrush(downloadsPath);
                }
                catch { /* ignore and keep fallback color */ }
            }
        }

        private static ImageBrush LoadImageBrush(string imagePath)
        {
            if (!File.Exists(imagePath)) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat | 
                                      BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                bitmap.Freeze();

                return new ImageBrush(bitmap) { Stretch = Stretch.Fill };
            }
            catch
            {
                return null;
            }
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!(currentField is A5_SnakeField field)) return;

            canvas.Children.Clear();
            canvas.Background = Brushes.Black;
            LoadImages();

            var scale = CalculateScale(canvas);
            var offset = CalculateOffset(canvas, scale);

            DrawGrassBackground(canvas, scale, offset);
            DrawSnake(canvas, field, scale, offset);
            DrawFood(canvas, field, scale, offset);
        }

        private double CalculateScale(Canvas canvas)
        {
            double scaleX = canvas.ActualWidth / A5_SnakeField.FIELD_WIDTH;
            double scaleY = canvas.ActualHeight / A5_SnakeField.FIELD_HEIGHT;
            return Math.Min(scaleX, scaleY);
        }

        private (double X, double Y) CalculateOffset(Canvas canvas, double scale)
        {
            double scaledWidth = A5_SnakeField.FIELD_WIDTH * scale;
            double scaledHeight = A5_SnakeField.FIELD_HEIGHT * scale;
            return ((canvas.ActualWidth - scaledWidth) / 2, (canvas.ActualHeight - scaledHeight) / 2);
        }

        private void DrawGrassBackground(Canvas canvas, double scale, (double X, double Y) offset)
        {
            // Draw one large background image covering the entire field area
            var bg = CreateRectangle(
                A5_SnakeField.FIELD_WIDTH * scale,
                A5_SnakeField.FIELD_HEIGHT * scale,
                _grassBrush != null ? (Brush)_grassBrush : Brushes.LightGreen,
                offset.X,
                offset.Y
            );
            canvas.Children.Add(bg);
        }

        private void DrawSnake(Canvas canvas, A5_SnakeField field, double scale, (double X, double Y) offset)
        {
            for (int i = 0; i < field.Snake.Count; i++)
            {
                var segment = field.Snake[i];
                Brush fill;
                
                if (i == 0)
                {
                    // Kopf
                    fill = _snakeBrush != null ? _snakeBrush : Brushes.Red;
                }
                else
                {
                    // Schwanz
                    fill = _tailBrush != null ? _tailBrush : Brushes.DarkRed;
                }

                var snakeSegment = CreateRectangle(
                    A5_SnakeField.SNAKE_SIZE * scale,
                    A5_SnakeField.SNAKE_SIZE * scale,
                    fill,
                    offset.X + (segment.X * scale),
                    offset.Y + (segment.Y * scale)
                );
                canvas.Children.Add(snakeSegment);
            }
        }

        private void DrawFood(Canvas canvas, A5_SnakeField field, double scale, (double X, double Y) offset)
        {
            if (field.Food == null) return;

            var foodRect = CreateRectangle(
                A5_SnakeField.SNAKE_SIZE * scale,
                A5_SnakeField.SNAKE_SIZE * scale,
                _foodBrush != null ? (Brush)_foodBrush : Brushes.Yellow,
                offset.X + (field.Food.X * scale),
                offset.Y + (field.Food.Y * scale)
            );
            canvas.Children.Add(foodRect);
        }

        private Rectangle CreateRectangle(double width, double height, Brush fill, double left, double top)
        {
            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fill
            };
            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);
            return rect;
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }
}