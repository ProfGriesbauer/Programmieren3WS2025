using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class B5_TicTacToePaint : IPaintGame
    {
        public string Name { get { return "B5 TicTacToe Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            // Clear the canvas
            canvas.Children.Clear();

            // Define the field dimensions
            double fieldWidth = canvas.ActualWidth;
            double fieldHeight = canvas.ActualHeight;
            double cellWidth = fieldWidth / 3;
            double cellHeight = fieldHeight / 3;

            // Draw vertical lines
            for (int i = 1; i < 3; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                line.X1 = i * cellWidth;
                line.Y1 = 0;
                line.X2 = i * cellWidth;
                line.Y2 = fieldHeight;
                canvas.Children.Add(line);
            }

            // Draw horizontal lines
            for (int i = 1; i < 3; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                line.X1 = 0;
                line.Y1 = i * cellHeight;
                line.X2 = fieldWidth;
                line.Y2 = i * cellHeight;
                canvas.Children.Add(line);
            }

            // Draw X's and O's if the field implements IB5_TicTacToeField
            if (currentField is IB5_TicTacToeField)
            {
                IB5_TicTacToeField ticTacToeField = (IB5_TicTacToeField)currentField;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int value = ticTacToeField.GetFieldValue(i, j);
                        double x = j * cellWidth;
                        double y = i * cellHeight;

                        if (value == 1) // X
                        {
                            DrawX(canvas, x, y, cellWidth, cellHeight);
                        }
                        else if (value == 2) // O
                        {
                            DrawO(canvas, x, y, cellWidth, cellHeight);
                        }
                    }
                }
            }
        }

        private void DrawX(Canvas canvas, double x, double y, double width, double height)
        {
            double margin = 20;

            // Draw first diagonal line (\)
            Line line1 = new Line();
            line1.Stroke = Brushes.Red;
            line1.StrokeThickness = 3;
            line1.X1 = x + margin;
            line1.Y1 = y + margin;
            line1.X2 = x + width - margin;
            line1.Y2 = y + height - margin;
            canvas.Children.Add(line1);

            // Draw second diagonal line (/)
            Line line2 = new Line();
            line2.Stroke = Brushes.Red;
            line2.StrokeThickness = 3;
            line2.X1 = x + width - margin;
            line2.Y1 = y + margin;
            line2.X2 = x + margin;
            line2.Y2 = y + height - margin;
            canvas.Children.Add(line2);
        }

        private void DrawO(Canvas canvas, double x, double y, double width, double height)
        {
            double margin = 20;
            Ellipse ellipse = new Ellipse();
            ellipse.Stroke = Brushes.Blue;
            ellipse.StrokeThickness = 3;
            ellipse.Width = width - (2 * margin);
            ellipse.Height = height - (2 * margin);
            Canvas.SetLeft(ellipse, x + margin);
            Canvas.SetTop(ellipse, y + margin);
            canvas.Children.Add(ellipse);
        }
    }
    
    public class B5_GameField : IB5_TicTacToeField
    {
        private int[,] fieldValues = new int[3, 3];

        public int GetFieldValue(int row, int col)
        {
            return fieldValues[row, col];
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is B5_TicTacToePaint;
        }
    }
}