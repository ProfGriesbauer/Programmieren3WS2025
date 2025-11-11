using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Painter für Gruppe B3 (Mika Röder) - zeichnet das 3x3 Spielfeld selbst
    public class B3_Mika_Roeder_Paint : IPaintGame
    {
        public string Name { get { return "B3 Mika Röder TicTacToe Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null || currentField == null)
                return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            double cellSize = 120.0; // etwas größer für bessere Sichtbarkeit
            double offset = 20.0;

            // Draw background rectangle for the whole board
            var bg = new System.Windows.Shapes.Rectangle()
            {
                Width = 3 * cellSize,
                Height = 3 * cellSize,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(bg, offset);
            Canvas.SetTop(bg, offset);
            canvas.Children.Add(bg);

            // Draw grid lines
            Brush lineStroke = Brushes.Black;
            for (int i = 1; i <= 2; i++)
            {
                Line v = new Line()
                {
                    X1 = offset + (i * cellSize),
                    Y1 = offset,
                    X2 = offset + (i * cellSize),
                    Y2 = offset + (3 * cellSize),
                    Stroke = lineStroke,
                    StrokeThickness = 3
                };
                canvas.Children.Add(v);

                Line h = new Line()
                {
                    X1 = offset,
                    Y1 = offset + (i * cellSize),
                    X2 = offset + (3 * cellSize),
                    Y2 = offset + (i * cellSize),
                    Stroke = lineStroke,
                    StrokeThickness = 3
                };
                canvas.Children.Add(h);
            }

            // If the field is our B3 field, access indexer directly. Otherwise try reflection.
            Func<int, int, int?> GetCell = (r, c) =>
            {
                if (currentField is B3_Mika_Roeder_Field bf)
                {
                    return bf[r, c];
                }
                else
                {
                    try
                    {
                        var t = currentField.GetType();
                        var prop = t.GetProperty("Item", new Type[] { typeof(int), typeof(int) });
                        if (prop == null) return null;
                        var v = prop.GetValue(currentField, new object[] { r, c });
                        if (v is int) return (int)v;
                        return null;
                    }
                    catch { return null; }
                }
            };

            // Draw X and O
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int? val = GetCell(row, col);
                    double x = offset + col * cellSize;
                    double y = offset + row * cellSize;

                    if (val == 1)
                    {
                        // Draw X
                        double m = cellSize * 0.18;
                        var l1 = new Line() { X1 = x + m, Y1 = y + m, X2 = x + cellSize - m, Y2 = y + cellSize - m, Stroke = Brushes.DarkBlue, StrokeThickness = 6 };
                        var l2 = new Line() { X1 = x + cellSize - m, Y1 = y + m, X2 = x + m, Y2 = y + cellSize - m, Stroke = Brushes.DarkBlue, StrokeThickness = 6 };
                        canvas.Children.Add(l1);
                        canvas.Children.Add(l2);
                    }
                    else if (val == 2)
                    {
                        // Draw O
                        double m = cellSize * 0.14;
                        var e = new Ellipse() { Width = cellSize - 2 * m, Height = cellSize - 2 * m, Stroke = Brushes.DarkRed, StrokeThickness = 6, Fill = Brushes.Transparent };
                        Canvas.SetLeft(e, x + m);
                        Canvas.SetTop(e, y + m);
                        canvas.Children.Add(e);
                    }
                }
            }
        }
    }
}
