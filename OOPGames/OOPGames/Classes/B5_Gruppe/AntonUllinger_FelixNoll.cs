using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OOPGames;

namespace OOPGames
{
    public class B5_HumanPlayer : IHumanGamePlayer
    {
        public string Name { get { return "B5 Human Player"; } }

        public int PlayerNumber { get; private set; }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B5_GameRules;
        }

        public IGamePlayer Clone()
        {
            B5_HumanPlayer clone = new B5_HumanPlayer();
            clone.SetPlayerNumber(PlayerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IClickSelection click)
            {
                // Benutze eine feste Zellengröße von 100x100 Pixeln
                const int cellSize = 100;
                int row = click.YClickPos / cellSize;
                int col = click.XClickPos / cellSize;

                // Prüfe, ob die Position gültig ist
                if (row >= 0 && row < 3 && col >= 0 && col < 3)
                {
                    // Prüfe, ob das Feld noch frei ist
                    if (field is IB5_TicTacToeField ticTacToeField &&
                        ticTacToeField.GetFieldValue(row, col) == 0)
                    {
                        return new B5_TicTacToeMove(PlayerNumber, row, col);
                    }
                }
            }
            return null;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }
    }

    public class B5_TicTacToeMove : IPlayMove, IRowMove, IColumnMove
    {
        public int PlayerNumber { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }

        public B5_TicTacToeMove(int playerNumber, int row, int col)
        {
            PlayerNumber = playerNumber;
            Row = row;
            Column = col;
        }
    }
}
