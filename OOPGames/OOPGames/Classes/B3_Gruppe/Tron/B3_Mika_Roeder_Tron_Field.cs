using System;

namespace OOPGames
{
    // Tron-Spielbrett: Raster mit Trails. 0 = leer, 1 = Spieler1, 2 = Spieler2
    public class B3_Mika_Roeder_Tron_Field : IGameField
    {
        private int[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }
    // Countdown in Sekunden before game starts (updated by rules)
    public int CountdownRemainingSeconds { get; set; }

    // Crash animation/state (set by rules when a player collides)
    public bool CrashActive { get; set; }
    public int CrashRow { get; set; }
    public int CrashCol { get; set; }
    public int CrashPlayerNumber { get; set; }
    // remaining animation ticks (tick ≈ 40ms)
    public int CrashRemainingTicks { get; set; }
    // whether flash is currently on (flashing animation)
    public bool CrashFlashOn { get; set; }

        public B3_Mika_Roeder_Tron_Field(int width = 60, int height = 40)
        {
            Width = width;
            Height = height;
            _grid = new int[Height, Width];
            CountdownRemainingSeconds = 0;
            CrashActive = false;
            CrashRow = -1; CrashCol = -1; CrashPlayerNumber = 0;
            CrashRemainingTicks = 0; CrashFlashOn = false;
        }

        public int this[int r, int c]
        {
            get { return _grid[r, c]; }
            set { _grid[r, c] = value; }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // Painter compatibility is generic — allow any painter.
            return painter != null;
        }

        // Helper to clear grid
        public void Clear()
        {
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    _grid[r, c] = 0;

            // clear crash state
            CrashActive = false;
            CrashRow = -1; CrashCol = -1; CrashPlayerNumber = 0;
            CrashRemainingTicks = 0; CrashFlashOn = false;
        }
    }
}
