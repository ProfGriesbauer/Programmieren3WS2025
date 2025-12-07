namespace OOPGames
{
    // V2 Config
    public static class A4_ShellStrikeLegendsV2_Config
    {
        // Constant downward speed in pixels per tick (~60Hz timer -> px per frame)
        public const double TankFallSpeedPxPerTick = 2.5;
        // Tank body render size (pixels)
        public const int TankBodyWidthPx = 30;
        public const int TankBodyHeightPx = 15;
        //Barrel Body render size (pixels)
        public static double BarrelWidthPx = 20;
        public static double BarrelHeightPx = 5;

        //Tank Spawn Position
        // Tank Startposition (von hier fällt er herunter)
        public static double TankSpawnX_Player1 = 80;    // links
        public static double TankSpawnX_Player2 = 800;   // rechts 
        public static double TankSpawnY = -10;  // negativ = über dem Screen 


        // Tank wheel pivot points (relative to tank origin: X=center, Y=top)
        // Adjust to match the sprite's track wheel location.
        public const int TankPivotLeftXOffsetPx = -12;   // left wheel ~10px left of center
        public const int TankPivotLeftYOffsetPx = 17;    // wheel center ~2px above bottom
        public const int TankPivotRightXOffsetPx = 12;   // right wheel ~10px right of center
        public const int TankPivotRightYOffsetPx = 17;   // wheel center ~2px above bottom

        // skalierte Ketten-Pivot-Offsets
        public static double TankPivotLeftXOffsetScaled => TankPivotLeftXOffsetPx * TankScale;
        public static double TankPivotRightXOffsetScaled => TankPivotRightXOffsetPx * TankScale;
        public static double TankPivotLeftYOffsetScaled => TankPivotLeftYOffsetPx * TankScale;
        public static double TankPivotRightYOffsetScaled => TankPivotRightYOffsetPx * TankScale;

        

        //Position des Barrels auf dem Tank
        public static double BarrelPivotOffsetX = -2;   // relativ zur Tankmitte
        public static double BarrelPivotOffsetY = 4;   // relativ zur Tankoberkante
        public static double BarrelPivotOffsetXScaled => BarrelPivotOffsetX * TankScale;//Skallierungsfaktor anwenden
        public static double BarrelPivotOffsetYScaled => BarrelPivotOffsetY * TankScale;

        // Skallierungsfaktor für den Panzer
        public static double TankScale = 1.6;
        // Horizontalgeschwindigkeit des Tanks (Pixel pro Move)
        public static double TankDriveSpeedPx = 6.0;
        // Wie stark das Rohr pro Eingabe (Taste) bewegt wird (Radiant)
        public static double BarrelAngleStepRad = 0.02;   // ~1.15° pro Tastendruck

        // Anfangsgeschwindigkeit des Projektils (Pixel pro Tick)
        public static double ProjectileStartSpeedPx = 10.0;

        // Gravitation für Projektil (Pixel pro Tick^2)
        public static double ProjectileGravityPx = 0.35;
    }
}
