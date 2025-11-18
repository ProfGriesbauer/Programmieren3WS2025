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
            // Simplified: Generate only flat terrain for now
            for (int i = 0; i < _width; i++)
            {
                _heightMap[i] = 0.8; // Flat at 80% of screen height
            }
            
            /* Original terrain generation - can be re-enabled later
            Random rand = new Random();
            
            switch (_terrainType)
            {
                case B5_Shellshock_TerrainType.Flat:
                    // Flat terrain with slight variations
                    for (int i = 0; i < _width; i++)
                    {
                        _heightMap[i] = 0.7 + (rand.NextDouble() * 0.05 - 0.025);
                    }
                    break;

                case B5_Shellshock_TerrainType.Hill:
                    // Hill in the middle
                    for (int i = 0; i < _width; i++)
                    {
                        double x = (double)i / _width;
                        _heightMap[i] = 0.9 - Math.Abs(x - 0.5) * 0.6;
                    }
                    break;

                case B5_Shellshock_TerrainType.Valley:
                    // Valley in the middle
                    for (int i = 0; i < _width; i++)
                    {
                        double x = (double)i / _width;
                        _heightMap[i] = 0.5 + Math.Abs(x - 0.5) * 0.6;
                    }
                    break;
            }
            */
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
            // Destructible terrain disabled for now
            // Just mark the hit but don't modify terrain
            return;
            
            /* Original destructible terrain code - disabled
            int centerX = (int)Math.Round(x);
            int radiusInt = (int)Math.Ceiling(radius);

            for (int i = centerX - radiusInt; i <= centerX + radiusInt; i++)
            {
                if (i >= 0 && i < _width)
                {
                    double distance = Math.Abs(i - centerX);
                    if (distance <= radius)
                    {
                        // Create crater: lower terrain in a circular pattern
                        double craterDepth = (radius - distance) / radius * 0.1;
                        _heightMap[i] = Math.Min(1.0, _heightMap[i] + craterDepth);
                    }
                }
            }
            */
        }
    }
}
