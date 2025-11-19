using System;

namespace OOPGames
{
    // Main game field containing all game objects: two tanks, terrain, active projectile, wind.
    public class B5_Shellshock_Field : IGameField
    {
        private B5_Shellshock_Tank _tank1;
        private B5_Shellshock_Tank _tank2;
        private B5_Shellshock_Terrain _terrain;
        private B5_Shellshock_Projectile _projectile;
        private double _wind;
        private int _movementsRemaining;
        private int _maxMovesPerTurn; // Set based on terrain type
        private int _activeTankNumber; // 1 or 2 indicating whose turn it is
        private System.Collections.Generic.List<B5_Shellshock_Point> _lastTrajectoryP1;
        private System.Collections.Generic.List<B5_Shellshock_Point> _lastTrajectoryP2;
        private B5_Shellshock_GamePhase _gamePhase;

        public B5_Shellshock_Tank Tank1 => _tank1;
        public B5_Shellshock_Tank Tank2 => _tank2;
        public B5_Shellshock_Terrain Terrain => _terrain;
        
        public B5_Shellshock_Projectile Projectile
        {
            get => _projectile;
            set => _projectile = value;
        }

        public double Wind
        {
            get => _wind;
            set => _wind = value;
        }

        public int MovementsRemaining
        {
            get => _movementsRemaining;
            set => _movementsRemaining = value;
        }

        public int MaxMovesPerTurn
        {
            get => _maxMovesPerTurn;
            set => _maxMovesPerTurn = value;
        }

        public int ActiveTankNumber
        {
            get => _activeTankNumber;
            set => _activeTankNumber = value;
        }

        public bool ProjectileInFlight
        {
            get => _projectile != null && _projectile.IsActive;
        }

        // Stores the last shot trajectory as a list of normalized coordinates (x in pixels, y in 0..1)
        public System.Collections.Generic.List<B5_Shellshock_Point> LastTrajectoryP1 => _lastTrajectoryP1;
        public System.Collections.Generic.List<B5_Shellshock_Point> LastTrajectoryP2 => _lastTrajectoryP2;

        public B5_Shellshock_GamePhase GamePhase
        {
            get => _gamePhase;
            set => _gamePhase = value;
        }

        public B5_Shellshock_Field()
        {
            Reset();
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is B5_Shellshock_Painter;
        }

        public void Reset()
        {
            // Create random terrain (800 units wide)
            Random rand = new Random();
            // Randomly select terrain type: Flat, Hill, or Curvy
            B5_Shellshock_TerrainType[] terrainTypes = { 
                B5_Shellshock_TerrainType.Flat, 
                B5_Shellshock_TerrainType.Hill, 
                B5_Shellshock_TerrainType.Curvy 
            };
            B5_Shellshock_TerrainType selectedTerrain = terrainTypes[rand.Next(terrainTypes.Length)];
            _terrain = new B5_Shellshock_Terrain(800, selectedTerrain);

            // Set max moves based on terrain type: 5 for Flat, 7 for Hill/Curvy
            _maxMovesPerTurn = selectedTerrain == B5_Shellshock_TerrainType.Flat ? 5 : 7;

            // Position tanks on terrain
            int tank1X = 100;
            int tank2X = 700;

            double tank1Y = _terrain.GetHeightAt(tank1X);
            double tank2Y = _terrain.GetHeightAt(tank2X);

            _tank1 = new B5_Shellshock_Tank(tank1X, tank1Y, B5_Shellshock_TankColor.Red);
            _tank2 = new B5_Shellshock_Tank(tank2X, tank2Y, B5_Shellshock_TankColor.Blue);
            // Mirror tank 2 angle to face towards tank 1 initially
            _tank2.Angle = 135; // Point left/up by default so it aims towards tank 1

            // Random wind between -5 and 5
            _wind = rand.NextDouble() * 10 - 5;

            _projectile = null;
            _movementsRemaining = _maxMovesPerTurn; // Start with max movements for terrain type
            _activeTankNumber = 1; // Tank 1 starts
            _lastTrajectoryP1 = new System.Collections.Generic.List<B5_Shellshock_Point>();
            _lastTrajectoryP2 = new System.Collections.Generic.List<B5_Shellshock_Point>();
            _gamePhase = B5_Shellshock_GamePhase.Setup; // start in setup (start screen)
        }

        public B5_Shellshock_Tank GetTankByPlayer(int playerNumber)
        {
            return playerNumber == 1 ? _tank1 : _tank2;
        }

        public B5_Shellshock_Tank GetOpponentTank(int playerNumber)
        {
            return playerNumber == 1 ? _tank2 : _tank1;
        }
    }

    // Simple point holder for trajectory (avoid WPF types in the field)
    public class B5_Shellshock_Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public B5_Shellshock_Point(double x, double y) { X = x; Y = y; }
    }
}
