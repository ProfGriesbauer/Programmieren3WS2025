using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames.B1_Gruppe
{
    // Einfaches 3x3 Board als Domain-Modell
    public class B1_Board
    {
        public int Size { get; } = 3;
        private readonly B1_Cell[,] _cells;

        public B1_Board(int size = 3)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            _cells = new B1_Cell[Size, Size];
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    _cells[r, c] = new B1_Cell(r, c);
        }

        public B1_Cell GetCell(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size) return null;
            return _cells[row, col];
        }

        public bool MakeMove(int row, int col, B1_Symbol symbol)
        {
            var cell = GetCell(row, col);
            if (cell == null) return false;
            return cell.SetSymbol(symbol);
        }

        public void Clear()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    _cells[r, c].Clear();
        }

        // Forcefully sets the symbol at the given cell (overwrites existing symbol)
        public void SetCellSymbol(int row, int col, B1_Symbol symbol)
        {
            var cell = GetCell(row, col);
            if (cell == null) return;
            // overwrite directly
            cell.Clear();
            if (symbol != null)
            {
                // ignore return value since we just cleared
                cell.SetSymbol(symbol);
            }
        }

        public bool IsFull() => _cells.Cast<B1_Cell>().All(c => !c.IsEmpty);

        // Gibt das Symbol zurück, das gewonnen hat, oder null
        public B1_Symbol CheckWinner()
        {
            // Reihen
            for (int r = 0; r < Size; r++)
            {
                var first = _cells[r, 0].OccupiedBy;
                if (first == null) continue;
                bool all = true;
                for (int c = 1; c < Size; c++) if (_cells[r, c].OccupiedBy?.GetType() != first.GetType()) { all = false; break; }
                if (all) return first;
            }

            // Spalten
            for (int c = 0; c < Size; c++)
            {
                var first = _cells[0, c].OccupiedBy;
                if (first == null) continue;
                bool all = true;
                for (int r = 1; r < Size; r++) if (_cells[r, c].OccupiedBy?.GetType() != first.GetType()) { all = false; break; }
                if (all) return first;
            }

            // Diagonalen
            var d1 = _cells[0, 0].OccupiedBy;
            if (d1 != null)
            {
                bool all = true;
                for (int i = 1; i < Size; i++) if (_cells[i, i].OccupiedBy?.GetType() != d1.GetType()) { all = false; break; }
                if (all) return d1;
            }

            var d2 = _cells[0, Size - 1].OccupiedBy;
            if (d2 != null)
            {
                bool all = true;
                for (int i = 1; i < Size; i++) if (_cells[i, Size - 1 - i].OccupiedBy?.GetType() != d2.GetType()) { all = false; break; }
                if (all) return d2;
            }

            return null;
        }

        public IEnumerable<B1_Cell> EmptyCells()
        {
            return _cells.Cast<B1_Cell>().Where(c => c.IsEmpty);
        }
    }
}
