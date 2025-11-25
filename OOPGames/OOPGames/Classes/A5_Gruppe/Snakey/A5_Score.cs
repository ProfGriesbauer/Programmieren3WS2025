namespace OOPGames
{
    /// <summary>
    /// Score-Counter für das Snake-Spiel mit Unterstützung für zwei Spieler
    /// </summary>
    public static class A5_Score
    {
        public static int Score1 { get; private set; } = 0;
        public static int Score2 { get; private set; } = 0;

        public static void Reset()
        {
            Score1 = 0;
            Score2 = 0;
        }

        public static void AddScore1(int v = 1) => Score1 += v;

        public static void AddScore2(int v = 1) => Score2 += v;
    }
}
