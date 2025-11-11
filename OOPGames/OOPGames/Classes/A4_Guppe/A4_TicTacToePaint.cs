using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    public class A4_TicTacToePaint : X_BaseTicTacToePaint
    {
        public override string Name { get { return "A4_TicTacToePaint"; } }

        public override void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            // draw grid and marks to completely fill the canvas for a 4x4 board
            canvas.Children.Clear();
            Color bgColor = Color.FromRgb(240, 240, 240);
            canvas.Background = new SolidColorBrush(bgColor);
            Color lineColor = Color.FromRgb(80, 80, 80);
            Brush lineStroke = new SolidColorBrush(lineColor);
            Color XColor = Color.FromRgb(200, 20, 20);
            Brush XStroke = new SolidColorBrush(XColor);
            Color OColor = Color.FromRgb(20, 50, 200);
            Brush OStroke = new SolidColorBrush(OColor);

            int N = A4_TicTacToeConfig.N; // NxN
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0) w = Math.Max(500, N * 60);
            if (h <= 0) h = Math.Max(500, N * 60);

            double cellW = w / N;
            double cellH = h / N;

            // vertical lines
            for (int c = 1; c < N; c++)
            {
                double x = c * cellW;
                Line lv = new Line() { X1 = x, Y1 = 0, X2 = x, Y2 = h, Stroke = lineStroke, StrokeThickness = 3.0 };
                canvas.Children.Add(lv);
            }

            // horizontal lines
            for (int r = 1; r < N; r++)
            {
                double y = r * cellH;
                Line lh = new Line() { X1 = 0, Y1 = y, X2 = w, Y2 = y, Stroke = lineStroke, StrokeThickness = 3.0 };
                canvas.Children.Add(lh);
            }

            double padding = Math.Min(cellW, cellH) * 0.12;

            // draw marks
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (currentField[i, j] == 1)
                    {
                        double x0 = j * cellW + padding;
                        double y0 = i * cellH + padding;
                        double x1 = (j + 1) * cellW - padding;
                        double y1 = (i + 1) * cellH - padding;
                        Line X1 = new Line() { X1 = x0, Y1 = y0, X2 = x1, Y2 = y1, Stroke = XStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(X1);
                        Line X2 = new Line() { X1 = x0, Y1 = y1, X2 = x1, Y2 = y0, Stroke = XStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(X2);
                    }
                    else if (currentField[i, j] == 2)
                    {
                        double left = j * cellW + padding;
                        double top = i * cellH + padding;
                        double width = cellW - 2 * padding;
                        double height = cellH - 2 * padding;
                        Ellipse OE = new Ellipse() { Width = width, Height = height, Stroke = OStroke, StrokeThickness = 4.0 };
                        Canvas.SetLeft(OE, left);
                        Canvas.SetTop(OE, top);
                        canvas.Children.Add(OE);
                    }
                }
            }
        }
    }
}
