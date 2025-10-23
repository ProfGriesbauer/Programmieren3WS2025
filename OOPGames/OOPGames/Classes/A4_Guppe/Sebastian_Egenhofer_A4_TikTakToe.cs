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
    // A4 group implementations for TicTacToe: Painter, Rules (+ Field/Move), Human Player
    public class A4_TicTacToePaint : X_BaseTicTacToePaint
    {
        public override string Name { get { return "A4_TicTacToePaint"; } }

        public override void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField)
        {
            // draw grid and marks to completely fill the canvas for a 5x5 board
            canvas.Children.Clear();
            Color bgColor = Color.FromRgb(240, 240, 240);
            canvas.Background = new SolidColorBrush(bgColor);
            Color lineColor = Color.FromRgb(80, 80, 80);
            Brush lineStroke = new SolidColorBrush(lineColor);
            Color XColor = Color.FromRgb(200, 20, 20);
            Brush XStroke = new SolidColorBrush(XColor);
            Color OColor = Color.FromRgb(20, 50, 200);
            Brush OStroke = new SolidColorBrush(OColor);

            int N = 4; // 4x4
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0) w = Math.Max(500, N * 60);
            if (h <= 0) h = Math.Max(500, N * 60);

            double cellW = w / N;
            double cellH = h / N;

            // vertical lines
            for (int c = 1; c < N; c++)
            {
                double x = c * cellW;
                Line lv = new Line() { X1 = x, Y1 = 0, X2 = x, Y2 = h, Stroke = lineStroke, StrokeThickness = 3.0 };
                canvas.Children.Add(lv);
            }

            // horizontal lines
            for (int r = 1; r < N; r++)
            {
                double y = r * cellH;
                Line lh = new Line() { X1 = 0, Y1 = y, X2 = w, Y2 = y, Stroke = lineStroke, StrokeThickness = 3.0 };
                canvas.Children.Add(lh);
            }

            double padding = Math.Min(cellW, cellH) * 0.12;

            // draw marks
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (currentField[i, j] == 1)
                    {
                        double x0 = j * cellW + padding;
                        double y0 = i * cellH + padding;
                        double x1 = (j + 1) * cellW - padding;
                        double y1 = (i + 1) * cellH - padding;
                        Line X1 = new Line() { X1 = x0, Y1 = y0, X2 = x1, Y2 = y1, Stroke = XStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(X1);
                        Line X2 = new Line() { X1 = x0, Y1 = y1, X2 = x1, Y2 = y0, Stroke = XStroke, StrokeThickness = 4.0 };
                        canvas.Children.Add(X2);
                    }
                    else if (currentField[i, j] == 2)
                    {
                        double left = j * cellW + padding;
                        double top = i * cellH + padding;
                        double width = cellW - 2 * padding;
                        double height = cellH - 2 * padding;
                        Ellipse OE = new Ellipse() { Width = width, Height = height, Stroke = OStroke, StrokeThickness = 4.0 };
                        Canvas.SetLeft(OE, left);
                        Canvas.SetTop(OE, top);
                        canvas.Children.Add(OE);
                    }
                }
            }
        }
    }

    public class A4_TicTacToeRules : X_BaseTicTacToeRules
    {
        A4_TicTacToeField _Field = new A4_TicTacToeField();

        public override IX_TicTacToeField TicTacToeField { get { return _Field; } }

        public override bool MovesPossible
        {
            get
            {
                int N = 4;
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
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

        public override string Name { get { return "A4_TicTacToeRules"; } }

        public override int CheckIfPLayerWon()
        {
            int N = 4;
            // rows
            for (int i = 0; i < N; i++)
            {
                int first = _Field[i, 0];
                if (first > 0)
                {
                    bool ok = true;
                    for (int j = 1; j < N; j++) if (_Field[i, j] != first) { ok = false; break; }
                    if (ok) return first;
                }
            }

            // cols
            for (int j = 0; j < N; j++)
            {
                int first = _Field[0, j];
                if (first > 0)
                {
                    bool ok = true;
                    for (int i = 1; i < N; i++) if (_Field[i, j] != first) { ok = false; break; }
                    if (ok) return first;
                }
            }

            // main diagonal
            int dfirst = _Field[0, 0];
            if (dfirst > 0)
            {
                bool ok = true;
                for (int i = 1; i < N; i++) if (_Field[i, i] != dfirst) { ok = false; break; }
                if (ok) return dfirst;
            }

            // anti diagonal
            dfirst = _Field[0, N - 1];
            if (dfirst > 0)
            {
                bool ok = true;
                for (int i = 1; i < N; i++) if (_Field[i, N - 1 - i] != dfirst) { ok = false; break; }
                if (ok) return dfirst;
            }

            return -1;
        }

        public override void ClearField()
        {
            int N = 4;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    _Field[i, j] = 0;
                }
            }
        }

        public override void DoTicTacToeMove(IX_TicTacToeMove move)
        {
            int N = 4;
            if (move.Row >= 0 && move.Row < N && move.Column >= 0 && move.Column < N)
            {
                _Field[move.Row, move.Column] = move.PlayerNumber;
            }
        }
    }

    public class A4_TicTacToeField : X_BaseTicTacToeField
    {
        int[,] _Field = new int[4, 4];

        public A4_TicTacToeField()
        {
            for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) _Field[i, j] = 0;
        }

        public override int this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 4 && c >= 0 && c < 4)
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
                if (r >= 0 && r < 4 && c >= 0 && c < 4)
                {
                    _Field[r, c] = value;
                }
            }
        }
    }

    public class A4_TicTacToeMove : IX_TicTacToeMove
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;

        public A4_TicTacToeMove(int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }
    }

    public class A4_TicTacToeHumanPlayer : X_BaseHumanTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "A4_HumanTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            A4_TicTacToeHumanPlayer p = new A4_TicTacToeHumanPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public override IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            // selection contains click coordinates relative to the canvas
            // we try to read optional canvas size via reflection (if ClickSelection was extended);
            // otherwise we fall back to a sensible default (500x500) and compute cell indices.

            if (selection is IClickSelection sel)
            {
                double canvasW = 500.0;
                double canvasH = 500.0;

                // try reflection to get optional CanvasWidth/CanvasHeight properties
                try
                {
                    var sType = selection.GetType();
                    var pw = sType.GetProperty("CanvasWidth");
                    var ph = sType.GetProperty("CanvasHeight");
                    if (pw != null)
                    {
                        var v = pw.GetValue(selection);
                        if (v is int) canvasW = (int)v;
                        else if (v is double) canvasW = (double)v;
                    }
                    if (ph != null)
                    {
                        var v = ph.GetValue(selection);
                        if (v is int) canvasH = (int)v;
                        else if (v is double) canvasH = (double)v;
                    }
                }
                catch { }

                int N = 4;
                double cellW = canvasW / N;
                double cellH = canvasH / N;

                int col = (int)(sel.XClickPos / cellW);
                int row = (int)(sel.YClickPos / cellH);

                if (row < 0) row = 0;
                if (col < 0) col = 0;
                if (row >= N) row = N - 1;
                if (col >= N) col = N - 1;

                if (field[row, col] <= 0)
                {
                    return new A4_TicTacToeMove(row, col, _PlayerNumber);
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
