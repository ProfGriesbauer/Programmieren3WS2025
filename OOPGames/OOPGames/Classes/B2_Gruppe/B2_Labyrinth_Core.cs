using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    /******************************************************************************
     * B2 Labyrinth Game - Core Components
     * 
     * Labyrinth-Spiel mit eingeschränktem Sichtfeld (Fog of War)
     * - Spieler navigiert durch Labyrinth mit Pfeiltasten
     * - Nur ein kleiner Bereich um den Spieler ist sichtbar
     * - Ziel: Finde den Ausgang
     ******************************************************************************/

    #region Enums and Constants

    /// <summary>
    /// Zellentypen im Labyrinth
    /// </summary>
    public enum B2_MazeCellType
    {
        Wall = 0,      // Wand - nicht passierbar
        Path = 1,      // Weg - passierbar
        Player1 = 2,   // Spieler 1 Position
        Player2 = 3,   // Spieler 2 Position
        Goal = 4,      // Ziel
        Visited = 5    // Bereits besuchter Weg
    }

    /// <summary>
    /// Bewegungsrichtungen
    /// </summary>
    public enum B2_MazeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    #endregion

    #region Abstract Base Classes

    /// <summary>
    /// Abstrakte Basis für das Labyrinth-Spielfeld
    /// </summary>
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

        public virtual int Rows => rows;
        public virtual int Cols => cols;
        public virtual int Player1Row => player1Row;
        public virtual int Player1Col => player1Col;
        public virtual int Player2Row => player2Row;
        public virtual int Player2Col => player2Col;
        public virtual int GoalRow => goalRow;
        public virtual int GoalCol => goalCol;

        /// <summary>
        /// Zugriff auf Zellentyp
        /// </summary>
        public virtual B2_MazeCellType this[int r, int c]
        {
            get
            {
                if (r < 0 || r >= rows || c < 0 || c >= cols) return B2_MazeCellType.Wall;
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

        /// <summary>
        /// Prüft ob eine Position passierbar ist
        /// </summary>
        public virtual bool IsWalkable(int r, int c)
        {
            if (r < 0 || r >= rows || c < 0 || c >= cols) return false;
            var cell = grid[r, c];
            return cell == B2_MazeCellType.Path || 
                   cell == B2_MazeCellType.Goal || 
                   cell == B2_MazeCellType.Visited;
        }

        /// <summary>
        /// Prüft ob eine Zelle bereits besucht wurde
        /// </summary>
        public virtual bool IsVisited(int r, int c)
        {
            return visitedCells.Contains((r, c));
        }

        /// <summary>
        /// Markiert aktuelle Position als besucht
        /// </summary>
        public virtual void MarkVisited(int r, int c)
        {
            if (r >= 0 && r < rows && c >= 0 && c < cols)
            {
                visitedCells.Add((r, c));
                if (grid[r, c] == B2_MazeCellType.Path)
                {
                    grid[r, c] = B2_MazeCellType.Visited;
                }
            }
        }

        public virtual bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }

    /// <summary>
    /// Abstrakte Basis für Labyrinth-Bewegungen
    /// </summary>
    public abstract class B2_AbstractMazeMove : IPlayMove
    {
        public abstract int PlayerNumber { get; }
        public abstract B2_MazeDirection Direction { get; }
    }

    /// <summary>
    /// Abstrakte Basis für Labyrinth-Regeln
    /// </summary>
    public abstract class B2_AbstractMazeRules : IGameRules
    {
        protected B2_AbstractMazeField field;

        protected B2_AbstractMazeRules(B2_AbstractMazeField field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public virtual string Name => "Abstract Maze Rules";
        public virtual IGameField CurrentField => field;

        /// <summary>
        /// Prüft ob Spieler das Ziel erreicht hat
        /// </summary>
        public abstract int CheckIfPLayerWon();

        /// <summary>
        /// Bewegungen sind möglich solange Spieler nicht am Ziel ist
        /// </summary>
        public virtual bool MovesPossible => CheckIfPLayerWon() == -1;

        public abstract void ClearField();
        public abstract void DoMove(IPlayMove move);
    }

    #endregion

    #region Concrete Implementations

    /// <summary>
    /// Konkretes Labyrinth-Spielfeld mit vorgeneriertem Labyrinth
    /// </summary>
    public class B2_MazeField : B2_AbstractMazeField
    {
        public B2_MazeField(int rows = 21, int cols = 21)
        {
            this.rows = rows;
            this.cols = cols;
            this.grid = new B2_MazeCellType[rows, cols];
            this.visitedCells = new HashSet<(int, int)>();
            
            GenerateMaze();
        }

        /// <summary>
        /// Generiert ein zufälliges Labyrinth mit Recursive Backtracking
        /// </summary>
        private void GenerateMaze()
        {
            // Initialisiere alles als Wand
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = B2_MazeCellType.Wall;
                }
            }

            Random rand = new Random();
            
            // Starte in der Mitte (ungerade Koordinaten für bessere Labyrinth-Struktur)
            int startR = 1;
            int startC = 1;
            
            // Recursive Backtracking Algorithmus
            Stack<(int r, int c)> stack = new Stack<(int, int)>();
            stack.Push((startR, startC));
            grid[startR, startC] = B2_MazeCellType.Path;

            var directions = new[] { (-2, 0), (2, 0), (0, -2), (0, 2) }; // Up, Down, Left, Right (2 Schritte)

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = new List<(int r, int c, int wallR, int wallC)>();

                // Finde unbesuchte Nachbarn
                foreach (var (dr, dc) in directions)
                {
                    int newR = current.r + dr;
                    int newC = current.c + dc;
                    int wallR = current.r + dr / 2;
                    int wallC = current.c + dc / 2;

                    if (newR > 0 && newR < rows - 1 && newC > 0 && newC < cols - 1 &&
                        grid[newR, newC] == B2_MazeCellType.Wall)
                    {
                        neighbors.Add((newR, newC, wallR, wallC));
                    }
                }

                if (neighbors.Count > 0)
                {
                    // Wähle zufälligen Nachbarn
                    var next = neighbors[rand.Next(neighbors.Count)];
                    grid[next.r, next.c] = B2_MazeCellType.Path;
                    grid[next.wallR, next.wallC] = B2_MazeCellType.Path; // Entferne Wand dazwischen
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
            grid[player1Row, player1Col] = B2_MazeCellType.Player1;
            visitedCells.Add((player1Row, player1Col));

            // Setze Spieler 2 Startposition (unten rechts)
            player2Row = rows - 2;
            player2Col = cols - 2;
            grid[player2Row, player2Col] = B2_MazeCellType.Player2;
            visitedCells.Add((player2Row, player2Col));

            // Setze Ziel (Mitte des Labyrinths)
            goalRow = rows / 2;
            goalCol = cols / 2;
            // Stelle sicher dass Ziel begehbar ist
            grid[goalRow, goalCol] = B2_MazeCellType.Goal;
        }

        /// <summary>
        /// Prüft ob eine Position eine Kreuzung ist (mehr als 2 Ausgänge)
        /// </summary>
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

        /// <summary>
        /// Bewegt den Spieler in eine Richtung bis zur nächsten Kreuzung
        /// </summary>
        public bool MovePlayer(int playerNumber, B2_MazeDirection direction)
        {
            int currentRow = playerNumber == 1 ? player1Row : player2Row;
            int currentCol = playerNumber == 1 ? player1Col : player2Col;
            B2_MazeCellType playerType = playerNumber == 1 ? B2_MazeCellType.Player1 : B2_MazeCellType.Player2;

            int newRow = currentRow;
            int newCol = currentCol;

            // Bestimme Richtungsvektor
            int dr = 0, dc = 0;
            switch (direction)
            {
                case B2_MazeDirection.Up: dr = -1; break;
                case B2_MazeDirection.Down: dr = 1; break;
                case B2_MazeDirection.Left: dc = -1; break;
                case B2_MazeDirection.Right: dc = 1; break;
            }

            // Erste Bewegung prüfen
            int nextRow = currentRow + dr;
            int nextCol = currentCol + dc;
            if (!IsWalkable(nextRow, nextCol))
                return false;

            // Bewege bis zur nächsten Kreuzung oder bis Weg endet
            bool moved = false;
            while (true)
            {
                nextRow = newRow + dr;
                nextCol = newCol + dc;

                // Prüfe ob Bewegung möglich ist
                if (!IsWalkable(nextRow, nextCol))
                    break;

                // Bewege einen Schritt
                newRow = nextRow;
                newCol = nextCol;
                moved = true;
                MarkVisited(newRow, newCol);

                // Stoppe bei Kreuzung oder am Ziel
                if (IsIntersection(newRow, newCol) || 
                    (newRow == goalRow && newCol == goalCol))
                    break;
            }

            if (!moved)
                return false;

            // Alte Position zurücksetzen
            if (grid[currentRow, currentCol] == playerType)
            {
                grid[currentRow, currentCol] = B2_MazeCellType.Visited;
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

            // Update Grid (außer wenn es das Ziel ist)
            if (grid[newRow, newCol] != B2_MazeCellType.Goal)
            {
                grid[newRow, newCol] = playerType;
            }

            return true;
        }
    }

    /// <summary>
    /// Konkreter Labyrinth-Move (Richtungsbewegung)
    /// </summary>
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

    /// <summary>
    /// Konkrete Labyrinth-Regeln
    /// </summary>
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
                return 1;
            }

            // Spieler 2 hat gewonnen
            if (mazeField.Player2Row == mazeField.GoalRow && 
                mazeField.Player2Col == mazeField.GoalCol)
            {
                return 2;
            }

            return -1; // Noch kein Gewinner
        }

        public override void ClearField()
        {
            // Neues Labyrinth generieren
            field = new B2_MazeField(21, 21);
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
