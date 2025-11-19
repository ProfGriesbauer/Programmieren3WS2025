using System;

namespace OOPGames
{
    // Represents the ground/battlefield. Stores height map for terrain. Can be damaged by explosions.
    public class B5_Shellshock_Terrain
    {
        private double[] _heightMap;
        private int _width;
        private B5_Shellshock_TerrainType _terrainType;

        public double[] HeightMap => _heightMap;
        public int Width => _width;
        public B5_Shellshock_TerrainType TerrainType => _terrainType;

        public B5_Shellshock_Terrain(int width, B5_Shellshock_TerrainType terrainType)
        {
            _width = width;
            _terrainType = terrainType;
            _heightMap = new double[width];
            GenerateTerrain();
        }

        private void GenerateTerrain()
        {
            Random rand = new Random();
            
            switch (_terrainType)
            {
                case B5_Shellshock_TerrainType.Flat:
                    // Plain terrain with very slight noise
                    for (int i = 0; i < _width; i++)
                    {
                        _heightMap[i] = 0.8 + (rand.NextDouble() * 0.01 - 0.005);
                    }
                    // Smooth to eliminate any sharp changes
                    SmoothTerrain(3);
                    break;

                case B5_Shellshock_TerrainType.Hill:
                    // Single hill with random angle and position
                    double hillCenter = 0.3 + rand.NextDouble() * 0.4; // Random center between 30-70%
                    double hillHeight = 0.15 + rand.NextDouble() * 0.15; // Random height
                    double hillWidth = 0.2 + rand.NextDouble() * 0.3; // Random width
                    double leftSlope = 0.5 + rand.NextDouble() * 1.5; // Random left slope steepness
                    double rightSlope = 0.5 + rand.NextDouble() * 1.5; // Random right slope steepness
                    
                    for (int i = 0; i < _width; i++)
                    {
                        double x = (double)i / _width;
                        double distFromCenter = Math.Abs(x - hillCenter);
                        
                        if (distFromCenter < hillWidth)
                        {
                            // On the hill - use different slopes for left/right
                            double slope = x < hillCenter ? leftSlope : rightSlope;
                            double normalizedDist = distFromCenter / hillWidth;
                            double hillFactor = Math.Pow(1 - normalizedDist, slope);
                            _heightMap[i] = 0.85 - hillHeight * hillFactor;
                        }
                        else
                        {
                            // Flat ground - no noise
                            _heightMap[i] = 0.85;
                        }
                    }
                    // Smooth to ensure gentle transitions
                    SmoothTerrain(5);
                    break;

                case B5_Shellshock_TerrainType.Curvy:
                    // Curvy landscape with gentle rolling hills - no sharp cliffs
                    double[] noise = new double[_width];
                    
                    // Generate smooth rolling terrain with reduced frequency
                    double phase1 = rand.NextDouble() * 10;
                    double phase2 = rand.NextDouble() * 10;
                    
                    for (int i = 0; i < _width; i++)
                    {
                        double value = 0;
                        double frequency = 0.008; // Reduced for gentler slopes
                        double amplitude = 0.12; // Reduced amplitude
                        
                        // Layer 1: Large rolling hills
                        value += Math.Sin(i * frequency * 2 + phase1) * amplitude;
                        
                        // Layer 2: Medium hills
                        value += Math.Sin(i * frequency * 4 + phase2) * amplitude * 0.4;
                        
                        noise[i] = value;
                    }
                    
                    // Apply multiple smoothing passes for very gentle terrain
                    for (int i = 0; i < _width; i++)
                    {
                        _heightMap[i] = 0.75 + noise[i];
                    }
                    
                    // Heavy smoothing to eliminate all sharp changes
                    SmoothTerrain(8);
                    
                    // Clamp to valid range
                    for (int i = 0; i < _width; i++)
                    {
                        _heightMap[i] = Math.Max(0.5, Math.Min(0.92, _heightMap[i]));
                    }
                    break;
                    
                case B5_Shellshock_TerrainType.Valley:
                    // Valley in the middle (kept for compatibility)
                    for (int i = 0; i < _width; i++)
                    {
                        double x = (double)i / _width;
                        _heightMap[i] = 0.5 + Math.Abs(x - 0.5) * 0.6;
                    }
                    break;
            }
        }

        private void SmoothTerrain(int passes)
        {
            // Apply multiple smoothing passes to eliminate sharp terrain changes
            for (int pass = 0; pass < passes; pass++)
            {
                double[] smoothed = new double[_width];
                for (int i = 0; i < _width; i++)
                {
                    if (i == 0)
                    {
                        smoothed[i] = (_heightMap[i] * 2 + _heightMap[i + 1]) / 3.0;
                    }
                    else if (i == _width - 1)
                    {
                        smoothed[i] = (_heightMap[i - 1] + _heightMap[i] * 2) / 3.0;
                    }
                    else
                    {
                        smoothed[i] = (_heightMap[i - 1] + _heightMap[i] * 2 + _heightMap[i + 1]) / 4.0;
                    }
                }
                _heightMap = smoothed;
            }
        }

        public double GetHeightAt(double x)
        {
            int index = (int)Math.Round(x);
            if (index < 0 || index >= _width)
                return 1.0; // Off screen = bottom

            return _heightMap[index];
        }

        public bool IsCollision(double x, double y)
        {
            if (x < 0 || x >= _width)
                return false;

            double terrainHeight = GetHeightAt(x);
            return y >= terrainHeight;
        }

        public void DestroyTerrain(double x, double radius)
        {
            // Create circular crater at impact point
            int centerX = (int)Math.Round(x);
            int radiusInt = (int)Math.Ceiling(radius);

            for (int i = centerX - radiusInt; i <= centerX + radiusInt; i++)
            {
                if (i >= 0 && i < _width)
                {
                    double distance = Math.Abs(i - centerX);
                    if (distance <= radius)
                    {
                        // Create circular crater by pushing terrain down
                        // Use circular falloff for smooth crater edges
                        double normalizedDist = distance / radius;
                        double craterDepth = (1 - normalizedDist) * 0.08; // Depth factor
                        _heightMap[i] = Math.Min(1.0, _heightMap[i] + craterDepth);
                    }
                }
            }
            
            // Smooth crater edges for natural appearance
            SmoothTerrain(2);
        }
    }
}
