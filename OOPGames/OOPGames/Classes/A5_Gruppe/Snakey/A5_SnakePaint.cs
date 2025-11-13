using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    /// <summary>
    /// Rendering-Klasse für das Snake-Spiel mit OOP-Struktur
    /// </summary>
    public class A5_SnakePaint : IPaintGame2
    {
        public string Name => "A5 Paint Snake";

        private readonly SnakeAssetLoader _assetLoader = new SnakeAssetLoader();

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!(currentField is A5_SnakeField field)) return;

            canvas.Children.Clear();
            canvas.Background = Brushes.Black;
            _assetLoader.LoadAssets();

            var renderContext = new RenderContext(canvas, A5_SnakeField.FIELD_WIDTH, A5_SnakeField.FIELD_HEIGHT);

            DrawBackground(renderContext);
            DrawSnake(field, renderContext);
            DrawFood(field, renderContext);

            if (field.IsCountingDown)
            {
                DrawCountdown(field, renderContext);
            }
            else if (!field.IsGameRunning)
            {
                DrawStartHint(renderContext);
            }
        }

        private void DrawBackground(RenderContext context)
        {
            var bg = context.CreateRectangle(
                context.FieldWidth * context.Scale,
                context.FieldHeight * context.Scale,
                _assetLoader.GrassBrush != null ? (Brush)_assetLoader.GrassBrush : Brushes.LightGreen,
                context.OffsetX,
                context.OffsetY
            );
            context.Canvas.Children.Add(bg);
        }

        private void DrawSnake(A5_SnakeField field, RenderContext context)
        {
            for (int i = 0; i < field.Snake.Count; i++)
            {
                var segment = field.Snake[i];
                Brush fill = GetSegmentBrush(i, field.Snake.Count);

                var snakeSegment = context.CreateRectangle(
                    A5_SnakeField.SNAKE_SIZE * context.Scale,
                    A5_SnakeField.SNAKE_SIZE * context.Scale,
                    fill,
                    context.OffsetX + (segment.X * context.Scale),
                    context.OffsetY + (segment.Y * context.Scale)
                );

                ApplyRotation(snakeSegment, segment.GetRotationAngle());
                context.Canvas.Children.Add(snakeSegment);
            }
        }

        private Brush GetSegmentBrush(int index, int totalCount)
        {
            if (index == 0)
                return _assetLoader.SnakeHeadBrush != null ? (Brush)_assetLoader.SnakeHeadBrush : Brushes.Red;

            if (index == totalCount - 1)
                return _assetLoader.RattleBrush != null ? (Brush)_assetLoader.RattleBrush : Brushes.Orange;

            return _assetLoader.BodyBrush != null ? (Brush)_assetLoader.BodyBrush : Brushes.DarkRed;
        }

        private void ApplyRotation(Rectangle rect, double angle)
        {
            if (angle != 0)
            {
                rect.RenderTransformOrigin = new Point(0.5, 0.5);
                rect.RenderTransform = new RotateTransform(angle);
            }
        }

        private void DrawFood(A5_SnakeField field, RenderContext context)
        {
            if (field.Food == null) return;

            var foodRect = context.CreateRectangle(
                A5_SnakeField.SNAKE_SIZE * context.Scale,
                A5_SnakeField.SNAKE_SIZE * context.Scale,
                _assetLoader.FoodBrush != null ? (Brush)_assetLoader.FoodBrush : Brushes.Yellow,
                context.OffsetX + (field.Food.X * context.Scale),
                context.OffsetY + (field.Food.Y * context.Scale)
            );
            context.Canvas.Children.Add(foodRect);
        }

        private void DrawCountdown(A5_SnakeField field, RenderContext context)
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

            double centerX = context.OffsetX + (context.FieldWidth * context.Scale / 2);
            double centerY = context.OffsetY + (context.FieldHeight * context.Scale / 2);

            Canvas.SetLeft(textBlock, centerX - 60 + 32);
            Canvas.SetTop(textBlock, centerY - 80);

            context.Canvas.Children.Add(textBlock);
        }

        private void DrawStartHint(RenderContext context)
        {
            string message = "Press SPACE to start";
            double centerX = context.OffsetX + (context.FieldWidth * context.Scale / 2);
            double centerY = context.OffsetY + (context.FieldHeight * context.Scale / 2);

            // Box dimensions (approx around the text + padding)
            double boxWidth = 560 * context.Scale;
            double boxHeight = 100 * context.Scale;
            double boxLeft = centerX - (boxWidth / 2) + 32; // shift 32px right
            double boxTop = centerY - (boxHeight / 2);

            var box = new Rectangle
            {
                Width = boxWidth,
                Height = boxHeight,
                RadiusX = 20 * context.Scale,
                RadiusY = 20 * context.Scale,
                Fill = new SolidColorBrush(Color.FromArgb(160, 128, 128, 128)) // semi-transparent gray
            };
            Canvas.SetLeft(box, boxLeft);
            Canvas.SetTop(box, boxTop);
            context.Canvas.Children.Add(box);

            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 48 * context.Scale,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Approx text position inside the box
            double textLeft = boxLeft + (boxWidth / 2) - (260 * context.Scale);
            double textTop = boxTop + (boxHeight / 2) - (30 * context.Scale);
            Canvas.SetLeft(textBlock, textLeft);
            Canvas.SetTop(textBlock, textTop);
            context.Canvas.Children.Add(textBlock);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        /// <summary>
        /// Hilfsklasse für Rendering-Kontext
        /// </summary>
        private class RenderContext
        {
            public Canvas Canvas { get; }
            public double Scale { get; }
            public double OffsetX { get; }
            public double OffsetY { get; }
            public int FieldWidth { get; }
            public int FieldHeight { get; }

            public RenderContext(Canvas canvas, int fieldWidth, int fieldHeight)
            {
                Canvas = canvas;
                FieldWidth = fieldWidth;
                FieldHeight = fieldHeight;

                double scaleX = canvas.ActualWidth / fieldWidth;
                double scaleY = canvas.ActualHeight / fieldHeight;
                Scale = Math.Min(scaleX, scaleY);

                double scaledWidth = fieldWidth * Scale;
                double scaledHeight = fieldHeight * Scale;
                OffsetX = (canvas.ActualWidth - scaledWidth) / 2;
                OffsetY = (canvas.ActualHeight - scaledHeight) / 2;
            }

            public Rectangle CreateRectangle(double width, double height, Brush fill, double left, double top)
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
        }
    }
}
