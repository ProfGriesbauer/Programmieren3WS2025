using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    // B5 Gruppe: eigene TicTacToe-Implementierung
    public class B5_TicTacToePaint : X_BaseTicTacToePaint
    {
        public override string Name { get { return "B5_TicTacToePaint"; } }

        public override void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            Brush lineStroke = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            Brush XStroke = new SolidColorBrush(Color.FromRgb(200, 20, 20));
            Brush OStroke = new SolidColorBrush(Color.FromRgb(20, 60, 200));

            // grid lines (same coordinates as reference implementation)
            canvas.Children.Add(new Line() { X1 = 120, Y1 = 20, X2 = 120, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 });
            canvas.Children.Add(new Line() { X1 = 220, Y1 = 20, X2 = 220, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 });
            canvas.Children.Add(new Line() { X1 = 20, Y1 = 120, X2 = 320, Y2 = 120, Stroke = lineStroke, StrokeThickness = 3.0 });
            canvas.Children.Add(new Line() { X1 = 20, Y1 = 220, X2 = 320, Y2 = 220, Stroke = lineStroke, StrokeThickness = 3.0 });

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (currentField[i, j] == 1)
                    {
                        Line X1 = new Line() { X1 = 20 + (j * 100), Y1 = 20 + (i * 100), X2 = 120 + (j * 100), Y2 = 120 + (i * 100), Stroke = XStroke, StrokeThickness = 4.0 };
                        Line X2 = new Line() { X1 = 20 + (j * 100), Y1 = 120 + (i * 100), X2 = 120 + (j * 100), Y2 = 20 + (i * 100), Stroke = XStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(X1);
                        canvas.Children.Add(X2);
                    }
                    else if (currentField[i, j] == 2)
                    {
                        Ellipse OE = new Ellipse() { Margin = new Thickness(20 + (j * 100), 20 + (i * 100), 0, 0), Width = 100, Height = 100, Stroke = OStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(OE);
                    }
                }
            }
        }
    }

    public class B5_TicTacToeField : X_BaseTicTacToeField
    {
        int[,] _Field = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

        public override int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    return _Field[r, c];
                }
                return -1;
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

    public class B5_TicTacToeRules : X_BaseTicTacToeRules
    {
        B5_TicTacToeField _Field = new B5_TicTacToeField();

        public override IX_TicTacToeField TicTacToeField { get { return _Field; } }

        public override bool MovesPossible
        {
            get
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (_Field[i, j] == 0) return true;
                return false;
            }
        }

        public override string Name { get { return "B5_TicTacToeRules"; } }

        public override int CheckIfPLayerWon()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_Field[i, 0] > 0 && _Field[i, 0] == _Field[i, 1] && _Field[i, 1] == _Field[i, 2])
                    return _Field[i, 0];
                if (_Field[0, i] > 0 && _Field[0, i] == _Field[1, i] && _Field[1, i] == _Field[2, i])
                    return _Field[0, i];
            }

            if (_Field[0, 0] > 0 && _Field[0, 0] == _Field[1, 1] && _Field[1, 1] == _Field[2, 2])
                return _Field[0, 0];
            if (_Field[0, 2] > 0 && _Field[0, 2] == _Field[1, 1] && _Field[1, 1] == _Field[2, 0])
                return _Field[0, 2];

            return -1;
        }

        public override void ClearField()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _Field[i, j] = 0;
        }

        public override void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            if (move.Row >= 0 && move.Row < 3 && move.Column >= 0 && move.Column < 3)
            {
                // only set if empty
                if (_Field[move.Row, move.Column] == 0)
                    _Field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }

    public class B5_TicTacToeMove : IX_TicTacToeMove
    {
        int _Row;
        int _Col;
        int _Player;

        public B5_TicTacToeMove(int row, int col, int player)
        {
            _Row = row; _Col = col; _Player = player;
        }

        public int Row { get { return _Row; } }
        public int Column { get { return _Col; } }
        public int PlayerNumber { get { return _Player; } }
    }

    public class B5_TicTacToeHumanPlayer : X_BaseHumanTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B5_HumanTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B5_TicTacToeHumanPlayer p = new B5_TicTacToeHumanPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
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
                            field[i, j] == 0)
                        {
                            return new B5_TicTacToeMove(i, j, _PlayerNumber);
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

    public class B5_TicTacToeComputerPlayer : X_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B5_ComputerTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B5_TicTacToeComputerPlayer p = new B5_TicTacToeComputerPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            Random rand = new Random();
            int start = rand.Next(0, 9);
            for (int i = 0; i < 9; i++)
            {
                int idx = (start + i) % 9;
                int r = idx / 3;
                int c = idx % 3;
                if (field[r, c] == 0)
                    return new B5_TicTacToeMove(r, c, _PlayerNumber);
            }
            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
