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
        private static bool _imagesLoaded = false;

        private const string GRASS_IMAGE = "grass.png";
        private const string SNAKE_IMAGE = "snake.png";

        private void LoadImages()
        {
            if (_imagesLoaded) return;
            _imagesLoaded = true;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assetsPath = System.IO.Path.Combine(baseDir, "Assets", "Snake");

            _grassBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, GRASS_IMAGE));
            _snakeBrush = LoadImageBrush(System.IO.Path.Combine(assetsPath, SNAKE_IMAGE));
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
            int tilesX = (int)Math.Ceiling(A5_SnakeField.FIELD_WIDTH / (double)A5_SnakeField.SNAKE_SIZE);
            int tilesY = (int)Math.Ceiling(A5_SnakeField.FIELD_HEIGHT / (double)A5_SnakeField.SNAKE_SIZE);

            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    var grassTile = CreateRectangle(
                        A5_SnakeField.SNAKE_SIZE * scale,
                        A5_SnakeField.SNAKE_SIZE * scale,
                        _grassBrush != null ? (Brush)_grassBrush : Brushes.LightGreen,
                        offset.X + (x * A5_SnakeField.SNAKE_SIZE * scale),
                        offset.Y + (y * A5_SnakeField.SNAKE_SIZE * scale)
                    );
                    canvas.Children.Add(grassTile);
                }
            }
        }

        private void DrawSnake(Canvas canvas, A5_SnakeField field, double scale, (double X, double Y) offset)
        {
            for (int i = 0; i < field.Snake.Count; i++)
            {
                var segment = field.Snake[i];
                bool isHead = i == 0;
                Brush fill = (isHead && _snakeBrush != null) ? _snakeBrush : Brushes.DarkGreen;

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