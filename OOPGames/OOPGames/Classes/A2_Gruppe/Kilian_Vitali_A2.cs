// ...existing code...
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reflection;

namespace OOPGames
{
    // Simple TicTacToe implementation bundled into A2 files.
    // All helper classes are kept in this file as requested.

    public enum A2_CellState { Empty = 0, X = 1, O = 2 }

    // Minimal game field implementing the IGameField marker interface
    public class A2_TicTacToeField : IGameField
    {
        public A2_CellState[,] Cells { get; } = new A2_CellState[3, 3];

        public A2_TicTacToeField()
        {
            Clear();
        }

        public void Clear()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Cells[r, c] = A2_CellState.Empty;
        }

        // Implement required interface member
        public bool CanBePaintedBy(IPaintGame painter)
        {
            // Accept our painter or any painter that identifies as A2
            return painter is A2_Painter || (painter?.Name?.Contains("A2") ?? false);
        }
    }

    // Concrete move with row & column
    public class A2_TicTacToeMove : IRowMove, IColumnMove
    {
        public int PlayerNumber { get; }
        public int Row { get; }
        public int Column { get; }

        public A2_TicTacToeMove(int playerNumber, int row, int column)
        {
            PlayerNumber = playerNumber;
            Row = row;
            Column = column;
        }
    }

    // Painter for the TicTacToe field
    public class A2_Painter : IPaintGame2
{
    public string Name => "A2_TicTacToe_Painter";

    // Merkt sich die letzte Canvas-Größe (für Mausklick-Berechnung)
    public static double LastCanvasWidth { get; private set; } = 300;
    public static double LastCanvasHeight { get; private set; } = 300;

    // Wird aufgerufen, wenn ein Zug gemacht wurde
    public void PaintGameField(Canvas canvas, IGameField currentField)
    {
        DoPaint(canvas, currentField);
    }

    // Wird alle 40ms vom Timer aufgerufen
    public void TickPaintGameField(Canvas canvas, IGameField currentField)
    {
        DoPaint(canvas, currentField);
    }

