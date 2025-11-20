using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    /******************************************************************************
     * B2 Labyrinth Game - Painter Component (2 Players)
     * 
     * Zeichnet das Labyrinth mit 2 Spielern in Vogelperspektive
     * - Helles, freundliches Design
     * - Männchen statt Punkte für Spieler
     * - Sichtfelder für beide Spieler
     * - Ziel in der Mitte
     ******************************************************************************/

    #region Abstract Base Classes
    /// Abstrakte Basis für Labyrinth-Painter
    public abstract class B2_AbstractMazePainter : IPaintGame2
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

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            // Tick-basiertes Zeichnen für automatisches Update
            PaintGameField(canvas, currentField);
        }

        protected abstract void PaintMazeField(Canvas canvas, B2_AbstractMazeField field);
    }

    #endregion

    #region Concrete Implementations
    /// Konkreter Labyrinth-Painter für 2-Spieler-Modus
    public class B2_MazePainter2Player : B2_AbstractMazePainter
    {
        private const int ViewRadius = 2; // Sichtradius um Spieler (2 = 5x5 Sichtfeld)
        
        public override string Name => "B2 - Maze Painter (2 Players)";

        protected override void PaintMazeField(Canvas canvas, B2_AbstractMazeField field)
        {
            if (canvas == null || field == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(245, 245, 250)); // Heller Hintergrund

            // Canvas-Dimensionen
            double canvasW = canvas.ActualWidth > 0 ? canvas.ActualWidth : 600;
            double canvasH = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;

            // Gesamtes Feld zeichnen (Vogelperspektive)
            double cellW = canvasW / field.Cols;
            double cellH = canvasH / field.Rows;
            double cellSize = Math.Min(cellW, cellH);

            // Offset für Zentrierung
            double offsetX = (canvasW - (field.Cols * cellSize)) / 2;
            double offsetY = (canvasH - (field.Rows * cellSize)) / 2;

            // Prüfe ob Spiel beendet ist (für Rules-Cast)
            bool gameEnded = false;
            if (field is B2_AbstractMazeField)
            {
                // Suche nach der Rules-Instanz über CanBeRuledBy
                var rulesType = typeof(B2_MazeRules);
                // Wir nutzen einen Workaround: Prüfe ob ein Spieler am Ziel ist
                gameEnded = (field.Player1Row == field.GoalRow && field.Player1Col == field.GoalCol) ||
                           (field.Player2Row == field.GoalRow && field.Player2Col == field.GoalCol);
            }

            // Zeichne Sichtfelder nur wenn Spiel noch läuft
            if (!gameEnded)
            {
                DrawViewRadius(canvas, offsetX, offsetY, cellSize, field.Player1Row, field.Player1Col, Color.FromArgb(30, 50, 120, 255));
                DrawViewRadius(canvas, offsetX, offsetY, cellSize, field.Player2Row, field.Player2Col, Color.FromArgb(30, 255, 80, 80));
            }

            // Zeichne komplettes Labyrinth
            for (int r = 0; r < field.Rows; r++)
            {
                for (int c = 0; c < field.Cols; c++)
                {
                    double x = offsetX + c * cellSize;
                    double y = offsetY + r * cellSize;

                    var cellType = field[r, c];
                    
                    // Sichtbarkeit für Spieler 1 berechnen
                    double dist1 = Math.Sqrt(Math.Pow(r - field.Player1Row, 2) + Math.Pow(c - field.Player1Col, 2));
                    bool visible1 = dist1 <= ViewRadius;
                    
                    // Sichtbarkeit für Spieler 2 berechnen
                    double dist2 = Math.Sqrt(Math.Pow(r - field.Player2Row, 2) + Math.Pow(c - field.Player2Col, 2));
                    bool visible2 = dist2 <= ViewRadius;

                    // Ziel ist immer sichtbar
                    bool isGoal = (r == field.GoalRow && c == field.GoalCol);

                    Color cellColor;
                    // Wenn Spiel beendet: Alles aufdecken, sonst nur Sichtfeld
                    if (gameEnded || visible1 || visible2 || isGoal)
                    {
                        // Vollständig sichtbar - helle Farben
                        cellColor = GetCellColor(cellType);
                    }
                    else
                    {
                        // Nebel - grau abgedunkelt
                        cellColor = Color.FromRgb(180, 180, 185);
                    }

                    // Zeichne Zelle
                    var rect = new Rectangle
                    {
                        Width = cellSize - 1,
                        Height = cellSize - 1,
                        Fill = new SolidColorBrush(cellColor),
                        Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 210)),
                        StrokeThickness = 0.5,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    canvas.Children.Add(rect);

                    // Zeichne Spieler 1 Männchen (Blau)
                    if (cellType.IsType("Player1"))
                    {
                        DrawPlayer(canvas, x, y, cellSize, Color.FromRgb(50, 120, 255), "1");
                    }

                    // Zeichne Spieler 2 Männchen (Rot)
                    if (cellType.IsType("Player2"))
                    {
                        DrawPlayer(canvas, x, y, cellSize, Color.FromRgb(255, 80, 80), "2");
                    }

                    // Zeichne Ziel-Symbol
                    if (cellType.IsType("Goal"))
                    {
                        // Stern-Form für Ziel
                        var goalStar = new Polygon
                        {
                            Fill = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                            Stroke = new SolidColorBrush(Color.FromRgb(200, 160, 0)),
                            StrokeThickness = 2,
                            Points = CreateStarPoints(x + cellSize / 2, y + cellSize / 2, cellSize * 0.4, 5),
                            IsHitTestVisible = false
                        };
                        canvas.Children.Add(goalStar);
                    }
                }
            }

            // Zeichne Countdown wenn aktiv
            if (field.IsCountdownActive)
            {
                DrawCountdown(canvas, canvasW, canvasH, field.RemainingCountdown);
            }
            
            // Zeichne HUD
            DrawHUD(canvas, canvasW, canvasH, field);
        }

        /// Zeichnet ein Männchen (Spieler)
        
        private void DrawPlayer(Canvas canvas, double x, double y, double cellSize, Color color, string number)
        {
            double centerX = x + cellSize / 2;
            double centerY = y + cellSize / 2;
            double size = cellSize * 0.7;

            // Körper (Rechteck)
            var body = new Rectangle
            {
                Width = size * 0.4,
                Height = size * 0.5,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(body, centerX - size * 0.2);
            Canvas.SetTop(body, centerY - size * 0.1);
            canvas.Children.Add(body);

            // Kopf (Kreis)
            var head = new Ellipse
            {
                Width = size * 0.35,
                Height = size * 0.35,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(head, centerX - size * 0.175);
            Canvas.SetTop(head, centerY - size * 0.4);
            canvas.Children.Add(head);

            // Spielernummer
            var text = new TextBlock
            {
                Text = number,
                FontSize = size * 0.3,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(text, centerX - size * 0.1);
            Canvas.SetTop(text, centerY - size * 0.35);
            canvas.Children.Add(text);
        }

        
        /// Zeichnet Sichtfeld-Radius
        
        private void DrawViewRadius(Canvas canvas, double offsetX, double offsetY, double cellSize, int row, int col, Color color)
        {
            double centerX = offsetX + col * cellSize + cellSize / 2;
            double centerY = offsetY + row * cellSize + cellSize / 2;
            double radius = (ViewRadius + 0.5) * cellSize;

            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(color),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            canvas.Children.Add(circle);
        }

        /// Bestimmt die Farbe einer Zelle basierend auf ihrem Typ
        
        private Color GetCellColor(B2_MazeCellType cellType)
        {
            if (cellType.IsType("Wall"))
            {
                return Color.FromRgb(60, 60, 70); // Dunkelgrau für Wände
            }
            else if (cellType.IsType("Path"))
            {
                return Color.FromRgb(240, 240, 245); // Fast weiß für Wege
            }
            else if (cellType.IsType("Visited"))
            {
                return Color.FromRgb(200, 220, 240); // Hellblau für besuchte Wege (allgemein)
            }
            else if (cellType.IsType("VisitedPlayer1"))
            {
                return Color.FromRgb(180, 200, 255); // Hellblau für Spieler 1 Spur
            }
            else if (cellType.IsType("VisitedPlayer2"))
            {
                return Color.FromRgb(255, 180, 180); // Hellrot für Spieler 2 Spur
            }
            else if (cellType.IsType("Player1") || cellType.IsType("Player2"))
            {
                return Color.FromRgb(240, 240, 245); // Wie Path (Spieler wird separat gezeichnet)
            }
            else if (cellType.IsType("Goal"))
            {
                return Color.FromRgb(255, 250, 220); // Helles Gelb für Ziel
            }
            else
            {
                return Colors.Gray;
            }
        }

        
        /// Erstellt Punkte für eine Stern-Form
        
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

        /// Zeichnet Countdown 3-2-1
        private void DrawCountdown(Canvas canvas, double canvasW, double canvasH, double remaining)
        {
            // Berechne welche Zahl angezeigt wird: 3, 2, oder 1
            int countdownNumber = (int)Math.Ceiling(remaining);
            if (countdownNumber < 1) countdownNumber = 1;
            if (countdownNumber > 3) countdownNumber = 3;

            // Große Zahl in der Mitte
            var countdownText = new TextBlock
            {
                Text = countdownNumber.ToString(),
                FontSize = 120,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 100, 0)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Messe die Text-Größe
            countdownText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = countdownText.DesiredSize.Width;
            double textHeight = countdownText.DesiredSize.Height;

            // Zentriere den Text
            Canvas.SetLeft(countdownText, (canvasW - textWidth) / 2);
            Canvas.SetTop(countdownText, (canvasH - textHeight) / 2);
            canvas.Children.Add(countdownText);

            // "Start in..." Text darüber
            var startText = new TextBlock
            {
                Text = "Start in...",
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 90))
            };

            startText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(startText, (canvasW - startText.DesiredSize.Width) / 2);
            Canvas.SetTop(startText, (canvasH - textHeight) / 2 - 60);
            canvas.Children.Add(startText);
        }

        
        /// Zeichnet HUD mit Spielerinformationen
        
        private void DrawHUD(Canvas canvas, double canvasW, double canvasH, B2_AbstractMazeField field)
        {
            // Titel
            var title = new TextBlock
            {
                Text = "Race to the Center!",
                Foreground = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                //Background = new SolidColorBrush(Color.FromArgb(225, 255, 255, 255)),
                Padding = new Thickness(15, 8, 15, 8)
            };
            Canvas.SetLeft(title, canvasW / 2 - 83);
            Canvas.SetTop(title, 2);
            canvas.Children.Add(title);

            // Spieler 1 Info
            var player1Info = new TextBlock
            {
                Text = $"Player 1 Blue",
                Foreground = new SolidColorBrush(Color.FromRgb(50, 120, 255)),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                //Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Padding = new Thickness(10, 6, 10, 6)
            };
            Canvas.SetLeft(player1Info, 10);
            Canvas.SetTop(player1Info, 5);
            canvas.Children.Add(player1Info);

            // Spieler 2 Info
            var player2Info = new TextBlock
            {
                Text = $"Player 2 Red",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 80, 80)),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                //Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Padding = new Thickness(10, 6, 10, 6)
            };
            Canvas.SetRight(player2Info, 11);
            Canvas.SetTop(player2Info, 5);
            canvas.Children.Add(player2Info);

            // Steuerungshinweise
            var instructions = new TextBlock
            {
                Text = "Use Arrow Keys / WASD\n to Move",
                Foreground = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                //Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Padding = new Thickness(15, 6, 15, 6)
            };
            Canvas.SetLeft(instructions, canvasW / 2 - 70);
            Canvas.SetBottom(instructions, 5);
            canvas.Children.Add(instructions);
        }
    }

    #endregion
}
