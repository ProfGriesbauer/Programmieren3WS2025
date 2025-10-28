using System;

namespace OOPGames
{
    /// <summary>
    /// Die Spielregeln-Klasse: Enthält die gesamte Spiellogik
    /// </summary>
    public class A5_Rules : IX_TicTacToeRules
    {
        private A5_Field _field = new A5_Field();
        private int _currentPlayer = 1;

        public IX_TicTacToeField TicTacToeField { get { return _field; } }
        
        public void ClearField()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _field[i, j] = 0;
                }
            }
            _currentPlayer = 1;
        }

        public int CheckIfPLayerWon()
        {
            return GetWinner();
        }

        public bool MovesPossible 
        { 
            get 
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (_field[i, j] == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

    // Display name shown in the UI dropdowns
    public string Name { get { return "A5 Rules"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool CanBeWonBy(IGamePlayer player)
        {
            return player.PlayerNumber == 1 || player.PlayerNumber == 2;
        }

        public void DoMove(IPlayMove move)
        {
            if (move is IX_TicTacToeMove)
            {
                DoTicTacToeMove((IX_TicTacToeMove)move);
            }
        }

        public void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            if (_field[move.Row, move.Column] == 0)
            {
                _field[move.Row, move.Column] = _currentPlayer;
                _currentPlayer = _currentPlayer == 1 ? 2 : 1;
            }
        }

        private int GetWinner()
        {
            // Überprüfe Zeilen
            for (int i = 0; i < 3; i++)
            {
                if (_field[i, 0] != 0 && _field[i, 0] == _field[i, 1] && _field[i, 1] == _field[i, 2])
                {
                    return _field[i, 0];
                }
            }

            // Überprüfe Spalten
            for (int j = 0; j < 3; j++)
            {
                if (_field[0, j] != 0 && _field[0, j] == _field[1, j] && _field[1, j] == _field[2, j])
                {
                    return _field[0, j];
                }
            }

            // Überprüfe Diagonalen
            if (_field[0, 0] != 0 && _field[0, 0] == _field[1, 1] && _field[1, 1] == _field[2, 2])
            {
                return _field[0, 0];
            }
            if (_field[0, 2] != 0 && _field[0, 2] == _field[1, 1] && _field[1, 1] == _field[2, 0])
            {
                return _field[0, 2];
            }

            return -1;
        }
    }
}