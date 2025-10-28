using System;

namespace OOPGames
{
    // Simple computer player for TicTacToe (A5)
    // Picks a random empty cell; easy to replace with smarter logic later
    public class A5_ComputerPlayer : X_BaseComputerTicTacToePlayer
    {
        private int _PlayerNumber = 0;
        private static Random _rand = new Random();

    // Display name shown in the UI dropdowns
    public override string Name { get { return "A5 Computer Player"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            A5_ComputerPlayer cp = new A5_ComputerPlayer();
            cp.SetPlayerNumber(_PlayerNumber);
            return cp;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            // Simple random move: start from a random index and scan for empty
            int start = _rand.Next(0, 9);
            for (int i = 0; i < 9; i++)
            {
                int idx = (start + i) % 9;
                int r = idx / 3;
                int c = idx % 3;
                if (field[r, c] <= 0)
                {
                    return new A5_Move { Row = r, Column = c, PlayerNumber = _PlayerNumber };
                }
            }

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
