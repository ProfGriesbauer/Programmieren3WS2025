using System;
using System.Windows.Controls;

namespace OOPGames
{
    // Einfaches 3x3 TicTacToe-Feld für Gruppe B3
    public class B3_Mika_Roeder_Field : IGameField
    {
        private int[,] _field = new int[3, 3];

        // Indexer: ermöglicht Zugriff per field[row, col]
        public int this[int r, int c]
        {
            get { return _field[r, c]; }
            set { _field[r, c] = value; }
        }

        // Wir erlauben, dass jeder Painter das Feld malen kann.
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }
}
