namespace OOPGames
{
    public sealed class Troop
    {
        public int OwnerId { get; }
        public int LocalIndex { get; } // 0 oder 1 pro Spieler

        public int X { get; set; }
        public int Y { get; set; }

        public Troop(int ownerId, int localIndex, int x, int y)
        {
            OwnerId = ownerId;
            LocalIndex = localIndex;
            X = x;
            Y = y;
        }
    }
}
