using System;

namespace OOPGames
{
    /// <summary>
    /// Eine einfache Version des Computer-Spielers, der zufällige Züge macht
    /// und nur offensichtliche Gewinnzüge oder Blocks erkennt
    /// </summary>
    public class A5_ComputerEasy : X_BaseComputerTicTacToePlayer
    {
        private int _PlayerNumber = 0;
        private static Random _rand = new Random();

        // Display name shown in the UI dropdowns
        public override string Name { get { return "A5 Computer (Easy) TicTacToe"; } }
        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            A5_ComputerEasy cp = new A5_ComputerEasy();
            cp.SetPlayerNumber(_PlayerNumber);
            return cp;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            // Mit 50% Wahrscheinlichkeit einen offensichtlichen Zug machen
            if (_rand.NextDouble() > 0.5)
            {
                // Prüfe auf eigenen Gewinnzug
                var winningMove = FindWinningMove(field, _PlayerNumber);
                if (winningMove != null) return winningMove;

                // Prüfe auf Block des Gegners
                var blockingMove = FindWinningMove(field, _PlayerNumber == 1 ? 2 : 1);
                if (blockingMove != null)
                    return new A5_Move { Row = blockingMove.Row, Column = blockingMove.Column, PlayerNumber = _PlayerNumber };
            }

            // Ansonsten: Wähle zufällig ein freies Feld
            // Versuche aber zuerst die Mitte zu nehmen (mit 40% Chance)
            if (field[1, 1] <= 0 && _rand.NextDouble() < 0.4)
            {
                return new A5_Move { Row = 1, Column = 1, PlayerNumber = _PlayerNumber };
            }

            // Mache einen zufälligen Zug
            var emptySpots = new System.Collections.Generic.List<(int row, int col)>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (field[i, j] <= 0)
                    {
                        emptySpots.Add((i, j));
                    }
                }
            }

            if (emptySpots.Count > 0)
            {
                var move = emptySpots[_rand.Next(emptySpots.Count)];
                return new A5_Move { Row = move.row, Column = move.col, PlayerNumber = _PlayerNumber };
            }

            return null;
        }

        private IX_TicTacToeMove FindWinningMove(IX_TicTacToeField field, int playerNum)
        {
            // Prüfe Zeilen
            for (int i = 0; i < 3; i++)
            {
                if (field[i, 0] == playerNum && field[i, 1] == playerNum && field[i, 2] <= 0)
                    return new A5_Move { Row = i, Column = 2, PlayerNumber = _PlayerNumber };
                if (field[i, 0] == playerNum && field[i, 2] == playerNum && field[i, 1] <= 0)
                    return new A5_Move { Row = i, Column = 1, PlayerNumber = _PlayerNumber };
                if (field[i, 1] == playerNum && field[i, 2] == playerNum && field[i, 0] <= 0)
                    return new A5_Move { Row = i, Column = 0, PlayerNumber = _PlayerNumber };
            }

            // Prüfe Spalten
            for (int j = 0; j < 3; j++)
            {
                if (field[0, j] == playerNum && field[1, j] == playerNum && field[2, j] <= 0)
                    return new A5_Move { Row = 2, Column = j, PlayerNumber = _PlayerNumber };
                if (field[0, j] == playerNum && field[2, j] == playerNum && field[1, j] <= 0)
                    return new A5_Move { Row = 1, Column = j, PlayerNumber = _PlayerNumber };
                if (field[1, j] == playerNum && field[2, j] == playerNum && field[0, j] <= 0)
                    return new A5_Move { Row = 0, Column = j, PlayerNumber = _PlayerNumber };
            }

            // Prüfe Diagonalen
            if (field[0, 0] == playerNum && field[1, 1] == playerNum && field[2, 2] <= 0)
                return new A5_Move { Row = 2, Column = 2, PlayerNumber = _PlayerNumber };
            if (field[0, 0] == playerNum && field[2, 2] == playerNum && field[1, 1] <= 0)
                return new A5_Move { Row = 1, Column = 1, PlayerNumber = _PlayerNumber };
            if (field[1, 1] == playerNum && field[2, 2] == playerNum && field[0, 0] <= 0)
                return new A5_Move { Row = 0, Column = 0, PlayerNumber = _PlayerNumber };

            if (field[0, 2] == playerNum && field[1, 1] == playerNum && field[2, 0] <= 0)
                return new A5_Move { Row = 2, Column = 0, PlayerNumber = _PlayerNumber };
            if (field[0, 2] == playerNum && field[2, 0] == playerNum && field[1, 1] <= 0)
                return new A5_Move { Row = 1, Column = 1, PlayerNumber = _PlayerNumber };
            if (field[1, 1] == playerNum && field[2, 0] == playerNum && field[0, 2] <= 0)
                return new A5_Move { Row = 0, Column = 2, PlayerNumber = _PlayerNumber };

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}