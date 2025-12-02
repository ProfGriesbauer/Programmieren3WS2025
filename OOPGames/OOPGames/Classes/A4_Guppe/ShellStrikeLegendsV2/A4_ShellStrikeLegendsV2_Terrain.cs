using System;

namespace OOPGames
{
    // Provides a fixed, deterministic sine-wave terrain profile for a given canvas size
    public class A4_ShellStrikeLegendsV2_Terrain
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public int[] Heights { get; private set; } = Array.Empty<int>();

        // Build a smooth sine-ground similar to the reference image
        // y = baseline - amplitude * sin(2π * cycles * x/width)
        public void BuildFixedSine(int width, int height, double baselineFraction = 0.80, double amplitudeFraction = 0.25, double cycles = 0.75)
        {
            CanvasWidth = Math.Max(1, width);
            CanvasHeight = Math.Max(1, height);
            Heights = new int[CanvasWidth];

            double baseline = baselineFraction * CanvasHeight;
            double amplitude = amplitudeFraction * CanvasHeight;
            for (int x = 0; x < CanvasWidth; x++)
            {
                double t = (double)x / Math.Max(1, CanvasWidth - 1);
                double y = baseline - amplitude * Math.Sin(2.0 * Math.PI * cycles * t);
                // clamp to canvas
                int yi = (int)Math.Round(Math.Clamp(y, 0, CanvasHeight));
                Heights[x] = yi;
            }
            // End caps slight uplift to avoid vertical edges
            if (CanvasWidth >= 2)
            {
                Heights[0] = Math.Min(CanvasHeight, Heights[0] + (int)(0.03 * CanvasHeight));
                Heights[^1] = Math.Min(CanvasHeight, Heights[^1] + (int)(0.03 * CanvasHeight));
            }
        }

        public int GroundYAt(double x)
        {
            if (Heights.Length == 0) return CanvasHeight;
            if (x <= 0) return Heights[0];
            if (x >= Heights.Length - 1) return Heights[^1];
            int x0 = (int)Math.Floor(x);
            int x1 = x0 + 1;
            double t = x - x0;
            double y = Heights[x0] + (Heights[x1] - Heights[x0]) * t;
            return (int)Math.Round(y);
        }
    }
}
