using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OOPGames.B1_Gruppe;

namespace OOPGames
{
    // Painter for B1 tic-tac-toe (simple copy of existing X painter but named for B1)
    public class B1_TicTacToePaint : X_BaseTicTacToePaint
    {
        public override string Name { get { return "B1_TicTacToePaint"; } }

        public override void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            canvas.Children.Clear();
            Color bgColor = Color.FromRgb(255, 255, 255);
            canvas.Background = new SolidColorBrush(bgColor);
            Color lineColor = Color.FromRgb(0, 0, 0);
            Brush lineStroke = new SolidColorBrush(lineColor);
            Color XColor = Color.FromRgb(0, 128, 0);
            Brush XStroke = new SolidColorBrush(XColor);
            Color OColor = Color.FromRgb(0, 0, 128);
            Brush OStroke = new SolidColorBrush(OColor);

            // draw grid
            Line l1 = new Line() { X1 = 120, Y1 = 20, X2 = 120, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l1);
            Line l2 = new Line() { X1 = 220, Y1 = 20, X2 = 220, Y2 = 320, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l2);
            Line l3 = new Line() { X1 = 20, Y1 = 120, X2 = 320, Y2 = 120, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l3);
            Line l4 = new Line() { X1 = 20, Y1 = 220, X2 = 320, Y2 = 220, Stroke = lineStroke, StrokeThickness = 3.0 };
            canvas.Children.Add(l4);

            // If the field exposes the B1 board, draw using actual objects
            if (currentField is B1_TicTacToeField b1Field)
            {
                var board = b1Field.Board;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var cell = board.GetCell(i, j);
                        if (cell?.OccupiedBy is B1_Cross)
                        {
                            Line X1 = new Line() { X1 = 20 + (j * 100), Y1 = 20 + (i * 100), X2 = 120 + (j * 100), Y2 = 120 + (i * 100), Stroke = XStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(X1);
                            Line X2 = new Line() { X1 = 20 + (j * 100), Y1 = 120 + (i * 100), X2 = 120 + (j * 100), Y2 = 20 + (i * 100), Stroke = XStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(X2);
                        }
                        else if (cell?.OccupiedBy is B1_Circle)
                        {
                            Ellipse OE = new Ellipse() { Margin = new System.Windows.Thickness(20 + (j * 100), 20 + (i * 100), 0, 0), Width = 100, Height = 100, Stroke = OStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(OE);
                        }
                    }
                }
            }
            else
            {
                // Fallback: use integer indexer
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (currentField[i, j] == 1)
                        {
                            Line X1 = new Line() { X1 = 20 + (j * 100), Y1 = 20 + (i * 100), X2 = 120 + (j * 100), Y2 = 120 + (i * 100), Stroke = XStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(X1);
                            Line X2 = new Line() { X1 = 20 + (j * 100), Y1 = 120 + (i * 100), X2 = 120 + (j * 100), Y2 = 20 + (i * 100), Stroke = XStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(X2);
                        }
                        else if (currentField[i, j] == 2)
                        {
                            Ellipse OE = new Ellipse() { Margin = new System.Windows.Thickness(20 + (j * 100), 20 + (i * 100), 0, 0), Width = 100, Height = 100, Stroke = OStroke, StrokeThickness = 3.0 };
                            canvas.Children.Add(OE);
                        }
                    }
                }
            }
        }
    }

    // Simple rules implementation for B1 using standard TicTacToe base
    public class B1_TicTacToeRules : X_BaseTicTacToeRules
    {
        B1_TicTacToeField _Field = new B1_TicTacToeField();

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

        public override string Name { get { return "B1_TicTacToeRules"; } }

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

    public class B1_TicTacToeField : X_BaseTicTacToeField
    {
        // Use the object-oriented board internally
        private OOPGames.B1_Gruppe.B1_Board _Board = new OOPGames.B1_Gruppe.B1_Board();

        public OOPGames.B1_Gruppe.B1_Board Board { get { return _Board; } }

