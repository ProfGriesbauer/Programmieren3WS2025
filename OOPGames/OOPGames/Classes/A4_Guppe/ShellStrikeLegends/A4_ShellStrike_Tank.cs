using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    public class A4_ShellStrike_Tank
    {
        public int PlayerNumber { get; }
        public double X { get; private set; }
        public double TurretAngleDeg { get; private set; } = 45; // default angle
        public int Health { get; private set; } = 3; // simple health

        // Base geometry (unscaled)
        private const double BaseWidth = 40;
        private const double BaseHeight = 20;
        private const double BaseTurretLength = 30;

        // Visual and physics scale (applies to width/height/turret)
        public double Scale { get; set; } = 1.4; // slightly smaller than before

        public double Width => BaseWidth * Scale;
        public double Height => BaseHeight * Scale;
        public double TurretLength => BaseTurretLength * Scale;

        // Sprite support
        public string HullSpritePath { get; set; } = "Assets/ShellStrikeLegends/camo-tank-4.png";
        public string BarrelSpritePath { get; set; } = "Assets/ShellStrikeLegends/camo-tank-barrel.png";
        // Cached loaded images (null if not loaded or failed)
        public ImageSource HullImage { get; private set; }
        public ImageSource BarrelImage { get; private set; }
        // Sprite geometry meta (pixels in sprite coordinate system)
        public Point HullPivot { get; set; } = new Point(20, 10); // center-bottom approx
        // Socket finetune (pixels from hull bottom), actual X is centered at runtime
        public double HullSocketYOffsetPx { get; set; } = 20;
        public Point BarrelPivot { get; set; } = new Point(5, 5);  // rotation origin on barrel image
        public Point BarrelMuzzleOffset { get; set; } = new Point(50, 5); // tip of barrel in sprite coords
        // Vertical adjustment (in world pixels after scaling) to lift the barrel so it sits in the turret
        public double BarrelYOffset { get; set; } = -10; // negative raises the barrel
        // When <= 0, painter will use overall Scale
        public double HullScale { get; set; } = 0.0;  // per-sprite override
        public double BarrelScale { get; set; } = 0.0;

        public A4_ShellStrike_Tank(int playerNumber, double startX)
        {
            PlayerNumber = playerNumber;
            X = startX;
        }

        public void EnsureSpritesLoaded()
        {
            if (HullImage == null && !string.IsNullOrWhiteSpace(HullSpritePath))
                HullImage = TryLoadImage(HullSpritePath);
            if (BarrelImage == null && !string.IsNullOrWhiteSpace(BarrelSpritePath))
                BarrelImage = TryLoadImage(BarrelSpritePath);
        }

        private static ImageSource TryLoadImage(string relativePath)
        {
            // Try as absolute file under output directory first
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string full = System.IO.Path.Combine(baseDir, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(full, UriKind.Absolute);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();
                    return bi;
                }
            }
            catch { }

            // Try pack URI (requires Build Action: Resource)
            try
            {
                var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = uri;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch { }

            return null;
        }

        public void Move(double delta, double minX, double maxX)
        {
            X = Math.Max(minX, Math.Min(maxX, X + delta));
        }

        public void AdjustTurret(double deltaDeg)
        {
            TurretAngleDeg = Math.Max(5, Math.Min(85, TurretAngleDeg + deltaDeg));
        }

        public A4_ShellStrike_Projectile Fire(double floorY)
        {
            // Fire projectile from the end of the turret
            double rad = TurretAngleDeg * Math.PI / 180.0;
            double sign = PlayerNumber == 1 ? 1.0 : -1.0;
            double cx = X + Width / 2.0;            // turret base center x
            double cy = floorY - Height;            // turret base y (top of tank)
            double tipX = cx + TurretLength * Math.Cos(rad) * sign; // turret end x
            double tipY = cy - TurretLength * Math.Sin(rad);        // turret end y
            double speed = 12.0;
            double vx = speed * Math.Cos(rad) * sign;               // horizontal velocity with direction
            double vy = -speed * Math.Sin(rad);                     // upward negative y
            return new A4_ShellStrike_Projectile(tipX, tipY, vx, vy);
        }

        public void Hit()
        {
            if (Health > 0) Health--;
        }
    }
}
