using System;

namespace OOPGames
{
    /// <summary>
    /// Das Spielfeld-Objekt: Verantwortlich für die Datenhaltung des Spielfelds
    /// </summary>
    public class A5_Field : IX_TicTacToeField
    {
        // Private Feld-Variable: Speichert den Zustand des 3x3 Spielfelds
        // 0 = leer, 1 = Spieler 1 (X), 2 = Spieler 2 (O)
        private int[,] _field = new int[3, 3];

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IX_PaintTicTacToe;
        }

        public int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    return _field[r, c];
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    _field[r, c] = value;
                }
            }
        }
    }
}