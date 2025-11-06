using System;

namespace OOPGames
{
    // Ein Zug für TicTacToe mit Zeile und Spalte
    public class B3_Mika_Roeder_Move : IRowMove, IColumnMove
    {
        private int _row;
        private int _column;
        private int _playerNumber;

        public B3_Mika_Roeder_Move(int row, int column, int playerNumber)
        {
            _row = row;
            _column = column;
            _playerNumber = playerNumber;
        }

        public int Row { get { return _row; } }
        public int Column { get { return _column; } }
        public int PlayerNumber { get { return _playerNumber; } }
    }
}
