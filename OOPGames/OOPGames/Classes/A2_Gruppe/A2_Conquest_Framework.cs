namespace OOPGames
{
    public sealed class A2_ConquestGameField : IGameField
    {
        public Game Game { get; }
        public A2_ConquestGameField(Game game) => Game = game;
        public bool CanBePaintedBy(IPaintGame painter) => painter is A2_ConquestPainter;
    }

    public sealed class A2_ConquestTroopMove : IRowMove, IColumnMove
    {
        public int PlayerNumber { get; }
        public int Row { get; }
        public int Column { get; }
        public int TroopLocalIndex { get; }

        public A2_ConquestTroopMove(int playerNumber, int troopLocalIndex, int row, int column)
        {
            PlayerNumber = playerNumber;
            TroopLocalIndex = troopLocalIndex;
            Row = row;
            Column = column;
        }
    }

    public sealed class A2_ConquestPassMove : IPlayMove
    {
        public int PlayerNumber { get; }
        public A2_ConquestPassMove(int playerNumber) => PlayerNumber = playerNumber;
    }
}
