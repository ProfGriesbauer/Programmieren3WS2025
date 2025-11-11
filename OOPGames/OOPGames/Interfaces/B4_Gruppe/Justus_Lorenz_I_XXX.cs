using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using OOPGames;

namespace OOPGames
{
     public interface B4_Lorenz_PaintTicTacToe : IPaintGame
    {
        void PaintTicTacToeField(Canvas canvas, B4_Lorenz_TicTacToeField currentField);
    }

    /// <summary>
    /// 3x3 TicTacToe-Spielfeld mit Indexer-Zugriff (B4_Lorenz)
    /// </summary>
    public interface B4_Lorenz_TicTacToeField : IGameField
    {
        int this[int r, int c] { get; set; }
    }

    /// <summary>
    /// Ein TicTacToe-Spielzug mit Zeile und Spalte (B4_Lorenz)
    /// </summary>
    public interface B4_Lorenz_TicTacToeMove : IRowMove, IColumnMove
    {
        // Row und Column werden von IRowMove und IColumnMove geerbt
    }

    /// <summary>
    /// Spielregeln für TicTacToe (B4_Lorenz)
    /// </summary>
    public interface B4_Lorenz_TicTacToeRules : IGameRules
    {
        B4_Lorenz_TicTacToeField TicTacToeField { get; }
        void DoTicTacToeMove(B4_Lorenz_TicTacToeMove move);
    }

    /// <summary>
    /// Menschlicher TicTacToe-Spieler (B4_Lorenz)
    /// </summary>
    public interface B4_Lorenz_HumanTicTacToePlayer : IHumanGamePlayer
    {
        B4_Lorenz_TicTacToeMove GetMove(IMoveSelection selection, B4_Lorenz_TicTacToeField field);
    }

    /// <summary>
    /// Computer TicTacToe-Spieler (B4_Lorenz)
    /// </summary>
    public interface B4_Lorenz_ComputerTicTacToePlayer : IComputerGamePlayer
    {
        B4_Lorenz_TicTacToeMove GetMove(B4_Lorenz_TicTacToeField field);
    }
    public interface B4_Lorenz_TicTacToeMark
    {
        double X { get; }
        double Y { get; }
        double Size { get; }
        double Margin { get; }

        UIElement BuildElement();
    }
}

    public class X_TicTacToeRules : X_BaseTicTacToeRules
    {
        X_TicTacToeField _Field = new X_TicTacToeField();

        public override IX_TicTacToeField TicTacToeField { get { return _Field; } }

        public override bool MovesPossible 
        { 
            get 
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
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

        public override string Name { get { return "GriesbauerTicTacToeRules"; } }

        public override int CheckIfPLayerWon()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_Field[i, 0] > 0 && _Field[i, 0] == _Field[i, 1] && _Field[i, 1] == _Field[i, 2])
                {
                    return _Field[i, 0];
                }
                else if (_Field[0, i] > 0 && _Field[0, i] == _Field[1, i] && _Field[1, i] == _Field[2, i])
                {
                    return _Field[0, i];
                }
            }

            if (_Field[0, 0] > 0 && _Field[0, 0] == _Field[1, 1] && _Field[1, 1] == _Field[2, 2])
            {
                return _Field[0, 0];
            }
            else if (_Field[0, 2] > 0 && _Field[0, 2] == _Field[1, 1] && _Field[1, 1] == _Field[2, 0])
            {
                return _Field[0, 2];
            }

            return -1;
        }

        public override void ClearField()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _Field[i, j] = 0;
                }
            }
        }

        public override void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            if (move.Row >= 0 && move.Row < 3 && move.Column >= 0 && move.Column < 3)
            {
                _Field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }

    public class X_TicTacToeField : X_BaseTicTacToeField
    {
        int[,] _Field = new int[3, 3] { { 0, 0 , 0}, { 0, 0, 0 }, { 0, 0, 0 } };

        public override int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
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
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    _Field[r, c] = value;
                }
            }
        }
    }

    public class X_TicTacToeMove : IX_TicTacToeMove
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;

        public X_TicTacToeMove (int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }
    }

    public class X_TicTacToeHumanPlayer : X_BaseHumanTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "GriesbauerHumanTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            X_TicTacToeHumanPlayer ttthp = new X_TicTacToeHumanPlayer();
            ttthp.SetPlayerNumber(_PlayerNumber);
            return ttthp;
        }

        public override IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            if (selection is IClickSelection)
            {
                IClickSelection sel = (IClickSelection)selection;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (sel.XClickPos > 20 + (j * 100) && sel.XClickPos < 120 + (j * 100) &&
                            sel.YClickPos > 20 + (i * 100) && sel.YClickPos < 120 + (i * 100) &&
                            field[i, j] <= 0)
                        {
                            return new X_TicTacToeMove(i, j, _PlayerNumber);
                        }
                    }
                }
            }

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }

    public class X_TicTacToeComputerPlayer : X_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "GriesbauerComputerTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            X_TicTacToeComputerPlayer ttthp = new X_TicTacToeComputerPlayer();
            ttthp.SetPlayerNumber(_PlayerNumber);
            return ttthp;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            //Ein intelligenter Algorithmus wäre hier besser...
            // Hier könnte man z.B. Minimax oder Alpha-Beta-Pruning implementieren
            Random rand = new Random();
            int f = rand.Next(0, 8);
            for (int i = 0; i < 9; i++)
            {
                int c = f % 3;
                int r = ((f - c) / 3) % 3;
                if (field[r, c] <= 0)
                {
                    return new X_TicTacToeMove(r, c, _PlayerNumber);
                }
                else
                {
                    f++;
                }
            }

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
}


