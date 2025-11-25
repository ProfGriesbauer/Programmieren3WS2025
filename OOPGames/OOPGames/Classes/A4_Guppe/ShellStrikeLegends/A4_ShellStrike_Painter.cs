using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    public class A4_ShellStrike_Painter : IPaintGame, IPaintGame2
    {
        public string Name => "A4 ShellStrikeLegends Painter";
        // Cache terrain path to avoid recreating thousands of shapes every frame
        private Path _cachedTerrainPath;
        private int _cachedW;
        private int _cachedH;

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(135, 206, 235)); // SkyBlue

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0) w = 800; // fallback
            if (h <= 0) h = 400; // fallback

            // Shared terrain color
            var terrainBrush = new SolidColorBrush(Color.FromRgb(205, 133, 63)); // Peru

            // Draw tanks & projectiles if field has them (null-safe)
            if (currentField is A4_ShellStrike_Field field)
            {
                // Ensure terrain exists and matches canvas width
                int wi = (int)Math.Max(1, Math.Round(w));
                int hi = (int)Math.Max(1, Math.Round(h));
                if (field.Terrain == null || field.Terrain.Heights.Length != wi || field.Terrain.CanvasHeight != hi)
                {
                    field.Terrain = new A4_ShellStrike_Terrain();
                    field.Terrain.Generate(wi, hi);
                }

                // Rebuild cached terrain path only if size changed or no cache
                if (_cachedTerrainPath == null || _cachedW != wi || _cachedH != hi)
                {
                    _cachedW = wi;
                    _cachedH = hi;
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        // Start bottom-left
                        ctx.BeginFigure(new Point(0, hi), isFilled: true, isClosed: true);
                        // Go up to first column top
                        ctx.LineTo(new Point(0, field.Terrain.Heights[0]), true, false);
                        // Trace terrain silhouette
                        for (int x = 1; x < field.Terrain.Heights.Length; x++)
                        {
                            ctx.LineTo(new Point(x, field.Terrain.Heights[x]), true, false);
                        }
                        // Down to bottom-right
                        ctx.LineTo(new Point(wi - 1, hi), true, false);
                    }
                    geom.Freeze();
                    _cachedTerrainPath = new Path { Data = geom, Fill = terrainBrush, StrokeThickness = 0 };
                }
                canvas.Children.Add(_cachedTerrainPath);

                // Draw tanks aligned to terrain
                if (field.Tank1 != null)
                {
                    double t1cx = field.Tank1.X + field.Tank1.Width / 2.0;
                    double t1Ground = field.Terrain.GroundYAt(t1cx);
                    double t1BaseY = t1Ground - field.Tank1.Height;
                    DrawTank(canvas, field.Tank1, t1BaseY);
                }
                if (field.Tank2 != null)
                {
                    double t2cx = field.Tank2.X + field.Tank2.Width / 2.0;
                    double t2Ground = field.Terrain.GroundYAt(t2cx);
                    double t2BaseY = t2Ground - field.Tank2.Height;
                    DrawTank(canvas, field.Tank2, t2BaseY);
                }

                // Draw projectiles
                if (field.Projectiles != null)
                {
                    foreach (var proj in field.Projectiles)
                    {
                        proj?.Draw(canvas);
                    }
                }
            }
        }

        private void DrawTank(Canvas canvas, A4_ShellStrike_Tank tank, double baseY)
        {
            if (tank == null) return;
            // body
            Rectangle body = new Rectangle
            {
                Width = tank.Width,
                Height = tank.Height,
                Fill = tank.PlayerNumber == 1 ? Brushes.DarkGreen : Brushes.DarkRed
            };
            Canvas.SetLeft(body, tank.X);
            Canvas.SetTop(body, baseY);
            canvas.Children.Add(body);

            // turret line
            double cx = tank.X + tank.Width / 2.0;
            double cy = baseY;
            double rad = tank.TurretAngleDeg * Math.PI / 180.0;
            double tx = cx + tank.TurretLength * Math.Cos(rad) * (tank.PlayerNumber == 1 ? 1 : -1);
            double ty = cy - tank.TurretLength * Math.Sin(rad);
            Line turret = new Line
            {
                X1 = cx,
                Y1 = cy,
                X2 = tx,
                Y2 = ty,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            canvas.Children.Add(turret);

            // health text
            TextBlock hp = new TextBlock
            {
                Text = $"HP:{tank.Health}",
                Foreground = Brushes.Black,
                FontSize = 12
            };
            Canvas.SetLeft(hp, tank.X);
            Canvas.SetTop(hp, baseY - 18);
            canvas.Children.Add(hp);
        }

        // Continuous repaint support for timer-based animation
        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }
    }
}
