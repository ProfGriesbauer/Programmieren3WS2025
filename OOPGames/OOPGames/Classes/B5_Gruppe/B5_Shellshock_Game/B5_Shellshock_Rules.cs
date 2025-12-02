using System;
using System.Linq;

namespace OOPGames
{
    /// <summary>
    /// Core game logic controller for Shellshock artillery game.
    /// Implements turn-based tank combat with ballistic physics simulation.
    /// Manages game state, collision detection, terrain destruction, and win conditions.
    /// 
    /// Game Flow:
    /// 1. Setup: Display start screen
    /// 2. PlayerTurn: Active player moves and aims, then shoots
    /// 3. ProjectileInFlight: Physics simulation until collision
    /// 4. Turn switch and repeat, or GameOver if tank destroyed
    /// </summary>
    public class B5_Shellshock_Rules : IGameRules, IGameRules2
    {
        private B5_Shellshock_Field _field;
        private B5_Shellshock_GamePhase _gamePhase;
        private double _gravity;
        private int _activeTankNumber;      // Which tank (1 or 2) is currently active
        private int _movementsRemaining;    // Remaining movement actions for current turn
        private bool _pendingTurnSwitch;    // Delayed turn switch after projectile impact
        private bool _lastMoveKeepsTurn;    // Framework flag: true = same player continues

        public string Name => "B5_Shellshock_Rules";

        public IGameField CurrentField => _field;

        /// <summary>
        /// Current player number (1 or 2). Used by framework to determine whose turn it is.
        /// </summary>
        public int CurrentPlayerNumber => _activeTankNumber;

        /// <summary>
        /// True if the last move should NOT switch players.
        /// In Shellshock, players can move/aim multiple times but turn switches after shooting.
        /// </summary>
        public bool LastMoveGivesExtraTurn => _lastMoveKeepsTurn;

        /// <summary>
        /// Returns true while game is ongoing (at least one tank alive).
        /// Used by framework to determine if game should continue.
        /// </summary>
        public bool MovesPossible
        {
            get
            {
                // Game ends when either tank is destroyed
                if (!_field.Tank1.IsAlive || !_field.Tank2.IsAlive) return false;
                return true;
            }
        }

        public B5_Shellshock_Rules()
        {
            _field = new B5_Shellshock_Field();
            _gamePhase = B5_Shellshock_GamePhase.Setup;
            _gravity = 9.8; // Standard Earth gravity (m/s²)
            _activeTankNumber = 1; // Tank 1 starts
            _movementsRemaining = _field.MaxMovesPerTurn;
            _pendingTurnSwitch = false;
            _field.MovementsRemaining = _movementsRemaining;
            _field.ActiveTankNumber = _activeTankNumber;
            _field.GamePhase = _gamePhase;
        }

        #region Move Handling

