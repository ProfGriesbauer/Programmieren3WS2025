using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Painter for Tron: implements IPaintGame2 for ticked animation support
    public class B3_Mika_Roeder_Tron_Paint : IPaintGame2
    {
        public string Name { get { return "B3 Mika Röder Tron Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            // For initial painting, call tick-paint to keep a single implementation
            TickPaintGameField(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null || currentField == null) return;

            canvas.Children.Clear();

            // If the field is our B3 field, use its dimensions
            int cols = 40, rows = 25;
            if (currentField is B3_Mika_Roeder_Tron_Field bf)
            {
                cols = bf.Width;
                rows = bf.Height;
            }

            double cellSize = Math.Min( (canvas.ActualWidth - 40) / cols, (canvas.ActualHeight - 40) / rows );
            if (double.IsNaN(cellSize) || cellSize <= 0) cellSize = 12.0;
            double offset = 20.0;

            // background
            var bg = new Rectangle() { Width = cols * cellSize, Height = rows * cellSize, Fill = Brushes.Black };
            Canvas.SetLeft(bg, offset); Canvas.SetTop(bg, offset); canvas.Children.Add(bg);

            // draw cells
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int val = 0;
                    if (currentField is B3_Mika_Roeder_Tron_Field bff)
                    {
                        val = bff[r, c];
                    }
                    else
                    {
                        // attempt reflection as fallback
                        try
                        {
                            var t = currentField.GetType();
                            var prop = t.GetProperty("Item", new Type[] { typeof(int), typeof(int) });
                            if (prop != null)
                            {
                                var v = prop.GetValue(currentField, new object[] { r, c });
                                if (v is int) val = (int)v;
                            }
                        }
                        catch { val = 0; }
                    }

                    if (val == 0) continue;

                    Rectangle rect = new Rectangle() { Width = cellSize - 1, Height = cellSize - 1 };
                    if (val == 1) rect.Fill = Brushes.Cyan; else rect.Fill = Brushes.Orange;
                    Canvas.SetLeft(rect, offset + c * cellSize + 0.5);
                    Canvas.SetTop(rect, offset + r * cellSize + 0.5);
                    canvas.Children.Add(rect);
                }
            }

            // crash flash: if crash active and flashing on, draw a red highlight
            if (currentField is B3_Mika_Roeder_Tron_Field bfCrash && bfCrash.CrashActive && bfCrash.CrashFlashOn)
            {
                int cr = Math.Max(0, Math.Min(rows - 1, bfCrash.CrashRow));
                int cc = Math.Max(0, Math.Min(cols - 1, bfCrash.CrashCol));
                var crashRect = new Rectangle()
                {
                    Width = cellSize,
                    Height = cellSize,
                    Fill = Brushes.Red,
                    Opacity = 0.9
                };
                Canvas.SetLeft(crashRect, offset + cc * cellSize);
                Canvas.SetTop(crashRect, offset + cr * cellSize);
                canvas.Children.Add(crashRect);
            }

            // if field has a countdown, draw an overlay with the remaining seconds
            if (currentField is B3_Mika_Roeder_Tron_Field bf2 && bf2.CountdownRemainingSeconds > 0)
            {
                double overlayWidth = cols * cellSize;
                double overlayHeight = rows * cellSize;

                var overlay = new Rectangle()
                {
                    Width = overlayWidth,
                    Height = overlayHeight,
                    Fill = Brushes.Black,
                    Opacity = 0.6
                };
                Canvas.SetLeft(overlay, offset);
                Canvas.SetTop(overlay, offset);
                canvas.Children.Add(overlay);

                double fontSize = Math.Min(120, cellSize * 5);
                var tb = new TextBlock()
                {
                    Width = overlayWidth,
                    Text = bf2.CountdownRemainingSeconds.ToString(),
                    FontSize = fontSize,
                    Foreground = Brushes.White,
                    FontWeight = System.Windows.FontWeights.Bold,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                // center vertically
                double textTop = offset + (overlayHeight - fontSize) / 2.0 - (fontSize * 0.15);
                Canvas.SetLeft(tb, offset);
                Canvas.SetTop(tb, textTop);
                canvas.Children.Add(tb);
            }
        }
    }
}
