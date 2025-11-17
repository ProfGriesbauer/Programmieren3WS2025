using System;

namespace OOPGames
{
    public class A4_TicTacToeRules : X_BaseTicTacToeRules
    {
        A4_TicTacToeField _Field = new A4_TicTacToeField();

        public override IX_TicTacToeField TicTacToeField { get { return _Field; } }

        public override bool MovesPossible
        {
            get
            {
                int N = 4;
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (_Field[i, j] == 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public override string Name { get { return "A4_TicTacToeRules"; } }

        public override int CheckIfPLayerWon()
        {
            int N = A4_TicTacToeConfig.N;
            // rows
            for (int i = 0; i < N; i++)
            {
                int first = _Field[i, 0];
                if (first > 0)
                {
                    bool ok = true;
                    for (int j = 1; j < N; j++) if (_Field[i, j] != first) { ok = false; break; }
                    if (ok) return first;
                }
            }

            // cols
            for (int j = 0; j < N; j++)
            {
                int first = _Field[0, j];
                if (first > 0)
                {
                    bool ok = true;
                    for (int i = 1; i < N; i++) if (_Field[i, j] != first) { ok = false; break; }
                    if (ok) return first;
                }
            }

            // main diagonal
            int dfirst = _Field[0, 0];
            if (dfirst > 0)
            {
                bool ok = true;
                for (int i = 1; i < N; i++) if (_Field[i, i] != dfirst) { ok = false; break; }
                if (ok) return dfirst;
            }

            // anti diagonal
            dfirst = _Field[0, N - 1];
            if (dfirst > 0)
            {
                bool ok = true;
                for (int i = 1; i < N; i++) if (_Field[i, N - 1 - i] != dfirst) { ok = false; break; }
                if (ok) return dfirst;
            }

            return -1;
        }

        public override void ClearField()
        {
            int N = A4_TicTacToeConfig.N;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    _Field[i, j] = 0;
                }
            }
        }

        public override void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            int N = A4_TicTacToeConfig.N;
            if (move.Row >= 0 && move.Row < N && move.Column >= 0 && move.Column < N)
            {
                _Field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }
}
