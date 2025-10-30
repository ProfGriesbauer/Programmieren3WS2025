using System;

namespace OOPGames
{
    public class A4_ComputerUnbeatable : X_BaseComputerTicTacToePlayer
    {
        private int _PlayerNumber = 0;

        public override string Name { get { return "A4 Computer (Unbeatable)"; } }
        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            var c = new A4_ComputerUnbeatable();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            // Immediate win
            var win = FindImmediate(field, _PlayerNumber);
            if (win != null) return win;

            // Block
            var block = FindImmediate(field, _PlayerNumber == 1 ? 2 : 1);
            if (block != null) return new A4_TicTacToeMove(block.Row, block.Column, _PlayerNumber);

            // Minimax search for best move
            var best = FindBestMove(field);
            if (best.row >= 0) return new A4_TicTacToeMove(best.row, best.col, _PlayerNumber);
            return null;
        }

        private IX_TicTacToeMove FindImmediate(IX_TicTacToeField field, int player)
        {
            int N = A4_TicTacToeConfig.N;
            // rows
            for (int i = 0; i < N; i++)
            {
                int count = 0; int empty = -1;
                for (int j = 0; j < N; j++) { if (field[i, j] == player) count++; else if (field[i, j] <= 0) empty = j; }
                if (count == N - 1 && empty >= 0) return new A4_TicTacToeMove(i, empty, _PlayerNumber);
            }
            // cols
            for (int j = 0; j < N; j++)
            {
                int count = 0; int empty = -1;
                for (int i = 0; i < N; i++) { if (field[i, j] == player) count++; else if (field[i, j] <= 0) empty = i; }
                if (count == N - 1 && empty >= 0) return new A4_TicTacToeMove(empty, j, _PlayerNumber);
            }
            // diag
            int cnt = 0; int emp = -1;
            for (int i = 0; i < N; i++) { if (field[i, i] == player) cnt++; else if (field[i, i] <= 0) emp = i; }
            if (cnt == N - 1 && emp >= 0) return new A4_TicTacToeMove(emp, emp, _PlayerNumber);
            cnt = 0; emp = -1;
            for (int i = 0; i < N; i++) { if (field[i, N - 1 - i] == player) cnt++; else if (field[i, N - 1 - i] <= 0) emp = i; }
            if (cnt == N - 1 && emp >= 0) return new A4_TicTacToeMove(emp, N - 1 - emp, _PlayerNumber);
            return null;
        }

        private (int row, int col) FindBestMove(IX_TicTacToeField field)
        {
            int N = A4_TicTacToeConfig.N;
            int bestScore = int.MinValue;
            (int row, int col) bestMove = (-1, -1);

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (field[i, j] <= 0)
                    {
                        var tmp = CloneField(field);
                        tmp[i, j] = _PlayerNumber;
                        int score = Minimax(tmp, 0, false, int.MinValue, int.MaxValue);
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

        private int Minimax(IX_TicTacToeField field, int depth, bool isMax, int alpha, int beta)
        {
            int winner = CheckWinner(field);
            if (winner == _PlayerNumber) return 100 - depth; // prefer fast wins
            int opp = _PlayerNumber == 1 ? 2 : 1;
            if (winner == opp) return depth - 100; // prefer slow losses
            if (IsBoardFull(field)) return 0;

            int N = A4_TicTacToeConfig.N;
            if (isMax)
            {
                int best = int.MinValue;
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (field[i, j] <= 0)
                        {
                            var tmp = CloneField(field);
                            tmp[i, j] = _PlayerNumber;
                            int val = Minimax(tmp, depth + 1, false, alpha, beta);
                            best = Math.Max(best, val);
                            alpha = Math.Max(alpha, val);
                            if (beta <= alpha) return best;
                        }
                    }
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (field[i, j] <= 0)
                        {
                            var tmp = CloneField(field);
                            tmp[i, j] = opp;
                            int val = Minimax(tmp, depth + 1, true, alpha, beta);
                            best = Math.Min(best, val);
                            beta = Math.Min(beta, val);
                            if (beta <= alpha) return best;
                        }
                    }
                }
                return best;
            }
        }

        private bool IsBoardFull(IX_TicTacToeField field)
        {
            int N = A4_TicTacToeConfig.N;
            for (int i = 0; i < N; i++) for (int j = 0; j < N; j++) if (field[i, j] <= 0) return false;
            return true;
        }

        private int CheckWinner(IX_TicTacToeField field)
        {
            int N = A4_TicTacToeConfig.N;
            // rows
            for (int i = 0; i < N; i++)
            {
                if (field[i, 0] > 0)
                {
                    bool ok = true; int v = field[i, 0];
                    for (int j = 1; j < N; j++) if (field[i, j] != v) { ok = false; break; }
                    if (ok) return v;
                }
            }
            // cols
            for (int j = 0; j < N; j++)
            {
                if (field[0, j] > 0)
                {
                    bool ok = true; int v = field[0, j];
                    for (int i = 1; i < N; i++) if (field[i, j] != v) { ok = false; break; }
                    if (ok) return v;
                }
            }
            // diag
            if (field[0, 0] > 0)
            {
                bool ok = true; int v = field[0, 0];
                for (int i = 1; i < N; i++) if (field[i, i] != v) { ok = false; break; }
                if (ok) return v;
            }
            if (field[0, N - 1] > 0)
            {
                bool ok = true; int v = field[0, N - 1];
                for (int i = 1; i < N; i++) if (field[i, N - 1 - i] != v) { ok = false; break; }
                if (ok) return v;
            }
            return 0;
        }

        private IX_TicTacToeField CloneField(IX_TicTacToeField field)
        {
            var newField = new A4_TicTacToeField();
            int N = A4_TicTacToeConfig.N;
            for (int i = 0; i < N; i++) for (int j = 0; j < N; j++) newField[i, j] = field[i, j];
            return newField;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
