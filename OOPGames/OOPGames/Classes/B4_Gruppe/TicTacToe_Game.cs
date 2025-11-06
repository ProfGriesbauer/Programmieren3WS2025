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
    public enum PlayerSymbol
    {
        Cross,
        Circle,
        Triangle,
        Rectangle
    }

    public class B4_TicTacToePaint : B4_BaseTicTacToePaint
    {
        private PlayerSymbol player1Symbol = PlayerSymbol.Cross;
        private PlayerSymbol player2Symbol = PlayerSymbol.Circle;
        private readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        public override string Name { get { return "B4_TicTacToe_Paint"; } }

        public void SetPlayerSymbol(int playerNumber, PlayerSymbol symbol)
        {
            if (playerNumber == 1)
                player1Symbol = symbol;
            else if (playerNumber == 2)
                player2Symbol = symbol;
        }

        private void DrawSymbol(Canvas canvas, PlayerSymbol symbol, int x, int y)
        {
            switch (symbol)
            {
                case PlayerSymbol.Cross:
                    Line X1 = new Line() { X1 = x, Y1 = y, X2 = x + 100, Y2 = y + 100, Stroke = blackBrush, StrokeThickness = 3.0 };
                    Line X2 = new Line() { X1 = x, Y1 = y + 100, X2 = x + 100, Y2 = y, Stroke = blackBrush, StrokeThickness = 3.0 };
                    canvas.Children.Add(X1);
                    canvas.Children.Add(X2);
                    break;

                case PlayerSymbol.Circle:
                    Ellipse circle = new Ellipse() 
                    { 
                        Width = 80, 
                        Height = 80, 
                        Stroke = blackBrush, 
                        StrokeThickness = 3.0,
                        Margin = new Thickness(x + 10, y + 10, 0, 0)
                    };
                    canvas.Children.Add(circle);
                    break;

                case PlayerSymbol.Triangle:
                    Polygon triangle = new Polygon()
                    {
                        Points = new PointCollection
                        {
                            new Point(x + 50, y + 10),      // Spitze
                            new Point(x + 90, y + 90),      // Rechts unten
                            new Point(x + 10, y + 90)       // Links unten
                        },
                        Stroke = blackBrush,
                        StrokeThickness = 3.0,
                        Fill = null
                    };
                    canvas.Children.Add(triangle);
                    break;

                case PlayerSymbol.Rectangle:
                    Rectangle rect = new Rectangle()
                    {
                        Width = 80,
                        Height = 80,
                        Stroke = blackBrush,
                        StrokeThickness = 3.0,
                        Margin = new Thickness(x + 10, y + 10, 0, 0)
                    };
                    canvas.Children.Add(rect);
                    break;
            }
        }

        public override void PaintTicTacToeField(Canvas canvas, IB4_TicTacToeField currentField)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Colors.White);

            // Zeichne schwarze Spielfeldlinien
            Line l1 = new Line() { X1 = 120, Y1 = 20, X2 = 120, Y2 = 320, Stroke = blackBrush, StrokeThickness = 3.0 };
            Line l2 = new Line() { X1 = 220, Y1 = 20, X2 = 220, Y2 = 320, Stroke = blackBrush, StrokeThickness = 3.0 };
            Line l3 = new Line() { X1 = 20, Y1 = 120, X2 = 320, Y2 = 120, Stroke = blackBrush, StrokeThickness = 3.0 };
            Line l4 = new Line() { X1 = 20, Y1 = 220, X2 = 320, Y2 = 220, Stroke = blackBrush, StrokeThickness = 3.0 };
            canvas.Children.Add(l1);
            canvas.Children.Add(l2);
            canvas.Children.Add(l3);
            canvas.Children.Add(l4);

            // Zeichne Spielersymbole
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (currentField[i, j] == 1)
                    {
                        DrawSymbol(canvas, player1Symbol, 20 + (j * 100), 20 + (i * 100));
                    }
                    else if (currentField[i, j] == 2)
                    {
                        DrawSymbol(canvas, player2Symbol, 20 + (j * 100), 20 + (i * 100));
                    }
                }
            }
        }
    }

    public class B4_TicTacToeRules : B4_BaseTicTacToeRules
    {
        B4_TicTacToeField _Field = new B4_TicTacToeField();

        public override IB4_TicTacToeField TicTacToeField { get { return _Field; } }

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

        public override string Name { get { return "B4_TicTacToe_Rules"; } }

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

        public override void DoTicTacToeMove(IB4_TicTacToeMove move)
        {
            if (move.Row >= 0 && move.Row < 3 && move.Column >= 0 && move.Column < 3)
            {
                _Field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }

    public class B4_TicTacToeField : B4_BaseTicTacToeField
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

    public class B4_TicTacToeMove : IB4_TicTacToeMove
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;

        public B4_TicTacToeMove(int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }
    }

    public class B4_TicTacToeHumanPlayer_01 : B4_BaseHumanTicTacToePlayer_01
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B4_HumanPlayer_01"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B4_TicTacToeHumanPlayer_01 ttthp = new B4_TicTacToeHumanPlayer_01();
            ttthp.SetPlayerNumber(_PlayerNumber);
            return ttthp;
        }

        public override IB4_TicTacToeMove GetMove(IMoveSelection selection, IB4_TicTacToeField field)
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
                            return new B4_TicTacToeMove(i, j, _PlayerNumber);
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

    public class B4_TicTacToeHumanPlayer_02 : B4_BaseHumanTicTacToePlayer_02
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B4_HumanPlayer_02"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B4_TicTacToeHumanPlayer_02 ttthp = new B4_TicTacToeHumanPlayer_02();
            ttthp.SetPlayerNumber(_PlayerNumber);
            return ttthp;
        }

        public override IB4_TicTacToeMove GetMove(IMoveSelection selection, IB4_TicTacToeField field)
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
                            return new B4_TicTacToeMove(i, j, _PlayerNumber);
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

    public class B4_TicTacToeComputerPlayer : B4_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B4_ComputerPlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B4_TicTacToeComputerPlayer ttthp = new B4_TicTacToeComputerPlayer();
            ttthp.SetPlayerNumber(_PlayerNumber);
            return ttthp;
        }

        public override IB4_TicTacToeMove GetMove(IB4_TicTacToeField field)
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
                    return new B4_TicTacToeMove(r, c, _PlayerNumber);
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
}
