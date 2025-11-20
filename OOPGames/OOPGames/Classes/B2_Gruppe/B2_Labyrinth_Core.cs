using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    /*
     * B2 Labyrinth Game - Core Components
     * Labyrinth-Spiel mit eingeschränktem Sichtfeld (Fog of War)
     * - Spieler navigiert durch Labyrinth mit Pfeiltasten
     * - Nur ein kleiner Bereich um den Spieler ist sichtbar
     * - Ziel: Finde den Ausgang
     */

    #region Cell Type Classes

    /// Zellentyp - jede Zelle bekommt ein eigenes Objekt
    public class B2_MazeCellType
    {
        public string Name { get; set; }  // Jetzt änderbar!
        public bool IsWalkable { get; private set; }

        /// Konstruktor - öffentlich, damit neue Instanzen erstellt werden können
        public B2_MazeCellType(string name, bool isWalkable)
        {
            Name = name;
            IsWalkable = isWalkable;
        }

        /// Factory-Methoden für die verschiedenen Zellentypen (erzeugen neue Instanzen!)
        public static B2_MazeCellType CreateWall() => new B2_MazeCellType("Wall", false);
        public static B2_MazeCellType CreatePath() => new B2_MazeCellType("Path", true);
        public static B2_MazeCellType CreatePlayer1() => new B2_MazeCellType("Player1", true);
        public static B2_MazeCellType CreatePlayer2() => new B2_MazeCellType("Player2", true);
        public static B2_MazeCellType CreateGoal() => new B2_MazeCellType("Goal", true);
        public static B2_MazeCellType CreateVisited() => new B2_MazeCellType("Visited", true);
        public static B2_MazeCellType CreateVisitedPlayer1() => new B2_MazeCellType("VisitedPlayer1", true);
        public static B2_MazeCellType CreateVisitedPlayer2() => new B2_MazeCellType("VisitedPlayer2", true);

        /// Methode zum Ändern des Zellentyps (für "verrücktes Labyrinth")
        public void ChangeType(string newName, bool newIsWalkable)
        {
            Name = newName;
            IsWalkable = newIsWalkable;
        }

        /// Convenience-Methoden zum Typ-Wechsel
        public void BecomeWall() => ChangeType("Wall", false);
        public void BecomePath() => ChangeType("Path", true);
        public void BecomeVisitedPlayer1() => ChangeType("VisitedPlayer1", true);
        public void BecomeVisitedPlayer2() => ChangeType("VisitedPlayer2", true);

        public override bool Equals(object obj)
        {
            return obj is B2_MazeCellType other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(B2_MazeCellType left, B2_MazeCellType right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(B2_MazeCellType left, B2_MazeCellType right)
        {
            return !(left == right);
        }

        /// Helper-Methode zum Prüfen des Typs
        public bool IsType(string typeName) => Name == typeName;
    }

    /// Bewegungsrichtung
    public class B2_MazeDirection
    {
        public string Name { get; }
        public int DeltaRow { get; }
        public int DeltaCol { get; }

        private B2_MazeDirection(string name, int deltaRow, int deltaCol)
        {
            Name = name;
            DeltaRow = deltaRow;
            DeltaCol = deltaCol;
        }

        // Statische Instanzen für die vier Richtungen
        public static readonly B2_MazeDirection Up = new B2_MazeDirection("Up", -1, 0);
        public static readonly B2_MazeDirection Down = new B2_MazeDirection("Down", 1, 0);
        public static readonly B2_MazeDirection Left = new B2_MazeDirection("Left", 0, -1);
        public static readonly B2_MazeDirection Right = new B2_MazeDirection("Right", 0, 1);

        public override bool Equals(object obj)
        {
            return obj is B2_MazeDirection other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(B2_MazeDirection left, B2_MazeDirection right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(B2_MazeDirection left, B2_MazeDirection right)
        {
            return !(left == right);
        }
    }

    #endregion

    #region Abstract Base Classes

    public abstract class B2_AbstractMazeField : IGameField
    {
        protected B2_MazeCellType[,] grid;
        protected int player1Row;
        protected int player1Col;
        protected int player2Row;
        protected int player2Col;
        protected int goalRow;
        protected int goalCol;
        protected int rows;
        protected int cols;
        protected HashSet<(int, int)> visitedCells;
        protected DateTime gameStartTime;
        protected const double CountdownSeconds = 3.0;

        public virtual int Rows => rows;
        public virtual int Cols => cols;
        public virtual int Player1Row => player1Row;
        public virtual int Player1Col => player1Col;
        public virtual int Player2Row => player2Row;
        public virtual int Player2Col => player2Col;
        public virtual int GoalRow => goalRow;
        public virtual int GoalCol => goalCol;
        public virtual DateTime GameStartTime => gameStartTime;
        
        /// Gibt zurück wie viele Sekunden vom Countdown noch übrig sind (0 wenn vorbei)
        public virtual double RemainingCountdown
        {
            get
            {
                double elapsed = (DateTime.Now - gameStartTime).TotalSeconds;
                double remaining = CountdownSeconds - elapsed;
                return remaining > 0 ? remaining : 0;
            }
        }
        
        /// Prüft ob der Countdown noch läuft
        public virtual bool IsCountdownActive => RemainingCountdown > 0;

        /// Zugriff auf Zellentyp
        public virtual B2_MazeCellType this[int r, int c]
        {
            get
            {
                if (r < 0 || r >= rows || c < 0 || c >= cols) return B2_MazeCellType.CreateWall();
                return grid[r, c];
            }
            protected set
            {
                if (r >= 0 && r < rows && c >= 0 && c < cols)
                {
                    grid[r, c] = value;
                }
            }
        }

        /// Prüft ob eine Position passierbar ist
        public virtual bool IsWalkable(int r, int c)
        {
            if (r < 0 || r >= rows || c < 0 || c >= cols) return false;
            var cell = grid[r, c];
            return cell.IsWalkable;
        }

        /// Prüft ob eine Zelle bereits besucht wurde
        public virtual bool IsVisited(int r, int c)
        {
            return visitedCells.Contains((r, c));
        }

        /// Markiert aktuelle Position als besucht
        public virtual void MarkVisited(int r, int c)
        {
            if (r >= 0 && r < rows && c >= 0 && c < cols)
            {
                visitedCells.Add((r, c));
                if (grid[r, c].IsType("Path"))
                {
                    grid[r, c] = B2_MazeCellType.CreateVisited();
                }
            }
        }

        public virtual bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }

    /// Abstrakte Basis für Labyrinth-Bewegungen
    public abstract class B2_AbstractMazeMove : IPlayMove
    {
        public abstract int PlayerNumber { get; }
        public abstract B2_MazeDirection Direction { get; }
    }

    /// Abstrakte Basis für Labyrinth-Regeln
    public abstract class B2_AbstractMazeRules : IGameRules
    {
        protected B2_AbstractMazeField field;
        protected bool gameEnded = false;

        protected B2_AbstractMazeRules(B2_AbstractMazeField field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public virtual string Name => "Abstract Maze Rules";
        public virtual IGameField CurrentField => field;
        public virtual bool GameEnded => gameEnded;

        /// Prüft ob Spieler das Ziel erreicht hat
        public abstract int CheckIfPLayerWon();

        /// Bewegungen sind möglich solange Countdown vorbei ist und Spieler nicht am Ziel ist
        public virtual bool MovesPossible
        {
            get
            {
                // Countdown muss vorbei sein UND Spiel nicht beendet
                return !field.IsCountdownActive && CheckIfPLayerWon() == -1;
            }
        }

        public abstract void ClearField();
        public abstract void DoMove(IPlayMove move);
    }

    #endregion

    #region Concrete Implementations

    /// Konkretes Labyrinth-Spielfeld mit vorgeneriertem Labyrinth
    public class B2_MazeField : B2_AbstractMazeField
    {
        public B2_MazeField(int rows = 31, int cols = 31)
        {
            this.rows = rows;
            this.cols = cols;
            this.grid = new B2_MazeCellType[rows, cols];
            this.visitedCells = new HashSet<(int, int)>();
            this.gameStartTime = DateTime.Now;
            
            GenerateMaze();
        }

        /// Generiert ein zufälliges Labyrinth mit Recursive Backtracking
        private void GenerateMaze()
        {
            // Initialisiere alles als Wand
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = B2_MazeCellType.CreateWall();
                }
            }

            Random rand = new Random();
            
            // Starte in der Mitte (ungerade Koordinaten für bessere Labyrinth-Struktur)
            int startR = 1;
            int startC = 1;
            
            // Recursive Backtracking Algorithmus
            Stack<(int r, int c)> stack = new Stack<(int, int)>();
            stack.Push((startR, startC));
            grid[startR, startC] = B2_MazeCellType.CreatePath();

            var directions = new[] { 
                (B2_MazeDirection.Up, -2, 0), 
                (B2_MazeDirection.Down, 2, 0), 
                (B2_MazeDirection.Left, 0, -2), 
                (B2_MazeDirection.Right, 0, 2) 
            };

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = new List<(int r, int c, int wallR, int wallC)>();

                // Finde unbesuchte Nachbarn
                foreach (var (dir, dr, dc) in directions)
                {
                    int newR = current.r + dr;
                    int newC = current.c + dc;
                    int wallR = current.r + dr / 2;
                    int wallC = current.c + dc / 2;

                    if (newR > 0 && newR < rows - 1 && newC > 0 && newC < cols - 1 &&
                        grid[newR, newC].IsType("Wall"))
                    {
                        neighbors.Add((newR, newC, wallR, wallC));
                    }
                }

                if (neighbors.Count > 0)
                {
                    // Wähle zufälligen Nachbarn
                    var next = neighbors[rand.Next(neighbors.Count)];
                    grid[next.r, next.c] = B2_MazeCellType.CreatePath();
                    grid[next.wallR, next.wallC] = B2_MazeCellType.CreatePath(); // Entferne Wand dazwischen
                    stack.Push((next.r, next.c));
                }
                else
                {
                    stack.Pop(); // Backtrack
                }
            }

            // Setze Spieler 1 Startposition (oben links)
            player1Row = 1;
            player1Col = 1;
            grid[player1Row, player1Col] = B2_MazeCellType.CreatePlayer1();
            visitedCells.Add((player1Row, player1Col));

            // Setze Spieler 2 Startposition (unten rechts)
            player2Row = rows - 2;
            player2Col = cols - 2;
            grid[player2Row, player2Col] = B2_MazeCellType.CreatePlayer2();
            visitedCells.Add((player2Row, player2Col));

            // Setze Ziel (Mitte des Labyrinths)
            goalRow = rows / 2;
            goalCol = cols / 2;
            // Stelle sicher dass Ziel begehbar ist
            grid[goalRow, goalCol] = B2_MazeCellType.CreateGoal();
        }

        /// Prüft ob eine Position eine Kreuzung ist (mehr als 2 Ausgänge)
        private bool IsIntersection(int row, int col)
        {
            if (!IsWalkable(row, col)) return false;
            
            int walkableNeighbors = 0;
            if (IsWalkable(row - 1, col)) walkableNeighbors++;
            if (IsWalkable(row + 1, col)) walkableNeighbors++;
            if (IsWalkable(row, col - 1)) walkableNeighbors++;
            if (IsWalkable(row, col + 1)) walkableNeighbors++;
            
            return walkableNeighbors > 2;
        }

        /// Bewegt den Spieler in eine Richtung um ein einzelnes Kästchen
        public bool MovePlayer(int playerNumber, B2_MazeDirection direction)
        {
            int currentRow = playerNumber == 1 ? player1Row : player2Row;
            int currentCol = playerNumber == 1 ? player1Col : player2Col;
            string playerTypeName = playerNumber == 1 ? "Player1" : "Player2";

            int newRow = currentRow + direction.DeltaRow;
            int newCol = currentCol + direction.DeltaCol;

            // Prüfe ob Bewegung möglich ist
            if (!IsWalkable(newRow, newCol))
                return false;

            // Alte Position zurücksetzen (mit spielerspezifischer Spur)
            if (grid[currentRow, currentCol].IsType(playerTypeName))
            {
                grid[currentRow, currentCol] = playerNumber == 1 ? 
                    B2_MazeCellType.CreateVisitedPlayer1() : 
                    B2_MazeCellType.CreateVisitedPlayer2();
            }

            // Neue Position setzen
            if (playerNumber == 1)
            {
                player1Row = newRow;
                player1Col = newCol;
            }
            else
            {
                player2Row = newRow;
                player2Col = newCol;
            }

            // Markiere als besucht
            MarkVisited(newRow, newCol);

            // Update Grid (außer wenn es das Ziel ist)
            if (!grid[newRow, newCol].IsType("Goal"))
            {
                grid[newRow, newCol] = playerNumber == 1 ? 
                    B2_MazeCellType.CreatePlayer1() : 
                    B2_MazeCellType.CreatePlayer2();
            }

            return true;
        }
    }

    /// Konkreter Labyrinth-Move (Richtungsbewegung)
    public class B2_MazeMove : B2_AbstractMazeMove
    {
        public override int PlayerNumber { get; }
        public override B2_MazeDirection Direction { get; }

        public B2_MazeMove(int playerNumber, B2_MazeDirection direction)
        {
            PlayerNumber = playerNumber;
            Direction = direction;
        }
    }

    /// Konkrete Labyrinth-Regeln
    public class B2_MazeRules : B2_AbstractMazeRules
    {
        public B2_MazeRules() : base(new B2_MazeField(21, 21))
        {
        }

        public override string Name => "B2 - Maze Game Rules";

        public override int CheckIfPLayerWon()
        {
            var mazeField = field as B2_MazeField;
            if (mazeField == null) return -1;

            // Spieler 1 hat gewonnen
            if (mazeField.Player1Row == mazeField.GoalRow && 
                mazeField.Player1Col == mazeField.GoalCol)
            {
                gameEnded = true;
                return 1;
            }

            // Spieler 2 hat gewonnen
            if (mazeField.Player2Row == mazeField.GoalRow && 
                mazeField.Player2Col == mazeField.GoalCol)
            {
                gameEnded = true;
                return 2;
            }

            return -1; // Noch kein Gewinner
        }

        public override void ClearField()
        {
            // Neues Labyrinth generieren
            field = new B2_MazeField(21, 21);
            gameEnded = false;
        }

        public override void DoMove(IPlayMove move)
        {
            if (move == null) return;
            if (!(move is B2_AbstractMazeMove mazeMove)) return;
            if (!(field is B2_MazeField mazeField)) return;

            // Führe Bewegung aus (mit Spielernummer)
            mazeField.MovePlayer(mazeMove.PlayerNumber, mazeMove.Direction);
        }
    }

    #endregion
}
