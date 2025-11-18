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

        public bool ProjectileInFlight
        {
            get => _projectile != null && _projectile.IsActive;
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
            // Create flat terrain (800 units wide)
            Random rand = new Random();
            // Always use flat terrain for now
            _terrain = new B5_Shellshock_Terrain(800, B5_Shellshock_TerrainType.Flat);

            // Position tanks on terrain
            int tank1X = 100;
            int tank2X = 700;

            double tank1Y = _terrain.GetHeightAt(tank1X);
            double tank2Y = _terrain.GetHeightAt(tank2X);

            _tank1 = new B5_Shellshock_Tank(tank1X, tank1Y, B5_Shellshock_TankColor.Red);
            _tank2 = new B5_Shellshock_Tank(tank2X, tank2Y, B5_Shellshock_TankColor.Blue);

            // Random wind between -5 and 5
            _wind = rand.NextDouble() * 10 - 5;

            _projectile = null;
            _movementsRemaining = 5; // Start with 5 movements
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
}
