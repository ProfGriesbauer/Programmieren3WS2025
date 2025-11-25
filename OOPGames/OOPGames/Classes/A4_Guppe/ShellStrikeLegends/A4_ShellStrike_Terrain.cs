using System;

namespace OOPGames
{
    public class A4_ShellStrike_Terrain
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public int[] Heights { get; private set; } = Array.Empty<int>();

        public void Generate(int width, int height, int? seed = null)
        {
            CanvasWidth = Math.Max(1, width);
            CanvasHeight = Math.Max(1, height);
            Heights = new int[CanvasWidth];

            var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
            // baseline around 70% down the canvas
            int minY = (int)(CanvasHeight * 0.35);   // limit peaks
            int maxY = (int)(CanvasHeight * 0.9);    // limit valleys
            int y = (int)(CanvasHeight * 0.75);
            y = Math.Clamp(y, minY, maxY);

            for (int x = 0; x < CanvasWidth; x++)
            {
                if (x > 0)
                {
                    // random step between -2 and +2
                    int step = rnd.Next(-2, 3); // [-2, -1, 0, 1, 2]
                    // ensure no more than 2px change already guaranteed by step range
                    y = Math.Clamp(y + step, minY, maxY);
                }
                Heights[x] = y;
            }
        }

        public int GroundYAt(double x)
        {
            if (Heights.Length == 0) return CanvasHeight; // default bottom
            int xi = (int)Math.Round(x);
            if (xi < 0) xi = 0;
            if (xi >= Heights.Length) xi = Heights.Length - 1;
            return Heights[xi];
        }
    }
}
