using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // -------------------------
    // Painter (anders implementiert, gleiche Funktion)
    // -------------------------
    public class A1_TicTacToePaint : X_BaseTicTacToePaint
    {
        public override string Name => "A1TicTacToePaint";

        // configurable constants (keine festen Magic-Values verteilt im Code)
        const double Margin = 20;
        const double CellSize = 100;
        const double LineThickness = 3.0;
        const double SymbolPadding = 12.0;

        public override void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            Brush gridBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            Brush xBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            Brush oBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

            // draw two vertical and two horizontal lines using a loop (different approach)
            for (int i = 1; i <= 2; i++)
            {
                // vertical
                var v = new Line()
                {
                    X1 = Margin + i * CellSize,
                    X2 = Margin + i * CellSize,
                    Y1 = Margin,
                    Y2 = Margin + 3 * CellSize,
                    Stroke = gridBrush,
                    StrokeThickness = LineThickness
                };
                canvas.Children.Add(v);

                // horizontal
                var h = new Line()
                {
                    Y1 = Margin + i * CellSize,
                    Y2 = Margin + i * CellSize,
                    X1 = Margin,
                    X2 = Margin + 3 * CellSize,
                    Stroke = gridBrush,
                    StrokeThickness = LineThickness
                };
                canvas.Children.Add(h);
            }

            // draw cell contents; this time compute cell rectangles once and reuse
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    int value = currentField[r, c];
                    double left = Margin + c * CellSize;
                    double top = Margin + r * CellSize;
                    if (value == 1) // X
                    {
                        // draw two crossing lines inside cell with padding
                        var l1 = new Line()
                        {
                            X1 = left + SymbolPadding,
                            Y1 = top + SymbolPadding,
                            X2 = left + CellSize - SymbolPadding,
                            Y2 = top + CellSize - SymbolPadding,
                            Stroke = xBrush,
                            StrokeThickness = LineThickness
                        };
                        var l2 = new Line()
                        {
                            X1 = left + SymbolPadding,
                            Y1 = top + CellSize - SymbolPadding,
                            X2 = left + CellSize - SymbolPadding,
                            Y2 = top + SymbolPadding,
                            Stroke = xBrush,
                            StrokeThickness = LineThickness
                        };
                        canvas.Children.Add(l1);
                        canvas.Children.Add(l2);
                    }
                    else if (value == 2) // O
                    {
                        // draw ellipse using Canvas.SetLeft/Top (different from Margin)
                        var e = new Ellipse()
                        {
                            Width = CellSize - 2 * SymbolPadding,
                            Height = CellSize - 2 * SymbolPadding,
                            Stroke = oBrush,
                            StrokeThickness = LineThickness
                        };
                        Canvas.SetLeft(e, left + SymbolPadding);
                        Canvas.SetTop(e, top + SymbolPadding);
                        canvas.Children.Add(e);
                    }
                }
            }
        }
    }

    // -------------------------
    // Field (flaches Array statt 2D und defensives Indexing)
    // -------------------------
    public class A1_TicTacToeField : X_BaseTicTacToeField
    {
        // flat internal representation (index = r*3 + c)
        private readonly int[] _cells = Enumerable.Repeat(0, 9).ToArray();

        public override int this[int r, int c]
        {
            get
            {
                if (InRange(r, c)) return _cells[r * 3 + c];
                return -1;
            }
            set
            {
                if (InRange(r, c)) _cells[r * 3 + c] = value;
            }
        }

        private static bool InRange(int r, int c) => r >= 0 && r < 3 && c >= 0 && c < 3;

        // expose helper to enumerate cells (useful internally)
        public IEnumerable<(int Row, int Col, int Value)> Enumerate()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    yield return (r, c, this[r, c]);
        }
    }

    // -------------------------
    // Rules (uses list of winning triples; MovesPossible via Any)
    // -------------------------
    public class A1_TicTacToeRules : X_BaseTicTacToeRules
    {
        private readonly A1_TicTacToeField _field = new A1_TicTacToeField();

        // precomputed winning lines as a list of triples (r,c)
        private static readonly (int r, int c)[][] WinningLines = new[]
        {
            // rows
            new[] { (0,0), (0,1), (0,2) },
            new[] { (1,0), (1,1), (1,2) },
            new[] { (2,0), (2,1), (2,2) },
            // cols
            new[] { (0,0), (1,0), (2,0) },
            new[] { (0,1), (1,1), (2,1) },
            new[] { (0,2), (1,2), (2,2) },
            // diagonals
            new[] { (0,0), (1,1), (2,2) },
            new[] { (0,2), (1,1), (2,0) },
        };

        public override IX_TicTacToeField TicTacToeField => _field;

        public override bool MovesPossible
        {
            get
            {
                foreach (var cell in _field.Enumerate())
                    if (cell.Value == 0) return true;
                return false;
            }
        }

        public override string Name => "A1TicTacToeRules";

        // returns 1 or 2 for the winner, -1 for none
        public override int CheckIfPLayerWon()
        {
            foreach (var line in WinningLines)
            {
                int a = _field[line[0].r, line[0].c];
                if (a <= 0) continue;
                int b = _field[line[1].r, line[1].c];
                int c = _field[line[2].r, line[2].c];
                if (a == b && b == c) return a;
            }
            return -1;
        }

        public override void ClearField()
        {
            // use array fill approach through the public indexer for clarity
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    _field[r, c] = 0;
        }

        public override void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            if (move == null) return;
            if (move.Row >= 0 && move.Row < 3 && move.Column >= 0 && move.Column < 3)
            {
                // do not validate occupancy here: caller (game engine) is expected to ensure move validity
                _field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }

    // -------------------------
    // Move (immutable, but different layout)
    // -------------------------
    public class A1_TicTacToeMove : IX_TicTacToeMove
    {
        public int Row { get; }
        public int Column { get; }
        public int PlayerNumber { get; }

        public A1_TicTacToeMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }
    }

    // -------------------------
    // Human Player (different click->cell calculation)
    // -------------------------
    public class A1_HumanTicTacToePlayer : X_BaseHumanTicTacToePlayer
    {
        private int _playerNumber = 0;
        public override string Name => "A1HumanPlayer";
        public override int PlayerNumber => _playerNumber;

        public override IGamePlayer Clone()
        {
            var clone = new A1_HumanTicTacToePlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public override void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;

        // compute clicked cell from click coords using arithmetic (different from nested ifs)
        public override IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            if (!(selection is IClickSelection click) || field == null) return null;

            // constants must match painter layout (this implementation follows same classic sizes)
            const double margin = 20;
            const double cell = 100;

            // map click to column/row
            int col = (int)((click.XClickPos - margin) / cell);
            int row = (int)((click.YClickPos - margin) / cell);

            // validate
            if (row < 0 || row > 2 || col < 0 || col > 2) return null;
            if (field[row, col] != 0) return null;

            return new A1_TicTacToeMove(row, col, _playerNumber);
        }
    }

    // -------------------------
    // Computer Player (simple heuristic: win if possible, else block, else center/corner/side)
    // -------------------------
    public class A1_ComputerTicTacToePlayer : X_BaseComputerTicTacToePlayer
    {
        private int _playerNumber = 0;
        private static readonly Random _rnd = new Random();

        public override string Name => "A1ComputerPlayer";
        public override int PlayerNumber => _playerNumber;

        public override IGamePlayer Clone()
        {
            var clone = new A1_ComputerTicTacToePlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public override void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;

        public override IX_TicTacToeMove GetMove(IX_TicTacToeField field)
        {
            if (field == null) return null;

            int me = _playerNumber;
            int opp = me == 1 ? 2 : 1;

            // helper to test a move on a copy
            bool WouldWin(int r, int c, int player)
            {
                // quick simulate
                if (field[r, c] != 0) return false;
                // place temporarily and check lines by scanning winning lines
                // We'll just check lines crossing (r,c) to be efficient
                // rows, cols, diagonals containing (r,c)
                // Row
                if (field[r, 0] == player && field[r, 1] == player && field[r, 2] == 0 && c == 2) return true;
                // Simpler: perform a small local evaluation: create a temp 3x3 array
                int[,] temp = new int[3, 3];
                for (int rr = 0; rr < 3; rr++)
                    for (int cc = 0; cc < 3; cc++)
                        temp[rr, cc] = field[rr, cc];
                temp[r, c] = player;

                int[,] lines = new int[,]
                {
                    {0,0,0}, {0,1,0}, {0,2,0} // placeholder (we'll check via known combos)
                };

                // check all winning combos (explicit)
                (int r1,int c1,int r2,int c2,int r3,int c3)[] combos = new[]
                {
                    (0,0,0,1,0,2),
                    (1,0,1,1,1,2),
                    (2,0,2,1,2,2),
                    (0,0,1,0,2,0),
                    (0,1,1,1,2,1),
                    (0,2,1,2,2,2),
                    (0,0,1,1,2,2),
                    (0,2,1,1,2,0),
                };
                foreach (var t in combos)
                {
                    if (temp[t.r1, t.c1] == player && temp[t.r2, t.c2] == player && temp[t.r3, t.c3] == player)
                        return true;
                }
                return false;
            }

            // 1) Win if possible
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (field[r, c] == 0 && WouldWin(r, c, me))
                        return new A1_TicTacToeMove(r, c, me);

            // 2) Block opponent's winning move
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (field[r, c] == 0 && WouldWin(r, c, opp))
                        return new A1_TicTacToeMove(r, c, me);

            // 3) Prefer center
            if (field[1, 1] == 0) return new A1_TicTacToeMove(1, 1, me);

            // 4) Then corners (shuffled order to vary behaviour)
            var corners = new List<(int r, int c)> { (0, 0), (0, 2), (2, 0), (2, 2) };
            // shuffle
            for (int i = 0; i < corners.Count; i++)
            {
                int j = _rnd.Next(i, corners.Count);
                var tmp = corners[i];
                corners[i] = corners[j];
                corners[j] = tmp;
            }
            foreach (var (r, c) in corners)
                if (field[r, c] == 0) return new A1_TicTacToeMove(r, c, me);

            // 5) fallback to any empty cell (scan randomized start to avoid deterministic pattern)
            int start = _rnd.Next(0, 9);
            for (int k = 0; k < 9; k++)
            {
                int idx = (start + k) % 9;
                int r = idx / 3;
                int c = idx % 3;
                if (field[r, c] == 0) return new A1_TicTacToeMove(r, c, me);
            }

            return null;
        }
    }
}
