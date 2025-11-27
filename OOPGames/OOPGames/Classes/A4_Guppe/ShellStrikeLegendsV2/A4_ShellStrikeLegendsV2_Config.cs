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

        // Tank wheel pivot points (relative to tank origin: X=center, Y=top)
        // Adjust to match the sprite's track wheel location.
        public const int TankPivotLeftXOffsetPx = -12;   // left wheel ~10px left of center
        public const int TankPivotLeftYOffsetPx = 17;    // wheel center ~2px above bottom
        public const int TankPivotRightXOffsetPx = 12;   // right wheel ~10px right of center
        public const int TankPivotRightYOffsetPx = 17;   // wheel center ~2px above bottom
    }
}
