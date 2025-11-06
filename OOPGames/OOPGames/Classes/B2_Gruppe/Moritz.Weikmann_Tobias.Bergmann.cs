using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using OOPGames;

namespace OOPGames.B2_Gruppe
{
    public class B2_TicTacToeField : IGameField
    {
        private int[,] _field = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

        public int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    return _field[r, c];
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
                    _field[r, c] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            if (painter is B2_TicTacToePaint)
            {
                return true;
            }
            return false;
        }
    }

    public class B2_TicTacToeRules : IGameRules
    {
        private B2_TicTacToeField _field = new B2_TicTacToeField();
        private int _currentPlayer = 1;

        public string Name { get { return "B2 TicTacToe"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible 
        {
            get
            {
                if (CheckIfPLayerWon() > 0) return false;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (_field[i, j] == 0) return true;
                    }
                }
                return false;
            }
        }

        public int CheckIfPLayerWon()
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (_field[i, 0] != 0 && _field[i, 0] == _field[i, 1] && _field[i, 1] == _field[i, 2])
                {
                    return _field[i, 0];
                }
            }

            // Check columns
            for (int i = 0; i < 3; i++)
            {
                if (_field[0, i] != 0 && _field[0, i] == _field[1, i] && _field[1, i] == _field[2, i])
                {
                    return _field[0, i];
                }
            }

            // Check diagonals
            if (_field[0, 0] != 0 && _field[0, 0] == _field[1, 1] && _field[1, 1] == _field[2, 2])
            {
                return _field[0, 0];
            }
            if (_field[0, 2] != 0 && _field[0, 2] == _field[1, 1] && _field[1, 1] == _field[2, 0])
            {
                return _field[0, 2];
            }

            return -1;
        }

        public void ClearField()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _field[i, j] = 0;
                }
            }
            _currentPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            if (move is B2_TicTacToeMove tttMove)
            {
                if (tttMove.Row >= 0 && tttMove.Row < 3 && 
                    tttMove.Column >= 0 && tttMove.Column < 3 &&
                    _field[tttMove.Row, tttMove.Column] == 0)
                {
                    _field[tttMove.Row, tttMove.Column] = _currentPlayer;
                    _currentPlayer = _currentPlayer == 1 ? 2 : 1;
                }
            }
        }

        public int GetCurrentPlayer()
        {
            return _currentPlayer;
        }
    }

    public class B2_TicTacToeMove : IPlayMove 
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int PlayerNumber { get; private set; }

        public B2_TicTacToeMove(int player, int row, int col)
        {
            PlayerNumber = player;
            Row = row;
            Column = col;
        }
    }

    public class B2_TicTacToePaint : IPaintGame
    {
        public string Name { get { return "B2 TicTacToe Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is B2_TicTacToeField field)
            {
                canvas.Children.Clear();
                Color bgColor = Color.FromRgb(255, 255, 255);
                canvas.Background = new SolidColorBrush(bgColor);

                double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 300;
                double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 300;
                double cellW = w / 3.0;
                double cellH = h / 3.0;

                // Draw grid lines
                for (int i = 1; i < 3; i++)
                {
                    Line l1 = new Line()
                    {
                        X1 = i * cellW,
                        Y1 = 0,
                        X2 = i * cellW,
                        Y2 = h,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 2
                    };
                    canvas.Children.Add(l1);

                    Line l2 = new Line()
                    {
                        X1 = 0,
                        Y1 = i * cellH,
                        X2 = w,
                        Y2 = i * cellH,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 2
                    };
                    canvas.Children.Add(l2);
                }

                // Draw X and O
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        double x = j * cellW;
                        double y = i * cellH;
                        if (field[i, j] == 1)  // Draw X
                        {
                            Line l1 = new Line()
                            {
                                X1 = x + 10,
                                Y1 = y + 10,
                                X2 = x + cellW - 10,
                                Y2 = y + cellH - 10,
                                Stroke = new SolidColorBrush(Colors.DarkBlue),
                                StrokeThickness = 3
                            };
                            canvas.Children.Add(l1);

                            Line l2 = new Line()
                            {
                                X1 = x + cellW - 10,
                                Y1 = y + 10,
                                X2 = x + 10,
                                Y2 = y + cellH - 10,
                                Stroke = new SolidColorBrush(Colors.DarkBlue),
                                StrokeThickness = 3
                            };
                            canvas.Children.Add(l2);
                        }
                        else if (field[i, j] == 2)  // Draw O
                        {
                            Ellipse e = new Ellipse()
                            {
                                Width = cellW - 20,
                                Height = cellH - 20,
                                Stroke = new SolidColorBrush(Colors.DarkRed),
                                StrokeThickness = 3,
                                Fill = new SolidColorBrush(Colors.Transparent)
                            };
                            Canvas.SetLeft(e, x + 10);
                            Canvas.SetTop(e, y + 10);
                            canvas.Children.Add(e);
                        }
                    }
                }
            }
        }
    }

    public class B2_TicTacToeHumanPlayer : IHumanGamePlayer
    {
        public string Name { get { return "B2 Human Player"; } }
        public int PlayerNumber { get; private set; }

        public B2_TicTacToeHumanPlayer()
        {
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B2_TicTacToeRules;
        }

        public IGamePlayer Clone()
        {
            return new B2_TicTacToeHumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IClickSelection clickSelection)
            {
                int xPos = clickSelection.XClickPos;
                int yPos = clickSelection.YClickPos;
                double h = ((Canvas)selection).Height;
                double w = ((Canvas)selection).Width;
                int row = (int)(yPos / (h / 3.0));
                int column = (int)(xPos / (w / 3.0));

                if (row >= 0 && row < 3 && column >= 0 && column < 3)
                {
                    return new B2_TicTacToeMove(PlayerNumber, row, column);
                }
            }
            return null;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }
    }

    public class B2_TicTacToeComputerPlayer : IComputerGamePlayer
    {
        private Random _random = new Random();
        public string Name { get { return "B2 Computer Player"; } }
        public int PlayerNumber { get; private set; }

        public B2_TicTacToeComputerPlayer()
        {
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B2_TicTacToeRules;
        }

        public IGamePlayer Clone()
        {
            return new B2_TicTacToeComputerPlayer();
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is B2_TicTacToeField tttField)
            {
                List<(int row, int col)> emptyFields = new List<(int row, int col)>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (tttField[i, j] == 0)
                        {
                            emptyFields.Add((i, j));
                        }
                    }
                }

                if (emptyFields.Count > 0)
                {
                    var move = emptyFields[_random.Next(emptyFields.Count)];
                    return new B2_TicTacToeMove(PlayerNumber, move.row, move.col);
                }
            }
            return null;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }
    }
}