        /// <summary>
        /// Processes player actions. 
        /// StartGame: Transitions from Setup to gameplay or restarts after GameOver.
        /// Shoot: Creates projectile and initiates physics simulation.
        /// Note: Movement and aiming handled directly in HumanPlayer for immediate feedback.
        /// </summary>
        public void DoMove(IPlayMove move)
        {
            if (move is not B5_Shellshock_Move shellMove) return;
            if (!_field.Tank1.IsAlive || !_field.Tank2.IsAlive) return;
            if (_field.ProjectileInFlight) return; // Block input during flight

            // By default, keep the turn (movement/aiming doesn't switch players)
            _lastMoveKeepsTurn = true;

            B5_Shellshock_Tank currentTank = _activeTankNumber == 1 ? _field.Tank1 : _field.Tank2;

            // Handle movement actions
            if (shellMove.ActionType == B5_Shellshock_ActionType.MoveLeft && _field.MovementsRemaining > 0)
            {
                double newX = currentTank.X - 10;
                if (newX > 20 && newX < _field.Terrain.Width - 20)
                {
                    currentTank.X = newX;
                    currentTank.Y = _field.Terrain.GetHeightAt(currentTank.X);
                    _field.MovementsRemaining--;
                }
                return;
            }
            if (shellMove.ActionType == B5_Shellshock_ActionType.MoveRight && _field.MovementsRemaining > 0)
            {
                double newX = currentTank.X + 10;
                if (newX > 20 && newX < _field.Terrain.Width - 20)
                {
                    currentTank.X = newX;
                    currentTank.Y = _field.Terrain.GetHeightAt(currentTank.X);
                    _field.MovementsRemaining--;
                }
                return;
            }

            // Handle angle adjustments
            if (shellMove.ActionType == B5_Shellshock_ActionType.IncreaseAngle)
            {
                currentTank.Angle = Math.Min(170, currentTank.Angle + 1);
                return;
            }
            if (shellMove.ActionType == B5_Shellshock_ActionType.DecreaseAngle)
            {
                currentTank.Angle = Math.Max(10, currentTank.Angle - 1);
                return;
            }

            // Handle power adjustments
            if (shellMove.ActionType == B5_Shellshock_ActionType.IncreasePower)
            {
                currentTank.Power = Math.Min(100, currentTank.Power + 1);
                return;
            }
            if (shellMove.ActionType == B5_Shellshock_ActionType.DecreasePower)
            {
                currentTank.Power = Math.Max(10, currentTank.Power - 1);
                return;
            }

            // Only handle Shoot via rules; movement/angle/power handled directly in HumanPlayer
            if (shellMove.ActionType == B5_Shellshock_ActionType.StartGame && _gamePhase == B5_Shellshock_GamePhase.Setup)
            {
                _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
                _field.GamePhase = _gamePhase;
                return;
            }
            if (shellMove.ActionType == B5_Shellshock_ActionType.StartGame && _gamePhase == B5_Shellshock_GamePhase.GameOver)
            {
                ClearField();
                _gamePhase = B5_Shellshock_GamePhase.PlayerTurn;
                _field.GamePhase = _gamePhase;
                return;
            }
            if (shellMove.ActionType == B5_Shellshock_ActionType.Shoot && (_field.Projectile == null || !_field.Projectile.IsActive) && _gamePhase == B5_Shellshock_GamePhase.PlayerTurn)
            {
                B5_Shellshock_Tank shootingTank = _activeTankNumber == 1 ? _field.Tank1 : _field.Tank2;
                
                // Calculate barrel end position for projectile spawn point
                // Barrel extends from tank center based on firing angle
                double barrelLengthNormalized = 0.02; // Approximately 15-20 pixels when scaled to screen
                double angleRad = shootingTank.Angle * Math.PI / 180.0;
                double barrelEndX = shootingTank.X + (barrelLengthNormalized * _field.Terrain.Width) * Math.Cos(angleRad);
                double barrelEndY = shootingTank.Y - barrelLengthNormalized * Math.Sin(angleRad);

                // Use tank's Fire method to create projectile
                _field.Projectile = shootingTank.Fire(shellMove.PlayerNumber);
                // Override position to barrel end (Fire uses tank center by default)
                _field.Projectile.X = barrelEndX;
                _field.Projectile.Y = barrelEndY;
                
                // Clear previous trajectory and start recording new one
                if (shellMove.PlayerNumber == 1) 
                    _field.LastTrajectoryP1.Clear();
                else 
                    _field.LastTrajectoryP2.Clear();
                    
                // Transition to flight phase and mark turn switch as pending
                _gamePhase = B5_Shellshock_GamePhase.ProjectileInFlight;
                _pendingTurnSwitch = true; // Switch after projectile collision
                _field.GamePhase = _gamePhase;
                _lastMoveKeepsTurn = false; // Switch player after this move
            }
        }

        #endregion

        #region Game State Management

        public void ClearField()
        {
            _field.Reset();
            _gamePhase = B5_Shellshock_GamePhase.Setup;
            _activeTankNumber = 1;
            _movementsRemaining = _field.MaxMovesPerTurn;
            _pendingTurnSwitch = false;
            _lastMoveKeepsTurn = true;
            _field.MovementsRemaining = _movementsRemaining;
            _field.ActiveTankNumber = _activeTankNumber;
            _field.GamePhase = _gamePhase;
        }

        /// <summary>
        /// Checks win condition.
        /// Returns 1 if Player 1 wins (Tank2 destroyed).
        /// Returns 2 if Player 2 wins (Tank1 destroyed).
        /// Returns -1 if game continues.
        /// Never returns 0 (would trigger unwanted draw/reset).
        /// </summary>
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
            // Initialize game when framework starts
            ClearField();
        }

        #endregion

        #region Physics Simulation

