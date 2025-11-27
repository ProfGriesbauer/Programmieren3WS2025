namespace OOPGames
{
    // Minimal V2 tank body only (no barrel/projectiles)
    public class A4_ShellStrikeLegendsV2_Tank
    {
        // World position (x in canvas pixels); y is taken from terrain at X
        public double X { get; set; }
        public double Y { get; set; }

        // Asset path for the hull sprite
        public string HullSpritePath { get; set; } = "Assets/ShellStrikeLegends/camo-tank-4 smaller.png";
    }
}
