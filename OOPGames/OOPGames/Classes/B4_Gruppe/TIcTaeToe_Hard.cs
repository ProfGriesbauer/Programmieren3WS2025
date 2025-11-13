using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class B4_TicTacToeHardComputer : B4_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B4_TicTacToeHardComputer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B4_TicTacToeHardComputer player = new B4_TicTacToeHardComputer();
            player.SetPlayerNumber(_PlayerNumber);
            return player;
        }

        public override IB4_TicTacToeMove GetMove(IB4_TicTacToeField field)
        {
            (int bestRow, int bestCol) = FindBestMove(field, _PlayerNumber);
            if (bestRow != -1 && bestCol != -1)
                return new B4_TicTacToeMove(bestRow, bestCol, _PlayerNumber);

            // Fallback falls kein sinnvoller Zug gefunden wurde
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] <= 0)
                        return new B4_TicTacToeMove(i, j, _PlayerNumber);

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        // Hilfsmethoden für Minimax:
        private int Minimax(IB4_TicTacToeField field, int depth, bool isMax, int me, int opponent)
        {
            int winner = Evaluate(field, me, opponent);
            if (winner != 0 || !MovesPossible(field))
                return winner;

            int best = isMax ? int.MinValue : int.MaxValue;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                    {
                        field[i, j] = isMax ? me : opponent;
                        int score = Minimax(field, depth + 1, !isMax, me, opponent);
                        field[i, j] = 0;
                        if (isMax)
                            best = Math.Max(best, score);
                        else
                            best = Math.Min(best, score);
                    }
            return best;
        }

        private bool MovesPossible(IB4_TicTacToeField field)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                        return true;
            return false;
        }

        private int Evaluate(IB4_TicTacToeField field, int me, int opponent)
        {
            // Zeilen, Spalten und Diagonalen überprüfen
            for (int i = 0; i < 3; i++)
            {
                if (field[i, 0] > 0 && field[i, 0] == field[i, 1] && field[i, 1] == field[i, 2])
                    return field[i, 0] == me ? 10 : -10;
                if (field[0, i] > 0 && field[0, i] == field[1, i] && field[1, i] == field[2, i])
                    return field[0, i] == me ? 10 : -10;
            }
            if (field[0, 0] > 0 && field[0, 0] == field[1, 1] && field[1, 1] == field[2, 2])
                return field[0, 0] == me ? 10 : -10;
            if (field[0, 2] > 0 && field[0, 2] == field[1, 1] && field[1, 1] == field[2, 0])
                return field[0, 2] == me ? 10 : -10;
            return 0;
        }

        private (int, int) FindBestMove(IB4_TicTacToeField field, int player)
        {
            int bestVal = int.MinValue;
            int row = -1, col = -1;
            int opponent = player == 1 ? 2 : 1;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (field[i, j] == 0)
                    {
                        field[i, j] = player;
                        int moveVal = Minimax(field, 0, false, player, opponent);
                        field[i, j] = 0;
                        if (moveVal > bestVal)
                        {
                            row = i; col = j; bestVal = moveVal;
                        }
                    }
            return (row, col);
        }
    }

}

