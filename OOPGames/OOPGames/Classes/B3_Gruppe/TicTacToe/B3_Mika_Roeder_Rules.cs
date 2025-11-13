using System;

namespace OOPGames
{
    // Spielregeln für B3 TicTacToe
    public class B3_Mika_Roeder_Rules : IGameRules
    {
        private B3_Mika_Roeder_Field _field;
        private int _currentPlayerNumber = 1;

        public B3_Mika_Roeder_Rules()
        {
            _field = new B3_Mika_Roeder_Field();
            ClearField();
        }

        public string Name { get { return "B3 Mika Röder TicTacToe Rules"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (_field[r, c] == 0)
                            return true;
                    }
                }
                return false;
            }
        }

        public void ClearField()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _field[r, c] = 0;
                }
            }
            _currentPlayerNumber = 1;
        }

        public void DoMove(IPlayMove move)
        {
            if (move == null) return;

            // Wir erwarten ein Move mit Row und Column
            if (move is IRowMove && move is IColumnMove)
            {
                int row = ((IRowMove)move).Row;
                int col = ((IColumnMove)move).Column;

                if (row >= 0 && row < 3 && col >= 0 && col < 3 && _field[row, col] == 0)
                {
                    _field[row, col] = move.PlayerNumber;
                    _currentPlayerNumber = (_currentPlayerNumber == 1) ? 2 : 1;
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            // Zeilen
            for (int r = 0; r < 3; r++)
            {
                if (_field[r, 0] != 0 && _field[r, 0] == _field[r, 1] && _field[r, 1] == _field[r, 2])
                    return _field[r, 0];
            }

            // Spalten
            for (int c = 0; c < 3; c++)
            {
                if (_field[0, c] != 0 && _field[0, c] == _field[1, c] && _field[1, c] == _field[2, c])
                    return _field[0, c];
            }

            // Diagonalen
            if (_field[0, 0] != 0 && _field[0, 0] == _field[1, 1] && _field[1, 1] == _field[2, 2])
                return _field[0, 0];
            if (_field[0, 2] != 0 && _field[0, 2] == _field[1, 1] && _field[1, 1] == _field[2, 0])
                return _field[0, 2];

            if (!MovesPossible)
                return -1; // Unentschieden

            return 0; // Spiel läuft
        }
    }
}
