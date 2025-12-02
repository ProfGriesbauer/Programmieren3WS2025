using System.Drawing;

namespace OOPGames.Classes.A4_Guppe.ShellStrikeLegends
{
    public static class Config
    {
        public static int FieldWidth = 800;
        public static int FieldHeight = 400;

        // Terrain generation parameters
        // Baseline height as a fraction of canvas height (0-1)
        public static double TerrainBaseline = 0.75;
        // Allowed vertical range [min,max] as fractions of canvas height
        public static double TerrainMinFraction = 0.35;
        public static double TerrainMaxFraction = 0.9;
        // Coarse feature spacing in pixels (bigger => broader hills)
        public static int TerrainFeatureSpanPx = 120;
        // Roughness amplitude in pixels (peak deviation from baseline)
        public static int TerrainRoughnessPx = 60;
        // Smoothing window size (odd number recommended)
        public static int TerrainSmoothWindow = 9;
        // Optional fixed seed for reproducibility; set null for random
        public static int? TerrainSeed = null;

        // Terrain mode
        public enum TerrainMode { RandomHills, Plateau }
        public static TerrainMode Mode = TerrainMode.RandomHills; // revert default to RandomHills

        // Sharp edge generation parameters
        public static bool TerrainEnableSharpEdges = false;              // master switch for injecting sharp peaks/valleys
        public static double TerrainSharpEdgeProbability = 0.08;        // probability per column to become a sharp feature
        public static int TerrainSharpEdgePeakHeightPx = 40;            // upward (visual peak -> smaller y) adjustment magnitude
        public static int TerrainSharpEdgeValleyDepthPx = 40;           // downward (visual valley -> larger y) adjustment magnitude
        public static int TerrainSharpEdgePreserveThresholdPx = 28;     // minimum height difference to treat as hard edge (excluded from smoothing)

        // Advanced random terrain shaping (ShellShock-like)
        public static int TerrainNoiseOctaves = 3;              // number of noise layers
        public static double TerrainNoisePersistence = 0.5;      // amplitude falloff per octave
        public static double TerrainNoiseFrequencyBase = 1.2;    // base frequency factor
        public static int TerrainPeakMin = 2;                    // min number of peaks
        public static int TerrainPeakMax = 5;                    // max number of peaks
        public static int TerrainPeakAmplitudePx = 55;           // peak vertical lift (subtract from y)
        public static int TerrainPeakWidthPx = 90;               // horizontal influence radius
        public static int TerrainValleyMin = 1;                  // min number of valleys
        public static int TerrainValleyMax = 3;                  // max number of valleys
        public static int TerrainValleyDepthPx = 55;             // valley vertical depth (add to y)
        public static int TerrainValleyWidthPx = 90;             // horizontal influence radius
        public static double TerrainFeatureFalloffPow = 2.2;     // falloff exponent (higher = sharper center)

        // Crest snapping configuration
        public static int CrestSlopeThresholdPx = 4; // minimum drop to both sides to count as sharp crest
        public static int CrestSnapOffsetPx = 1;     // how many pixels to place the front wheel beyond the crest when snapping
    }
}
