namespace OOPGames
{

    /// Score-Counter für das Snake-Spiel mit Unterstützung für zwei Spieler und Highscore

    public static class A5_Score
    {
        private static int _score1 = 0;
        private static int _score2 = 0;
        public static int HighScore1 { get; private set; } = 0;
        public static int HighScore2 { get; private set; } = 0;

        public static int Score1
        {
            get => _score1;
            private set
            {
                _score1 = value;
                // Update high score immediately if current score is higher
                if (_score1 > HighScore1)
                    HighScore1 = _score1;
            }
        }

        public static int Score2
        {
            get => _score2;
            private set
            {
                _score2 = value;
                // Update high score immediately if current score is higher
                if (_score2 > HighScore2)
                    HighScore2 = _score2;
            }
        }

        public static void Reset()
        {
            Score1 = 0;
            Score2 = 0;
        }

        public static void AddScore1(int v = 1) => Score1 += v;

        public static void AddScore2(int v = 1) => Score2 += v;
    }
}