        public override int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 3 && c >= 0 && c < 3)
                {
                    var cell = _Board.GetCell(r, c);
                    if (cell == null) return -1;
                    var sym = cell.OccupiedBy;
                    if (sym == null) return 0;
                    if (sym.GetType().Name == "B1_Cross" || sym is OOPGames.B1_Gruppe.B1_Cross) return 1;
                    if (sym.GetType().Name == "B1_Circle" || sym is OOPGames.B1_Gruppe.B1_Circle) return 2;
                    return -1;
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
                    if (value == 0)
                    {
                        _Board.SetCellSymbol(r, c, null);
                    }
                    else if (value == 1)
                    {
                        _Board.SetCellSymbol(r, c, new OOPGames.B1_Gruppe.B1_Cross());
                    }
                    else if (value == 2)
                    {
                        _Board.SetCellSymbol(r, c, new OOPGames.B1_Gruppe.B1_Circle());
                    }
                }
            }
        }
    }

    public class B1_TicTacToeMove : IX_TicTacToeMove
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;

        public B1_TicTacToeMove(int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }
    }

    public class B1_TicTacToeHumanPlayer : X_BaseHumanTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B1_HumanTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B1_TicTacToeHumanPlayer p = new B1_TicTacToeHumanPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public override IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            if (selection is IClickSelection sel)
            {
                // Match painter's coordinates: grid from (20,20) to (320,320) with 100px cells
                int x = sel.XClickPos;
                int y = sel.YClickPos;

                // Check if click is within game grid bounds (20..320)
                if (x < 20 || x > 320 || y < 20 || y > 320) return null;

                // Convert to cell coordinates (each cell is 100x100)
                int col = (x - 20) / 100;
                int row = (y - 20) / 100;

                // Validate cell bounds (should always be 0-2 given the above checks)
                if (row < 0 || row > 2 || col < 0 || col > 2) return null;

                // If the field is our B1_TicTacToeField, use the object-board to check emptiness
                if (field is B1_TicTacToeField bf)
                {
                    var cell = bf.Board.GetCell(row, col);
                    if (cell != null && cell.IsEmpty)
                    {
                        return new B1_TicTacToeMove(row, col, _PlayerNumber);
                    }
                }
                else
                {
                    if (field[row, col] <= 0)
                    {
                        return new B1_TicTacToeMove(row, col, _PlayerNumber);
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

    public class B1_TicTacToeComputerPlayer : X_BaseComputerTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "B1_ComputerTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            B1_TicTacToeComputerPlayer p = new B1_TicTacToeComputerPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            // If we can access the B1 board, run minimax; otherwise fallback to random
            int[,] grid = new int[3, 3];
            for (int r = 0; r < 3; r++) for (int c = 0; c < 3; c++) grid[r, c] = field[r, c];

            var best = MinimaxBestMove(grid, _PlayerNumber);
            if (best != null)
            {
                return new B1_TicTacToeMove(best.Item1, best.Item2, _PlayerNumber);
            }

            return null;
        }

        // Minimax helper that returns (row,col) for best move or null
        private Tuple<int, int> MinimaxBestMove(int[,] grid, int player)
        {
            int bestScore = int.MinValue;
            Tuple<int, int> bestMove = null;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (grid[r, c] == 0)
                    {
                        grid[r, c] = player;
                        int score = Minimax(grid, false, player, 0);
                        grid[r, c] = 0;
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = Tuple.Create(r, c);
                        }
                    }
                }
            }
            return bestMove;
        }

        // simple minimax with no alpha-beta, depth-limited to full search (3x3 small)
        private int Minimax(int[,] grid, bool isMaximizing, int player, int depth)
        {
            int winner = EvaluateWinner(grid);
            if (winner == player) return 10 - depth;
            if (winner == (3 - player)) return depth - 10;
            if (IsFull(grid)) return 0;

            if (isMaximizing)
            {
                int best = int.MinValue;
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        if (grid[r, c] == 0)
                        {
                            grid[r, c] = player;
                            best = Math.Max(best, Minimax(grid, false, player, depth + 1));
                            grid[r, c] = 0;
                        }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                int opponent = 3 - player;
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        if (grid[r, c] == 0)
                        {
                            grid[r, c] = opponent;
                            best = Math.Min(best, Minimax(grid, true, player, depth + 1));
                            grid[r, c] = 0;
                        }
                return best;
            }
        }

        private int EvaluateWinner(int[,] g)
        {
            for (int i = 0; i < 3; i++)
            {
                if (g[i, 0] > 0 && g[i, 0] == g[i, 1] && g[i, 1] == g[i, 2]) return g[i, 0];
                if (g[0, i] > 0 && g[0, i] == g[1, i] && g[1, i] == g[2, i]) return g[0, i];
            }
            if (g[0, 0] > 0 && g[0, 0] == g[1, 1] && g[1, 1] == g[2, 2]) return g[0, 0];
            if (g[0, 2] > 0 && g[0, 2] == g[1, 1] && g[1, 1] == g[2, 0]) return g[0, 2];
            return -1;
        }

        private bool IsFull(int[,] g)
        {
            for (int r = 0; r < 3; r++) for (int c = 0; c < 3; c++) if (g[r, c] == 0) return false;
            return true;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
