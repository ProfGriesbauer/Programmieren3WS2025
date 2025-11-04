using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    /// <summary>
    /// Die Zeichnen-Klasse: Verantwortlich für die grafische Darstellung des Spiels
    /// </summary>
    public class A5_Paint : IX_PaintTicTacToe
    {

    // Display name shown in the UI dropdowns
    public string Name { get { return "A5 Paint TicTacToe"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is IX_TicTacToeField)
            {
                PaintTicTacToeField(canvas, (IX_TicTacToeField)currentField);
            }
        }

        public void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            canvas.Children.Clear();
            Color backgroundColor = Color.FromRgb(255, 255, 255);
            Color lineColor = Color.FromRgb(255, 0, 0);
            Color xColor = Color.FromRgb(0, 255, 0);
            Color oColor = Color.FromRgb(0, 0, 255);

            canvas.Background = new SolidColorBrush(backgroundColor);
            Brush lineStroke = new SolidColorBrush(lineColor);
            Brush xStroke = new SolidColorBrush(xColor);
            Brush oStroke = new SolidColorBrush(oColor);

            // Zeichne die Linien
            Line l1 = new Line() { X1 = 120, Y1 = 20, X2 = 120, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l1);
            Line l2 = new Line() { X1 = 220, Y1 = 20, X2 = 220, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l2);
            Line l3 = new Line() { X1 = 20, Y1 = 120, X2 = 320, Y2 = 120, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l3);
            Line l4 = new Line() { X1 = 20, Y1 = 220, X2 = 320, Y2 = 220, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l4);

            // Zeichne X und O
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (currentField[i, j] == 1)  // X
                    {
                        Line X1 = new Line() 
                        { 
                            X1 = 20 + (j * 100), 
                            Y1 = 20 + (i * 100), 
                            X2 = 120 + (j * 100), 
                            Y2 = 120 + (i * 100), 
                            Stroke = xStroke, 
                            StrokeThickness = 3.0
                        };
                        canvas.Children.Add(X1);
                        
                        Line X2 = new Line() 
                        { 
                            X1 = 20 + (j * 100), 
                            Y1 = 120 + (i * 100), 
                            X2 = 120 + (j * 100), 
                            Y2 = 20 + (i * 100), 
                            Stroke = xStroke, 
                            StrokeThickness = 3.0
                        };
                        canvas.Children.Add(X2);
                    }
                    else if (currentField[i, j] == 2)  // O
                    {
                        Ellipse OE = new Ellipse() 
                        { 
                            Margin = new Thickness(20 + (j * 100), 20 + (i * 100), 0, 0), 
                            Width = 100, 
                            Height = 100, 
                            Stroke = oStroke, 
                            StrokeThickness = 3.0
                        };
                        canvas.Children.Add(OE);
                    }
                }
            }
        }
    }
}