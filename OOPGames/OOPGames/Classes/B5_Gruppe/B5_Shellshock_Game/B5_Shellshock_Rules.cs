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
        private int _movementsRemaining; // Remaining position moves for current tank
        private const int MAX_MOVES = 5;
        private bool _pendingTurnSwitch; // Set after a shot, processed after projectile collision

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
            _movementsRemaining = MAX_MOVES;
            _pendingTurnSwitch = false;
            _field.MovementsRemaining = _movementsRemaining;
            _field.ActiveTankNumber = _activeTankNumber;
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not B5_Shellshock_Move shellMove) return;
            if (!_field.Tank1.IsAlive || !_field.Tank2.IsAlive) return;
            if (_field.ProjectileInFlight) return; // Block input during flight

            // Only handle Shoot via rules; movement/angle/power handled directly in HumanPlayer
            if (shellMove.ActionType == B5_Shellshock_ActionType.Shoot && (_field.Projectile == null || !_field.Projectile.IsActive))
            {
                B5_Shellshock_Tank currentTank = _activeTankNumber == 1 ? _field.Tank1 : _field.Tank2;
                // Barrel length in normalized coordinates (terrain is 0-1 scale, width is 800)
                double barrelLengthNormalized = 0.02; // About 15-20 pixels when scaled
                double angleRad = currentTank.Angle * Math.PI / 180.0;
                double barrelEndX = currentTank.X + (barrelLengthNormalized * _field.Terrain.Width) * Math.Cos(angleRad);
                double barrelEndY = currentTank.Y - barrelLengthNormalized * Math.Sin(angleRad);

                _field.Projectile = new B5_Shellshock_Projectile(
                    barrelEndX,
                    barrelEndY,
                    currentTank.Angle,
                    currentTank.Power,
                    shellMove.PlayerNumber
                );
                _gamePhase = B5_Shellshock_GamePhase.ProjectileInFlight;
                _pendingTurnSwitch = true; // Delay tank switch until collision
            }
        }

        public void ClearField()
        {
            _field.Reset();
            _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
            _activeTankNumber = 1;
            _movementsRemaining = MAX_MOVES;
            _pendingTurnSwitch = false;
            _field.MovementsRemaining = _movementsRemaining;
            _field.ActiveTankNumber = _activeTankNumber;
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
            if (_field.ProjectileInFlight && _field.Projectile != null)
            {
                B5_Shellshock_Projectile proj = _field.Projectile;
                if (proj.IsActive)
                {
                    proj.UpdatePosition(_gravity, _field.Wind, 0.04);
                    bool collision = false;
                    if (_field.Terrain.IsCollision(proj.X, proj.Y)) collision = true;
                    if (CheckTankCollision(_field.Tank1, proj)) { collision = true; _field.Tank1.TakeDamage(50); }
                    if (CheckTankCollision(_field.Tank2, proj)) { collision = true; _field.Tank2.TakeDamage(50); }
                    if (proj.X < 0 || proj.X > _field.Terrain.Width || proj.Y > 1.0) collision = true;
                    if (collision)
                    {
                        proj.IsActive = false;
                        _field.Projectile = null;
                        if (_pendingTurnSwitch)
                        {
                            _activeTankNumber = _activeTankNumber == 1 ? 2 : 1;
                            _movementsRemaining = MAX_MOVES;
                            _field.MovementsRemaining = _movementsRemaining;
                            _field.ActiveTankNumber = _activeTankNumber;
                            _pendingTurnSwitch = false;
                        }
                        _gamePhase = CheckIfPLayerWon() != -1 ? B5_Shellshock_GamePhase.GameOver : B5_Shellshock_GamePhase.PlayerTurn;
                    }
                }
            }
        }

        private bool CheckTankCollision(B5_Shellshock_Tank tank, B5_Shellshock_Projectile proj)
        {
            // Simple bounding box collision (normalized coordinates)
            double tankWidthNormalized = 20; // Pixel width in terrain space
            double tankHeightNormalized = 0.02; // Height in 0-1 space

            return proj.X >= tank.X - tankWidthNormalized / 2 &&
                   proj.X <= tank.X + tankWidthNormalized / 2 &&
                   proj.Y >= tank.Y - tankHeightNormalized &&
                   proj.Y <= tank.Y;
        }
    }
}
