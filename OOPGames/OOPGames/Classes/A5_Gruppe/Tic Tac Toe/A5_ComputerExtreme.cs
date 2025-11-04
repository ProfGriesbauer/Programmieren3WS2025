using System;

namespace OOPGames
{
    /// <summary>
    /// Enhanced computer player for TicTacToe using minimax algorithm with alpha-beta pruning
    /// Features:
    /// - Optimal move calculation using minimax
    /// - Alpha-beta pruning for better performance
    /// - Immediate win/block detection
    /// - Center and corner move prioritization
    /// </summary>
    public class A5_ComputerExtreme : X_BaseComputerTicTacToePlayer
    {
        private int _PlayerNumber = 0;
        private static Random _rand = new Random();

        // Display name shown in the UI dropdowns
        public override string Name { get { return "A5 Computer (Extreme) TicTacToe"; } }
        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            A5_ComputerExtreme cp = new A5_ComputerExtreme();
            cp.SetPlayerNumber(_PlayerNumber);
            return cp;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            // First, check for immediate winning move
            var winningMove = FindWinningMove(field, _PlayerNumber);
            if (winningMove != null) return winningMove;

            // Then, check if we need to block opponent's winning move
            var blockingMove = FindWinningMove(field, _PlayerNumber == 1 ? 2 : 1);
            if (blockingMove != null)
                return new A5_Move { Row = blockingMove.Row, Column = blockingMove.Column, PlayerNumber = _PlayerNumber };

            // If no immediate win/block, use minimax to find best move
            var bestMove = FindBestMove(field);
            return new A5_Move { Row = bestMove.row, Column = bestMove.col, PlayerNumber = _PlayerNumber };
        }

        private IX_TicTacToeMove FindWinningMove(IX_TicTacToeField field, int playerNum)
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (field[i, 0] == playerNum && field[i, 1] == playerNum && field[i, 2] <= 0)
                    return new A5_Move { Row = i, Column = 2, PlayerNumber = _PlayerNumber };
                if (field[i, 0] == playerNum && field[i, 2] == playerNum && field[i, 1] <= 0)
                    return new A5_Move { Row = i, Column = 1, PlayerNumber = _PlayerNumber };
                if (field[i, 1] == playerNum && field[i, 2] == playerNum && field[i, 0] <= 0)
                    return new A5_Move { Row = i, Column = 0, PlayerNumber = _PlayerNumber };
            }

            // Check columns
            for (int j = 0; j < 3; j++)
            {
                if (field[0, j] == playerNum && field[1, j] == playerNum && field[2, j] <= 0)
                    return new A5_Move { Row = 2, Column = j, PlayerNumber = _PlayerNumber };
                if (field[0, j] == playerNum && field[2, j] == playerNum && field[1, j] <= 0)
                    return new A5_Move { Row = 1, Column = j, PlayerNumber = _PlayerNumber };
                if (field[1, j] == playerNum && field[2, j] == playerNum && field[0, j] <= 0)
                    return new A5_Move { Row = 0, Column = j, PlayerNumber = _PlayerNumber };
            }

            // Check diagonals
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

        private (int row, int col) FindBestMove(IX_TicTacToeField field)
        {
            int bestScore = int.MinValue;
            (int row, int col) bestMove = (-1, -1);

            // Try each possible move
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Is the spot available?
                    if (field[i, j] <= 0)
                    {
                        // Make the move
                        var tempField = CloneField(field);
                        tempField[i, j] = _PlayerNumber;

                        // Find score for this move using minimax
                        int score = Minimax(tempField, 0, false, int.MinValue, int.MaxValue);

                        // If better score found, update best move
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = (i, j);
                        }
                    }
                }
            }

            return bestMove;
        }

        private int Minimax(IX_TicTacToeField field, int depth, bool isMaximizing, int alpha, int beta)
        {
            // Check for terminal states
            int winner = CheckWinner(field);
            if (winner == _PlayerNumber) return 10 - depth; // Win (prefer faster wins)
            if (winner == (_PlayerNumber == 1 ? 2 : 1)) return depth - 10; // Loss (prefer slower losses)
            if (IsBoardFull(field)) return 0; // Draw

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (field[i, j] <= 0)
                        {
                            var tempField = CloneField(field);
                            tempField[i, j] = _PlayerNumber;
                            int eval = Minimax(tempField, depth + 1, false, alpha, beta);
                            maxEval = Math.Max(maxEval, eval);
                            alpha = Math.Max(alpha, eval);
                            if (beta <= alpha)
                                break;
                        }
                    }
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                int opponent = _PlayerNumber == 1 ? 2 : 1;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (field[i, j] <= 0)
                        {
                            var tempField = CloneField(field);
                            tempField[i, j] = opponent;
                            int eval = Minimax(tempField, depth + 1, true, alpha, beta);
                            minEval = Math.Min(minEval, eval);
                            beta = Math.Min(beta, eval);
                            if (beta <= alpha)
                                break;
                        }
                    }
                }
                return minEval;
            }
        }

        private bool IsBoardFull(IX_TicTacToeField field)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] <= 0)
                        return false;
            return true;
        }

        private int CheckWinner(IX_TicTacToeField field)
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (field[i, 0] > 0 && field[i, 0] == field[i, 1] && field[i, 0] == field[i, 2])
                    return field[i, 0];
            }

            // Check columns
            for (int j = 0; j < 3; j++)
            {
                if (field[0, j] > 0 && field[0, j] == field[1, j] && field[0, j] == field[2, j])
                    return field[0, j];
            }

            // Check diagonals
            if (field[0, 0] > 0 && field[0, 0] == field[1, 1] && field[0, 0] == field[2, 2])
                return field[0, 0];
            if (field[0, 2] > 0 && field[0, 2] == field[1, 1] && field[0, 2] == field[2, 0])
                return field[0, 2];

            return 0;
        }

        private IX_TicTacToeField CloneField(IX_TicTacToeField field)
        {
            A5_Field newField = new A5_Field();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    newField[i, j] = field[i, j];
            return newField;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}