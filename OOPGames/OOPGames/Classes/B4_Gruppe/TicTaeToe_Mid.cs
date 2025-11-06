using System;

namespace OOPGames
{
    public class B4_TicTacToeMediumComputer : B4_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;
        public override string Name { get { return "B4_TicTacToeMediumComputer"; } }
        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B4_TicTacToeMediumComputer player = new B4_TicTacToeMediumComputer();
            player.SetPlayerNumber(_PlayerNumber);
            return player;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        public override IB4_TicTacToeMove GetMove(IB4_TicTacToeField field)
        {
            int opponent = _PlayerNumber == 1 ? 2 : 1;

            // Check for winning move
            var win = FindWinningMove(field, _PlayerNumber);
            if (win != null) return win;

            // Block opponent's winning move
            var block = FindWinningMove(field, opponent);
            if (block != null) return block;

            // Pick the center if available
            if (field[1, 1] == 0)
                return new B4_TicTacToeMove(1, 1, _PlayerNumber);

            // Pick a corner if available
            (int, int)[] corners = { (0, 0), (0, 2), (2, 0), (2, 2) };
            foreach (var c in corners)
            {
                if (field[c.Item1, c.Item2] == 0)
                    return new B4_TicTacToeMove(c.Item1, c.Item2, _PlayerNumber);
            }

            // Otherwise, pick any open spot
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                        return new B4_TicTacToeMove(i, j, _PlayerNumber);

            return null;
        }

        // Hilfsmethode prüft, ob ein Gewinnerzug möglich ist
        private IB4_TicTacToeMove FindWinningMove(IB4_TicTacToeField field, int player)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                    {
                        field[i, j] = player;
                        if (IsWinner(field, player))
                        {
                            field[i, j] = 0;
                            return new B4_TicTacToeMove(i, j, _PlayerNumber);
                        }
                        field[i, j] = 0;
                    }
            return null;
        }

        // Einfache Gewinnkontrolle
        private bool IsWinner(IB4_TicTacToeField field, int player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (field[i, 0] == player && field[i, 1] == player && field[i, 2] == player) return true;
                if (field[0, i] == player && field[1, i] == player && field[2, i] == player) return true;
            }
            if (field[0, 0] == player && field[1, 1] == player && field[2, 2] == player) return true;
            if (field[0, 2] == player && field[1, 1] == player && field[2, 0] == player) return true;
            return false;
        }
    }
}

