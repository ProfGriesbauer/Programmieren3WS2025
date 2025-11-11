using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    public class A5_SnakePaint : IPaintGame2
    {
        public string Name => "A5 Paint Snake";

        // Bild-Texturen für Spielfeld
        private static ImageBrush _grassBrush;
        private static ImageBrush _snakeBrush;
        private static bool _imagesLoaded = false;

        private void LoadImages()
        {
            if (_imagesLoaded) return;
            _imagesLoaded = true;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string grassPath = System.IO.Path.Combine(baseDir, "Assets", "Snake", "grass.png");
                string snakePath = System.IO.Path.Combine(baseDir, "Assets", "Snake", "snake.png");

                if (System.IO.File.Exists(grassPath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(grassPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
                    bmp.EndInit();
                    bmp.Freeze();
                    _grassBrush = new ImageBrush(bmp)
                    {
                        Stretch = Stretch.Fill
                    };
                }

                if (System.IO.File.Exists(snakePath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(snakePath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
                    bmp.EndInit();
                    bmp.Freeze();
                    _snakeBrush = new ImageBrush(bmp)
                    {
                        Stretch = Stretch.Fill
                    };
                }
            }
            catch
            {
                // Ignoriere Ladefehler
            }
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is A5_SnakeField field)
            {
                canvas.Children.Clear();
                canvas.Background = Brushes.Black;

                LoadImages();

                // Spielfeld in Canvas-Größe skalieren
                double scaleX = canvas.ActualWidth / A5_SnakeField.FIELD_WIDTH;
                double scaleY = canvas.ActualHeight / A5_SnakeField.FIELD_HEIGHT;
                double scale = Math.Min(scaleX, scaleY);

                double scaledWidth = A5_SnakeField.FIELD_WIDTH * scale;
                double scaledHeight = A5_SnakeField.FIELD_HEIGHT * scale;
                double offsetX = (canvas.ActualWidth - scaledWidth) / 2;
                double offsetY = (canvas.ActualHeight - scaledHeight) / 2;

                // Zeichne Hintergrund mit Gras-Tiles
                int tilesX = (int)Math.Ceiling(A5_SnakeField.FIELD_WIDTH / (double)A5_SnakeField.SNAKE_SIZE);
                int tilesY = (int)Math.Ceiling(A5_SnakeField.FIELD_HEIGHT / (double)A5_SnakeField.SNAKE_SIZE);

                for (int x = 0; x < tilesX; x++)
                {
                    for (int y = 0; y < tilesY; y++)
                    {
                        Rectangle grassRect = new Rectangle
                        {
                            Width = A5_SnakeField.SNAKE_SIZE * scale,
                            Height = A5_SnakeField.SNAKE_SIZE * scale,
                            Fill = _grassBrush != null ? (Brush)_grassBrush : Brushes.LightGreen
                        };
                        Canvas.SetLeft(grassRect, offsetX + (x * A5_SnakeField.SNAKE_SIZE * scale));
                        Canvas.SetTop(grassRect, offsetY + (y * A5_SnakeField.SNAKE_SIZE * scale));
                        canvas.Children.Add(grassRect);
                    }
                }

                // Zeichne Schlangensegmente an ihren exakten Pixel-Positionen
                for (int i = 0; i < field.Snake.Count; i++)
                {
                    var segment = field.Snake[i];
                    Rectangle snakeRect = new Rectangle
                    {
                        Width = A5_SnakeField.SNAKE_SIZE * scale,
                        Height = A5_SnakeField.SNAKE_SIZE * scale,
                        // Nur der Kopf (Index 0) bekommt das Bild, der Rest eine Farbe
                        Fill = (i == 0 && _snakeBrush != null) ? (Brush)_snakeBrush : Brushes.DarkGreen
                    };
                    Canvas.SetLeft(snakeRect, offsetX + (segment.X * scale));
                    Canvas.SetTop(snakeRect, offsetY + (segment.Y * scale));
                    canvas.Children.Add(snakeRect);
                }
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }
}