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
    // Minimal, self-contained TicTacToe implementation that uses the
    // existing interfaces in /Interfaces without modifying them.
    // - TicTacToeField: holds a 3x3 int grid (0=empty,1=player1,2=player2)
    // - TicTacToeMove: simple IRowMove + IColumnMove implementation
    // - TicTacToeRules: implements IGameRules and contains win/draw logic
    // - Human/Computer players: implement IHumanGamePlayer / IComputerGamePlayer
    // - TicTacToePainter: simple WPF Canvas painter drawing grid and X/O

    // Abstract base field sits between the IGameField interface and concrete field
    public abstract class B2_AbstractTicTacToeField : IGameField
    {
        protected readonly int[,] grid = new int[3, 3];

        // indexer convenience for rules/paints
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

        public virtual void Clear()
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    grid[r, c] = 0;
        }

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
            // Simple policy: any painter can try to paint this field.
            return painter != null;
        }
    }

    // Concrete field now derives from the abstract base
    public class B2_TicTacToeField : B2_AbstractTicTacToeField
    {
    }

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

    // Abstract base rules class that implements common logic and keeps a reference to the field
    public abstract class B2_AbstractTicTacToeRules : IGameRules
    {
        protected readonly B2_AbstractTicTacToeField field;

        protected B2_AbstractTicTacToeRules(B2_AbstractTicTacToeField field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public virtual string Name => "Abstract TicTacToe Rules";

        public virtual IGameField CurrentField => field;

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

    // Concrete rules derive from the abstract base
    // Concrete rules derive from the abstract base
    public class B2_TicTacToeRules : B2_AbstractTicTacToeRules
    {

        public B2_TicTacToeRules() : base(new B2_TicTacToeField()) { }

        public override string Name => "B2 - Simple TicTacToe Rules";

        // -1 = no winner yet, 1/2 = player won
        public override int CheckIfPLayerWon()
        {
            var f = (B2_AbstractTicTacToeField)field;
            // check rows
            for (int r = 0; r < 3; r++)
            {
                if (f[r, 0] != 0 && f[r, 0] == f[r, 1] && f[r, 1] == f[r, 2])
                    return f[r, 0];
            }
            // check cols
            for (int c = 0; c < 3; c++)
            {
                if (f[0, c] != 0 && f[0, c] == f[1, c] && f[1, c] == f[2, c])
                    return f[0, c];
            }
            // diagonals
            if (f[0, 0] != 0 && f[0, 0] == f[1, 1] && f[1, 1] == f[2, 2]) return f[0, 0];
            if (f[0, 2] != 0 && f[0, 2] == f[1, 1] && f[1, 1] == f[2, 0]) return f[0, 2];

            return -1; // interface: -1 if no player won
        }

        public override void ClearField()
        {
            field.Clear();
        }

    public override void DoMove(IPlayMove move)
        {
            if (move == null) return;

            // Expect row+column moves
            if (move is IRowMove rmove && move is IColumnMove cmove)
            {
                int r = rmove.Row;
                int c = cmove.Column;

                // bounds check
                if (r < 0 || r > 2 || c < 0 || c > 2) return;

                // field already occupied?
                if (field[r, c] != 0) return;

                // apply
                try
                {
                    // PlayerNumber from IPlayMove
                    int p = move.PlayerNumber;
                    if (p != 1 && p != 2) return; // only two players supported
                    field[r, c] = p;
                }
                catch
                {
                    // ignore invalid moves silently (could be logged)
                }
            }
        }
    }

    // Abstract human player to sit between IHumanGamePlayer and concrete implementation
    // Abstract human player to sit between IHumanGamePlayer and concrete implementation
    public abstract class B2_AbstractHumanTicTacToePlayer : IHumanGamePlayer
    {
        public virtual string Name { get; protected set; }
        public virtual int PlayerNumber { get; protected set; }

        public virtual void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public virtual bool CanBeRuledBy(IGameRules rules) => rules is B2_AbstractTicTacToeRules;

        public abstract IGamePlayer Clone();

        public abstract IPlayMove GetMove(IMoveSelection selection, IGameField field);
    }

    public class B2_HumanTicTacToePlayer : B2_AbstractHumanTicTacToePlayer
    {

        // optional: how many pixels per cell the selection uses when click coords are given
        public int CellSize { get; set; } = 100;

        public B2_HumanTicTacToePlayer(string name = "B2 - Human")
        {
            Name = name;
        }


        public override void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }
        public override bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B2_AbstractTicTacToeRules;
        }

        public override IGamePlayer Clone()
        {
            var clone = new B2_HumanTicTacToePlayer(Name) { CellSize = this.CellSize };
            return clone;
        }

        public override IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            // we only accept click selections here
            if (selection is IPaintGame2) return null; // sanity

            if (selection is IClickSelection click)
            {
                int col = click.XClickPos / Math.Max(1, CellSize);
                int row = click.YClickPos / Math.Max(1, CellSize);

                if (row < 0 || row > 2 || col < 0 || col > 2) return null;

                return new B2_TicTacToeMove(PlayerNumber, row, col);
            }

            return null;
        }
    }
    // Abstract computer player
    public abstract class B2_AbstractComputerTicTacToePlayer : IComputerGamePlayer
    {
        public virtual string Name { get; protected set; }
        public virtual int PlayerNumber { get; protected set; }

        public virtual void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public virtual bool CanBeRuledBy(IGameRules rules) => rules is B2_AbstractTicTacToeRules;

        public abstract IGamePlayer Clone();

        public abstract IPlayMove GetMove(IGameField field);
    }
    public class B2_ComputerTicTacToePlayer : B2_AbstractComputerTicTacToePlayer
    {
        private readonly Random rnd = new Random();

        public B2_ComputerTicTacToePlayer(string name = "B2 - Computer") { Name = name; }

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

    // Abstract painter provides a hook between IPaintGame and concrete painter
    public abstract class B2_AbstractTicTacToePainter : IPaintGame
    {
        public abstract string Name { get; }

        // Template method: check field type then forward
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
    public class B2_TicTacToePainter : B2_AbstractTicTacToePainter
    {
        public override string Name => "B2 - TicTacToe Painter";

        protected override void PaintTicTacToeField(Canvas canvas, B2_AbstractTicTacToeField field)
        {
            if (canvas == null) return;

            canvas.Children.Clear();

            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : (canvas.Width > 0 ? canvas.Width : 300);
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : (canvas.Height > 0 ? canvas.Height : 300);

            double cellW = w / 3.0;
            double cellH = h / 3.0;

            var lineBrush = new SolidColorBrush(Colors.Black);

            // draw grid lines
            for (int i = 1; i <= 2; i++)
            {
                var v = new Line()
                {
                    X1 = i * cellW,
                    Y1 = 0,
                    X2 = i * cellW,
                    Y2 = h,
                    Stroke = lineBrush,
                    StrokeThickness = 2
                };
                canvas.Children.Add(v);

                var hline = new Line()
                {
                    X1 = 0,
                    Y1 = i * cellH,
                    X2 = w,
                    Y2 = i * cellH,
                    Stroke = lineBrush,
                    StrokeThickness = 2
                };
                canvas.Children.Add(hline);
            }

            // draw X/O
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    double x = c * cellW;
                    double y = r * cellH;
                    int val = field[r, c];
                    if (val == 1)
                    {
                        // draw X
                        var l1 = new Line()
                        {
                            X1 = x + 10,
                            Y1 = y + 10,
                            X2 = x + cellW - 10,
                            Y2 = y + cellH - 10,
                            Stroke = Brushes.DarkBlue,
                            StrokeThickness = 4
                        };
                        var l2 = new Line()
                        {
                            X1 = x + cellW - 10,
                            Y1 = y + 10,
                            X2 = x + 10,
                            Y2 = y + cellH - 10,
                            Stroke = Brushes.DarkBlue,
                            StrokeThickness = 4
                        };
                        canvas.Children.Add(l1);
                        canvas.Children.Add(l2);
                    }
                    else if (val == 2)
                    {
                        // draw O
                        var ellipse = new Ellipse()
                        {
                            Width = Math.Max(0, cellW - 20),
                            Height = Math.Max(0, cellH - 20),
                            Stroke = Brushes.DarkRed,
                            StrokeThickness = 4
                        };
                        Canvas.SetLeft(ellipse, x + 10);
                        Canvas.SetTop(ellipse, y + 10);
                        canvas.Children.Add(ellipse);
                    }
                }
            }
        }
    }
}
