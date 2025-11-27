using System;

namespace OOPGames
{
    // Wrapper, damit das Framework ein IGameField bekommt
    public sealed class A2_ConquestGameField : IGameField
    {
        public Game Game { get; }

        public A2_ConquestGameField(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public bool CanBePaintedBy(IPaintGame painter) => painter is A2_ConquestPainter;
    }

    // Click -> Tile Koordinate (Row = y, Column = x)
    public sealed class A2_ConquestMove : IRowMove, IColumnMove
    {
        public int PlayerNumber { get; }
        public int Row { get; }       // y
        public int Column { get; }    // x

        public A2_ConquestMove(int playerNumber, int row, int column)
        {
            PlayerNumber = playerNumber;
            Row = row;
            Column = column;
        }
    }

    // Immer erlaubter Zug, damit Computer niemals "null" liefern muss
    public sealed class A2_ConquestPassMove : IPlayMove
    {
        public int PlayerNumber { get; }
        public A2_ConquestPassMove(int playerNumber) => PlayerNumber = playerNumber;
    }
}
