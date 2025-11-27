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
}
