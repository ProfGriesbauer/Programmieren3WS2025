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

            // Highlight active tank similar to B2 visibility emphasis (draw BEFORE tanks as background)
            B5_Shellshock_Tank activeTank = field.ActiveTankNumber == 1 ? field.Tank1 : field.Tank2;
            DrawActiveTankHalo(canvas, activeTank, scaleX, scaleY);

            // Draw tanks
            DrawTank(canvas, field.Tank1, scaleX, scaleY);
            DrawTank(canvas, field.Tank2, scaleX, scaleY);

            // Draw projectile if active
            if (field.Projectile != null && field.Projectile.IsActive)
            {
                System.Diagnostics.Debug.WriteLine($"Drawing projectile at ({field.Projectile.X}, {field.Projectile.Y})");
                DrawProjectile(canvas, field.Projectile, scaleX, scaleY);
            }

            // Draw UI
            DrawUI(canvas, field, canvasWidth, canvasHeight);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            // Repaint entire field for smooth animation
            PaintGameField(canvas, currentField);
        }

        private void DrawActiveTankHalo(Canvas canvas, B5_Shellshock_Tank tank, double scaleX, double scaleY)
        {
            if (!tank.IsAlive) return;
            double x = tank.X * scaleX;
            double y = tank.Y * scaleY;
            // Halo ellipse behind tank to indicate active turn (inspired by B2 view radius concept)
            Ellipse halo = new Ellipse
            {
                Width = 40,
                Height = 20,
                Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 0)),
                Stroke = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                StrokeThickness = 2
            };
            Canvas.SetLeft(halo, x - halo.Width / 2);
            Canvas.SetTop(halo, y - halo.Height / 2 - 5);
            canvas.Children.Add(halo);
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
                Width = 20,
                Height = 10,
                Fill = tank.Color == B5_Shellshock_TankColor.Red ? Brushes.Red : Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(body, x - 10);
            Canvas.SetTop(body, y - 10);
            canvas.Children.Add(body);

            // Tank barrel
            double barrelLength = 15;
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
                StrokeThickness = 3
            };
            canvas.Children.Add(barrel);

            // Angle indicator arc (small arc showing angle)
            DrawAngleIndicator(canvas, tank, x, y);
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
                Width = 8,
                Height = 8,
                Fill = Brushes.Black,
                Stroke = Brushes.Yellow,
                StrokeThickness = 1
            };
            Canvas.SetLeft(shell, x - 4);
            Canvas.SetTop(shell, y - 4);
            canvas.Children.Add(shell);
        }

        private void DrawUI(Canvas canvas, B5_Shellshock_Field field, double canvasWidth, double canvasHeight)
        {
            // Draw tank 1 info (left side)
            DrawTankInfo(canvas, field.Tank1, 1, 10, 10);

            // Draw tank 2 info (right side)
            DrawTankInfo(canvas, field.Tank2, 2, canvasWidth - 150, 10);

            // Draw wind indicator (center top)
            DrawWindIndicator(canvas, field.Wind, canvasWidth / 2, 10);

            // Draw game status
            string gameStatus;
            Brush statusColor;
            
            if (!field.Tank1.IsAlive)
            {
                gameStatus = "PLAYER 2 WINS!";
                statusColor = Brushes.Blue;
            }
            else if (!field.Tank2.IsAlive)
            {
                gameStatus = "PLAYER 1 WINS!";
                statusColor = Brushes.Red;
            }
            else
            {
                gameStatus = "SHELLSHOCK";
                statusColor = Brushes.White;
            }

            TextBlock statusText = new TextBlock
            {
                Text = gameStatus,
                FontSize = 20,
                Foreground = statusColor,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(10, 5, 10, 5)
            };
            Canvas.SetLeft(statusText, canvasWidth / 2 - 80);
            Canvas.SetTop(statusText, 40);
            canvas.Children.Add(statusText);

            // Draw movements remaining indicator
            TextBlock movementsText = new TextBlock
            {
                Text = $"Movements: {field.MovementsRemaining}/5",
                FontSize = 16,
                Foreground = field.MovementsRemaining > 0 ? Brushes.LightGreen : Brushes.Red,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(10, 5, 10, 5)
            };
            Canvas.SetLeft(movementsText, canvasWidth / 2 - 70);
            Canvas.SetTop(movementsText, 70);
            canvas.Children.Add(movementsText);
        }

        private void DrawTankInfo(Canvas canvas, B5_Shellshock_Tank tank, int playerNum, double x, double y)
        {
            // Player label
            TextBlock playerLabel = new TextBlock
            {
                Text = $"Player {playerNum}",
                FontSize = 14,
                Foreground = tank.Color == B5_Shellshock_TankColor.Red ? Brushes.Red : Brushes.Blue,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(playerLabel, x);
            Canvas.SetTop(playerLabel, y);
            canvas.Children.Add(playerLabel);

            // Health bar
            DrawHealthBar(canvas, tank, x, y + 20);

            // Angle display
            TextBlock angleText = new TextBlock
            {
                Text = $"Angle: {tank.Angle:F0}°",
                FontSize = 12,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(angleText, x);
            Canvas.SetTop(angleText, y + 40);
            canvas.Children.Add(angleText);

            // Power display
            DrawPowerBar(canvas, tank, x, y + 60);
        }

        private void DrawHealthBar(Canvas canvas, B5_Shellshock_Tank tank, double x, double y)
        {
            // Background bar
            Rectangle healthBg = new Rectangle
            {
                Width = 100,
                Height = 15,
                Fill = Brushes.DarkGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(healthBg, x);
            Canvas.SetTop(healthBg, y);
            canvas.Children.Add(healthBg);

            // Health bar (filled portion)
            double healthWidth = (tank.Health / 100.0) * 100;
            Rectangle healthBar = new Rectangle
            {
                Width = healthWidth,
                Height = 15,
                Fill = tank.Health > 50 ? Brushes.Green : (tank.Health > 25 ? Brushes.Orange : Brushes.Red)
            };
            Canvas.SetLeft(healthBar, x);
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
            Canvas.SetLeft(healthText, x + 40);
            Canvas.SetTop(healthText, y + 2);
            canvas.Children.Add(healthText);
        }

        private void DrawPowerBar(Canvas canvas, B5_Shellshock_Tank tank, double x, double y)
        {
            // Background bar
            Rectangle powerBg = new Rectangle
            {
                Width = 100,
                Height = 15,
                Fill = Brushes.DarkGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(powerBg, x);
            Canvas.SetTop(powerBg, y);
            canvas.Children.Add(powerBg);

            // Power bar (filled portion)
            double powerWidth = (tank.Power / 100.0) * 100;
            Rectangle powerBar = new Rectangle
            {
                Width = powerWidth,
                Height = 15,
                Fill = Brushes.Yellow
            };
            Canvas.SetLeft(powerBar, x);
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
            Canvas.SetLeft(powerText, x + 5);
            Canvas.SetTop(powerText, y + 2);
            canvas.Children.Add(powerText);
        }

        private void DrawWindIndicator(Canvas canvas, double wind, double x, double y)
        {
            TextBlock windText = new TextBlock
            {
                Text = $"Wind: {wind:F1}",
                FontSize = 14,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(windText, x - 40);
            Canvas.SetTop(windText, y);
            canvas.Children.Add(windText);

            // Wind arrow
            double arrowLength = Math.Abs(wind) * 3;
            double arrowX = wind > 0 ? x + 40 : x - 40 - arrowLength;

            Line windArrow = new Line
            {
                X1 = arrowX,
                Y1 = y + 7,
                X2 = arrowX + (wind > 0 ? arrowLength : -arrowLength),
                Y2 = y + 7,
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            canvas.Children.Add(windArrow);

            // Arrow head
            if (wind != 0)
            {
                Polygon arrowHead = new Polygon
                {
                    Fill = Brushes.White,
                    Points = new PointCollection
                    {
                        new Point(windArrow.X2, windArrow.Y2),
                        new Point(windArrow.X2 + (wind > 0 ? -5 : 5), windArrow.Y2 - 3),
                        new Point(windArrow.X2 + (wind > 0 ? -5 : 5), windArrow.Y2 + 3)
                    }
                };
                canvas.Children.Add(arrowHead);
            }
        }
    }
}