    private void DoPaint(Canvas canvas, IGameField currentField)
    {
        if (canvas == null) return;

        canvas.Children.Clear();

        var field = currentField as A2_TicTacToeField;

        double width = Math.Max(1.0, canvas.ActualWidth);
        double height = Math.Max(1.0, canvas.ActualHeight);

        // Canvas-Größe merken, damit der HumanPlayer sie für Mausklicks nutzen kann
        LastCanvasWidth = width;
        LastCanvasHeight = height;

        double size = Math.Min(width, height);
        double cell = size / 3.0;
        double offsetX = (width - size) / 2.0;
        double offsetY = (height - size) / 2.0;

        // Gitter zeichnen
        var lineBrush = Brushes.Black;
        double thickness = 2.0;
        for (int i = 1; i <= 2; i++)
        {
            // vertikale Linien
            var v = new Line()
            {
                X1 = offsetX + i * cell,
                Y1 = offsetY,
                X2 = offsetX + i * cell,
                Y2 = offsetY + 3 * cell,
                Stroke = lineBrush,
                StrokeThickness = thickness
            };
            canvas.Children.Add(v);

            // horizontale Linien
            var h = new Line()
            {
                X1 = offsetX,
                Y1 = offsetY + i * cell,
                X2 = offsetX + 3 * cell,
                Y2 = offsetY + i * cell,
                Stroke = lineBrush,
                StrokeThickness = thickness
            };
            canvas.Children.Add(h);
        }

        if (field == null) return;

        // X / O zeichnen
        double padding = cell * 0.15;
        double inner = cell - 2 * padding;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var state = field.Cells[r, c];
                double x = offsetX + c * cell + padding;
                double y = offsetY + r * cell + padding;

                if (state == A2_CellState.O)
                {
                    var ellipse = new Ellipse()
                    {
                        Width = inner,
                        Height = inner,
                        Stroke = Brushes.DarkBlue,
                        StrokeThickness = 4
                    };
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                    canvas.Children.Add(ellipse);
                }
                else if (state == A2_CellState.X)
                {
                    var l1 = new Line()
                    {
                        X1 = x,
                        Y1 = y,
                        X2 = x + inner,
                        Y2 = y + inner,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 4,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };
                    var l2 = new Line()
                    {
                        X1 = x + inner,
                        Y1 = y,
                        X2 = x,
                        Y2 = y + inner,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 4,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };
                    canvas.Children.Add(l1);
                    canvas.Children.Add(l2);
                }
            }
        }
    }
}



    public class A2_Rules : IGameRules
    {
        private readonly A2_TicTacToeField _field = new A2_TicTacToeField();
        private int _currentPlayer = 1; // players: 1 -> X, 2 -> O

        public A2_Rules()
        {
            ClearField();
        }

        // IGameRules implementation
        public string Name => "A2_TicTacToe_Rules";

        public IGameField CurrentField => _field;

        // true if at least one empty cell and nobody has already won
        public bool MovesPossible => (_field.Cells.Cast<A2_CellState>().Any(s => s == A2_CellState.Empty)) && (CheckIfPlayerWon() == 0);

        // Adds the move if valid. Expects IRowMove & IColumnMove
        public void DoMove(IPlayMove move)
        {
            if (move == null) return;

            if (!(move is IRowMove rmove) || !(move is IColumnMove cmove)) return;

            int r = rmove.Row;
            int c = cmove.Column;

            if (r < 0 || r > 2 || c < 0 || c > 2) return;

            if (_field.Cells[r, c] != A2_CellState.Empty) return;

            A2_CellState mark = (move.PlayerNumber == 1) ? A2_CellState.X : A2_CellState.O;
            _field.Cells[r, c] = mark;

            // switch to other player if moves remain
            if (CheckIfPlayerWon() == 0)
            {
                _currentPlayer = (_currentPlayer == 1) ? 2 : 1;
            }
        }

        // convenience: reset board
        public void ClearField()
        {
            _field.Clear();
            _currentPlayer = 1;
        }

        // returns 0 = none, 1 = player 1 wins, 2 = player 2 wins, 3 = draw
        public int CheckIfPlayerWon()
        {
            // rows & columns
            for (int i = 0; i < 3; i++)
            {
                if (_field.Cells[i, 0] != A2_CellState.Empty &&
                    _field.Cells[i, 0] == _field.Cells[i, 1] && _field.Cells[i, 1] == _field.Cells[i, 2])
                {
                    return (_field.Cells[i, 0] == A2_CellState.X) ? 1 : 2;
                }

                if (_field.Cells[0, i] != A2_CellState.Empty &&
                    _field.Cells[0, i] == _field.Cells[1, i] && _field.Cells[1, i] == _field.Cells[2, i])
                {
                    return (_field.Cells[0, i] == A2_CellState.X) ? 1 : 2;
                }
            }

            // diagonals
            if (_field.Cells[0, 0] != A2_CellState.Empty &&
                _field.Cells[0, 0] == _field.Cells[1, 1] && _field.Cells[1, 1] == _field.Cells[2, 2])
            {
                return (_field.Cells[0, 0] == A2_CellState.X) ? 1 : 2;
            }
            if (_field.Cells[0, 2] != A2_CellState.Empty &&
                _field.Cells[0, 2] == _field.Cells[1, 1] && _field.Cells[1, 1] == _field.Cells[2, 0])
            {
                return (_field.Cells[0, 2] == A2_CellState.X) ? 1 : 2;
            }

            // draw?
            bool anyEmpty = _field.Cells.Cast<A2_CellState>().Any(s => s == A2_CellState.Empty);
            if (!anyEmpty) return 3;

            return 0;
        }

        // Implemented to satisfy possible interface typo
        public int CheckIfPLayerWon()
        {
            return CheckIfPlayerWon();
        }

        // Expose current player for external use (optional)
        public int CurrentPlayer => _currentPlayer;
    }

    // Human player implementation compatible with project move-selection types.
    public class A2_HumanPlayer : IHumanGamePlayer
{
    public string Name => "A2_Human_Player";
    public int PlayerNumber { get; private set; }

    public A2_HumanPlayer(int playerNumber = 1)
    {
        PlayerNumber = playerNumber;
    }

    public void SetPlayerNumber(int number) => PlayerNumber = number;

    public bool CanBeRuledBy(IGameRules rules) => rules is A2_Rules;

    public IGamePlayer Clone() => new A2_HumanPlayer(PlayerNumber);

