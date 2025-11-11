using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows;
using IOPath = System.IO.Path;
using IOFile = System.IO.File;

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
        private const string FOOD_IMAGE = "strawberry.png";

        private void LoadImages()
        {
            if (_imagesLoaded) return;
            _imagesLoaded = true;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assetsPath = IOPath.Combine(baseDir, "Assets", "Snake");

            _grassBrush = LoadImageBrush(IOPath.Combine(assetsPath, GRASS_IMAGE));
            _snakeBrush = LoadImageBrush(IOPath.Combine(assetsPath, SNAKE_IMAGE));
            _tailBrush = LoadImageBrush(IOPath.Combine(assetsPath, TAIL_IMAGE));
            _foodBrush = LoadImageBrush(IOPath.Combine(assetsPath, FOOD_IMAGE));
        }

        private static ImageBrush LoadImageBrush(string imagePath)
        {
            if (!IOFile.Exists(imagePath)) return null;

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

            if (field.IsCountingDown)
            {
                DrawCountdown(canvas, field, scale, offset);
            }
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
                Brush fill = i == 0
                    ? (_snakeBrush != null ? (Brush)_snakeBrush : Brushes.Red)
                    : (_tailBrush != null ? (Brush)_tailBrush : Brushes.DarkRed);

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

        private void DrawCountdown(Canvas canvas, A5_SnakeField field, double scale, (double X, double Y) offset)
        {
            if (field.CountdownSeconds <= 0) return;

            var textBlock = new TextBlock
            {
                Text = field.CountdownSeconds.ToString(),
                FontSize = 120,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            double centerX = offset.X + (A5_SnakeField.FIELD_WIDTH * scale / 2);
            double centerY = offset.Y + (A5_SnakeField.FIELD_HEIGHT * scale / 2);

            Canvas.SetLeft(textBlock, centerX - 60);
            Canvas.SetTop(textBlock, centerY - 80);

            canvas.Children.Add(textBlock);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }
}