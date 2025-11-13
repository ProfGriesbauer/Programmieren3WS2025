using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace OOPGames
{
    // Zustände eines Feldes
    public enum A2_CellState
    {
        Empty = 0,
        X = 1,
        O = 2
    }

    // Spielfeld 3x3
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
            {
                for (int c = 0; c < 3; c++)
                {
                    Cells[r, c] = A2_CellState.Empty;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A2_Painter;
        }
    }

    // Spielzug
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

    // Statischer Painter (fixe Feldgröße wie im Beispiel)
    public class A2_Painter : IPaintGame2
    {
        public string Name => "A2_TicTacToe_Painter";

        // Feste Geometrie des Spielfelds
        private const int BoardLeft = 20;
        private const int BoardTop = 20;
        private const int CellSize = 100;          // 3 * 100 = 300 => Boardgröße 300x300
        private const int BoardSize = CellSize * 3;

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField as A2_TicTacToeField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField as A2_TicTacToeField);
        }

        private void Draw(Canvas canvas, A2_TicTacToeField field)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // Gitterlinien (identisch zur Click-Geometrie)
            var lineStroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            // Vertikale Linien bei x = 120 und 220
            Line l1 = new Line() { X1 = BoardLeft + CellSize, Y1 = BoardTop, X2 = BoardLeft + CellSize, Y2 = BoardTop + BoardSize, Stroke = lineStroke, StrokeThickness = 2.0 };
            canvas.Children.Add(l1);
            Line l2 = new Line() { X1 = BoardLeft + 2 * CellSize, Y1 = BoardTop, X2 = BoardLeft + 2 * CellSize, Y2 = BoardTop + BoardSize, Stroke = lineStroke, StrokeThickness = 2.0 };
            canvas.Children.Add(l2);

            // Horizontale Linien bei y = 120 und 220
            Line l3 = new Line() { X1 = BoardLeft, Y1 = BoardTop + CellSize, X2 = BoardLeft + BoardSize, Y2 = BoardTop + CellSize, Stroke = lineStroke, StrokeThickness = 2.0 };
            canvas.Children.Add(l3);
            Line l4 = new Line() { X1 = BoardLeft, Y1 = BoardTop + 2 * CellSize, X2 = BoardLeft + BoardSize, Y2 = BoardTop + 2 * CellSize, Stroke = lineStroke, StrokeThickness = 2.0 };
            canvas.Children.Add(l4);

            if (field == null) return;

            // X / O zeichnen
            double padding = CellSize * 0.15;
            double inner = CellSize - 2 * padding;

            Brush xStroke = new SolidColorBrush(Color.FromRgb(200, 0, 0));
            Brush oStroke = new SolidColorBrush(Color.FromRgb(0, 0, 200));

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    A2_CellState state = field.Cells[r, c];
                    double cellLeft = BoardLeft + c * CellSize;
                    double cellTop = BoardTop + r * CellSize;

                    double x = cellLeft + padding;
                    double y = cellTop + padding;

                    if (state == A2_CellState.X)
                    {
                        Line x1 = new Line()
                        {
                            X1 = x,
                            Y1 = y,
                            X2 = x + inner,
                            Y2 = y + inner,
                            Stroke = xStroke,
                            StrokeThickness = 3.0
                        };
                        Line x2 = new Line()
                        {
                            X1 = x + inner,
                            Y1 = y,
                            X2 = x,
                            Y2 = y + inner,
                            Stroke = xStroke,
                            StrokeThickness = 3.0
                        };
                        canvas.Children.Add(x1);
                        canvas.Children.Add(x2);
                    }
                    else if (state == A2_CellState.O)
                    {
                        Ellipse e = new Ellipse()
                        {
                            Width = inner,
                            Height = inner,
                            Stroke = oStroke,
                            StrokeThickness = 3.0
                        };
                        Canvas.SetLeft(e, x);
                        Canvas.SetTop(e, y);
                        canvas.Children.Add(e);
                    }
                }
            }
        }
    }

    // Regeln
    public class A2_Rules : IGameRules
    {
        private readonly A2_TicTacToeField _field = new A2_TicTacToeField();
        private int _currentPlayer = 1; // 1 = X, 2 = O

        public string Name => "A2_TicTacToe_Rules";

        public IGameField CurrentField => _field;

        public bool MovesPossible
        {
            get
            {
                // Es gibt mindestens ein leeres Feld
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (_field.Cells[r, c] == A2_CellState.Empty) return true;
                    }
                }
                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not IRowMove rMove || move is not IColumnMove cMove)
                return;

            int row = rMove.Row;
            int col = cMove.Column;

            if (row < 0 || row > 2 || col < 0 || col > 2)
                return;

            if (_field.Cells[row, col] != A2_CellState.Empty)
                return;

            _field.Cells[row, col] = move.PlayerNumber == 1 ? A2_CellState.X : A2_CellState.O;

            // Spielerwechsel nur, wenn noch niemand gewonnen hat
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
            // Reihen und Spalten
            for (int i = 0; i < 3; i++)
            {
                // Zeile i
                if (_field.Cells[i, 0] != A2_CellState.Empty &&
                    _field.Cells[i, 0] == _field.Cells[i, 1] &&
                    _field.Cells[i, 1] == _field.Cells[i, 2])
                {
                    return _field.Cells[i, 0] == A2_CellState.X ? 1 : 2;
                }

                // Spalte i
                if (_field.Cells[0, i] != A2_CellState.Empty &&
                    _field.Cells[0, i] == _field.Cells[1, i] &&
                    _field.Cells[1, i] == _field.Cells[2, i])
                {
                    return _field.Cells[0, i] == A2_CellState.X ? 1 : 2;
                }
            }

            // Diagonalen
            if (_field.Cells[0, 0] != A2_CellState.Empty &&
                _field.Cells[0, 0] == _field.Cells[1, 1] &&
                _field.Cells[1, 1] == _field.Cells[2, 2])
            {
                return _field.Cells[0, 0] == A2_CellState.X ? 1 : 2;
            }

            if (_field.Cells[0, 2] != A2_CellState.Empty &&
                _field.Cells[0, 2] == _field.Cells[1, 1] &&
                _field.Cells[1, 1] == _field.Cells[2, 0])
            {
                return _field.Cells[0, 2] == A2_CellState.X ? 1 : 2;
            }

            // Kein Gewinner
            return -1;
        }

        public int CurrentPlayer => _currentPlayer;
    }

    // Human-Player mit Numpad und statischem Mausklick
    public class A2_HumanPlayer : IHumanGamePlayer
    {
        public string Name => "A2_Human_Player";

        public int PlayerNumber { get; private set; }

        public A2_HumanPlayer(int playerNumber = 1)
        {
            PlayerNumber = playerNumber;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A2_Rules;
        }

        public IGamePlayer Clone()
        {
            return new A2_HumanPlayer(PlayerNumber);
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || field is not A2_TicTacToeField tttField)
                return null;

            if (selection is IKeySelection keySel)
            {
                return GetMoveFromKey(keySel.Key, tttField);
            }

            if (selection is IClickSelection clickSel)
            {
                return GetMoveFromClick(clickSel, tttField);
            }

            return null;
        }

        // Numpad-Layout:
        // 7 8 9
        // 4 5 6
        // 1 2 3
        private IPlayMove GetMoveFromKey(Key key, A2_TicTacToeField field)
        {
            int? pos = key switch
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

            if (!pos.HasValue)
                return null;

            int row = pos.Value / 3;
            int col = pos.Value % 3;

            if (field.Cells[row, col] != A2_CellState.Empty)
                return null;

            return new A2_TicTacToeMove(PlayerNumber, row, col);
        }

        // Maus-Click mit statischen Koordinaten wie im Painter
        private IPlayMove GetMoveFromClick(IClickSelection sel, A2_TicTacToeField field)
        {
            const int BoardLeft = 20;
            const int BoardTop = 20;
            const int CellSize = 100;
            const int BoardSize = CellSize * 3;

            int x = sel.XClickPos;
            int y = sel.YClickPos;

            // Außerhalb des Boards?
            if (x < BoardLeft || x > BoardLeft + BoardSize ||
                y < BoardTop || y > BoardTop + BoardSize)
            {
                return null;
            }

            int col = (x - BoardLeft) / CellSize;
            int row = (y - BoardTop) / CellSize;

            if (row < 0 || row > 2 || col < 0 || col > 2)
                return null;

            if (field.Cells[row, col] != A2_CellState.Empty)
                return null;

            return new A2_TicTacToeMove(PlayerNumber, row, col);
        }
    }
        // Unbesiegbarer Computer-Spieler für A2-TicTacToe
    public class A2_ComputerPlayer : IComputerGamePlayer
    {
        public string Name => "A2_Computer_Player";

        public int PlayerNumber { get; private set; }

        public A2_ComputerPlayer(int playerNumber = 2)
        {
            PlayerNumber = playerNumber;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A2_Rules;
        }

        public IGamePlayer Clone()
        {
            return new A2_ComputerPlayer(PlayerNumber);
        }

        // Wird vom Framework aufgerufen, wenn der Computer am Zug ist
        public IPlayMove GetMove(IGameField field)
        {
            if (field is not A2_TicTacToeField tttField)
                return null;

            // Unbesiegbare Strategie mit Minimax
            int bestRow = -1;
            int bestCol = -1;
            int bestScore = int.MinValue;

            A2_CellState ai = PlayerNumber == 1 ? A2_CellState.X : A2_CellState.O;
            A2_CellState opp = PlayerNumber == 1 ? A2_CellState.O : A2_CellState.X;

            // Über alle möglichen Züge iterieren
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (tttField.Cells[r, c] == A2_CellState.Empty)
                    {
                        // Zug simulieren
                        tttField.Cells[r, c] = ai;
                        int score = Minimax(tttField, false, ai, opp);
                        // Rückgängig machen
                        tttField.Cells[r, c] = A2_CellState.Empty;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRow = r;
                            bestCol = c;
                        }
                    }
                }
            }

            if (bestRow == -1 || bestCol == -1)
            {
                // Kein gültiger Zug (z.B. Board voll)
                return null;
            }

            return new A2_TicTacToeMove(PlayerNumber, bestRow, bestCol);
        }

        // Minimax-Algorithmus: Computer maximiert, Gegner minimiert
        private int Minimax(A2_TicTacToeField field, bool isMaximizing,
                            A2_CellState ai, A2_CellState opp)
        {
            int eval = Evaluate(field, ai, opp);
            if (eval != 0)
            {
                return eval; // Sieg oder Niederlage
            }

            if (!HasEmptyCell(field))
            {
                return 0;    // Unentschieden
            }

            if (isMaximizing)
            {
                int best = int.MinValue;
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (field.Cells[r, c] == A2_CellState.Empty)
                        {
                            field.Cells[r, c] = ai;
                            int val = Minimax(field, false, ai, opp);
                            field.Cells[r, c] = A2_CellState.Empty;
                            if (val > best) best = val;
                        }
                    }
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (field.Cells[r, c] == A2_CellState.Empty)
                        {
                            field.Cells[r, c] = opp;
                            int val = Minimax(field, true, ai, opp);
                            field.Cells[r, c] = A2_CellState.Empty;
                            if (val < best) best = val;
                        }
                    }
                }
                return best;
            }
        }

        // Bewertet das Board aus Sicht des Computers:
        // +10 = Computer gewinnt, -10 = Gegner gewinnt, 0 = noch keiner
        private int Evaluate(A2_TicTacToeField field, A2_CellState ai, A2_CellState opp)
        {
            // Reihen & Spalten
            for (int i = 0; i < 3; i++)
            {
                // Zeile
                if (field.Cells[i, 0] != A2_CellState.Empty &&
                    field.Cells[i, 0] == field.Cells[i, 1] &&
                    field.Cells[i, 1] == field.Cells[i, 2])
                {
                    return field.Cells[i, 0] == ai ? 10 : -10;
                }

                // Spalte
                if (field.Cells[0, i] != A2_CellState.Empty &&
                    field.Cells[0, i] == field.Cells[1, i] &&
                    field.Cells[1, i] == field.Cells[2, i])
                {
                    return field.Cells[0, i] == ai ? 10 : -10;
                }
            }

            // Diagonalen
            if (field.Cells[0, 0] != A2_CellState.Empty &&
                field.Cells[0, 0] == field.Cells[1, 1] &&
                field.Cells[1, 1] == field.Cells[2, 2])
            {
                return field.Cells[0, 0] == ai ? 10 : -10;
            }

            if (field.Cells[0, 2] != A2_CellState.Empty &&
                field.Cells[0, 2] == field.Cells[1, 1] &&
                field.Cells[1, 1] == field.Cells[2, 0])
            {
                return field.Cells[0, 2] == ai ? 10 : -10;
            }

            return 0;
        }

        private bool HasEmptyCell(A2_TicTacToeField field)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (field.Cells[r, c] == A2_CellState.Empty)
                        return true;
                }
            }
            return false;
        }
    }

}
