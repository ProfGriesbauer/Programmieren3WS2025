using System;

namespace OOPGames
{
    public class B5_TicTacToeField : IB5_TicTacToeField
    {
        private readonly int[,] _field = new int[3, 3];

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is B5_TicTacToePaint;
        }

        public IGameField Clone()
        {
            B5_TicTacToeField clone = new B5_TicTacToeField();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    clone._field[i, j] = _field[i, j];
                }
            }
            return clone;
        }

        public int GetFieldValue(int row, int col)
        {
            if (row >= 0 && row < 3 && col >= 0 && col < 3)
            {
                return _field[row, col];
            }
            return -1;
        }

        public bool SetFieldValue(int row, int col, int value)
        {
            if (row >= 0 && row < 3 && col >= 0 && col < 3 && (value == 0 || value == 1 || value == 2))
            {
                _field[row, col] = value;
                return true;
            }
            return false;
        }
    }

    public class B5_GameRules : IGameRules
    {
        public string Name => "B5 TicTacToe Rules";

        public IGameField CurrentField { get; private set; }

        // Alle möglichen Gewinnkombinationen für TicTacToe
        private readonly int[,] WinCombinations = new int[8, 3] {
            // Horizontale Reihen (3 Möglichkeiten)
            { 0, 1, 2 },  // Obere Reihe
            { 3, 4, 5 },  // Mittlere Reihe
            { 6, 7, 8 },  // Untere Reihe
            
            // Vertikale Reihen (3 Möglichkeiten)
            { 0, 3, 6 },  // Linke Spalte
            { 1, 4, 7 },  // Mittlere Spalte
            { 2, 5, 8 },  // Rechte Spalte
            
            // Diagonalen (2 Möglichkeiten)
            { 0, 4, 8 },  // Diagonal von links oben nach rechts unten
            { 2, 4, 6 }   // Diagonal von rechts oben nach links unten
        };

        public B5_GameRules()
        {
            CurrentField = new B5_TicTacToeField();
        }

        public bool MovesPossible
        {
            get
            {
                if (CurrentField is B5_TicTacToeField field)
                {
                    // Überprüfe, ob es noch leere Felder gibt
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            if (field.GetFieldValue(row, col) == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (CurrentField is B5_TicTacToeField field &&
                move is IRowMove rowMove &&
                move is IColumnMove colMove)
            {
                field.SetFieldValue(rowMove.Row, colMove.Column, move.PlayerNumber);
            }
        }

        public void ClearField()
        {
            if (CurrentField is B5_TicTacToeField field)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        field.SetFieldValue(row, col, 0);
                    }
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            if (CurrentField is B5_TicTacToeField field)
            {
                // Überprüfe alle möglichen Gewinnkombinationen
                for (int i = 0; i < 8; i++)
                {
                    // Hole die drei Positionen der aktuellen Kombination
                    int pos1 = WinCombinations[i, 0];
                    int pos2 = WinCombinations[i, 1];
                    int pos3 = WinCombinations[i, 2];

                    // Konvertiere die Positionen in Zeilen und Spalten
                    int row1 = pos1 / 3;
                    int col1 = pos1 % 3;
                    int row2 = pos2 / 3;
                    int col2 = pos2 % 3;
                    int row3 = pos3 / 3;
                    int col3 = pos3 % 3;

                    // Hole die Werte der drei Positionen
                    int val1 = field.GetFieldValue(row1, col1);
                    int val2 = field.GetFieldValue(row2, col2);
                    int val3 = field.GetFieldValue(row3, col3);

                    // Wenn alle drei Werte gleich sind und nicht 0 (leeres Feld),
                    // haben wir einen Gewinner gefunden
                    if (val1 != 0 && val1 == val2 && val2 == val3)
                    {
                        return val1; // Gibt die Spielernummer zurück (1 oder 2)
                    }
                }
            }
            return -1; // Kein Gewinner

        }
    }
}