        /// <summary>
        /// Called every ~40ms by framework for physics simulation.
        /// Updates projectile position, checks collisions, handles terrain destruction,
        /// manages turn switching, and randomizes wind after each shot.
        /// </summary>
        public void TickGameCall()
        {
            if (_field.ProjectileInFlight && _field.Projectile != null)
            {
                B5_Shellshock_Projectile proj = _field.Projectile;
                if (proj.IsActive)
                {
                    proj.UpdatePosition(_gravity, _field.Wind, 0.04);
                    // Record trajectory point for visualization
                    var pt = new B5_Shellshock_Point(proj.X, proj.Y);
                    if (proj.PlayerNumber == 1) 
                        _field.LastTrajectoryP1.Add(pt); 
                    else 
                        _field.LastTrajectoryP2.Add(pt);
                        
                    // Collision detection
                    bool collision = false;
                    bool terrainHit = false;
                    
                    // Check terrain collision
                    if (_field.Terrain.IsCollision(proj.X, proj.Y)) 
                    { 
                        collision = true; 
                        terrainHit = true; 
                    }
                    
                    // Check tank collisions (applies damage)
                    if (CheckTankCollision(_field.Tank1, proj)) 
                    { 
                        collision = true; 
                        _field.Tank1.TakeDamage(20); 
                    }
                    if (CheckTankCollision(_field.Tank2, proj)) 
                    { 
                        collision = true; 
                        _field.Tank2.TakeDamage(20); 
                    }
                    
                    // Check health pack collision (applies healing)
                    if (_field.HealthPack != null && _field.HealthPack.IsActive && _field.HealthPack.CollidesWith(proj))
                    {
                        // Heal the shooting player's tank
                        B5_Shellshock_Tank shootingTank = proj.PlayerNumber == 1 ? _field.Tank1 : _field.Tank2;
                        shootingTank.Health = Math.Min(100, shootingTank.Health + B5_Shellshock_HealthPack.HealAmount);
                        _field.HealthPack.IsActive = false;
                        _field.HealthPack = null; // Remove health pack
                    }
                    
                    // Check out of bounds
                    if (proj.X < 0 || proj.X > _field.Terrain.Width || proj.Y > 1.0) 
                        collision = true;
                        
                    if (collision)
                    {
                        // Destroy terrain at impact point if hit ground
                        if (terrainHit)
                        {
                            _field.Terrain.DestroyTerrain(proj.X, 30); // 30 pixel radius crater
                            // Update tank positions to rest on modified terrain
                            _field.UpdateTankPositions();
                        }
                        
                        // Deactivate projectile
                        proj.IsActive = false;
                        _field.Projectile = null;
                        
                        // Execute delayed turn switch
                        if (_pendingTurnSwitch)
                        {
                            _activeTankNumber = _activeTankNumber == 1 ? 2 : 1;
                            _movementsRemaining = _field.MaxMovesPerTurn;
                            _field.MovementsRemaining = _movementsRemaining;
                            _field.ActiveTankNumber = _activeTankNumber;
                            _pendingTurnSwitch = false;
                            _lastMoveKeepsTurn = true; // Allow the new player to take multiple moves
                        }
                        
                        // Randomize wind for next shot
                        Random rnd = new Random();
                        _field.Wind = rnd.NextDouble() * 10 - 5; // Range: -5 to +5
                        
                        // Spawn health pack randomly (50% chance) after shot lands
                        // Spawn anywhere horizontally, but below GUI panels (Y > 110px, normalized ~0.18)
                        if (rnd.NextDouble() > 0.5)
                        {
                            double packX = rnd.NextDouble() * _field.Terrain.Width; // Any X position
                            double packY = 0.18 + rnd.NextDouble() * 0.52; // Between GUI bottom (0.18) and terrain (0.7)
                            _field.HealthPack = new B5_Shellshock_HealthPack(packX, packY);
                        }
                        
                        // Check for game over or continue
                        _gamePhase = CheckIfPLayerWon() != -1 ? B5_Shellshock_GamePhase.GameOver : B5_Shellshock_GamePhase.PlayerTurn;
                        _field.GamePhase = _gamePhase;
                    }
                }
            }
        }

        #endregion

        #region Collision Detection

        /// <summary>
        /// Checks collision between a tank and projectile using the tank's collision method.
        /// Encapsulates collision logic within the Tank class following OOP principles.
        /// </summary>
        private bool CheckTankCollision(B5_Shellshock_Tank tank, B5_Shellshock_Projectile proj)
        {
            return tank.CollidesWith(proj);
        }

        #endregion
    }
}
