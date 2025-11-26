using System;
using System.Linq; // Needed for .Where LINQ on projectile list
using OOPGames.Classes.A4_Guppe.ShellStrikeLegends;

namespace OOPGames
{
    public class A4_ShellStrike_Rules : IGameRules2
    {
        private readonly A4_ShellStrike_Field _field = new A4_ShellStrike_Field();
        private int _tickCounter = 0;

        public string Name => "A4 ShellStrikeLegends Rules";
        public IGameField CurrentField => _field;
        public bool MovesPossible => true; // continuous game

        public void StartedGameCall() { ClearField(); }
        public void TickGameCall()
        {
            _tickCounter++;
            int maxX = _field.Terrain?.Heights?.Length > 0 ? _field.Terrain.Heights.Length - 1 : 800;
            foreach (var p in _field.Projectiles)
            {
                p.Tick(0.5, _field.Terrain, maxX);
            }
            _field.Projectiles.RemoveAll(p => !p.Active);

            // Simple collision check using terrain-based tank centers
            foreach (var p in _field.Projectiles)
            {
                if (!p.Active) continue;
                if (_field.Tank1 != null)
                {
                    double cx1 = _field.Tank1.X + _field.Tank1.Width / 2.0;
                    double cy1 = (_field.Terrain?.GroundYAt(cx1) ?? 320) - _field.Tank1.Height / 2.0;
                    if (Distance(p.X, p.Y, cx1, cy1) < 25) { _field.Tank1.Hit(); p.Active = false; }
                }
                if (_field.Tank2 != null)
                {
                    double cx2 = _field.Tank2.X + _field.Tank2.Width / 2.0;
                    double cy2 = (_field.Terrain?.GroundYAt(cx2) ?? 320) - _field.Tank2.Height / 2.0;
                    if (Distance(p.X, p.Y, cx2, cy2) < 25) { _field.Tank2.Hit(); p.Active = false; }
                }
            }
        }

        private double Distance(double x1,double y1,double x2,double y2) => Math.Sqrt((x1-x2)*(x1-x2)+(y1-y2)*(y1-y2));

