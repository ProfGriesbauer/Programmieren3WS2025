using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
namespace OOPGames
{
    public class SpielPainter : IGamePainter
    {
        public void PaintGame(IGame game, Canvas canvas)
        {
            // Clear the canvas
            canvas.Children.Clear();

            // Set canvas background to white
            canvas.Background = Brushes.White;

            // Define board dimensions
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            double cellWidth = width / 3;
            double cellHeight = height / 3;

            // Draw vertical lines
            for (int i = 1; i < 3; i++)
            {
                Line verticalLine = new Line
                {
                    X1 = i * cellWidth,
                    Y1 = 0,
                    X2 = i * cellWidth,
                    Y2 = height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                canvas.Children.Add(verticalLine);
            }

            // Draw horizontal lines
            for (int i = 1; i < 3; i++)
            {
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = i * cellHeight,
                    X2 = width,
                    Y2 = i * cellHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                canvas.Children.Add(horizontalLine);
            }

            // Draw X's and O's if the game is a TicTacToe game
            if (game is ITicTacToe ticTacToeGame)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        int value = ticTacToeGame.GetFieldContent(row, col);
                        if (value != 0)
                        {
                            // Calculate center position of the cell
                            double centerX = col * cellWidth + cellWidth / 2;
                            double centerY = row * cellHeight + cellHeight / 2;

                            if (value == 1) // Draw X
                            {
                                double offset = Math.Min(cellWidth, cellHeight) * 0.3;

                                Line line1 = new Line
                                {
                                    X1 = centerX - offset,
                                    Y1 = centerY - offset,
                                    X2 = centerX + offset,
                                    Y2 = centerY + offset,
                                    Stroke = Brushes.Blue,
                                    StrokeThickness = 3
                                };

                                Line line2 = new Line
                                {
                                    X1 = centerX - offset,
                                    Y1 = centerY + offset,
                                    X2 = centerX + offset,
                                    Y2 = centerY - offset,
                                    Stroke = Brushes.Blue,
                                    StrokeThickness = 3
                                };

                                canvas.Children.Add(line1);
                                canvas.Children.Add(line2);
                            }
                            else if (value == 2) // Draw O
                            {
                                double radius = Math.Min(cellWidth, cellHeight) * 0.3;
                                Ellipse ellipse = new Ellipse
                                {
                                    Width = radius * 2,
                                    Height = radius * 2,
                                    Stroke = Brushes.Red,
                                    StrokeThickness = 3
                                };

                                Canvas.SetLeft(ellipse, centerX - radius);
                                Canvas.SetTop(ellipse, centerY - radius);
                                canvas.Children.Add(ellipse);
                            }
                        }
                    }
                }
            }
        }
    }
}