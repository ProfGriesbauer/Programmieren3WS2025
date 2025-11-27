namespace OOPGames
{
    public sealed class Troop
    {
        public int OwnerId { get; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public Troop(int ownerId, int x, int y)
        {
            OwnerId = ownerId;
            X = x;
            Y = y;
        }

        // Für später (Movement):
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
