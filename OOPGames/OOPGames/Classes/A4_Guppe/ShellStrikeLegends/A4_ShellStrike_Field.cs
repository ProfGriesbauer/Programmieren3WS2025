using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A4_ShellStrike_Field : IGameField
    {
        public A4_ShellStrike_Tank Tank1 { get; set; }
        public A4_ShellStrike_Tank Tank2 { get; set; }
        public List<A4_ShellStrike_Projectile> Projectiles { get; } = new List<A4_ShellStrike_Projectile>();
        public A4_ShellStrike_Terrain Terrain { get; set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A4_ShellStrike_Painter;
        }
    }

    public class A4_ShellStrike_Tank
    {
        public int PlayerNumber { get; }
        public double X { get; private set; }
        public double TurretAngleDeg { get; private set; } = 45; // default angle
        public int Health { get; private set; } = 3; // simple health
        public double Width { get; } = 40;
        public double Height { get; } = 20;
        public double TurretLength { get; } = 30;

        public A4_ShellStrike_Tank(int playerNumber, double startX)
        {
            PlayerNumber = playerNumber;
            X = startX;
        }

        public void Move(double delta, double minX, double maxX)
        {
            X = Math.Max(minX, Math.Min(maxX, X + delta));
        }

        public void AdjustTurret(double deltaDeg)
        {
            TurretAngleDeg = Math.Max(5, Math.Min(85, TurretAngleDeg + deltaDeg));
        }

        public A4_ShellStrike_Projectile Fire(double floorY)
        {
            // Fire projectile from the end of the turret
            double rad = TurretAngleDeg * Math.PI / 180.0;
            double sign = PlayerNumber == 1 ? 1.0 : -1.0;
            double cx = X + Width / 2.0;            // turret base center x
            double cy = floorY - Height;            // turret base y (top of tank)
            double tipX = cx + TurretLength * Math.Cos(rad) * sign; // turret end x
            double tipY = cy - TurretLength * Math.Sin(rad);        // turret end y
            double speed = 12.0;
            double vx = speed * Math.Cos(rad) * sign;               // horizontal velocity with direction
            double vy = -speed * Math.Sin(rad);                     // upward negative y
            return new A4_ShellStrike_Projectile(tipX, tipY, vx, vy);
        }

        public void Hit()
        {
            if (Health > 0) Health--;
        }
    }
}
