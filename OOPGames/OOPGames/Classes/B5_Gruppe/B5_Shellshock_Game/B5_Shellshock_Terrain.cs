using System;

namespace OOPGames
{
    /// <summary>
    /// Represents the game terrain (battlefield ground).
    /// Uses Strategy Pattern via ITerrainGenerator for terrain generation.
    /// Supports destructible terrain via crater creation from projectile impacts.
    /// Uses height map for efficient collision detection and rendering.
    /// 
    /// OOP Concepts:
    /// - Komposition über Vererbung: Uses ITerrainGenerator instead of inheriting
    /// - Strategy Pattern: Terrain generation algorithm is pluggable
    /// - Kapselung: Height map is encapsulated, modified only via public methods
    /// 
    /// Invarianten:
    /// - _heightMap.Length == _width
    /// - All height values are in range [0.0, 1.0]
    /// - Width is immutable after construction
    /// </summary>
    public class B5_Shellshock_Terrain
    {
        private double[] _heightMap;          // Y-coordinates (normalized 0-1) for each X position
        private readonly int _width;          // Terrain width in pixels
        private readonly B5_Shellshock_TerrainType _terrainType;
        private readonly ITerrainGenerator _generator; // Strategy pattern

        public double[] HeightMap => _heightMap;
        public int Width => _width;
        public B5_Shellshock_TerrainType TerrainType => _terrainType;

        /// <summary>
        /// Creates terrain using the specified type.
        /// Uses Factory to create appropriate generator (Strategy Pattern).
        /// 
        /// Vorbedingung: width > 0
        /// Nachbedingung: Terrain generated and ready for gameplay
        /// </summary>
        public B5_Shellshock_Terrain(int width, B5_Shellshock_TerrainType terrainType)
        {
            _width = width;
            _terrainType = terrainType;
            _heightMap = new double[width];
            
            // Use Factory to get appropriate generator (Strategy Pattern)
            _generator = B5_Shellshock_TerrainGeneratorFactory.Create(terrainType);
            GenerateTerrain();
        }

        /// <summary>
        /// Creates terrain with a custom generator (Dependency Injection).
        /// Allows for testing and custom terrain algorithms.
        /// 
        /// Demonstriert: Dependency Injection für bessere Testbarkeit
        /// </summary>
        public B5_Shellshock_Terrain(int width, ITerrainGenerator generator)
        {
            _width = width;
            _terrainType = B5_Shellshock_TerrainType.Flat; // Default
            _heightMap = new double[width];
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            GenerateTerrain();
        }

        #region Terrain Generation

        /// <summary>
        /// Delegates terrain generation to the generator strategy.
        /// </summary>
        private void GenerateTerrain()
        {
            _generator.Generate(_heightMap, _width);
        }

        /// <summary>
        /// Applies multiple smoothing passes to eliminate sharp terrain transitions.
        /// Each pass averages neighboring height values using weighted averaging.
        /// More passes = smoother terrain but less detail.
        /// </summary>
        /// <param name="passes">Number of smoothing iterations to apply</param>
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

        #endregion

        #region Collision and Queries

        /// <summary>
        /// Returns terrain height at specified X coordinate.
        /// Uses nearest-neighbor sampling (rounded to integer X).
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <returns>Y position in normalized coordinates [0-1]</returns>
        public double GetHeightAt(double x)
        {
            int index = (int)Math.Round(x);
            if (index < 0 || index >= _width)
                return 1.0; // Off screen = bottom

            return _heightMap[index];
        }

        /// <summary>
        /// Checks if a point collides with the terrain.
        /// Collision occurs when point Y is at or below terrain surface.
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in normalized coordinates [0-1]</param>
        /// <returns>True if collision detected</returns>
        public bool IsCollision(double x, double y)
        {
            if (x < 0 || x >= _width)
                return false;

            double terrainHeight = GetHeightAt(x);
            return y >= terrainHeight;
        }

        #endregion

        #region Terrain Destruction

        /// <summary>
        /// Creates a circular crater at the specified impact point.
        /// Uses circular falloff for smooth crater edges.
        /// Applies smoothing to blend crater with surrounding terrain.
        /// </summary>
        /// <param name="x">Center X position of crater in pixels</param>
        /// <param name="radius">Crater radius in pixels</param>
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

        #endregion
    }
}
