namespace OOPGames
{
    /// <summary>
    /// Einfacher globaler Score-Counter für das Snake-Spiel
    /// </summary>
    public static class A5_Score
    {
        public static int Value { get; private set; } = 0;

        public static void Reset() => Value = 0;

        public static void Add(int v = 1) => Value += v;
    }
}
