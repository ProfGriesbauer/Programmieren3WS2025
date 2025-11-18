using System;
using System.Linq;

namespace OOPGames
{
    // Manages game logic: Turn-based tank control, Projectile physics simulation, 
    // Collision detection, Win condition: returns 1 or 2 if tank destroyed, -1 otherwise
    public class B5_Shellshock_Rules : IGameRules, IGameRules2
    {
        private B5_Shellshock_Field _field;
        private B5_Shellshock_GamePhase _gamePhase;
        private double _gravity;
        private int _activeTankNumber; // Which tank (1 or 2) is currently taking its turn

        public string Name => "B5_Shellshock_Rules";

        public IGameField CurrentField => _field;

        public bool MovesPossible
        {
            get
            {
                // Always return true if game is not over
                if (!_field.Tank1.IsAlive || !_field.Tank2.IsAlive) return false;
                
                return true;
            }
        }

        public B5_Shellshock_Rules()
        {
            _field = new B5_Shellshock_Field();
            _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
            _gravity = 9.8;
            _activeTankNumber = 1; // Tank 1 starts
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not B5_Shellshock_Move shellMove) return;
            
            // Don't process moves if game is over
            if (!_field.Tank1.IsAlive || !_field.Tank2.IsAlive) return;
            
            // Don't process moves during projectile flight
            if (_field.ProjectileInFlight) return;

            // IGNORE player number - work with active tank instead
            // This way both "Player 1" and "Player 2" framework instances control the same tank
            B5_Shellshock_Tank currentTank = _activeTankNumber == 1 ? _field.Tank1 : _field.Tank2;

            switch (shellMove.ActionType)
            {
                case B5_Shellshock_ActionType.MoveLeft:
                    if (_field.MovementsRemaining > 0)
                    {
                        double newX = currentTank.X - 10;
                        if (newX > 20 && newX < _field.Terrain.Width - 20)
                        {
                            currentTank.X = newX;
                            currentTank.Y = _field.Terrain.GetHeightAt(currentTank.X);
                            _field.MovementsRemaining--;
                        }
                    }
                    break;

                case B5_Shellshock_ActionType.MoveRight:
                    if (_field.MovementsRemaining > 0)
                    {
                        double newX = currentTank.X + 10;
                        if (newX > 20 && newX < _field.Terrain.Width - 20)
                        {
                            currentTank.X = newX;
                            currentTank.Y = _field.Terrain.GetHeightAt(currentTank.X);
                            _field.MovementsRemaining--;
                        }
                    }
                    break;

                case B5_Shellshock_ActionType.IncreaseAngle:
                    currentTank.AdjustAngle(5);
                    break;

                case B5_Shellshock_ActionType.DecreaseAngle:
                    currentTank.AdjustAngle(-5);
                    break;

                case B5_Shellshock_ActionType.IncreasePower:
                    currentTank.AdjustPower(5);
                    break;

                case B5_Shellshock_ActionType.DecreasePower:
                    currentTank.AdjustPower(-5);
                    break;

                case B5_Shellshock_ActionType.Shoot:
                    // Shooting ends the turn
                    if (_field.Projectile == null || !_field.Projectile.IsActive)
                    {
                        // Calculate barrel end position
                        double barrelLength = 15;
                        double angleRad = currentTank.Angle * Math.PI / 180.0;
                        double barrelEndX = currentTank.X + barrelLength * Math.Cos(angleRad);
                        double barrelEndY = currentTank.Y - barrelLength * Math.Sin(angleRad);

                        _field.Projectile = new B5_Shellshock_Projectile(
                            barrelEndX,
                            barrelEndY,
                            currentTank.Angle,
                            currentTank.Power,
                            shellMove.PlayerNumber
                        );

                        _gamePhase = B5_Shellshock_GamePhase.ProjectileInFlight;
                        
                        // Switch to next tank
                        _activeTankNumber = _activeTankNumber == 1 ? 2 : 1;
                        // Reset movement counter for next turn
                        _field.MovementsRemaining = 5;
                    }
                    break;
            }
        }

        public void ClearField()
        {
            _field.Reset();
            _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
            _activeTankNumber = 1; // Reset to Tank 1
        }

        public int CheckIfPLayerWon()
        {
            // Only check for winner, don't return draw
            if (!_field.Tank1.IsAlive)
                return 2; // Player 2 wins

            if (!_field.Tank2.IsAlive)
                return 1; // Player 1 wins

            // Game continues - NEVER return 0 (would trigger draw/reset)
            return -1;
        }

        public void StartedGameCall()
        {
            // Initialize game when started
            ClearField();
        }

        public void TickGameCall()
        {
            // Handle projectile physics every tick (40ms)
            if (_field.ProjectileInFlight && _field.Projectile != null)
            {
                B5_Shellshock_Projectile proj = _field.Projectile;

                if (proj.IsActive)
                {
                    // Update projectile position (deltaTime = 0.04 seconds = 40ms)
                    proj.UpdatePosition(_gravity, _field.Wind, 0.04);

                    // Check for collisions
                    bool collision = false;

                    // Check terrain collision
                    if (_field.Terrain.IsCollision(proj.X, proj.Y))
                    {
                        collision = true;
                    }

                    // Check tank collisions
                    if (CheckTankCollision(_field.Tank1, proj))
                    {
                        collision = true;
                        _field.Tank1.TakeDamage(50);
                    }

                    if (CheckTankCollision(_field.Tank2, proj))
                    {
                        collision = true;
                        _field.Tank2.TakeDamage(50);
                    }

                    // Check if projectile is off screen
                    if (proj.X < 0 || proj.X > _field.Terrain.Width || proj.Y > 1.0)
                    {
                        collision = true;
                    }

                    if (collision)
                    {
                        proj.IsActive = false;
                        _field.Projectile = null;

                        // Check if someone won
                        if (CheckIfPLayerWon() != -1)
                        {
                            _gamePhase = B5_Shellshock_GamePhase.GameOver;
                        }
                        else
                        {
                            _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
                        }
                    }
                }
            }
        }

        private bool CheckTankCollision(B5_Shellshock_Tank tank, B5_Shellshock_Projectile proj)
        {
            // Simple bounding box collision
            double tankWidth = 20;
            double tankHeight = 10;

            return proj.X >= tank.X - tankWidth / 2 &&
                   proj.X <= tank.X + tankWidth / 2 &&
                   proj.Y >= tank.Y - tankHeight &&
                   proj.Y <= tank.Y;
        }
    }
}
