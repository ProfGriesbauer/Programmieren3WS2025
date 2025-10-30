using System;

namespace OOPGames
{
    public class A4_TicTacToeField : X_BaseTicTacToeField
    {
        int[,] _Field = new int[A4_TicTacToeConfig.N, A4_TicTacToeConfig.N];

        public A4_TicTacToeField()
        {
            for (int i = 0; i < A4_TicTacToeConfig.N; i++) for (int j = 0; j < A4_TicTacToeConfig.N; j++) _Field[i, j] = 0;
        }

        public override int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < A4_TicTacToeConfig.N && c >= 0 && c < A4_TicTacToeConfig.N)
                {
                    return _Field[r, c];
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (r >= 0 && r < A4_TicTacToeConfig.N && c >= 0 && c < A4_TicTacToeConfig.N)
                {
                    _Field[r, c] = value;
                }
            }
        }
    }
}
