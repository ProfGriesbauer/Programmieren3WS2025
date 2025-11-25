using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Renders all game visuals: Terrain with height map, Tanks with color and orientation, 
    // Flying projectile, UI (angle, power, health, wind)
    public class B5_Shellshock_Painter : IPaintGame2
    {
        public string Name => "B5_Shellshock_Painter";

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is not B5_Shellshock_Field field) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(135, 206, 235)); // Sky blue

            double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
            double canvasHeight = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;

            // Scale factor for terrain
            double scaleX = canvasWidth / field.Terrain.Width;
            double scaleY = canvasHeight;

            // Draw terrain
            DrawTerrain(canvas, field.Terrain, scaleX, scaleY);

            // Draw tanks
            DrawTank(canvas, field.Tank1, scaleX, scaleY);
            DrawTank(canvas, field.Tank2, scaleX, scaleY);

            // Draw active tank indicator (small dot) on top of the active tank
            B5_Shellshock_Tank activeTank = field.ActiveTankNumber == 1 ? field.Tank1 : field.Tank2;
            DrawActiveTankDot(canvas, activeTank, scaleX, scaleY);

            // Draw projectile if active
            if (field.Projectile != null && field.Projectile.IsActive)
            {
                System.Diagnostics.Debug.WriteLine($"Drawing projectile at ({field.Projectile.X}, {field.Projectile.Y})");
                DrawProjectile(canvas, field.Projectile, scaleX, scaleY);
            }

            // Draw health pack if active
            if (field.HealthPack != null && field.HealthPack.IsActive)
            {
                DrawHealthPack(canvas, field.HealthPack, scaleX, scaleY);
            }

            // Draw UI (tank info + wind)
            DrawUI(canvas, field, canvasWidth, canvasHeight);

            // Overlay for start or win
            DrawOverlay(canvas, field, canvasWidth, canvasHeight);

            // Draw last trajectories as dotted colored lines
            DrawTrajectory(canvas, field.LastTrajectoryP1, scaleX, scaleY, Brushes.Red);
            DrawTrajectory(canvas, field.LastTrajectoryP2, scaleX, scaleY, Brushes.Blue);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            // Repaint entire field for smooth animation
            PaintGameField(canvas, currentField);
        }

        private void DrawActiveTankDot(Canvas canvas, B5_Shellshock_Tank tank, double scaleX, double scaleY)
        {
            if (!tank.IsAlive) return;
            double x = tank.X * scaleX;
            double y = tank.Y * scaleY;
            // Small dot above the tank body to mark the active tank
            Ellipse dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(dot, x - 4);
            Canvas.SetTop(dot, y - 18); // slightly above the tank body
            canvas.Children.Add(dot);
        }

        private void DrawTerrain(Canvas canvas, B5_Shellshock_Terrain terrain, double scaleX, double scaleY)
        {
            PointCollection points = new PointCollection();

            // Add terrain points
            for (int i = 0; i < terrain.Width; i++)
            {
                double x = i * scaleX;
                double y = terrain.HeightMap[i] * scaleY;
                points.Add(new Point(x, y));
            }

            // Close the polygon at bottom corners
            points.Add(new Point(terrain.Width * scaleX, scaleY));
            points.Add(new Point(0, scaleY));

            Polygon terrainPolygon = new Polygon
            {
                Points = points,
                Fill = new SolidColorBrush(Color.FromRgb(34, 139, 34)), // Forest green
                Stroke = new SolidColorBrush(Color.FromRgb(0, 100, 0)),
                StrokeThickness = 1
            };

            canvas.Children.Add(terrainPolygon);
        }

        private void DrawTank(Canvas canvas, B5_Shellshock_Tank tank, double scaleX, double scaleY)
        {
            if (!tank.IsAlive) return;

            double x = tank.X * scaleX;
            double y = tank.Y * scaleY;

            // Tank body
            Rectangle body = new Rectangle
            {
                Width = 26,
                Height = 14,
                Fill = tank.Color == B5_Shellshock_TankColor.Red ? Brushes.Red : Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(body, x - 13);
            Canvas.SetTop(body, y - 14);
            canvas.Children.Add(body);

            // Tank barrel
            double barrelLength = 24; // make barrel longer for visibility
            double angleRad = tank.Angle * Math.PI / 180.0;
            double barrelEndX = x + barrelLength * Math.Cos(angleRad);
            double barrelEndY = y - barrelLength * Math.Sin(angleRad);

            Line barrel = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = barrelEndX,
                Y2 = barrelEndY,
                Stroke = Brushes.Black,
                StrokeThickness = 4
            };
            canvas.Children.Add(barrel);

            // Removed angle indicator arc for cleaner look
        }

        private void DrawAngleIndicator(Canvas canvas, B5_Shellshock_Tank tank, double x, double y)
        {
            // Draw a small arc showing the current angle
            double radius = 20;
            double angleRad = tank.Angle * Math.PI / 180.0;
            
            // Draw arc from 0 to current angle
            for (double a = 0; a <= angleRad; a += 0.1)
            {
                double arcX = x + radius * Math.Cos(a);
                double arcY = y - radius * Math.Sin(a);
                
                Ellipse dot = new Ellipse
                {
                    Width = 2,
                    Height = 2,
                    Fill = Brushes.Yellow
                };
                Canvas.SetLeft(dot, arcX - 1);
                Canvas.SetTop(dot, arcY - 1);
                canvas.Children.Add(dot);
            }
        }

        private void DrawProjectile(Canvas canvas, B5_Shellshock_Projectile projectile, double scaleX, double scaleY)
        {
            double x = projectile.X * scaleX;
            double y = projectile.Y * scaleY;

            Ellipse shell = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Brushes.Black,
                Stroke = Brushes.Yellow,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(shell, x - 6);
            Canvas.SetTop(shell, y - 6);
            canvas.Children.Add(shell);
        }

        /// <summary>
        /// Draws a health pack as a red box with rounded corners and white cross.
        /// </summary>
        private void DrawHealthPack(Canvas canvas, B5_Shellshock_HealthPack healthPack, double scaleX, double scaleY)
        {
            double x = healthPack.X * scaleX;
            double y = healthPack.Y * scaleY;
            double size = B5_Shellshock_HealthPack.PackSize;

            // Red box with rounded corners
            Rectangle box = new Rectangle
            {
                Width = size,
                Height = size,
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                RadiusX = 4,
                RadiusY = 4
            };
            Canvas.SetLeft(box, x - size / 2);
            Canvas.SetTop(box, y - size / 2);
            canvas.Children.Add(box);

            // White cross (vertical line)
            Rectangle verticalLine = new Rectangle
            {
                Width = 3,
                Height = size * 0.6,
                Fill = Brushes.White
            };
            Canvas.SetLeft(verticalLine, x - 1.5);
            Canvas.SetTop(verticalLine, y - size * 0.3);
            canvas.Children.Add(verticalLine);

            // White cross (horizontal line)
            Rectangle horizontalLine = new Rectangle
            {
                Width = size * 0.6,
                Height = 3,
                Fill = Brushes.White
            };
            Canvas.SetLeft(horizontalLine, x - size * 0.3);
            Canvas.SetTop(horizontalLine, y - 1.5);
            canvas.Children.Add(horizontalLine);
        }

        private void DrawTrajectory(Canvas canvas, System.Collections.Generic.List<B5_Shellshock_Point> points, double scaleX, double scaleY, Brush color)
        {
            if (points == null || points.Count < 2) return;

            var polyline = new Polyline
            {
                Stroke = color,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 2, 4 },
                IsHitTestVisible = false
            };

            var pc = new PointCollection();
            int step = 1; // use all points; dashed stroke will create dotted look
            for (int i = 0; i < points.Count; i += step)
            {
                double x = points[i].X * scaleX;
                double y = points[i].Y * scaleY;
                pc.Add(new Point(x, y));
            }
            polyline.Points = pc;
            canvas.Children.Add(polyline);
        }

        private void DrawUI(Canvas canvas, B5_Shellshock_Field field, double canvasWidth, double canvasHeight)
        {
            // Draw tank 1 info (left edge)
            DrawTankInfo(canvas, field.Tank1, 1, 10, 10, false);

            // Draw tank 2 info (right edge) using right alignment
            DrawTankInfo(canvas, field.Tank2, 2, 10, 10, true);

            // Combined wind + movements indicator (center top)
            DrawWindAndMovesIndicator(canvas, field.Wind, field.MovementsRemaining, field.MaxMovesPerTurn, canvasWidth / 2, 10);

            // (Movements now integrated with wind indicator panel)
        }

        private void DrawOverlay(Canvas canvas, B5_Shellshock_Field field, double canvasWidth, double canvasHeight)
        {
            if (field.GamePhase == B5_Shellshock_GamePhase.PlayerTurn || field.GamePhase == B5_Shellshock_GamePhase.ProjectileInFlight) return;

            string title;
            string body;
            Brush borderBrush = Brushes.Gold;
            Brush titleBrush = Brushes.Yellow;
            if (field.GamePhase == B5_Shellshock_GamePhase.Setup)
            {
                title = "Shellshock Tanks";
                body = "Controls:\nA/D: Move\nW/S: Angle\nQ/E: Power\nSpace: Fire / Start\nThe Terrain is Randomly Generated";
            }
            else // GameOver
            {
                if (!field.Tank1.IsAlive)
                    title = "Player 2 Wins!";
                else if (!field.Tank2.IsAlive)
                    title = "Player 1 Wins!";
                else
                    title = "Game Over";
                body = ""; // No restart info displayed per request
            }

            double boxW = 320;
            double boxH = 180;
            double left = (canvasWidth - boxW) / 2;
            double top = (canvasHeight - boxH) / 2;

            Rectangle bg = new Rectangle
            {
                Width = boxW,
                Height = boxH,
                Fill = new SolidColorBrush(Color.FromArgb(190, 20, 20, 25)),
                Stroke = borderBrush,
                StrokeThickness = 3,
                RadiusX = 12,
                RadiusY = 12
            };
            Canvas.SetLeft(bg, left);
            Canvas.SetTop(bg, top);
            canvas.Children.Add(bg);

            TextBlock titleText = new TextBlock
            {
                Text = title,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = titleBrush,
                TextAlignment = TextAlignment.Center,
                Width = boxW
            };
            Canvas.SetLeft(titleText, left);
            Canvas.SetTop(titleText, top + 15);
            canvas.Children.Add(titleText);

            if (!string.IsNullOrEmpty(body))
            {
                TextBlock bodyText = new TextBlock
                {
                    Text = body,
                    FontSize = 14,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Left,
                    Width = boxW - 30,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0)
                };
                Canvas.SetLeft(bodyText, left + 15);
                Canvas.SetTop(bodyText, top + 65);
                canvas.Children.Add(bodyText);
            }
        }

        private void DrawTankInfo(Canvas canvas, B5_Shellshock_Tank tank, int playerNum, double x, double y, bool alignRight)
        {
            // Background panel matching wind/moves panel style
            double panelW = 130;
            double panelH = 100;
            Rectangle panel = new Rectangle
            {
                Width = panelW,
                Height = panelH,
                Fill = new SolidColorBrush(Color.FromArgb(190, 18, 18, 22)),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                RadiusX = 10,
                RadiusY = 10
            };
            if (alignRight) Canvas.SetRight(panel, x); else Canvas.SetLeft(panel, x);
            Canvas.SetTop(panel, y);
            canvas.Children.Add(panel);

            // Player label
            TextBlock playerLabel = new TextBlock
            {
                Text = $"Player {playerNum}",
                FontSize = 14,
                Foreground = tank.Color == B5_Shellshock_TankColor.Red ? Brushes.Red : Brushes.Blue,
                FontWeight = FontWeights.Bold
            };
            if (alignRight) Canvas.SetRight(playerLabel, x + 10); else Canvas.SetLeft(playerLabel, x + 10);
            Canvas.SetTop(playerLabel, y + 8);
            canvas.Children.Add(playerLabel);

            // Health bar
            DrawHealthBar(canvas, tank, x + 10, y + 28, alignRight);

            // Angle display (inverted for player 2 for display consistency)
            double displayAngle = playerNum == 2 ? 180 - tank.Angle : tank.Angle;
            TextBlock angleText = new TextBlock
            {
                Text = $"Angle: {displayAngle:F0}°",
                FontSize = 12,
                Foreground = Brushes.White
            };
            if (alignRight) Canvas.SetRight(angleText, x + 10); else Canvas.SetLeft(angleText, x + 10);
            Canvas.SetTop(angleText, y + 48);
            canvas.Children.Add(angleText);

            // Power display
            DrawPowerBar(canvas, tank, x + 10, y + 68, alignRight);
        }

        private void DrawHealthBar(Canvas canvas, B5_Shellshock_Tank tank, double x, double y, bool alignRight)
        {
            // Background bar
            Rectangle healthBg = new Rectangle
            {
                Width = 110,
                Height = 15,
                Fill = Brushes.DarkGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            if (alignRight) Canvas.SetRight(healthBg, x); else Canvas.SetLeft(healthBg, x);
            Canvas.SetTop(healthBg, y);
            canvas.Children.Add(healthBg);

            // Health bar (filled portion)
            double healthWidth = (tank.Health / 100.0) * 110;
            Rectangle healthBar = new Rectangle
            {
                Width = healthWidth,
                Height = 15,
                Fill = tank.Health > 50 ? Brushes.Green : (tank.Health > 25 ? Brushes.Orange : Brushes.Red)
            };
            if (alignRight) Canvas.SetRight(healthBar, x); else Canvas.SetLeft(healthBar, x);
            Canvas.SetTop(healthBar, y);
            canvas.Children.Add(healthBar);

            // Health text
            TextBlock healthText = new TextBlock
            {
                Text = $"{tank.Health}",
                FontSize = 10,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };
            if (alignRight) Canvas.SetRight(healthText, x + 40); else Canvas.SetLeft(healthText, x + 40);
            Canvas.SetTop(healthText, y + 2);
            canvas.Children.Add(healthText);
        }

        private void DrawPowerBar(Canvas canvas, B5_Shellshock_Tank tank, double x, double y, bool alignRight)
        {
            // Background bar
            Rectangle powerBg = new Rectangle
            {
                Width = 110,
                Height = 15,
                Fill = Brushes.DarkGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            if (alignRight) Canvas.SetRight(powerBg, x); else Canvas.SetLeft(powerBg, x);
            Canvas.SetTop(powerBg, y);
            canvas.Children.Add(powerBg);

            // Power bar (filled portion)
            double powerWidth = (tank.Power / 100.0) * 110;
            Rectangle powerBar = new Rectangle
            {
                Width = powerWidth,
                Height = 15,
                Fill = Brushes.Yellow
            };
            if (alignRight) Canvas.SetRight(powerBar, x); else Canvas.SetLeft(powerBar, x);
            Canvas.SetTop(powerBar, y);
            canvas.Children.Add(powerBar);

            // Power text
            TextBlock powerText = new TextBlock
            {
                Text = $"Power: {tank.Power:F0}",
                FontSize = 10,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            };
            if (alignRight) Canvas.SetRight(powerText, x + 5); else Canvas.SetLeft(powerText, x + 5);
            Canvas.SetTop(powerText, y + 2);
            canvas.Children.Add(powerText);
        }

        private void DrawWindAndMovesIndicator(Canvas canvas, double wind, int movesRemaining, int maxMoves, double centerX, double top)
        {
            string windText = $"Wind: {wind:F1}";
            string movesText = $"Moves: {movesRemaining}/{maxMoves}";

            // Fixed compact panel sizing for neat layout with slimmer borders
            double panelW = 140;
            double panelH = 66;
            double left = centerX - panelW / 2;

            Rectangle panel = new Rectangle
            {
                Width = panelW,
                Height = panelH,
                Fill = new SolidColorBrush(Color.FromArgb(190, 18, 18, 22)),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                RadiusX = 10,
                RadiusY = 10
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);

            // Wind label centered with contrast
            TextBlock windLabel = new TextBlock
            {
                Text = windText,
                FontSize = 16,
                Foreground = Brushes.Cyan,
                FontWeight = FontWeights.Bold,
                Width = panelW,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(windLabel, left);
            Canvas.SetTop(windLabel, top + 6);
            canvas.Children.Add(windLabel);

            // Arrow placed below label - shows direction only, not magnitude
            if (wind != 0)
            {
                double arrowLength = 50; // fixed length regardless of wind strength
                double arrowCenterY = top + 32;
                
                // Draw solid arrow from center outward in wind direction
                double arrowStartX = centerX;
                double arrowEndX = centerX + (wind > 0 ? arrowLength : -arrowLength);

                Line windArrow = new Line
                {
                    X1 = arrowStartX,
                    Y1 = arrowCenterY,
                    X2 = arrowEndX,
                    Y2 = arrowCenterY,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 3
                };
                canvas.Children.Add(windArrow);

                // Arrowhead
                Polygon head = new Polygon
                {
                    Fill = Brushes.Orange,
                    Points = new PointCollection
                    {
                        new Point(arrowEndX, arrowCenterY),
                        new Point(arrowEndX + (wind > 0 ? -9 : 9), arrowCenterY - 5),
                        new Point(arrowEndX + (wind > 0 ? -9 : 9), arrowCenterY + 5)
                    }
                };
                canvas.Children.Add(head);
            }

            // Movements label centered at bottom
            TextBlock moveLabel = new TextBlock
            {
                Text = movesText,
                FontSize = 16,
                Foreground = movesRemaining > 0 ? Brushes.LightGreen : Brushes.Red,
                FontWeight = FontWeights.Bold,
                Width = panelW,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(moveLabel, left);
            Canvas.SetTop(moveLabel, top + panelH - 24);
            canvas.Children.Add(moveLabel);
        }
    }
}
