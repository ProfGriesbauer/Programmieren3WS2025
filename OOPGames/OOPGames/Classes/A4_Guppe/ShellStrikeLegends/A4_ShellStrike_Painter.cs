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
                    // Use Config-driven seed if defined
                    field.Terrain.Generate(wi, hi, OOPGames.Classes.A4_Guppe.ShellStrikeLegends.Config.TerrainSeed);
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

                // Draw slope-aligned tanks with wheel contact logic
                if (field.Tank1 != null) DrawTankSlopeAligned(canvas, field, field.Tank1);
                if (field.Tank2 != null) DrawTankSlopeAligned(canvas, field, field.Tank2);

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
            // Try sprite-based rendering
            tank.EnsureSpritesLoaded();
            bool hasHull = tank.HullImage != null;
            bool hasBarrel = tank.BarrelImage != null;

            double cx = tank.X + tank.Width / 2.0;
            double cy = baseY;
            double rad = tank.TurretAngleDeg * Math.PI / 180.0;
            double dir = (tank.Facing >= 0 ? 1.0 : -1.0);
            // optional turret socket world coords (unused in legacy DrawTank)

            if (hasHull)
            {
                var hullImg = new Image { Source = tank.HullImage };
                var hullBmp = tank.HullImage as System.Windows.Media.Imaging.BitmapSource;
                double hullW = hullBmp != null ? hullBmp.PixelWidth : 1.0;
                double hullH = hullBmp != null ? hullBmp.PixelHeight : 1.0;
                // Scale hull to desired tank size relative to sprite pivot
                double scale = tank.HullScale > 0 ? tank.HullScale : tank.Scale;
                hullImg.RenderTransformOrigin = new Point(
                    tank.HullPivot.X / hullW,
                    tank.HullPivot.Y / hullH);
                var hullGroup = new TransformGroup();
                hullGroup.Children.Add(new ScaleTransform(dir * scale, scale)); // mirror for player 2
                hullImg.RenderTransform = hullGroup;
                // Position so that pivot sits at (cx, cy)
                double left = cx - tank.HullPivot.X * scale;
                double top = cy - tank.HullPivot.Y * scale;
                Canvas.SetLeft(hullImg, left);
                Canvas.SetTop(hullImg, top);
                canvas.Children.Add(hullImg);
            }
            else
            {
                // Rectangle fallback for hull
                Rectangle body = new Rectangle
                {
                    Width = tank.Width,
                    Height = tank.Height,
                    Fill = tank.PlayerNumber == 1 ? Brushes.DarkGreen : Brushes.DarkRed
                };
                Canvas.SetLeft(body, tank.X);
                Canvas.SetTop(body, baseY);
                canvas.Children.Add(body);
            }

            if (hasBarrel)
            {
                var barrelImg = new Image { Source = tank.BarrelImage };
                var barrelBmp = tank.BarrelImage as System.Windows.Media.Imaging.BitmapSource;
                double barrelW = barrelBmp != null ? barrelBmp.PixelWidth : 1.0;
                double barrelH = barrelBmp != null ? barrelBmp.PixelHeight : 1.0;
                double bScale = tank.BarrelScale > 0 ? tank.BarrelScale : tank.Scale;
                barrelImg.RenderTransformOrigin = new Point(
                    tank.BarrelPivot.X / barrelW,
                    tank.BarrelPivot.Y / barrelH);
                var barrelGroup = new TransformGroup();
                // Scale; do NOT mirror, rotation will handle left/right orientation
                barrelGroup.Children.Add(new ScaleTransform(bScale, bScale));
                // Compute rotation from the actual turret direction vector used by fallback line
                rad = tank.TurretAngleDeg * Math.PI / 180.0;
                double tx = cx + tank.TurretLength * Math.Cos(rad) * dir;
                double ty = cy - tank.TurretLength * Math.Sin(rad);
                double deg = Math.Atan2(ty - cy, tx - cx) * 180.0 / Math.PI;
                barrelGroup.Children.Add(new RotateTransform(deg));
                barrelImg.RenderTransform = barrelGroup;
                // Place pivot at (cx, cy)
                double bLeft = cx - tank.BarrelPivot.X * bScale;
                double bTop = cy - tank.BarrelPivot.Y * bScale;
                Canvas.SetLeft(barrelImg, bLeft);
                Canvas.SetTop(barrelImg, bTop);
                canvas.Children.Add(barrelImg);
            }
            else
            {
                // Line fallback for turret
                double tx = cx + tank.TurretLength * Math.Cos(rad) * dir;
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
            }

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
//IMPLEMENTATION NOTE: The following method aligns the tank to the terrain slope based on wheel contact points.
        private void DrawTankSlopeAligned(Canvas canvas, A4_ShellStrike_Field field, A4_ShellStrike_Tank tank)
        {
            if (tank == null || field?.Terrain == null) { return; }
            tank.EnsureSpritesLoaded();
            bool hasHull = tank.HullImage != null;
            bool hasBarrel = tank.BarrelImage != null;
            double dir = (tank.Facing >= 0 ? 1.0 : -1.0);

            // Wheel sample positions (outermost wheels at left/right edges)
            double leftWheelX = tank.X;
            double rightWheelX = tank.X + tank.Width;
            // Use interpolated terrain sampling for smoother contact
            double groundLeftY = field.Terrain.GroundYAt(leftWheelX);
            double groundRightY = field.Terrain.GroundYAt(rightWheelX);

            // Slope of terrain between wheels (y increases downward)
            double slopeRad = Math.Atan2(groundRightY - groundLeftY, rightWheelX - leftWheelX);
            double slopeDeg = slopeRad * 180.0 / Math.PI;

            // Determine anchor side. Default: earliest contact (higher terrain -> smaller y)
            bool anchorLeft = groundLeftY <= groundRightY;
            // Ridge snap: if more than half the tank spans beyond a local peak between wheels,
            // snap anchor to the far side to avoid clipping.
            int peakSamples = 17;
            double spanX = Math.Max(1, rightWheelX - leftWheelX);
            double peakY = double.MinValue;
            double peakX = leftWheelX;
            for (int i = 0; i <= peakSamples; i++)
            {
                double t = i / (double)peakSamples;
                double sx = leftWheelX + t * spanX;
                double sy = field.Terrain.GroundYAt(sx);
                if (sy > peakY) { peakY = sy; peakX = sx; }
            }
            // How much of the hull is over the peak to the right/left
            double overRight = Math.Max(0, rightWheelX - peakX);
            double overLeft = Math.Max(0, peakX - leftWheelX);
            double half = tank.Width * 0.5;
            if (overRight >= half) anchorLeft = false;
            else if (overLeft >= half) anchorLeft = true;
            double anchorX = anchorLeft ? leftWheelX : rightWheelX;
            double anchorY = anchorLeft ? groundLeftY : groundRightY;

            double hullScale = (tank.HullScale > 0 ? tank.HullScale : tank.Scale);
            Image hullImg = null;
            double hullPivotPixelX = 0, hullPivotPixelY = 0, hullW = 1, hullH = 1;

            if (hasHull)
            {
                hullImg = new Image { Source = tank.HullImage };
                var bmp = tank.HullImage as System.Windows.Media.Imaging.BitmapSource;
                hullW = bmp != null ? bmp.PixelWidth : 1.0;
                hullH = bmp != null ? bmp.PixelHeight : 1.0;
                bool mirrored = dir < 0;
                // Compute full matrix: Translate(-pivot) -> Scale(mirror*scale, scale) -> Rotate(slope) -> Translate(anchorX, anchorY)
                hullPivotPixelX = anchorLeft ? (mirrored ? hullW : 0) : (mirrored ? 0 : hullW);
                // Use calibrated ground line if provided, else fallback to HullPivot.Y, else sprite bottom
                double groundPivotY = tank.HullGroundPivotYPx > 0 ? tank.HullGroundPivotYPx : (tank.HullPivot.Y > 0 ? tank.HullPivot.Y : hullH);
                hullPivotPixelY = groundPivotY;
                var tg = new TransformGroup();
                //tg.Children.Add(new TranslateTransform(-hullPivotPixelX, -hullPivotPixelY));
                tg.Children.Add(new ScaleTransform(dir * hullScale, hullScale));
                tg.Children.Add(new RotateTransform(slopeDeg));
                tg.Children.Add(new TranslateTransform(anchorX, anchorY));
                hullImg.RenderTransform = tg;
                // Place at origin; transform moves it to final position
                Canvas.SetLeft(hullImg, 0);
                Canvas.SetTop(hullImg, 0);
                // Compute world turret base by transforming a socket point
                // Socket X: symmetric middle of hull; Socket Y: slightly above bottom
                var socketLocal = new Point(hullW / 2.0, hullH -tank.HullSocketYOffsetPx);
                var socketWorld = tg.Value.Transform(socketLocal);
                double socketBaseX = socketWorld.X;
                double socketBaseY = socketWorld.Y;
                // legacy DrawTank does not use socket attachment
                // We add hull AFTER barrel so raise its ZIndex
            }
            else
            {
                // Fallback rectangle with rotation about anchor wheel
                var rect = new Rectangle
                {
                    Width = tank.Width,
                    Height = tank.Height,
                    Fill = tank.PlayerNumber == 1 ? Brushes.DarkGreen : Brushes.DarkRed
                };
                Canvas.SetLeft(rect, anchorX - (anchorLeft ? 0 : tank.Width));
                Canvas.SetTop(rect, anchorY - tank.Height);
                rect.RenderTransformOrigin = new Point(anchorLeft ? 0 : 1, 1);
                rect.RenderTransform = new RotateTransform(slopeDeg);
                hullImg = null; // using rect instead
                canvas.Children.Add(rect);
                Canvas.SetZIndex(rect, 10); // ensure body above barrel
                hullH = tank.Height / hullScale; // approximate for turret placement
            }

            // Turret base: from hull socket if sprite available; otherwise from midpoint approximation
            // socketBaseX/Y already set if hull sprite path was used
    double centerX = (leftWheelX + rightWheelX) / 2.0;
    double tFactor = (centerX - leftWheelX) / (rightWheelX - leftWheelX);
    double bottomCenterY = groundLeftY + (groundRightY - groundLeftY) * tFactor;
    double hullHeightWorld = hullScale * hullH; // scaled sprite pixel height (or fallback height)
    double cxWorld;
    double cyWorld;
    if (hasHull)
    {
        var socketLocal2 = new Point(hullW / 2.0, hullH - tank.HullSocketYOffsetPx);
        var socketWorld2 = ((TransformGroup)hullImg.RenderTransform).Value.Transform(socketLocal2);
        cxWorld = socketWorld2.X;
        cyWorld = socketWorld2.Y;
    }
    else
    {
        cxWorld = centerX;
        cyWorld = bottomCenterY - hullHeightWorld;
    }

            // Turret direction (world) using original angle relative to horizontal
            double turretRad = tank.TurretAngleDeg * Math.PI / 180.0;
            double vx = Math.Cos(turretRad) * (tank.Facing >= 0 ? 1 : -1);
            double vy = -Math.Sin(turretRad);
            // Apply barrel vertical offset (raise/lower within turret)
            cyWorld += tank.BarrelYOffset;
            double tipX = cxWorld + vx * tank.TurretLength;
            double tipY = cyWorld + vy * tank.TurretLength;

            if (hasBarrel)
            {
                var barrelImg = new Image { Source = tank.BarrelImage };
                var bbmp = tank.BarrelImage as System.Windows.Media.Imaging.BitmapSource;
                double bW = bbmp != null ? bbmp.PixelWidth : 1.0;
                double bH = bbmp != null ? bbmp.PixelHeight : 1.0;
                double bScale = tank.BarrelScale > 0 ? tank.BarrelScale : tank.Scale;
                // Build transform to rotate around WORLD turret base (cxWorld, cyWorld)
                var btg = new TransformGroup();
                // 1) Scale in local space
                btg.Children.Add(new ScaleTransform(bScale, bScale));
                // 2) Translate so the local pivot (barrel base pixel) is at origin
                btg.Children.Add(new TranslateTransform(-tank.BarrelPivot.X, -tank.BarrelPivot.Y));
                // 3) Rotate around origin
                double barrelDeg = Math.Atan2(tipY - cyWorld, tipX - cxWorld) * 180.0 / Math.PI;
                btg.Children.Add(new RotateTransform(barrelDeg));
                // 4) Translate to world turret base position
                btg.Children.Add(new TranslateTransform(cxWorld, cyWorld));
                barrelImg.RenderTransform = btg;
                // Place at (0,0) since transform handles final position
                Canvas.SetLeft(barrelImg, 0);
                Canvas.SetTop(barrelImg, 0);
                canvas.Children.Add(barrelImg);
                Canvas.SetZIndex(barrelImg, 5); // behind hull
            }
            else
            {
                Line turretLine = new Line
                {
                    X1 = cxWorld,
                    Y1 = cyWorld,
                    X2 = tipX,
                    Y2 = tipY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 3
                };
                canvas.Children.Add(turretLine);
                Canvas.SetZIndex(turretLine, 5);
            }

            // Add hull sprite last if sprite exists so ZIndex works
            if (hullImg != null)
            {
                canvas.Children.Add(hullImg);
                Canvas.SetZIndex(hullImg, 10);
            }

            // HP label
            TextBlock hp = new TextBlock
            {
                Text = $"HP:{tank.Health}",
                Foreground = Brushes.Black,
                FontSize = 12
            };
            Canvas.SetLeft(hp, cxWorld - 18);
            Canvas.SetTop(hp, cyWorld - 22);
            canvas.Children.Add(hp);
        }
    }
}
