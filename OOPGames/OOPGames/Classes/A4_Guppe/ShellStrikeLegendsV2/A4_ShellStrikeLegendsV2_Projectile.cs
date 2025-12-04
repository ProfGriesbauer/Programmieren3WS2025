namespace OOPGames
{
    // V2 Projectile placeholder (no physics yet)
    public class A4_ShellStrikeLegendsV2_Projectile
    {
        public bool IsActive { get; set; } = false;

        public double X { get; set; }
        public double Y { get; set; }

        public double VX { get; set; }
        public double VY { get; set; }

        // Optional für Rendering
        public double Radius => 3.0;
    }
}
