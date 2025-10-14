using System;
using System.Linq;

namespace SebastiansTikTacToe
{
    // Simple TicTacToe game logic
    public class TicTacToeGame
    {
        private int[,] _board = new int[3,3];
        public int CurrentPlayer { get; private set; } = 1; // 1=X, 2=O
        public int Winner { get; private set; } = 0; // 0 none, 1/2 winner
        public bool IsDraw { get; private set; } = false;
        public bool PlayAgainstComputer { get; private set; } = true;
        public bool IsComputerTurn => PlayAgainstComputer && CurrentPlayer == 2;
        public bool IsGameOver => Winner != 0 || IsDraw;

        public void Reset(bool playAgainstComputer = true, bool computerStarts = false)
        {
            _board = new int[3,3];
            PlayAgainstComputer = playAgainstComputer;
            CurrentPlayer = computerStarts ? 2 : 1;
            Winner = 0;
            IsDraw = false;
        }

        public int GetCell(int r, int c) => _board[r,c];

        public bool MakeMove(int r, int c)
        {
            if (IsGameOver) return false;
            if (r < 0 || r > 2 || c < 0 || c > 2) return false;
            if (_board[r,c] != 0) return false;
            _board[r,c] = CurrentPlayer;
            UpdateState();
            if (!IsGameOver) CurrentPlayer = 3 - CurrentPlayer; // switch
            return true;
        }

        public void MakeComputerMove()
        {
            if (!IsComputerTurn || IsGameOver) return;
            // Simple strategy: center -> corners -> sides
            (int r,int c)?[] prefs = new (int,int)?[] { (1,1), (0,0), (0,2), (2,0), (2,2), (0,1), (1,0), (1,2), (2,1) };
            foreach (var p in prefs)
            {
                if (p.HasValue && _board[p.Value.r, p.Value.c] == 0)
                {
                    _board[p.Value.r, p.Value.c] = CurrentPlayer;
                    UpdateState();
                    if (!IsGameOver) CurrentPlayer = 3 - CurrentPlayer;
                    return;
                }
            }
        }

        private void UpdateState()
        {
            // check rows/cols
            for (int i = 0; i < 3; i++)
            {
                if (_board[i,0] != 0 && _board[i,0] == _board[i,1] && _board[i,1] == _board[i,2]) { Winner = _board[i,0]; return; }
                if (_board[0,i] != 0 && _board[0,i] == _board[1,i] && _board[1,i] == _board[2,i]) { Winner = _board[0,i]; return; }
            }
            if (_board[0,0] != 0 && _board[0,0] == _board[1,1] && _board[1,1] == _board[2,2]) { Winner = _board[0,0]; return; }
            if (_board[0,2] != 0 && _board[0,2] == _board[1,1] && _board[1,1] == _board[2,0]) { Winner = _board[0,2]; return; }

            // draw
            bool anyEmpty = _board.Cast<int>().Any(v => v == 0);
            if (!anyEmpty) { IsDraw = true; }
        }
    }
}