        public void DoMove(IPlayMove move)
        {
            if (move is A4_ShellStrike_Move sm)
            {
                var tank = sm.PlayerNumber == 1 ? _field.Tank1 : _field.Tank2;
                if (tank == null) return;
                int terrainWidth = _field.Terrain?.Heights?.Length ?? 0;
                int slopeThreshold = Config.CrestSlopeThresholdPx; // pixels difference to count as sharp crest
                bool IsCrest(int idx)
                {
                    if (_field.Terrain == null || _field.Terrain.Heights == null) return false;
                    var h = _field.Terrain.Heights;
                    if (idx <= 0 || idx >= h.Length - 1) return false;
                    int y = h[idx];
                    int yL = h[idx - 1];
                    int yR = h[idx + 1];
                    // y grows downward; a sharp crest (mountain peak) is a local minimum with steep drop to both sides
                    return (yL - y) >= slopeThreshold && (yR - y) >= slopeThreshold; // local min, sharp
                }
                switch (sm.Action)
                {
                    case ShellStrikeAction.MoveLeft:
                    {
                        int maxXBound = (_field.Terrain?.Heights?.Length ?? 800) - (int)tank.Width;
                        // Face left when moving left
                        tank.Facing = -1;
                        // Moving left: front wheel is the left wheel; clear opposite crest flag
                        tank.CrestReadyRight = false;
                        if (terrainWidth >= 3 && _field.Terrain != null)
                        {
                            int iLeft = (int)Math.Floor(tank.X);
                            int iNextLeft = Math.Max(0, iLeft - 1); // next front-wheel pixel if we move left

                            // If already armed and currently on crest pixel, snap across now
                            if (tank.CrestReadyLeft && IsCrest(iLeft))
                            {
                                int off = Math.Max(1, Config.CrestSnapOffsetPx);
                                // Place right wheel at (crest - off)
                                double targetX = (iLeft - off) - tank.Width;
                                tank.SnapTo(targetX, 0, Math.Max(0, maxXBound));
                                tank.CrestReadyLeft = false;
                                break;
                            }

                            // If the immediate next pixel is a crest, clamp to it and arm
                            if (IsCrest(iNextLeft))
                            {
                                tank.SnapTo(iNextLeft, 0, Math.Max(0, maxXBound));
                                tank.CrestReadyLeft = true;
                                break;
                            }
                            else
                            {
                                // No crest immediately ahead; disarm and move freely
                                tank.CrestReadyLeft = false;
                            }
                        }
                        tank.Move(-Math.Abs(sm.Magnitude), 0, Math.Max(0, maxXBound));
                        break;
                    }
                    case ShellStrikeAction.MoveRight:
                    {
                        int maxXBound = (_field.Terrain?.Heights?.Length ?? 800) - (int)tank.Width;
                        // Face right when moving right
                        tank.Facing = 1;
                        // Moving right: front wheel is the right wheel; clear opposite crest flag
                        tank.CrestReadyLeft = false;
                        if (terrainWidth >= 3 && _field.Terrain != null)
                        {
                            int iRight = (int)Math.Ceiling(tank.X + tank.Width);
                            int lastIndex = _field.Terrain.Heights.Length - 1;
                            int iNextRight = Math.Min(lastIndex, iRight + 1); // next front-wheel pixel if we move right

                            // If already armed and currently on crest pixel, snap across now
                            if (tank.CrestReadyRight && IsCrest(iRight))
                            {
                                int off = Math.Max(1, Config.CrestSnapOffsetPx);
                                // Place left wheel at (crest + off)
                                double targetX = (iRight + off) - tank.Width;
                                tank.SnapTo(targetX, 0, Math.Max(0, maxXBound));
                                tank.CrestReadyRight = false;
                                break;
                            }

                            // If the immediate next pixel is a crest, clamp to it and arm
                            if (IsCrest(iNextRight))
                            {
                                double targetX = iNextRight - tank.Width;
                                tank.SnapTo(targetX, 0, Math.Max(0, maxXBound));
                                tank.CrestReadyRight = true;
                                break;
                            }
                            else
                            {
                                // No crest immediately ahead; disarm and move freely
                                tank.CrestReadyRight = false;
                            }
                        }
                        tank.Move(Math.Abs(sm.Magnitude), 0, Math.Max(0, maxXBound));
                        break;
                    }
                    case ShellStrikeAction.TurretUp:
                        tank.AdjustTurret(sm.Magnitude);
                        break;
                    case ShellStrikeAction.TurretDown:
                        tank.AdjustTurret(-sm.Magnitude);
                        break;
                    case ShellStrikeAction.Fire:
                        double cx = tank.X + tank.Width / 2.0;
                        double floorY = _field.Terrain?.GroundYAt(cx) ?? 320;
                        _field.Projectiles.Add(tank.Fire(floorY));
                        break;
                }
            }
        }

        public void ClearField()
        {
            _field.Projectiles.Clear();
            _field.Tank1 = new A4_ShellStrike_Tank(1, 100);
            _field.Tank2 = new A4_ShellStrike_Tank(2, 600);
            _field.Terrain = null; // force regeneration to canvas size on next paint
        }

        public int CheckIfPLayerWon()
        {
            if (_field.Tank1 != null && _field.Tank1.Health <= 0) return 2;
            if (_field.Tank2 != null && _field.Tank2.Health <= 0) return 1;
            return -1;
        }
    }

    public enum ShellStrikeAction { MoveLeft, MoveRight, TurretUp, TurretDown, Fire }

    public class A4_ShellStrike_Move : IPlayMove
    {
        public int PlayerNumber { get; }
        public ShellStrikeAction Action { get; }
        public double Magnitude { get; }
        public A4_ShellStrike_Move(int playerNumber, ShellStrikeAction action, double magnitude = 5)
        { PlayerNumber = playerNumber; Action = action; Magnitude = magnitude; }
    }
}
