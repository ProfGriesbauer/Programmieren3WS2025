using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class A5_SnakePaint : IPaintGame2
    {
        public string Name => "A5 Paint Snake";

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is A5_SnakeField field)
            {
                canvas.Children.Clear();

                // Berechne die optimale Zellengröße basierend auf der Canvas-Größe
                double cellWidth = canvas.ActualWidth / A5_SnakeField.FIELD_SIZE;
                double cellHeight = canvas.ActualHeight / A5_SnakeField.FIELD_SIZE;
                double cellSize = Math.Min(cellWidth, cellHeight);

                // Berechne den Offset, um das Spielfeld zu zentrieren
                double offsetX = (canvas.ActualWidth - (cellSize * A5_SnakeField.FIELD_SIZE)) / 2;
                double offsetY = (canvas.ActualHeight - (cellSize * A5_SnakeField.FIELD_SIZE)) / 2;
                
                // Zeichne das Spielfeld
                for (int x = 0; x < A5_SnakeField.FIELD_SIZE; x++)
                {
                    for (int y = 0; y < A5_SnakeField.FIELD_SIZE; y++)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = cellSize;
                        rect.Height = cellSize;
                        rect.Stroke = Brushes.Gray;
                        
                        if (field.GetPosition(x, y) == 1)
                        {
                            rect.Fill = Brushes.Green; // Schlange
                        }
                        else
                        {
                            rect.Fill = Brushes.White; // Leeres Feld
                        }

                        Canvas.SetLeft(rect, offsetX + (x * cellSize));     // x für horizontale Position
                        Canvas.SetTop(rect, offsetY + (y * cellSize));      // y für vertikale Position
                        canvas.Children.Add(rect);
                    }
                }
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }
}