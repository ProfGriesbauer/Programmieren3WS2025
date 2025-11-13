using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A4_ComputerNormal : X_BaseComputerTicTacToePlayer
    {
        private int _PlayerNumber = 0;
        private static Random _rand = new Random();

        public override string Name { get { return "A4 Computer (Normal)"; } }
        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            var c = new A4_ComputerNormal();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            int N = A4_TicTacToeConfig.N;

            // Try immediate win
            var win = FindWinningMove(field, _PlayerNumber);
            if (win.row >= 0) return new A4_TicTacToeMove(win.row, win.col, _PlayerNumber);

            // Try block
            var block = FindWinningMove(field, _PlayerNumber == 1 ? 2 : 1);
            if (block.row >= 0) return new A4_TicTacToeMove(block.row, block.col, _PlayerNumber);

            // Prefer center-ish cells (for even N pick one of the central block)
            var preferred = new List<(int r, int c)>();
            if (N % 2 == 1)
            {
                int m = N / 2;
                preferred.Add((m, m));
            }
            else
            {
                int a = N / 2 - 1;
                int b = N / 2;
                preferred.Add((a, a)); preferred.Add((a, b)); preferred.Add((b, a)); preferred.Add((b, b));
            }

            foreach (var p in preferred)
            {
                if (field[p.r, p.c] <= 0) return new A4_TicTacToeMove(p.r, p.c, _PlayerNumber);
            }

            // Otherwise pick a random empty cell
            var empties = new List<(int r, int c)>();
            for (int i = 0; i < N; i++) for (int j = 0; j < N; j++) if (field[i, j] <= 0) empties.Add((i, j));

            if (empties.Count == 0) return null;
            var sel = empties[_rand.Next(empties.Count)];
            return new A4_TicTacToeMove(sel.r, sel.c, _PlayerNumber);
        }

        private (int row, int col) FindWinningMove(IX_TicTacToeField field, int playerNum)
        {
            int N = A4_TicTacToeConfig.N;
            // rows
            for (int i = 0; i < N; i++)
            {
                int count = 0; int emptyC = -1;
                for (int j = 0; j < N; j++)
                {
                    if (field[i, j] == playerNum) count++;
                    else if (field[i, j] <= 0) emptyC = j;
                }
                if (count == N - 1 && emptyC >= 0) return (i, emptyC);
            }

            // cols
            for (int j = 0; j < N; j++)
            {
                int count = 0; int emptyR = -1;
                for (int i = 0; i < N; i++)
                {
                    if (field[i, j] == playerNum) count++;
                    else if (field[i, j] <= 0) emptyR = i;
                }
                if (count == N - 1 && emptyR >= 0) return (emptyR, j);
            }

            // main diagonal
            int cdiag = 0; int emptyDiag = -1;
            for (int i = 0; i < N; i++)
            {
                if (field[i, i] == playerNum) cdiag++;
                else if (field[i, i] <= 0) emptyDiag = i;
            }
            if (cdiag == N - 1 && emptyDiag >= 0) return (emptyDiag, emptyDiag);

            // anti diagonal
            cdiag = 0; emptyDiag = -1;
            for (int i = 0; i < N; i++)
            {
                if (field[i, N - 1 - i] == playerNum) cdiag++;
                else if (field[i, N - 1 - i] <= 0) emptyDiag = i;
            }
            if (cdiag == N - 1 && emptyDiag >= 0) return (emptyDiag, N - 1 - emptyDiag);

            return (-1, -1);
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
