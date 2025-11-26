using System;
using System.Collections.Generic;
using OOPGames.Classes.A4_Guppe.ShellStrikeLegends;

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

            var rnd = seed.HasValue ? new Random(seed.Value) : (Config.TerrainSeed.HasValue ? new Random(Config.TerrainSeed.Value) : new Random());

            // Parameters
            int minY = (int)(CanvasHeight * Config.TerrainMinFraction);
            int maxY = (int)(CanvasHeight * Config.TerrainMaxFraction);
            int baselineY = (int)(CanvasHeight * Config.TerrainBaseline);
            baselineY = Math.Clamp(baselineY, minY, maxY);

                int rough = Math.Max(0, Config.TerrainRoughnessPx);
                if (Config.Mode == Config.TerrainMode.RandomHills)
            {
                    // Initialize baseline
                    for (int x = 0; x < CanvasWidth; x++) Heights[x] = baselineY;
                    // Multi-octave value noise (simple) for rolling variation
                    int octaves = Math.Max(1, Config.TerrainNoiseOctaves);
                    double persistence = Math.Clamp(Config.TerrainNoisePersistence, 0.2, 0.9);
                    double baseFreq = Math.Max(0.2, Config.TerrainNoiseFrequencyBase);
                    for (int o = 0; o < octaves; o++)
                    {
                        double freq = baseFreq * Math.Pow(2, o);
                        int period = Math.Max(2, (int)(CanvasWidth / freq));
                        double amp = rough * Math.Pow(persistence, o);
                        // Generate random values at control points
                        int pointCount = (CanvasWidth + period - 1) / period + 2;
                        double[] pv = new double[pointCount];
                        for (int i = 0; i < pointCount; i++) pv[i] = rnd.NextDouble() * 2 - 1; // [-1,1]
                        for (int x = 0; x < CanvasWidth; x++)
                        {
                            int idx = x / period;
                            double t = (x % period) / (double)period;
                            double v0 = pv[idx];
                            double v1 = pv[idx + 1];
                            // smoothstep interpolation
                            double tt = t * t * (3 - 2 * t);
                            double v = v0 + (v1 - v0) * tt;
                            Heights[x] = Math.Clamp((int)Math.Round(Heights[x] + v * amp), minY, maxY);
                        }
                    }
                    // Feature injection: peaks (raise visually -> y smaller) and valleys (lower visually -> y larger)
                    int peakCount = rnd.Next(Config.TerrainPeakMin, Config.TerrainPeakMax + 1);
                    int valleyCount = rnd.Next(Config.TerrainValleyMin, Config.TerrainValleyMax + 1);
                    double fallPow = Math.Max(0.5, Config.TerrainFeatureFalloffPow);
                    for (int p = 0; p < peakCount; p++)
                    {
                        int cx = rnd.Next(20, CanvasWidth - 20);
                        int wPeak = Config.TerrainPeakWidthPx;
                        int amp = Config.TerrainPeakAmplitudePx;
                        for (int x = Math.Max(0, cx - wPeak); x <= Math.Min(CanvasWidth - 1, cx + wPeak); x++)
                        {
                            double d = Math.Abs(x - cx) / (double)wPeak;
                            double shape = Math.Pow(1 - d, fallPow); // decays to 0 at edges
                            int delta = (int)(-amp * shape); // subtract => smaller y => higher peak
                            Heights[x] = Math.Clamp(Heights[x] + delta, minY, maxY);
                        }
                    }
                    for (int v = 0; v < valleyCount; v++)
                    {
                        int cx = rnd.Next(20, CanvasWidth - 20);
                        int wValley = Config.TerrainValleyWidthPx;
                        int amp = Config.TerrainValleyDepthPx;
                        for (int x = Math.Max(0, cx - wValley); x <= Math.Min(CanvasWidth - 1, cx + wValley); x++)
                        {
                            double d = Math.Abs(x - cx) / (double)wValley;
                            double shape = Math.Pow(1 - d, fallPow);
                            int delta = (int)(amp * shape); // add => larger y => deeper valley
                            Heights[x] = Math.Clamp(Heights[x] + delta, minY, maxY);
                        }
                    }
            }

            // Inject sharp edges (peaks/valleys) before smoothing if enabled (only for RandomHills)
            if (Config.Mode == Config.TerrainMode.RandomHills && Config.TerrainEnableSharpEdges && CanvasWidth > 4)
                if (Config.Mode == Config.TerrainMode.RandomHills && Config.TerrainEnableSharpEdges && CanvasWidth > 4)
            {
                for (int x = 2; x < CanvasWidth - 2; x++)
                {
                    if (rnd.NextDouble() < Config.TerrainSharpEdgeProbability)
                    {
                        bool makePeak = rnd.Next(2) == 0; // peak or valley
                        int delta = makePeak ? -Config.TerrainSharpEdgePeakHeightPx : Config.TerrainSharpEdgeValleyDepthPx;
                        int newY = Math.Clamp(Heights[x] + delta, minY, maxY);
                        Heights[x] = newY;
                        // Accentuation: optionally pull immediate neighbors halfway toward original to steepen edge
                        int left = x - 1, right = x + 1;
                        if (left >= 0) Heights[left] = Math.Clamp(Heights[left] + delta / 4, minY, maxY);
                        if (right < CanvasWidth) Heights[right] = Math.Clamp(Heights[right] + delta / 4, minY, maxY);
                    }
                }
            }

            // Edge-aware smoothing: blend neighbors unless large discontinuity should be preserved
            int win = Math.Max(3, Config.TerrainSmoothWindow);
            if (win % 2 == 0) win += 1; // make odd
            int half = win / 2;
            int[] smoothed = new int[CanvasWidth];
            for (int x = 0; x < CanvasWidth; x++)
            {
                int sum = 0;
                int count = 0;
                for (int k = -half; k <= half; k++)
                {
                    int xi = x + k;
                    if (xi < 0 || xi >= CanvasWidth) continue;
                    int hCenter = Heights[x];
                    int hNeighbor = Heights[xi];
                    int diff = Math.Abs(hNeighbor - hCenter);
                        if (diff >= Config.TerrainSharpEdgePreserveThresholdPx && xi != x)
                    {
                        // Preserve hard edge: skip neighbor contribution
                        continue;
                    }
                    sum += hNeighbor;
                    count++;
                }
                int y = count > 0 ? sum / count : Heights[x];
                smoothed[x] = Math.Clamp(y, minY, maxY);
            }
            Heights = smoothed;
        }

        public int GroundYAt(double x)
        {
            if (Heights.Length == 0) return CanvasHeight; // default bottom
            // Bilinear-like interpolation between neighboring columns for smoothness
            if (x <= 0) return Heights[0];
            if (x >= Heights.Length - 1) return Heights[Heights.Length - 1];
            int x0 = (int)Math.Floor(x);
            int x1 = x0 + 1;
            double t = x - x0;
            double y = Heights[x0] + (Heights[x1] - Heights[x0]) * t;
            return (int)Math.Round(y);
        }
    }
}
