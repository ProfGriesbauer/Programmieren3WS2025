using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace OOPGames
{
    // Cell states for Tic Tac Toe
    public enum B5_TicTacToe_CellState
    {
        Empty = 0,
        X = 1,
        O = 2
    }

    // Game Field - 3x3 Tic Tac Toe Board
    public class B5_TicTacToe_Field : IGameField
    {
        public B5_TicTacToe_CellState[,] Cells { get; } = new B5_TicTacToe_CellState[3, 3];

        public B5_TicTacToe_Field()
        {
            Clear();
        }

        public void Clear()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Cells[row, col] = B5_TicTacToe_CellState.Empty;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is B5_TicTacToe_Painter;
        }
    }

    // Move - represents a single move in Tic Tac Toe
    public class B5_TicTacToe_Move : IRowMove, IColumnMove
    {
        public int PlayerNumber { get; }
        public int Row { get; }
        public int Column { get; }

        public B5_TicTacToe_Move(int playerNumber, int row, int column)
        {
            PlayerNumber = playerNumber;
            Row = row;
            Column = column;
        }
    }

    // Painter - handles rendering the game to the canvas
    public class B5_TicTacToe_Painter : IPaintGame2
    {
        public string Name => "B5_TicTacToe_Painter";

        // Board layout constants
        private const int BoardLeft = 20;
        private const int BoardTop = 20;
        private const int CellSize = 100;
        private const int BoardSize = CellSize * 3;

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField as B5_TicTacToe_Field);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField as B5_TicTacToe_Field);
        }

        private void Draw(Canvas canvas, B5_TicTacToe_Field field)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // Draw grid lines
            var lineStroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            // Vertical lines
            Line verticalLine1 = new Line()
            {
                X1 = BoardLeft + CellSize,
                Y1 = BoardTop,
                X2 = BoardLeft + CellSize,
                Y2 = BoardTop + BoardSize,
                Stroke = lineStroke,
                StrokeThickness = 2.0
            };
            canvas.Children.Add(verticalLine1);

            Line verticalLine2 = new Line()
            {
                X1 = BoardLeft + 2 * CellSize,
                Y1 = BoardTop,
                X2 = BoardLeft + 2 * CellSize,
                Y2 = BoardTop + BoardSize,
                Stroke = lineStroke,
                StrokeThickness = 2.0
            };
            canvas.Children.Add(verticalLine2);

            // Horizontal lines
            Line horizontalLine1 = new Line()
            {
                X1 = BoardLeft,
                Y1 = BoardTop + CellSize,
                X2 = BoardLeft + BoardSize,
                Y2 = BoardTop + CellSize,
                Stroke = lineStroke,
                StrokeThickness = 2.0
            };
            canvas.Children.Add(horizontalLine1);

            Line horizontalLine2 = new Line()
            {
                X1 = BoardLeft,
                Y1 = BoardTop + 2 * CellSize,
                X2 = BoardLeft + BoardSize,
                Y2 = BoardTop + 2 * CellSize,
                Stroke = lineStroke,
                StrokeThickness = 2.0
            };
            canvas.Children.Add(horizontalLine2);

            if (field == null) return;

            // Draw X's and O's
            double padding = CellSize * 0.15;
            double symbolSize = CellSize - 2 * padding;

            Brush xBrush = new SolidColorBrush(Color.FromRgb(200, 0, 0));
            Brush oBrush = new SolidColorBrush(Color.FromRgb(0, 0, 200));

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    B5_TicTacToe_CellState state = field.Cells[row, col];
                    double cellLeft = BoardLeft + col * CellSize;
                    double cellTop = BoardTop + row * CellSize;

                    double x = cellLeft + padding;
                    double y = cellTop + padding;

                    if (state == B5_TicTacToe_CellState.X)
                    {
                        // Draw X - two diagonal lines
                        Line x1 = new Line()
                        {
                            X1 = x,
                            Y1 = y,
                            X2 = x + symbolSize,
                            Y2 = y + symbolSize,
                            Stroke = xBrush,
                            StrokeThickness = 3.0
                        };
                        Line x2 = new Line()
                        {
                            X1 = x + symbolSize,
                            Y1 = y,
                            X2 = x,
                            Y2 = y + symbolSize,
                            Stroke = xBrush,
                            StrokeThickness = 3.0
                        };
                        canvas.Children.Add(x1);
                        canvas.Children.Add(x2);
                    }
                    else if (state == B5_TicTacToe_CellState.O)
                    {
                        // Draw O - circle
                        Ellipse circle = new Ellipse()
                        {
                            Width = symbolSize,
                            Height = symbolSize,
                            Stroke = oBrush,
                            StrokeThickness = 3.0
                        };
                        Canvas.SetLeft(circle, x);
                        Canvas.SetTop(circle, y);
                        canvas.Children.Add(circle);
                    }
                }
            }
        }
    }

    // Rules - manages game logic and state
    public class B5_TicTacToe_Rules : IGameRules
    {
        private readonly B5_TicTacToe_Field _field = new B5_TicTacToe_Field();
        private int _currentPlayer = 1; // 1 = X, 2 = O

        public string Name => "B5_TicTacToe_Rules";

        public IGameField CurrentField => _field;

        public bool MovesPossible
        {
            get
            {
                // Check if there's at least one empty cell
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (_field.Cells[row, col] == B5_TicTacToe_CellState.Empty)
                            return true;
                    }
                }
                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not IRowMove rowMove || move is not IColumnMove colMove)
                return;

            int row = rowMove.Row;
            int col = colMove.Column;

            // Validate move coordinates
            if (row < 0 || row > 2 || col < 0 || col > 2)
                return;

            // Check if cell is empty
            if (_field.Cells[row, col] != B5_TicTacToe_CellState.Empty)
                return;

            // Place the move
            _field.Cells[row, col] = move.PlayerNumber == 1 ? B5_TicTacToe_CellState.X : B5_TicTacToe_CellState.O;

            // Switch player only if no one has won yet
            if (CheckIfPLayerWon() == -1)
            {
                _currentPlayer = _currentPlayer == 1 ? 2 : 1;
            }
        }

        public void ClearField()
        {
            _field.Clear();
            _currentPlayer = 1;
        }

        public int CheckIfPLayerWon()
        {
            // Check rows and columns
            for (int i = 0; i < 3; i++)
            {
                // Check row i
                if (_field.Cells[i, 0] != B5_TicTacToe_CellState.Empty &&
                    _field.Cells[i, 0] == _field.Cells[i, 1] &&
                    _field.Cells[i, 1] == _field.Cells[i, 2])
                {
                    return _field.Cells[i, 0] == B5_TicTacToe_CellState.X ? 1 : 2;
                }

                // Check column i
                if (_field.Cells[0, i] != B5_TicTacToe_CellState.Empty &&
                    _field.Cells[0, i] == _field.Cells[1, i] &&
                    _field.Cells[1, i] == _field.Cells[2, i])
                {
                    return _field.Cells[0, i] == B5_TicTacToe_CellState.X ? 1 : 2;
                }
            }

            // Check diagonals
            // Top-left to bottom-right
            if (_field.Cells[0, 0] != B5_TicTacToe_CellState.Empty &&
                _field.Cells[0, 0] == _field.Cells[1, 1] &&
                _field.Cells[1, 1] == _field.Cells[2, 2])
            {
                return _field.Cells[0, 0] == B5_TicTacToe_CellState.X ? 1 : 2;
            }

            // Top-right to bottom-left
            if (_field.Cells[0, 2] != B5_TicTacToe_CellState.Empty &&
                _field.Cells[0, 2] == _field.Cells[1, 1] &&
                _field.Cells[1, 1] == _field.Cells[2, 0])
            {
                return _field.Cells[0, 2] == B5_TicTacToe_CellState.X ? 1 : 2;
            }

            // No winner
            return -1;
        }

        public int CurrentPlayer => _currentPlayer;
    }

    // Human Player - handles user input via keyboard and mouse
    public class B5_TicTacToe_HumanPlayer : IHumanGamePlayer
    {
        public string Name => "B5_TicTacToe_Human_Player";

        public int PlayerNumber { get; private set; }

        public B5_TicTacToe_HumanPlayer(int playerNumber = 1)
        {
            PlayerNumber = playerNumber;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B5_TicTacToe_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B5_TicTacToe_HumanPlayer(PlayerNumber);
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || field is not B5_TicTacToe_Field tttField)
                return null;

            if (selection is IKeySelection keySelection)
            {
                return GetMoveFromKey(keySelection.Key, tttField);
            }

            if (selection is IClickSelection clickSelection)
            {
                return GetMoveFromClick(clickSelection, tttField);
            }

            return null;
        }

        // Handle keyboard input (numpad layout)
        // 7 8 9
        // 4 5 6
        // 1 2 3
        private IPlayMove GetMoveFromKey(Key key, B5_TicTacToe_Field field)
        {
            int? position = key switch
            {
                Key.NumPad7 or Key.D7 => 0,
                Key.NumPad8 or Key.D8 => 1,
                Key.NumPad9 or Key.D9 => 2,
                Key.NumPad4 or Key.D4 => 3,
                Key.NumPad5 or Key.D5 => 4,
                Key.NumPad6 or Key.D6 => 5,
                Key.NumPad1 or Key.D1 => 6,
                Key.NumPad2 or Key.D2 => 7,
                Key.NumPad3 or Key.D3 => 8,
                _ => null
            };

            if (!position.HasValue)
                return null;

            int row = position.Value / 3;
            int col = position.Value % 3;

            // Check if cell is empty
            if (field.Cells[row, col] != B5_TicTacToe_CellState.Empty)
                return null;

            return new B5_TicTacToe_Move(PlayerNumber, row, col);
        }

        // Handle mouse click input
        private IPlayMove GetMoveFromClick(IClickSelection selection, B5_TicTacToe_Field field)
        {
            const int BoardLeft = 20;
            const int BoardTop = 20;
            const int CellSize = 100;
            const int BoardSize = CellSize * 3;

            int x = selection.XClickPos;
            int y = selection.YClickPos;

            // Check if click is within board boundaries
            if (x < BoardLeft || x > BoardLeft + BoardSize ||
                y < BoardTop || y > BoardTop + BoardSize)
            {
                return null;
            }

            int col = (x - BoardLeft) / CellSize;
            int row = (y - BoardTop) / CellSize;

            // Validate coordinates
            if (row < 0 || row > 2 || col < 0 || col > 2)
                return null;

            // Check if cell is empty
            if (field.Cells[row, col] != B5_TicTacToe_CellState.Empty)
                return null;

            return new B5_TicTacToe_Move(PlayerNumber, row, col);
        }
    }

    // Computer Player - AI opponent using Minimax algorithm
    public class B5_TicTacToe_ComputerPlayer : IComputerGamePlayer
    {
        public string Name => "B5_TicTacToe_Computer_Player";

        public int PlayerNumber { get; private set; }

        public B5_TicTacToe_ComputerPlayer(int playerNumber = 2)
        {
            PlayerNumber = playerNumber;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B5_TicTacToe_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B5_TicTacToe_ComputerPlayer(PlayerNumber);
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is not B5_TicTacToe_Field tttField)
                return null;

            // Find best move using Minimax algorithm
            int bestRow = -1;
            int bestCol = -1;
            int bestScore = int.MinValue;

            B5_TicTacToe_CellState aiSymbol = PlayerNumber == 1 ? B5_TicTacToe_CellState.X : B5_TicTacToe_CellState.O;
            B5_TicTacToe_CellState opponentSymbol = PlayerNumber == 1 ? B5_TicTacToe_CellState.O : B5_TicTacToe_CellState.X;

            // Try all possible moves
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (tttField.Cells[row, col] == B5_TicTacToe_CellState.Empty)
                    {
                        // Simulate move
                        tttField.Cells[row, col] = aiSymbol;
                        int score = Minimax(tttField, false, aiSymbol, opponentSymbol);
                        // Undo move
                        tttField.Cells[row, col] = B5_TicTacToe_CellState.Empty;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRow = row;
                            bestCol = col;
                        }
                    }
                }
            }

            if (bestRow == -1 || bestCol == -1)
            {
                // No valid move found (board is full)
                return null;
            }

            return new B5_TicTacToe_Move(PlayerNumber, bestRow, bestCol);
        }

        // Minimax algorithm: AI maximizes, opponent minimizes
        private int Minimax(B5_TicTacToe_Field field, bool isMaximizing,
                            B5_TicTacToe_CellState aiSymbol, B5_TicTacToe_CellState opponentSymbol)
        {
            int evaluation = Evaluate(field, aiSymbol, opponentSymbol);
            if (evaluation != 0)
            {
                return evaluation; // Win or loss detected
            }

            if (!HasEmptyCell(field))
            {
                return 0; // Draw
            }

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (field.Cells[row, col] == B5_TicTacToe_CellState.Empty)
                        {
                            field.Cells[row, col] = aiSymbol;
                            int score = Minimax(field, false, aiSymbol, opponentSymbol);
                            field.Cells[row, col] = B5_TicTacToe_CellState.Empty;
                            if (score > bestScore)
                                bestScore = score;
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (field.Cells[row, col] == B5_TicTacToe_CellState.Empty)
                        {
                            field.Cells[row, col] = opponentSymbol;
                            int score = Minimax(field, true, aiSymbol, opponentSymbol);
                            field.Cells[row, col] = B5_TicTacToe_CellState.Empty;
                            if (score < bestScore)
                                bestScore = score;
                        }
                    }
                }
                return bestScore;
            }
        }

        // Evaluate the board from AI's perspective
        // +10 = AI wins, -10 = opponent wins, 0 = no winner yet
        private int Evaluate(B5_TicTacToe_Field field, B5_TicTacToe_CellState aiSymbol, B5_TicTacToe_CellState opponentSymbol)
        {
            // Check rows and columns
            for (int i = 0; i < 3; i++)
            {
                // Check row i
                if (field.Cells[i, 0] != B5_TicTacToe_CellState.Empty &&
                    field.Cells[i, 0] == field.Cells[i, 1] &&
                    field.Cells[i, 1] == field.Cells[i, 2])
                {
                    return field.Cells[i, 0] == aiSymbol ? 10 : -10;
                }

                // Check column i
                if (field.Cells[0, i] != B5_TicTacToe_CellState.Empty &&
                    field.Cells[0, i] == field.Cells[1, i] &&
                    field.Cells[1, i] == field.Cells[2, i])
                {
                    return field.Cells[0, i] == aiSymbol ? 10 : -10;
                }
            }

            // Check diagonals
            if (field.Cells[0, 0] != B5_TicTacToe_CellState.Empty &&
                field.Cells[0, 0] == field.Cells[1, 1] &&
                field.Cells[1, 1] == field.Cells[2, 2])
            {
                return field.Cells[0, 0] == aiSymbol ? 10 : -10;
            }

            if (field.Cells[0, 2] != B5_TicTacToe_CellState.Empty &&
                field.Cells[0, 2] == field.Cells[1, 1] &&
                field.Cells[1, 1] == field.Cells[2, 0])
            {
                return field.Cells[0, 2] == aiSymbol ? 10 : -10;
            }

            return 0; // No winner yet
        }

        private bool HasEmptyCell(B5_TicTacToe_Field field)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (field.Cells[row, col] == B5_TicTacToe_CellState.Empty)
                        return true;
                }
            }
            return false;
        }
    }
}
