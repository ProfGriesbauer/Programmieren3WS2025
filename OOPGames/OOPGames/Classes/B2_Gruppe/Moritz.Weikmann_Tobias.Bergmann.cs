using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace OOPGames
{
    /*
     * B2 TicTacToe Implementation (Moritz Weikmann & Tobias Bergmann)
     * 
     * Struktur:
     * 1. Abstrakte Basisklassen (Abstract Base Classes)
     * 2. Konkrete Implementierungen (Concrete Implementations)
     * 
     
     */

    #region Abstract Base Classes

    
    /// Abstrakte Basis für das TicTacToe Spielfeld.
    /// Verwaltet ein 3x3 Grid und bietet grundlegende Operationen.
    
    public abstract class B2_AbstractTicTacToeField : IGameField
    {
        protected readonly int[,] grid = new int[3, 3];
        /// Indexer für direkten Zugriff auf Zellen. 0=leer, 1=Spieler1, 2=Spieler2
        public virtual int this[int r, int c]
        {
            get
            {
                if (r < 0 || r > 2 || c < 0 || c > 2) throw new IndexOutOfRangeException();
                return grid[r, c];
            }
            set
            {
                if (r < 0 || r > 2 || c < 0 || c > 2) throw new IndexOutOfRangeException();
                grid[r, c] = value;
            }
        }
        /// Setzt alle Zellen auf 0 (leer) zurück.
        public virtual void Clear()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    grid[r, c] = 0;
        }
        /// Gibt alle leeren Zellen als Liste zurück.
        public virtual List<(int row, int col)> GetEmptyCells()
        {
            var list = new List<(int, int)>();
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (grid[r, c] == 0) list.Add((r, c));
            return list;
        }

        public virtual bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }


    /// Abstrakte Basis für TicTacToe Spielregeln.
    /// Implementiert gemeinsame Logik und verwaltet das Spielfeld.
    public abstract class B2_AbstractTicTacToeRules : IGameRules
    {
        protected readonly B2_AbstractTicTacToeField field;

        protected B2_AbstractTicTacToeRules(B2_AbstractTicTacToeField field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public virtual string Name => "Abstract TicTacToe Rules";

        public virtual IGameField CurrentField => field;

        /// Prüft ob ein Spieler gewonnen hat.
        /// <returns>-1 = kein Gewinner, 1 oder 2 = Spieler hat gewonnen</returns>
        public abstract int CheckIfPLayerWon();

        public virtual bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() != -1) return false;
                return field.GetEmptyCells().Count > 0;
            }
        }

        public abstract void ClearField();

        public abstract void DoMove(IPlayMove move);
    }

    /// Abstrakte Basis für menschliche TicTacToe Spieler.
    public abstract class B2_AbstractHumanTicTacToePlayer : IHumanGamePlayer
    {
        public virtual string Name { get; protected set; }
        public virtual int PlayerNumber { get; protected set; }

        public virtual void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public virtual bool CanBeRuledBy(IGameRules rules) => rules is B2_AbstractTicTacToeRules;

        public abstract IGamePlayer Clone();

        public abstract IPlayMove GetMove(IMoveSelection selection, IGameField field);
    }

    /// Abstrakte Basis für Computer TicTacToe Spieler.
    public abstract class B2_AbstractComputerTicTacToePlayer : IComputerGamePlayer
    {
        public virtual string Name { get; protected set; }
        public virtual int PlayerNumber { get; protected set; }

        public virtual void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public virtual bool CanBeRuledBy(IGameRules rules) => rules is B2_AbstractTicTacToeRules;

        public abstract IGamePlayer Clone();

        public abstract IPlayMove GetMove(IGameField field);
    }

    /// Abstrakte Basis für TicTacToe Painter.
    /// Verwendet Template Method Pattern für typsichere Rendering-Delegation
    public abstract class B2_AbstractTicTacToePainter : IPaintGame
    {
        public abstract string Name { get; }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null) return;
            if (currentField is B2_AbstractTicTacToeField f)
            {
                PaintTicTacToeField(canvas, f);
            }
        }

        protected abstract void PaintTicTacToeField(Canvas canvas, B2_AbstractTicTacToeField field);
    }

    #endregion

    #region Concrete Implementations

    /// Konkrete Implementierung des TicTacToe Spielfelds.
    public class B2_TicTacToeField : B2_AbstractTicTacToeField
    {
        // Erbt alle Funktionalität von der abstrakten Basis
    }

    /// Repräsentiert einen Spielzug mit Zeile und Spalte.
    public class B2_TicTacToeMove : IRowMove, IColumnMove
    {
        public int PlayerNumber { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }

        public B2_TicTacToeMove(int player, int row, int col)
        {
            PlayerNumber = player;
            Row = row;
            Column = col;
        }
    }

    /// Konkrete Implementierung der TicTacToe Spielregeln.
    /// Prüft auf Gewinner in Reihen, Spalten und Diagonalen.
    public class B2_TicTacToeRules : B2_AbstractTicTacToeRules
    {
        public B2_TicTacToeRules() : base(new B2_TicTacToeField()) { }

        public override string Name => "B2 - TicTacToe Rules";

        public override int CheckIfPLayerWon()
        {
            var f = (B2_AbstractTicTacToeField)field;
            
            // Prüfe Reihen
            for (int r = 0; r < 3; r++)
            {
                if (f[r, 0] != 0 && f[r, 0] == f[r, 1] && f[r, 1] == f[r, 2])
                    return f[r, 0];
            }
            
            // Prüfe Spalten
            for (int c = 0; c < 3; c++)
            {
                if (f[0, c] != 0 && f[0, c] == f[1, c] && f[1, c] == f[2, c])
                    return f[0, c];
            }
            
            // Prüfe Diagonalen
            if (f[0, 0] != 0 && f[0, 0] == f[1, 1] && f[1, 1] == f[2, 2]) 
                return f[0, 0];
            if (f[0, 2] != 0 && f[0, 2] == f[1, 1] && f[1, 1] == f[2, 0]) 
                return f[0, 2];

            return -1;
        }

        public override void ClearField()
        {
            field.Clear();
        }

        public override void DoMove(IPlayMove move)
        {
            if (move == null) return;

            if (move is IRowMove rmove && move is IColumnMove cmove)
            {
                int r = rmove.Row;
                int c = cmove.Column;

                // Bounds check
                if (r < 0 || r > 2 || c < 0 || c > 2) return;

                // Zelle bereits belegt?
                if (field[r, c] != 0) return;

                // Zug ausführen
                int p = move.PlayerNumber;
                if (p == 1 || p == 2)
                {
                    field[r, c] = p;
                }
            }
        }
    }

    /// Menschlicher Spieler - konvertiert Mausklicks in Spielzüge.
    /// Verwendet dynamische Canvas-Größenerkennung für korrekte Zellenberechnung.

    public class B2_HumanTicTacToePlayer : B2_AbstractHumanTicTacToePlayer
    {
        public B2_HumanTicTacToePlayer(string name = "B2 - Human")
        {
            Name = name;
        }

        public override IGamePlayer Clone()
        {
            return new B2_HumanTicTacToePlayer(Name);
        }

        public override IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IClickSelection click)
            {
                // Canvas-Dimensionen ermitteln (dynamisch)
                double canvasW = 300, canvasH = 300;
                
                try
                {
                    var selType = click.GetType();
                    var propW = selType.GetProperty("CanvasWidth");
                    var propH = selType.GetProperty("CanvasHeight");
                    if (propW != null && propH != null)
                    {
                        canvasW = Convert.ToDouble(propW.GetValue(click));
                        canvasH = Convert.ToDouble(propH.GetValue(click));
                    }
                }
                catch { }
                
                // Fallback: Canvas aus MainWindow suchen
                if (canvasW == 300 && canvasH == 300)
                {
                    try
                    {
                        var app = System.Windows.Application.Current;
                        var main = app?.MainWindow;
                        var canvas = main?.FindName("PaintCanvas") as System.Windows.Controls.Canvas;
                        if (canvas != null)
                        {
                            canvasW = canvas.ActualWidth > 0 ? canvas.ActualWidth : 300;
                            canvasH = canvas.ActualHeight > 0 ? canvas.ActualHeight : 300;
                        }
                    }
                    catch { }
                }
                
                // Zellengröße berechnen (muss mit Painter übereinstimmen)
                double cellW = canvasW / 3.0;
                double cellH = canvasH / 3.0;
                
                // Durchsuche alle 9 Zellen
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        double cellLeft = j * cellW;
                        double cellRight = (j + 1) * cellW;
                        double cellTop = i * cellH;
                        double cellBottom = (i + 1) * cellH;
                        
                        // Klick innerhalb der Zelle? (> und < exkludieren Gitterlinien)
                        if (click.XClickPos > cellLeft && click.XClickPos < cellRight &&
                            click.YClickPos > cellTop && click.YClickPos < cellBottom)
                        {
                            // Prüfe ob Zelle leer ist
                            if (field is B2_AbstractTicTacToeField f && f[i, j] != 0)
                            {
                                return null;
                            }
                            
                            return new B2_TicTacToeMove(PlayerNumber, i, j);
                        }
                    }
                }
            }

            return null;
        }
    }

    /// Einfacher Computer-Spieler - wählt zufällige leere Felder.
    public class B2_ComputerTicTacToePlayer : B2_AbstractComputerTicTacToePlayer
    {
        private readonly Random rnd = new Random();

        public B2_ComputerTicTacToePlayer(string name = "B2 - Computer (Easy)")
        {
            Name = name;
        }

        public override IGamePlayer Clone()
        {
            return new B2_ComputerTicTacToePlayer(Name);
        }

        public override IPlayMove GetMove(IGameField field)
        {
            if (!(field is B2_AbstractTicTacToeField tfield)) return null;
            
            var empties = tfield.GetEmptyCells();
            if (empties.Count == 0) return null;
            
            var pick = empties[rnd.Next(empties.Count)];
            return new B2_TicTacToeMove(PlayerNumber, pick.row, pick.col);
        }
    }

    /// Intelligenter Computer-Spieler mit Minimax-Algorithmus.
    /// Spielt perfekt - sehr schwer zu schlagen!
    
    public class B2_SmartComputerTicTacToePlayer : B2_AbstractComputerTicTacToePlayer
    {
        public B2_SmartComputerTicTacToePlayer(string name = "B2 - Computer (Hard)")
        {
            Name = name;
        }

        public override IGamePlayer Clone()
        {
            return new B2_SmartComputerTicTacToePlayer(Name);
        }

        public override IPlayMove GetMove(IGameField field)
        {
            if (!(field is B2_AbstractTicTacToeField tfield)) return null;

            var empties = tfield.GetEmptyCells();
            if (empties.Count == 0) return null;

            // Wenn es der erste Zug ist, wähle die Mitte oder eine Ecke
            if (empties.Count == 9)
            {
                return new B2_TicTacToeMove(PlayerNumber, 1, 1); // Mitte
            }

            // Minimax-Algorithmus verwenden
            int bestScore = int.MinValue;
            (int row, int col) bestMove = (-1, -1);

            foreach (var (row, col) in empties)
            {
                // Simuliere den Zug
                tfield[row, col] = PlayerNumber;
                
                int score = Minimax(tfield, 0, false);
                
                // Rückgängig machen
                tfield[row, col] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (row, col);
                }
            }

            if (bestMove.row >= 0)
            {
                return new B2_TicTacToeMove(PlayerNumber, bestMove.row, bestMove.col);
            }

            return null;
        }

        /// Minimax-Algorithmus: Rekursive Spielbaumsuche für optimale Züge.
    
        /// <param name="field">Aktuelles Spielfeld</param>
        /// <param name="depth">Aktuelle Tiefe im Suchbaum</param>
        /// <param name="isMaximizing">True = Maximierender Spieler (wir), False = Minimierender Spieler (Gegner)</param>
        /// <returns>Bewertung der Position (+10 = Sieg, -10 = Niederlage, 0 = Unentschieden)</returns>
        private int Minimax(B2_AbstractTicTacToeField field, int depth, bool isMaximizing)
        {
            int winner = CheckWinner(field);
            
            // Terminal-Zustände bewerten
            if (winner == PlayerNumber) return 10 - depth; // Je schneller der Sieg, desto besser
            if (winner != -1 && winner != PlayerNumber) return depth - 10; // Je später die Niederlage, desto besser
            
            var empties = field.GetEmptyCells();
            if (empties.Count == 0) return 0; // Unentschieden

            if (isMaximizing)
            {
                // Maximierender Spieler (Computer)
                int maxScore = int.MinValue;
                foreach (var (row, col) in empties)
                {
                    field[row, col] = PlayerNumber;
                    int score = Minimax(field, depth + 1, false);
                    field[row, col] = 0;
                    maxScore = Math.Max(maxScore, score);
                }
                return maxScore;
            }
            else
            {
                // Minimierender Spieler (Gegner)
                int minScore = int.MaxValue;
                int opponent = (PlayerNumber == 1) ? 2 : 1;
                foreach (var (row, col) in empties)
                {
                    field[row, col] = opponent;
                    int score = Minimax(field, depth + 1, true);
                    field[row, col] = 0;
                    minScore = Math.Min(minScore, score);
                }
                return minScore;
            }
        }

        /// Prüft ob jemand gewonnen hat (benötigt für Minimax).
        private int CheckWinner(B2_AbstractTicTacToeField f)
        {
            // Reihen prüfen
            for (int r = 0; r < 3; r++)
            {
                if (f[r, 0] != 0 && f[r, 0] == f[r, 1] && f[r, 1] == f[r, 2])
                    return f[r, 0];
            }
            
            // Spalten prüfen
            for (int c = 0; c < 3; c++)
            {
                if (f[0, c] != 0 && f[0, c] == f[1, c] && f[1, c] == f[2, c])
                    return f[0, c];
            }
            
            // Diagonalen prüfen
            if (f[0, 0] != 0 && f[0, 0] == f[1, 1] && f[1, 1] == f[2, 2]) 
                return f[0, 0];
            if (f[0, 2] != 0 && f[0, 2] == f[1, 1] && f[1, 1] == f[2, 0]) 
                return f[0, 2];

            return -1;
        }
    }

    /// Painter für TicTacToe - zeichnet Spielfeld mit X und O auf Canvas.
    /// Verwendet dynamische Größenanpassung und WPF-optimierte Event-Behandlung.
    public class B2_TicTacToePainter : B2_AbstractTicTacToePainter
    {
        public override string Name => "B2 - TicTacToe Painter";

        protected override void PaintTicTacToeField(Canvas canvas, B2_AbstractTicTacToeField field)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            
            // KRITISCH: Transparenter Hintergrund ermöglicht Mausklick-Events!
            // Ohne dies empfängt das Canvas keine Klicks auf leeren Bereichen.
            canvas.Background = new SolidColorBrush(Colors.Transparent);

            // Dynamische Canvas-Größe ermitteln
            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : (canvas.Width > 0 ? canvas.Width : 300);
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : (canvas.Height > 0 ? canvas.Height : 300);

            double cellW = w / 3.0;
            double cellH = h / 3.0;

            var lineBrush = new SolidColorBrush(Colors.Black);

            // Gitterlinien zeichnen
            for (int i = 1; i <= 2; i++)
            {
                // Vertikale Linie
                var vLine = new Line()
                {
                    X1 = i * cellW,
                    Y1 = 0,
                    X2 = i * cellW,
                    Y2 = h,
                    Stroke = lineBrush,
                    StrokeThickness = 2,
                    IsHitTestVisible = false  // Klicks durchlassen!
                };
                canvas.Children.Add(vLine);

                // Horizontale Linie
                var hLine = new Line()
                {
                    X1 = 0,
                    Y1 = i * cellH,
                    X2 = w,
                    Y2 = i * cellH,
                    Stroke = lineBrush,
                    StrokeThickness = 2,
                    IsHitTestVisible = false  // Klicks durchlassen!
                };
                canvas.Children.Add(hLine);
            }

            // X und O zeichnen
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    double x = c * cellW;
                    double y = r * cellH;
                    int val = field[r, c];
                    
                    if (val == 1)
                    {
                        // X zeichnen (zwei sich kreuzende Linien)
                        var l1 = new Line()
                        {
                            X1 = x + 10,
                            Y1 = y + 10,
                            X2 = x + cellW - 10,
                            Y2 = y + cellH - 10,
                            Stroke = Brushes.DarkBlue,
                            StrokeThickness = 4,
                            IsHitTestVisible = false
                        };
                        var l2 = new Line()
                        {
                            X1 = x + cellW - 10,
                            Y1 = y + 10,
                            X2 = x + 10,
                            Y2 = y + cellH - 10,
                            Stroke = Brushes.DarkBlue,
                            StrokeThickness = 4,
                            IsHitTestVisible = false
                        };
                        canvas.Children.Add(l1);
                        canvas.Children.Add(l2);
                    }
                    else if (val == 2)
                    {
                        // O zeichnen (Kreis)
                        var ellipse = new Ellipse()
                        {
                            Width = Math.Max(0, cellW - 20),
                            Height = Math.Max(0, cellH - 20),
                            Stroke = Brushes.DarkRed,
                            StrokeThickness = 4,
                            IsHitTestVisible = false
                        };
                        Canvas.SetLeft(ellipse, x + 10);
                        Canvas.SetTop(ellipse, y + 10);
                        canvas.Children.Add(ellipse);
                    }
                }
            }
        }
    }

    #endregion
}
