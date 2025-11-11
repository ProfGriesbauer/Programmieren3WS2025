using System;
using System.Collections.Generic;
using System.Linq;


namespace OOPGames
{
    public class B4_TicTacToeMediumComputer : B4_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;
        private static readonly Random _rng = new Random(); // <--- einzelne Random-Instanz
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

            // Blockiere den gegnerischen Gewinnzug, falls möglich
            var block = FindWinningMove(field, opponent);
            if (block != null) return block;

            // Wähle das Zentrum manchmal (mit 50% Wahrscheinlichkeit)
            if (field[1, 1] == 0 && _rng.NextDouble() < 0.5)
                return new B4_TicTacToeMove(1, 1, _PlayerNumber);

            // Wähle eine freie Ecke manchmal (mit 50% Wahrscheinlichkeit)
            (int, int)[] corners = { (0, 0), (0, 2), (2, 0), (2, 2) };
            var freieEcken = corners.Where(c => field[c.Item1, c.Item2] == 0).ToList();
            if (freieEcken.Count > 0 && _rng.NextDouble() < 0.5)
            {
                var pos = freieEcken[_rng.Next(freieEcken.Count)];
                return new B4_TicTacToeMove(pos.Item1, pos.Item2, _PlayerNumber);
            }

            // Sonst wähle ein beliebiges freies Feld
            List<(int, int)> freieFelder = new List<(int, int)>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                        freieFelder.Add((i, j));
            if (freieFelder.Count > 0)
            {
                var pos = freieFelder[_rng.Next(freieFelder.Count)];
                return new B4_TicTacToeMove(pos.Item1, pos.Item2, _PlayerNumber);
            }

            return null;
        }

        // Prüft, ob ein Blockzug möglich ist („block“-Logik)
        private IB4_TicTacToeMove FindWinningMove(IB4_TicTacToeField field, int player)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                    {
                        if (WouldWin(field, player, i, j))
                            return new B4_TicTacToeMove(i, j, _PlayerNumber);
                    }
            return null;
        }

        // Prüft, ob das Platzieren von 'player' bei (row,col) sofort gewinnt (ohne das Feld zu verändern)
        private bool WouldWin(IB4_TicTacToeField field, int player, int row, int col)
        {
            // Reihe prüfen
            int count = 0;
            for (int c = 0; c < 3; c++)
                count += (c == col) ? 1 : (field[row, c] == player ? 1 : 0);
            if (count == 3) return true;

            // Spalte prüfen
            count = 0;
            for (int r = 0; r < 3; r++)
                count += (r == row) ? 1 : (field[r, col] == player ? 1 : 0);
            if (count == 3) return true;

            // Diagonale links oben -> rechts unten prüfen
            if (row == col)
            {
                count = 0;
                for (int k = 0; k < 3; k++)
                    count += (k == row) ? 1 : (field[k, k] == player ? 1 : 0);
                if (count == 3) return true;
            }

            // Diagonale rechts oben -> links unten prüfen
            if (row + col == 2)
            {
                count = 0;
                for (int k = 0; k < 3; k++)
                {
                    int r = k, c = 2 - k;
                    count += (r == row && c == col) ? 1 : (field[r, c] == player ? 1 : 0);
                }
                if (count == 3) return true;
            }

            return false;
        }

        // Gewinnkontrolle für ein gegebenes Feld
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
