using System;

namespace OOPGames.B1_Gruppe
{
    public class B1_Cell
    {
        public int Row { get; }
        public int Col { get; }
        public B1_Symbol OccupiedBy { get; private set; }

        public B1_Cell(int row, int col)
        {
            Row = row;
            Col = col;
            OccupiedBy = null;
        }

        public bool IsEmpty => OccupiedBy == null;

        public bool SetSymbol(B1_Symbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            if (!IsEmpty) return false;
            OccupiedBy = symbol;
            return true;
        }

        public void Clear() => OccupiedBy = null;
    }
}
