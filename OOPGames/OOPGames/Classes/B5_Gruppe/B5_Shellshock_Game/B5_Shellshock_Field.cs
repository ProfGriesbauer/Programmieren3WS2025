using System;
using System.Collections.Generic;

namespace OOPGames
{
    /// <summary>
    /// Central game state container for Shellshock.
    /// Implements IGameField for framework integration.
    /// 
    /// Contains:
    /// - Two tanks (players 1 and 2)
    /// - Procedurally generated terrain
    /// - Active projectile (during shots)
    /// - Optional health pack pickup
    /// - Environmental factors (wind)
    /// - Turn management state
    /// - Trajectory history for visualization
    /// </summary>
    public class B5_Shellshock_Field : IGameField
    {
        #region Game Objects

        private B5_Shellshock_Tank _tank1;
        private B5_Shellshock_Tank _tank2;
        private B5_Shellshock_Terrain _terrain;
        private B5_Shellshock_Projectile _projectile;
        private B5_Shellshock_HealthPack _healthPack;

        #endregion

        #region Environment

        private double _wind;

        #endregion

        #region Turn Management

        private int _movementsRemaining;
        private int _maxMovesPerTurn;
        private int _activeTankNumber;
        private B5_Shellshock_GamePhase _gamePhase;

        #endregion

        #region Trajectory History

        private List<B5_Shellshock_Point> _lastTrajectoryP1;
        private List<B5_Shellshock_Point> _lastTrajectoryP2;

        #endregion

        #region Properties - Game Objects

        /// <summary>Player 1's tank (red, starts on left side).</summary>
        public B5_Shellshock_Tank Tank1 => _tank1;

        /// <summary>Player 2's tank (blue, starts on right side).</summary>
        public B5_Shellshock_Tank Tank2 => _tank2;

        /// <summary>The battlefield terrain with height map.</summary>
        public B5_Shellshock_Terrain Terrain => _terrain;

        /// <summary>Currently active projectile, or null if none in flight.</summary>
        public B5_Shellshock_Projectile Projectile
        {
            get => _projectile;
            set => _projectile = value;
        }

        /// <summary>Active health pack on field, or null if none spawned.</summary>
        public B5_Shellshock_HealthPack HealthPack
        {
            get => _healthPack;
            set => _healthPack = value;
        }

        /// <summary>True if a projectile is currently in flight.</summary>
        public bool ProjectileInFlight => _projectile != null && _projectile.IsActive;

        #endregion

        #region Properties - Environment

        /// <summary>Wind force. Positive = right, Negative = left. Range: [-5, 5].</summary>
        public double Wind
        {
            get => _wind;
            set => _wind = value;
        }

        #endregion

        #region Properties - Turn Management

        /// <summary>Movement actions remaining this turn.</summary>
        public int MovementsRemaining
        {
            get => _movementsRemaining;
            set => _movementsRemaining = value;
        }

        /// <summary>Maximum movement actions per turn (terrain-dependent).</summary>
        public int MaxMovesPerTurn
        {
            get => _maxMovesPerTurn;
            set => _maxMovesPerTurn = value;
        }

        /// <summary>Which player's turn it is (1 or 2).</summary>
        public int ActiveTankNumber
        {
            get => _activeTankNumber;
            set => _activeTankNumber = value;
        }

        /// <summary>Current game phase (Setup, PlayerTurn, ProjectileInFlight, GameOver).</summary>
        public B5_Shellshock_GamePhase GamePhase
        {
            get => _gamePhase;
            set => _gamePhase = value;
        }

        #endregion

        #region Properties - Trajectory History

        /// <summary>Trajectory points from Player 1's last shot (for visualization).</summary>
        public List<B5_Shellshock_Point> LastTrajectoryP1 => _lastTrajectoryP1;

        /// <summary>Trajectory points from Player 2's last shot (for visualization).</summary>
        public List<B5_Shellshock_Point> LastTrajectoryP2 => _lastTrajectoryP2;

        #endregion

        #region Constructor

        public B5_Shellshock_Field()
        {
            Reset();
        }

        #endregion

        #region IGameField Implementation

        /// <summary>
        /// Framework compatibility check for painter.
        /// Only B5_Shellshock_Painter can render this field.
        /// </summary>
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is B5_Shellshock_Painter;
        }

        #endregion

        #region Game Setup

        /// <summary>
        /// Resets the game to initial state.
        /// - Generates new random terrain
        /// - Positions tanks at opposite ends
        /// - Sets random wind
        /// - Resets turn management
        /// </summary>
        public void Reset()
        {
            Random rand = new Random();
            
            // Select random terrain type
            B5_Shellshock_TerrainType[] terrainTypes = { 
                B5_Shellshock_TerrainType.Flat, 
                B5_Shellshock_TerrainType.Hill, 
                B5_Shellshock_TerrainType.Curvy 
            };
            B5_Shellshock_TerrainType selectedTerrain = terrainTypes[rand.Next(terrainTypes.Length)];
            _terrain = new B5_Shellshock_Terrain(800, selectedTerrain);

            // Set max moves based on terrain (more moves for complex terrain)
            _maxMovesPerTurn = selectedTerrain == B5_Shellshock_TerrainType.Flat ? 5 : 7;

            // Position tanks on terrain at opposite ends
            const int Tank1X = 100;
            const int Tank2X = 700;

            _tank1 = new B5_Shellshock_Tank(Tank1X, _terrain.GetHeightAt(Tank1X), B5_Shellshock_TankColor.Red);
            _tank2 = new B5_Shellshock_Tank(Tank2X, _terrain.GetHeightAt(Tank2X), B5_Shellshock_TankColor.Blue);
            
            // Tank 2 faces left (towards Tank 1)
            _tank2.Angle = 135;

            // Random wind between -5 and 5
            _wind = rand.NextDouble() * 10 - 5;

            // Clear projectile and health pack
            _projectile = null;
            _healthPack = null;

            // Initialize turn management
            _movementsRemaining = _maxMovesPerTurn;
            _activeTankNumber = 1;
            _gamePhase = B5_Shellshock_GamePhase.Setup;

            // Clear trajectory history
            _lastTrajectoryP1 = new List<B5_Shellshock_Point>();
            _lastTrajectoryP2 = new List<B5_Shellshock_Point>();
        }

        #endregion

        #region Tank Access

        /// <summary>Gets tank by player number (1 or 2).</summary>
        public B5_Shellshock_Tank GetTankByPlayer(int playerNumber)
        {
            return playerNumber == 1 ? _tank1 : _tank2;
        }

        /// <summary>Gets the opponent's tank for given player number.</summary>
        public B5_Shellshock_Tank GetOpponentTank(int playerNumber)
        {
            return playerNumber == 1 ? _tank2 : _tank1;
        }

        /// <summary>
        /// Updates both tank Y positions to match terrain height.
        /// Called after terrain destruction to prevent floating tanks.
        /// </summary>
        public void UpdateTankPositions()
        {
            _tank1.Y = _terrain.GetHeightAt(_tank1.X);
            _tank2.Y = _terrain.GetHeightAt(_tank2.X);
        }

        #endregion
    }

    /// <summary>
    /// Simple 2D point for trajectory visualization.
    /// Uses game coordinates (X in pixels, Y in normalized 0-1 range).
    /// Avoids WPF dependency in game logic layer.
    /// </summary>
    public class B5_Shellshock_Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public B5_Shellshock_Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