    // Hilfsfunktion, falls wir irgendwann doch wieder Reflection-Werte casten wollen
    private bool TryGetDouble(object o, out double d)
    {
        if (o == null)
        {
            d = 0;
            return false;
        }

        switch (o)
        {
            case double x: d = x; return true;
            case float x: d = x; return true;
            case int x: d = x; return true;
            case long x: d = x; return true;
            case short x: d = x; return true;
            case decimal x: d = (double)x; return true;
            case uint x: d = x; return true;
            case ulong x: d = x; return true;
            case byte x: d = x; return true;
            case sbyte x: d = x; return true;
            case string s when double.TryParse(s, out var v): d = v; return true;
            default:
                d = 0;
                return false;
        }
    }

    public IPlayMove GetMove(IMoveSelection selection, IGameField field)
    {
        if (!(field is A2_TicTacToeField gameField) || selection == null)
            return null;

        // Tastatur (Numpad / Zahlenreihe)
        if (selection is IKeySelection keySelection)
        {
            return GetMoveFromKey(keySelection.Key, gameField);
        }

        // Maus
        if (selection is IClickSelection clickSelection)
        {
            return GetMoveFromClick(clickSelection, gameField);
        }

        return null;
    }

    // Numpad-Layout:
    // 7 8 9
    // 4 5 6
    // 1 2 3
    private IPlayMove GetMoveFromKey(Key key, A2_TicTacToeField gameField)
    {
        int? position = key switch
        {
            Key.NumPad7 or Key.D7 => 0, // oben links
            Key.NumPad8 or Key.D8 => 1, // oben Mitte
            Key.NumPad9 or Key.D9 => 2, // oben rechts

            Key.NumPad4 or Key.D4 => 3, // Mitte links
            Key.NumPad5 or Key.D5 => 4, // Mitte
            Key.NumPad6 or Key.D6 => 5, // Mitte rechts

            Key.NumPad1 or Key.D1 => 6, // unten links
            Key.NumPad2 or Key.D2 => 7, // unten Mitte
            Key.NumPad3 or Key.D3 => 8, // unten rechts

            _ => (int?)null
        };

        if (!position.HasValue)
        {
            return null;
        }

        int pos = position.Value;
        int row = pos / 3;
        int col = pos % 3;

        if (gameField.Cells[row, col] != A2_CellState.Empty)
        {
            return null;
        }

        return new A2_TicTacToeMove(PlayerNumber, row, col);
    }

    private IPlayMove GetMoveFromClick(IClickSelection clickSel, A2_TicTacToeField gameField)
    {
        // Klickposition auf der Canvas
        Point pos = new Point(clickSel.XClickPos, clickSel.YClickPos);

        // Basis: letzte Canvas-Größe aus dem Painter
        double width = A2_Painter.LastCanvasWidth;
        double height = A2_Painter.LastCanvasHeight;

        // Fallback, falls noch nichts gemalt wurde
        if (width <= 0 || height <= 0)
        {
            width = 300;
            height = 300;
        }

        // OPTIONAL: falls der Selection-Typ CanvasWidth/CanvasHeight Properties hat, diese nutzen
        var selType = clickSel.GetType();
        var pw = selType.GetProperty("CanvasWidth", BindingFlags.Instance | BindingFlags.Public);
        var ph = selType.GetProperty("CanvasHeight", BindingFlags.Instance | BindingFlags.Public);
        if (pw != null && ph != null)
        {
            var wv = pw.GetValue(clickSel);
            var hv = ph.GetValue(clickSel);
            if (TryGetDouble(wv, out double wd)) width = wd;
            if (TryGetDouble(hv, out double hd)) height = hd;
        }

        // Geometrie exakt wie im Painter
        double size = Math.Min(width, height);
        double cellSize = size / 3.0;
        double offsetX = (width - size) / 2.0;
        double offsetY = (height - size) / 2.0;

        int col = (int)Math.Floor((pos.X - offsetX) / cellSize);
        int row = (int)Math.Floor((pos.Y - offsetY) / cellSize);

        // Außerhalb des Spielfelds?
        if (row < 0 || row >= 3 || col < 0 || col >= 3)
        {
            return null;
        }

        // Feld belegt?
        if (gameField.Cells[row, col] != A2_CellState.Empty)
        {
            return null;
        }

        return new A2_TicTacToeMove(PlayerNumber, row, col);
    }
}


}