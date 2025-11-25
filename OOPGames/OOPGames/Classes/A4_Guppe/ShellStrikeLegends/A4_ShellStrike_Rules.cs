using System;
using System.Linq; // Needed for .Where LINQ on projectile list

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
                double groundY = _field.Terrain?.GroundYAt(p.X) ?? 320;
                p.Tick(0.5, groundY, maxX);
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
                switch (sm.Action)
                {
                    case ShellStrikeAction.MoveLeft:
                    {
                        int maxXBound = (_field.Terrain?.Heights?.Length ?? 800) - (int)tank.Width;
                        tank.Move(sm.Magnitude, 0, Math.Max(0, maxXBound));
                        break;
                    }
                    case ShellStrikeAction.MoveRight:
                    {
                        int maxXBound = (_field.Terrain?.Heights?.Length ?? 800) - (int)tank.Width;
                        tank.Move(sm.Magnitude, 0, Math.Max(0, maxXBound));
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
