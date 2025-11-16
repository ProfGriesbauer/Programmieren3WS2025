using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    /******************************************************************************
     * B2 Labyrinth Game - Painter Component
     * 
     * Zeichnet das Labyrinth mit eingeschränktem Sichtfeld (Fog of War)
     * Nur ein kleiner Bereich um den Spieler herum ist sichtbar
     ******************************************************************************/

    #region Abstract Base Classes

    /// <summary>
    /// Abstrakte Basis für Labyrinth-Painter
    /// </summary>
    public abstract class B2_AbstractMazePainter : IPaintGame
    {
        public abstract string Name { get; }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null) return;
            if (currentField is B2_AbstractMazeField mazeField)
            {
                PaintMazeField(canvas, mazeField);
            }
        }

        protected abstract void PaintMazeField(Canvas canvas, B2_AbstractMazeField field);
    }

    #endregion

    #region Concrete Implementations

    /// <summary>
    /// Konkreter Labyrinth-Painter mit Sichtfeld-Mechanik
    /// </summary>
    public class B2_MazePainter : B2_AbstractMazePainter
    {
        private const int ViewRadius = 3; // Sichtradius um Spieler (3 = 7x7 Sichtfeld)
        
        public override string Name => "B2 - Maze Painter";

        protected override void PaintMazeField(Canvas canvas, B2_AbstractMazeField field)
        {
            if (canvas == null || field == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Colors.Black); // Dunkler Hintergrund für "Nebel"

            // Canvas-Dimensionen
            double canvasW = canvas.ActualWidth > 0 ? canvas.ActualWidth : 600;
            double canvasH = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;

            // Sichtfeld berechnen
            int playerR = field.PlayerRow;
            int playerC = field.PlayerCol;
            
            int minRow = Math.Max(0, playerR - ViewRadius);
            int maxRow = Math.Min(field.Rows - 1, playerR + ViewRadius);
            int minCol = Math.Max(0, playerC - ViewRadius);
            int maxCol = Math.Min(field.Cols - 1, playerC + ViewRadius);

            int visibleRows = maxRow - minRow + 1;
            int visibleCols = maxCol - minCol + 1;

            // Zellengröße basierend auf sichtbarem Bereich
            double cellW = canvasW / visibleCols;
            double cellH = canvasH / visibleRows;
            double cellSize = Math.Min(cellW, cellH);

            // Offset für Zentrierung
            double offsetX = (canvasW - (visibleCols * cellSize)) / 2;
            double offsetY = (canvasH - (visibleRows * cellSize)) / 2;

            // Zeichne nur sichtbaren Bereich
            for (int r = minRow; r <= maxRow; r++)
            {
                for (int c = minCol; c <= maxCol; c++)
                {
                    int displayR = r - minRow;
                    int displayC = c - minCol;

                    double x = offsetX + displayC * cellSize;
                    double y = offsetY + displayR * cellSize;

                    // Entfernung zum Spieler für Nebel-Effekt
                    double distance = Math.Sqrt(Math.Pow(r - playerR, 2) + Math.Pow(c - playerC, 2));
                    double visibility = 1.0 - (distance / (ViewRadius + 1));
                    visibility = Math.Max(0.2, Math.Min(1.0, visibility));

                    var cellType = field[r, c];
                    Color cellColor = GetCellColor(cellType);
                    
                    // Abdunkeln basierend auf Entfernung
                    cellColor = Color.FromArgb(
                        255,
                        (byte)(cellColor.R * visibility),
                        (byte)(cellColor.G * visibility),
                        (byte)(cellColor.B * visibility)
                    );

                    // Zeichne Zelle
                    var rect = new Rectangle
                    {
                        Width = cellSize - 1,
                        Height = cellSize - 1,
                        Fill = new SolidColorBrush(cellColor),
                        Stroke = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    canvas.Children.Add(rect);

                    // Zeichne Spieler-Symbol
                    if (cellType == B2_MazeCellType.Player)
                    {
                        var playerCircle = new Ellipse
                        {
                            Width = cellSize * 0.6,
                            Height = cellSize * 0.6,
                            Fill = new SolidColorBrush(Colors.Yellow),
                            Stroke = new SolidColorBrush(Colors.Orange),
                            StrokeThickness = 2
                        };
                        Canvas.SetLeft(playerCircle, x + cellSize * 0.2);
                        Canvas.SetTop(playerCircle, y + cellSize * 0.2);
                        canvas.Children.Add(playerCircle);
                    }

                    // Zeichne Ziel-Symbol
                    if (cellType == B2_MazeCellType.Goal)
                    {
                        // Stern-Form für Ziel
                        var goalStar = new Polygon
                        {
                            Fill = new SolidColorBrush(Colors.Gold),
                            Stroke = new SolidColorBrush(Colors.DarkGoldenrod),
                            StrokeThickness = 1.5,
                            Points = CreateStarPoints(x + cellSize / 2, y + cellSize / 2, cellSize * 0.4, 5)
                        };
                        canvas.Children.Add(goalStar);
                    }
                }
            }

            // Zeichne Legende / HUD
            DrawHUD(canvas, canvasW, canvasH, field);
        }

        /// <summary>
        /// Bestimmt die Farbe einer Zelle basierend auf ihrem Typ
        /// </summary>
        private Color GetCellColor(B2_MazeCellType cellType)
        {
            switch (cellType)
            {
                case B2_MazeCellType.Wall:
                    return Color.FromRgb(40, 40, 40); // Dunkelgrau
                case B2_MazeCellType.Path:
                    return Color.FromRgb(200, 200, 200); // Hellgrau
                case B2_MazeCellType.Visited:
                    return Color.FromRgb(150, 150, 180); // Bläulich (bereits besucht)
                case B2_MazeCellType.Player:
                    return Color.FromRgb(100, 100, 100); // Hintergrund unter Spieler
                case B2_MazeCellType.Goal:
                    return Color.FromRgb(100, 180, 100); // Grünlich
                default:
                    return Colors.Black;
            }
        }

        /// <summary>
        /// Erstellt Punkte für eine Stern-Form
        /// </summary>
        private PointCollection CreateStarPoints(double centerX, double centerY, double radius, int points)
        {
            var collection = new PointCollection();
            double angleStep = Math.PI * 2 / points;
            double innerRadius = radius * 0.4;

            for (int i = 0; i < points * 2; i++)
            {
                double angle = i * angleStep / 2 - Math.PI / 2;
                double r = (i % 2 == 0) ? radius : innerRadius;
                double x = centerX + Math.Cos(angle) * r;
                double y = centerY + Math.Sin(angle) * r;
                collection.Add(new Point(x, y));
            }

            return collection;
        }

        /// <summary>
        /// Zeichnet HUD mit Anweisungen
        /// </summary>
        private void DrawHUD(Canvas canvas, double canvasW, double canvasH, B2_AbstractMazeField field)
        {
            // Steuerungshinweise
            var instructions = new TextBlock
            {
                Text = "Arrow Keys / WASD to move\nFind the golden star!",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(10, 5, 10, 5)
            };
            Canvas.SetLeft(instructions, 10);
            Canvas.SetTop(instructions, 10);
            canvas.Children.Add(instructions);

            // Position anzeigen
            var position = new TextBlock
            {
                Text = $"Position: ({field.PlayerRow}, {field.PlayerCol})",
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(8, 4, 8, 4)
            };
            Canvas.SetLeft(position, 10);
            Canvas.SetBottom(position, 10);
            canvas.Children.Add(position);
        }
    }

    #endregion
}
