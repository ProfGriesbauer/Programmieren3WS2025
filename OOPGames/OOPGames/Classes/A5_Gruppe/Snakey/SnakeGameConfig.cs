namespace OOPGames
{

    /// Zentrale Konfiguration für das Snake-Spiel

    public class SnakeGameConfig
    {
        public int FieldWidth { get; }
        public int FieldHeight { get; }
        public int CellSize { get; }
        public double Speed { get; }
        public int TimerIntervalMs { get; }
        public double SegmentGap { get; }
        public int DeathMargin { get; }
        public int CountdownSeconds { get; }

        public SnakeGameConfig(
            int fieldWidth = 480,
            int fieldHeight = 480,
            int cellSize = 32,
            double speed = 4,
            int timerIntervalMs = 8,
            int deathMargin = 8,
            int countdownSeconds = 3)
        {
            FieldWidth = fieldWidth;
            FieldHeight = fieldHeight;
            CellSize = cellSize;
            Speed = speed;
            TimerIntervalMs = timerIntervalMs;
            SegmentGap = cellSize;
            DeathMargin = deathMargin;
            CountdownSeconds = countdownSeconds;
        }

        public int StepGap => System.Math.Max(1, (int)System.Math.Round(SegmentGap / Speed));
    }
}
