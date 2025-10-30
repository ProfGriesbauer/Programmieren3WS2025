using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    /**************************************************************************
     * PAINTER - Zeichnet das TicTacToe Spielfeld
     **************************************************************************/
    public class A3_LEA_TicTacToePaint : A3_LEA_BaseTicTacToePaint
    {
        public override string Name { get { return "A3 LEA TicTacToe"; } }

        public override void PaintTicTacToeField(Canvas canvas, IA3_LEA_TicTacToeField currentField)
        {
            // Canvas leeren
            canvas.Children.Clear();
            Color bgColor = Color.FromRgb(255, 255, 255);
            canvas.Background = new SolidColorBrush(bgColor);

            // Feste Größe wie im Griesbauer-Beispiel für konsistente Klick-Erkennung
            double cellSize = 100.0;
            double offset = 20.0;

            // Gitterlinien zeichnen
            Color lineColor = Color.FromRgb(0, 0, 0);
            Brush lineStroke = new SolidColorBrush(lineColor);

            // Vertikale Linien
            Line l1 = new Line() { X1 = offset + cellSize, Y1 = offset, X2 = offset + cellSize, Y2 = offset + (3 * cellSize), Stroke = lineStroke, StrokeThickness = 2 };
            canvas.Children.Add(l1);
            Line l2 = new Line() { X1 = offset + (2 * cellSize), Y1 = offset, X2 = offset + (2 * cellSize), Y2 = offset + (3 * cellSize), Stroke = lineStroke, StrokeThickness = 2 };
            canvas.Children.Add(l2);

            // Horizontale Linien
            Line l3 = new Line() { X1 = offset, Y1 = offset + cellSize, X2 = offset + (3 * cellSize), Y2 = offset + cellSize, Stroke = lineStroke, StrokeThickness = 2 };
            canvas.Children.Add(l3);
            Line l4 = new Line() { X1 = offset, Y1 = offset + (2 * cellSize), X2 = offset + (3 * cellSize), Y2 = offset + (2 * cellSize), Stroke = lineStroke, StrokeThickness = 2 };
            canvas.Children.Add(l4);

            // Spielsteine zeichnen (X und O)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int cellValue = currentField[row, col];
                    
                    if (cellValue == 1) // Spieler 1: X
                    {
                        // X zeichnen (zwei Linien)
                        double x = offset + (col * cellSize);
                        double y = offset + (row * cellSize);
                        double margin = cellSize * 0.1;

                        Line line1 = new Line()
                        {
                            X1 = x + margin,
                            Y1 = y + margin,
                            X2 = x + cellSize - margin,
                            Y2 = y + cellSize - margin,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 3
                        };
                        canvas.Children.Add(line1);

                        Line line2 = new Line()
                        {
                            X1 = x + cellSize - margin,
                            Y1 = y + margin,
                            X2 = x + margin,
                            Y2 = y + cellSize - margin,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 3
                        };
                        canvas.Children.Add(line2);
                    }
                    else if (cellValue == 2) // Spieler 2: O
                    {
                        // O zeichnen (Kreis)
                        double x = offset + (col * cellSize);
                        double y = offset + (row * cellSize);
                        double margin = cellSize * 0.1;
                        double diameter = cellSize - (2 * margin);

                        Ellipse ellipse = new Ellipse()
                        {
                            Width = diameter,
                            Height = diameter,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3
                        };
                        Canvas.SetLeft(ellipse, x + margin);
                        Canvas.SetTop(ellipse, y + margin);
                        canvas.Children.Add(ellipse);
                    }
                }
            }
        }
    }

    /**************************************************************************
     * FIELD - Das 3x3 Spielfeld
     **************************************************************************/
    public class A3_LEA_TicTacToeField : A3_LEA_BaseTicTacToeField
    {
        private int[,] _field = new int[3, 3];

        public override int this[int r, int c]
        {
            get { return _field[r, c]; }
            set { _field[r, c] = value; }
        }
    }

    /**************************************************************************
     * RULES - Spielregeln und Logik
     **************************************************************************/
    public class A3_LEA_TicTacToeRules : A3_LEA_BaseTicTacToeRules
    {
        private A3_LEA_TicTacToeField _field;
        private int _currentPlayerNumber = 1;

        public A3_LEA_TicTacToeRules()
        {
            _field = new A3_LEA_TicTacToeField();
            ClearField();
        }

        public override IA3_LEA_TicTacToeField TicTacToeField { get { return _field; } }

        public override string Name { get { return "A3 LEA TicTacToe Rules"; } }

        public override bool MovesPossible
        {
            get
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (_field[r, c] == 0)
                            return true;
                    }
                }
                return false;
            }
        }

        public override void ClearField()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _field[r, c] = 0;
                }
            }
            _currentPlayerNumber = 1;
        }

        public override void DoTicTacToeMove(IA3_LEA_TicTacToeMove move)
        {
            if (move == null) return;

            int row = move.Row;
            int col = move.Column;

            // Prüfen ob Feld gültig und leer ist
            if (row >= 0 && row < 3 && col >= 0 && col < 3 && _field[row, col] == 0)
            {
                _field[row, col] = _currentPlayerNumber;
                _currentPlayerNumber = (_currentPlayerNumber == 1) ? 2 : 1;
            }
        }

        public override int CheckIfPLayerWon()
        {
            // Zeilen prüfen
            for (int r = 0; r < 3; r++)
            {
                if (_field[r, 0] != 0 && _field[r, 0] == _field[r, 1] && _field[r, 1] == _field[r, 2])
                {
                    return _field[r, 0];
                }
            }

            // Spalten prüfen
            for (int c = 0; c < 3; c++)
            {
                if (_field[0, c] != 0 && _field[0, c] == _field[1, c] && _field[1, c] == _field[2, c])
                {
                    return _field[0, c];
                }
            }

            // Diagonalen prüfen
            if (_field[0, 0] != 0 && _field[0, 0] == _field[1, 1] && _field[1, 1] == _field[2, 2])
            {
                return _field[0, 0];
            }

            if (_field[0, 2] != 0 && _field[0, 2] == _field[1, 1] && _field[1, 1] == _field[2, 0])
            {
                return _field[0, 2];
            }

            // Unentschieden oder Spiel läuft noch
            if (!MovesPossible)
                return -1; // Unentschieden

            return 0; // Spiel läuft noch
        }
    }

    /**************************************************************************
     * MOVE - Ein Spielzug (Zeile, Spalte)
     **************************************************************************/
    public class A3_LEA_TicTacToeMove : IA3_LEA_TicTacToeMove
    {
        private int _row;
        private int _column;
        private int _playerNumber;

        public A3_LEA_TicTacToeMove(int row, int column, int playerNumber)
        {
            _row = row;
            _column = column;
            _playerNumber = playerNumber;
        }

        public int Row { get { return _row; } }
        public int Column { get { return _column; } }
        public int PlayerNumber { get { return _playerNumber; } }
    }

    /**************************************************************************
     * HUMAN PLAYER - Menschlicher Spieler
     **************************************************************************/
    public class A3_LEA_TicTacToeHumanPlayer : A3_LEA_BaseHumanTicTacToePlayer
    {
        private int _playerNumber = 0;

        public override string Name { get { return "A3 LEA Human"; } }

        public override int PlayerNumber { get { return _playerNumber; } }

        public override void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public override IA3_LEA_TicTacToeMove GetMove(IMoveSelection selection, IA3_LEA_TicTacToeField field)
        {
            if (selection == null) return null;

            // Mausklick in Zeile/Spalte umrechnen
            if (selection is IClickSelection)
            {
                IClickSelection clickSelection = (IClickSelection)selection;
                
                // Verwende die gleichen Koordinaten wie im Painter
                double cellSize = 100.0;
                double offset = 20.0;
                
                // Durchlaufe alle Felder und prüfe, ob der Klick in einem Feld liegt
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        double xMin = offset + (col * cellSize);
                        double xMax = offset + ((col + 1) * cellSize);
                        double yMin = offset + (row * cellSize);
                        double yMax = offset + ((row + 1) * cellSize);
                        
                        if (clickSelection.XClickPos >= xMin && 
                            clickSelection.XClickPos <= xMax &&
                            clickSelection.YClickPos >= yMin && 
                            clickSelection.YClickPos <= yMax &&
                            field[row, col] == 0)
                        {
                            return new A3_LEA_TicTacToeMove(row, col, _playerNumber);
                        }
                    }
                }
            }

            return null;
        }

        public override IGamePlayer Clone()
        {
            return new A3_LEA_TicTacToeHumanPlayer();
        }
    }

    /**************************************************************************
     * COMPUTER PLAYER - Unschlagbarer KI-Spieler mit Minimax-Algorithmus
     **************************************************************************/
    public class A3_LEA_TicTacToeComputerPlayer : A3_LEA_BaseComputerTicTacToePlayer
    {
        private int _playerNumber = 0;
        private Random _random = new Random();

        public override string Name { get { return "A3 LEA Computer (Unschlagbar)"; } }

        public override int PlayerNumber { get { return _playerNumber; } }

        public override void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public override IA3_LEA_TicTacToeMove GetMove(IA3_LEA_TicTacToeField field)
        {
            // Verwende Minimax-Algorithmus für den besten Zug
            int bestScore = int.MinValue;
            IA3_LEA_TicTacToeMove bestMove = null;

            // Alle möglichen Züge durchgehen
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (field[row, col] == 0)
                    {
                        // Simuliere den Zug
                        field[row, col] = _playerNumber;

                        // Bewerte den Zug mit Minimax
                        int score = Minimax(field, 0, false);

                        // Rückgängig machen
                        field[row, col] = 0;

                        // Besten Zug speichern
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = new A3_LEA_TicTacToeMove(row, col, _playerNumber);
                        }
                    }
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Minimax-Algorithmus: Findet den besten Zug durch rekursive Spielbaum-Suche
        /// </summary>
        /// <param name="field">Aktuelles Spielfeld</param>
        /// <param name="depth">Aktuelle Tiefe im Spielbaum</param>
        /// <param name="isMaximizing">true = Computer ist dran, false = Gegner ist dran</param>
        /// <returns>Score des besten Zugs</returns>
        private int Minimax(IA3_LEA_TicTacToeField field, int depth, bool isMaximizing)
        {
            // Prüfe, ob das Spiel vorbei ist
            int winner = CheckWinner(field);
            
            // Computer gewinnt: Positiver Score (weniger Züge = besser)
            if (winner == _playerNumber)
                return 10 - depth;
            
            // Gegner gewinnt: Negativer Score (mehr Züge = weniger schlimm)
            int opponentNumber = (_playerNumber == 1) ? 2 : 1;
            if (winner == opponentNumber)
                return depth - 10;
            
            // Unentschieden: Neutraler Score
            if (!HasMovesLeft(field))
                return 0;

            if (isMaximizing)
            {
                // Computer ist dran: Maximiere den Score
                int bestScore = int.MinValue;

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (field[row, col] == 0)
                        {
                            // Simuliere Computerzug
                            field[row, col] = _playerNumber;
                            int score = Minimax(field, depth + 1, false);
                            field[row, col] = 0;

                            bestScore = Math.Max(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                // Gegner ist dran: Minimiere den Score
                int bestScore = int.MaxValue;
                int opponentNum = (_playerNumber == 1) ? 2 : 1;

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (field[row, col] == 0)
                        {
                            // Simuliere Gegnerzug
                            field[row, col] = opponentNum;
                            int score = Minimax(field, depth + 1, true);
                            field[row, col] = 0;

                            bestScore = Math.Min(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
        }

        /// <summary>
        /// Prüft, ob noch Züge möglich sind
        /// </summary>
        private bool HasMovesLeft(IA3_LEA_TicTacToeField field)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (field[row, col] == 0)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Prüft, ob jemand gewonnen hat
        /// </summary>
        /// <returns>Spielernummer des Gewinners, 0 wenn niemand gewonnen hat</returns>
        private int CheckWinner(IA3_LEA_TicTacToeField field)
        {
            // Prüfe Zeilen
            for (int row = 0; row < 3; row++)
            {
                if (field[row, 0] != 0 && 
                    field[row, 0] == field[row, 1] && 
                    field[row, 1] == field[row, 2])
                {
                    return field[row, 0];
                }
            }

            // Prüfe Spalten
            for (int col = 0; col < 3; col++)
            {
                if (field[0, col] != 0 && 
                    field[0, col] == field[1, col] && 
                    field[1, col] == field[2, col])
                {
                    return field[0, col];
                }
            }

            // Prüfe Diagonalen
            if (field[0, 0] != 0 && 
                field[0, 0] == field[1, 1] && 
                field[1, 1] == field[2, 2])
            {
                return field[0, 0];
            }

            if (field[0, 2] != 0 && 
                field[0, 2] == field[1, 1] && 
                field[1, 1] == field[2, 0])
            {
                return field[0, 2];
            }

            return 0; // Niemand hat gewonnen
        }

        public override IGamePlayer Clone()
        {
            return new A3_LEA_TicTacToeComputerPlayer();
        }
    }
}
