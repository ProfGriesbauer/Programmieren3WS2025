using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    /**************************************************************************
     * IQ PUZZLER PRO 2D - Vollständige Implementierung
     **************************************************************************/

    #region Puzzle Pieces (Polyominos)

    /// <summary>
    /// Basisklasse für ein IQ Puzzle Stück
    /// </summary>
    public class A3_LEA_IQPuzzlePiece : IA3_LEA_IQPuzzlePiece
    {
        private int _id;
        private string _name;
        private Color _color;
        private int[,] _shape;

        public A3_LEA_IQPuzzlePiece(int id, string name, Color color, int[,] shape)
        {
            _id = id;
            _name = name;
            _color = color;
            _shape = shape;
        }

        public int Id => _id;
        public string Name => _name;
        public Color Color => _color;
        public int[,] Shape => _shape;
        public int Width => _shape.GetLength(1);
        public int Height => _shape.GetLength(0);

        public IA3_LEA_IQPuzzlePiece Rotate()
        {
            int h = Height;
            int w = Width;
            int[,] rotated = new int[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    rotated[x, h - 1 - y] = _shape[y, x];
                }
            }

            return new A3_LEA_IQPuzzlePiece(_id, _name, _color, rotated);
        }

        public IA3_LEA_IQPuzzlePiece Flip()
        {
            int h = Height;
            int w = Width;
            int[,] flipped = new int[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    flipped[y, w - 1 - x] = _shape[y, x];
                }
            }

            return new A3_LEA_IQPuzzlePiece(_id, _name, _color, flipped);
        }

        public IA3_LEA_IQPuzzlePiece Clone()
        {
            int[,] clonedShape = (int[,])_shape.Clone();
            return new A3_LEA_IQPuzzlePiece(_id, _name, _color, clonedShape);
        }
    }

    /// <summary>
    /// Factory für alle 12 IQ Puzzler Pro Stücke
    /// </summary>
    public static class A3_LEA_IQPuzzlePieceFactory
    {
        public static List<IA3_LEA_IQPuzzlePiece> CreateAllPieces()
        {
            var pieces = new List<IA3_LEA_IQPuzzlePiece>();

            // Teil 1: Lila - X X X
            //              X
            pieces.Add(new A3_LEA_IQPuzzlePiece(1, "Lila", Color.FromRgb(148, 0, 211), new int[,]
            {
                { 1, 1, 1 },
                { 1, 0, 0 }
            }));

            // Teil 2: Hellgrün - X X X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(2, "Hellgrün", Color.FromRgb(144, 238, 144), new int[,]
            {
                { 1, 1, 1, 1 }
            }));

            // Teil 3: Hautfarben - X X X
            //                      X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(3, "Hautfarben", Color.FromRgb(255, 218, 185), new int[,]
            {
                { 1, 1, 1 },
                { 1, 1, 0 }
            }));

            // Teil 4: Rot - X X
            //               X
            //               X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(4, "Rot", Color.FromRgb(220, 20, 60), new int[,]
            {
                { 1, 1 },
                { 1, 0 },
                { 1, 1 }
            }));

            // Teil 5: Weiß - X
            //                X X
            //                  X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(5, "Weiß", Color.FromRgb(240, 240, 240), new int[,]
            {
                { 1, 0, 0 },
                { 1, 1, 0 },
                { 0, 1, 1 }
            }));

            // Teil 6: Orange -   X
            //                  X X X
            //                    X
            pieces.Add(new A3_LEA_IQPuzzlePiece(6, "Orange", Color.FromRgb(255, 140, 0), new int[,]
            {
                { 0, 1, 0 },
                { 1, 1, 1 },
                { 0, 1, 0 }
            }));

            // Teil 7: Dunkelblau - X X X
            //                          X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(7, "Dunkelblau", Color.FromRgb(0, 0, 139), new int[,]
            {
                { 1, 1, 1, 0 },
                { 0, 0, 1, 1 }
            }));

            // Teil 8: Hellblau - X X X X
            //                      X
            pieces.Add(new A3_LEA_IQPuzzlePiece(8, "Hellblau", Color.FromRgb(135, 206, 250), new int[,]
            {
                { 1, 1, 1, 1 },
                { 0, 1, 0, 0 }
            }));

            // Teil 9: Dunkelgrün - X X
            //                      X
            pieces.Add(new A3_LEA_IQPuzzlePiece(9, "Dunkelgrün", Color.FromRgb(0, 100, 0), new int[,]
            {
                { 1, 1 },
                { 1, 0 }
            }));

            // Teil 10: Grau - X X X X
            //                 X
            pieces.Add(new A3_LEA_IQPuzzlePiece(10, "Grau", Color.FromRgb(169, 169, 169), new int[,]
            {
                { 1, 1, 1, 1 },
                { 1, 0, 0, 0 }
            }));

            // Teil 11: Gelb - X X X
            //                 X
            //                 X
            pieces.Add(new A3_LEA_IQPuzzlePiece(11, "Gelb", Color.FromRgb(255, 215, 0), new int[,]
            {
                { 1, 1, 1 },
                { 1, 0, 0 },
                { 1, 0, 0 }
            }));

            // Teil 12: Pink - X X
            //                 X X
            pieces.Add(new A3_LEA_IQPuzzlePiece(12, "Pink", Color.FromRgb(255, 192, 203), new int[,]
            {
                { 1, 1 },
                { 1, 1 }
            }));

            return pieces;
        }
    }

    #endregion

    #region Game Field

    /// <summary>
    /// 5x11 Spielfeld für IQ Puzzler Pro
    /// </summary>
    public class A3_LEA_IQPuzzleField : A3_LEA_BaseIQPuzzleField
    {
        private int[,] _grid;
        private const int FIELD_WIDTH = 11;
        private const int FIELD_HEIGHT = 5;

        public A3_LEA_IQPuzzleField()
        {
            _grid = new int[FIELD_HEIGHT, FIELD_WIDTH];
        }

        public override int Width => FIELD_WIDTH;
        public override int Height => FIELD_HEIGHT;

        public override int this[int x, int y]
        {
            get
            {
                if (IsValidPosition(x, y))
                    return _grid[y, x];
                return -1;
            }
            set
            {
                if (IsValidPosition(x, y))
                    _grid[y, x] = value;
            }
        }

        public override bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < FIELD_WIDTH && y >= 0 && y < FIELD_HEIGHT;
        }

        public override bool IsFull()
        {
            for (int y = 0; y < FIELD_HEIGHT; y++)
            {
                for (int x = 0; x < FIELD_WIDTH; x++)
                {
                    if (_grid[y, x] == 0)
                        return false;
                }
            }
            return true;
        }

        public override List<(int x, int y, int pieceId)> GetOccupiedCells()
        {
            var occupied = new List<(int x, int y, int pieceId)>();
            for (int y = 0; y < FIELD_HEIGHT; y++)
            {
                for (int x = 0; x < FIELD_WIDTH; x++)
                {
                    if (_grid[y, x] > 0)
                        occupied.Add((x, y, _grid[y, x]));
                }
            }
            return occupied;
        }
    }

    #endregion

    #region Move

    /// <summary>
    /// Ein Zug im IQ Puzzle
    /// </summary>
    public class A3_LEA_IQPuzzleMove : IA3_LEA_IQPuzzleMove
    {
        private IA3_LEA_IQPuzzlePiece _piece;
        private int _x;
        private int _y;
        private int _playerNumber;
        private bool _isRemove;

        public A3_LEA_IQPuzzleMove(IA3_LEA_IQPuzzlePiece piece, int x, int y, int playerNumber, bool isRemove = false)
        {
            _piece = piece;
            _x = x;
            _y = y;
            _playerNumber = playerNumber;
            _isRemove = isRemove;
        }

        public IA3_LEA_IQPuzzlePiece Piece => _piece;
        public int X => _x;
        public int Y => _y;
        public int PlayerNumber => _playerNumber;
        public bool IsRemove => _isRemove;
    }

    #endregion

    #region Rules

    /// <summary>
    /// Spielregeln für IQ Puzzler Pro mit Backtracking-Solver
    /// </summary>
    public class A3_LEA_IQPuzzleRules : A3_LEA_BaseIQPuzzleRules
    {
        private A3_LEA_IQPuzzleField _field;
        private List<IA3_LEA_IQPuzzlePiece> _allPieces;
        private List<IA3_LEA_IQPuzzlePiece> _placedPieces;
        private Dictionary<int, (int x, int y)> _piecePlacements;
        private int _currentDifficulty;

        // Öffentliches Property für ausgewähltes Teil (für Painter-Zugriff)
        public IA3_LEA_IQPuzzlePiece SelectedPieceForPainting { get; set; }
        
        // Preview-Position (wird beim Linksklick gesetzt, bei Enter platziert)
        public int PreviewX { get; set; } = -1;
        public int PreviewY { get; set; } = -1;

        // Live Mausposition (wird bei OnMouseMoved() aktualisiert)
        public int MouseX { get; set; } = -1;
        public int MouseY { get; set; } = -1;

        public A3_LEA_IQPuzzleRules()
        {
            _field = new A3_LEA_IQPuzzleField();
            _allPieces = A3_LEA_IQPuzzlePieceFactory.CreateAllPieces();
            _placedPieces = new List<IA3_LEA_IQPuzzlePiece>();
            _piecePlacements = new Dictionary<int, (int x, int y)>();
            _currentDifficulty = 1;
            SelectedPieceForPainting = null;
            // Kein automatisches Laden eines Levels beim Start
            // LoadChallenge(1);
        }

        public override IA3_LEA_IQPuzzleField IQPuzzleField => _field;
        public override List<IA3_LEA_IQPuzzlePiece> AllPieces => _allPieces;
        public override List<IA3_LEA_IQPuzzlePiece> PlacedPieces => _placedPieces;
        public override int DifficultyLevel => _currentDifficulty;

        public override List<IA3_LEA_IQPuzzlePiece> AvailablePieces
        {
            get
            {
                var placedIds = _placedPieces.Select(p => p.Id).ToHashSet();
                return _allPieces.Where(p => !placedIds.Contains(p.Id)).ToList();
            }
        }

        public override bool MovesPossible => AvailablePieces.Count > 0 && !_field.IsFull();
        public override string Name => "A3 LEA IQ Puzzler Pro";

        public override void ClearField()
        {
            _field = new A3_LEA_IQPuzzleField();
            _placedPieces.Clear();
            _piecePlacements.Clear();
        }

        public override bool CanPlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y)
        {
            if (piece == null) return false;

            // Prüfe ob Stück bereits platziert ist
            if (_placedPieces.Any(p => p.Id == piece.Id))
                return false;

            // Prüfe jede Zelle des Stücks
            for (int py = 0; py < piece.Height; py++)
            {
                for (int px = 0; px < piece.Width; px++)
                {
                    if (piece.Shape[py, px] == 1)
                    {
                        int fieldX = x + px;
                        int fieldY = y + py;

                        // Außerhalb des Feldes?
                        if (!_field.IsValidPosition(fieldX, fieldY))
                            return false;

                        // Bereits belegt?
                        if (_field[fieldX, fieldY] != 0)
                            return false;
                    }
                }
            }

            return true;
        }

        public override void PlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y)
        {
            if (!CanPlacePiece(piece, x, y))
                return;

            // Platziere das Stück
            for (int py = 0; py < piece.Height; py++)
            {
                for (int px = 0; px < piece.Width; px++)
                {
                    if (piece.Shape[py, px] == 1)
                    {
                        _field[x + px, y + py] = piece.Id;
                    }
                }
            }

            _placedPieces.Add(piece);
            _piecePlacements[piece.Id] = (x, y);
        }

        public override void RemovePiece(IA3_LEA_IQPuzzlePiece piece)
        {
            if (piece == null || !_placedPieces.Any(p => p.Id == piece.Id))
                return;

            // Entferne das Stück vom Feld
            for (int y = 0; y < _field.Height; y++)
            {
                for (int x = 0; x < _field.Width; x++)
                {
                    if (_field[x, y] == piece.Id)
                        _field[x, y] = 0;
                }
            }

            _placedPieces.RemoveAll(p => p.Id == piece.Id);
            _piecePlacements.Remove(piece.Id);
        }

        public override void DoIQPuzzleMove(IA3_LEA_IQPuzzleMove move)
        {
            if (move == null) return;

            if (move.IsRemove)
            {
                RemovePiece(move.Piece);
            }
            else
            {
                PlacePiece(move.Piece, move.X, move.Y);
            }
        }

        public override int CheckIfPLayerWon()
        {
            if (_field.IsFull())
                return 1; // Spieler hat gewonnen
            
            if (!MovesPossible && !_field.IsFull())
                return -1; // Unentschieden/Nicht lösbar
            
            return 0; // Spiel läuft noch
        }

        public override IA3_LEA_IQPuzzleMove GetHint()
        {
            // Einfacher Hint: Finde einen gültigen Zug durch Brute-Force
            foreach (var piece in AvailablePieces)
            {
                // Probiere alle Rotationen
                var currentPiece = piece;
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    // Probiere alle Positionen
                    for (int y = 0; y < _field.Height; y++)
                    {
                        for (int x = 0; x < _field.Width; x++)
                        {
                            if (CanPlacePiece(currentPiece, x, y))
                            {
                                return new A3_LEA_IQPuzzleMove(currentPiece, x, y, 1);
                            }
                        }
                    }
                    currentPiece = currentPiece.Rotate();
                }

                // Probiere auch gespiegelt
                currentPiece = piece.Flip();
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    for (int y = 0; y < _field.Height; y++)
                    {
                        for (int x = 0; x < _field.Width; x++)
                        {
                            if (CanPlacePiece(currentPiece, x, y))
                            {
                                return new A3_LEA_IQPuzzleMove(currentPiece, x, y, 1);
                            }
                        }
                    }
                    currentPiece = currentPiece.Rotate();
                }
            }

            return null;
        }

        public override void LoadChallenge(int challengeNumber)
        {
            ClearField();
            _currentDifficulty = Math.Min(Math.Max(challengeNumber / 24 + 1, 1), 5);

            // Lade Level aus der Level-Datei
            var level = A3_LEA_IQPuzzleLevels.GetLevel(challengeNumber);
            if (level != null && level.GridLayout != null)
            {
                // Lade das vordefinierte Level-Layout
                LoadLevelFromGrid(level.GridLayout);
            }
            // Kein Fallback - Grid bleibt leer wenn kein Level definiert ist
        }

        /// <summary>
        /// Lädt ein Level aus einem Grid-Layout
        /// </summary>
        private void LoadLevelFromGrid(int[,] gridLayout)
        {
            // Grid ist [11, 5] = [width, height]
            for (int x = 0; x < 11; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    int pieceId = gridLayout[x, y];
                    if (pieceId > 0)
                    {
                        _field[x, y] = pieceId;
                    }
                }
            }

            // Ermittle welche Teile bereits platziert wurden
            var placedPieceIds = new HashSet<int>();
            for (int x = 0; x < 11; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    int pieceId = gridLayout[x, y];
                    if (pieceId > 0 && !placedPieceIds.Contains(pieceId))
                    {
                        placedPieceIds.Add(pieceId);
                        
                        // Finde das entsprechende Piece-Objekt
                        var piece = _allPieces.FirstOrDefault(p => p.Id == pieceId);
                        if (piece != null && !_placedPieces.Contains(piece))
                        {
                            _placedPieces.Add(piece);
                            _piecePlacements[piece.Id] = (x, y);
                        }
                    }
                }
            }
        }

        public override bool SolvePuzzle()
        {
            return SolveRecursive();
        }

        private bool SolveRecursive()
        {
            if (_field.IsFull())
                return true;

            var available = AvailablePieces;
            if (available.Count == 0)
                return false;

            foreach (var piece in available)
            {
                // Probiere alle Rotationen
                var currentPiece = piece;
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    for (int y = 0; y < _field.Height; y++)
                    {
                        for (int x = 0; x < _field.Width; x++)
                        {
                            if (CanPlacePiece(currentPiece, x, y))
                            {
                                PlacePiece(currentPiece, x, y);

                                if (SolveRecursive())
                                    return true;

                                RemovePiece(currentPiece);
                            }
                        }
                    }
                    currentPiece = currentPiece.Rotate();
                }

                // Probiere gespiegelt
                currentPiece = piece.Flip();
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    for (int y = 0; y < _field.Height; y++)
                    {
                        for (int x = 0; x < _field.Width; x++)
                        {
                            if (CanPlacePiece(currentPiece, x, y))
                            {
                                PlacePiece(currentPiece, x, y);

                                if (SolveRecursive())
                                    return true;

                                RemovePiece(currentPiece);
                            }
                        }
                    }
                    currentPiece = currentPiece.Rotate();
                }
            }

            return false;
        }
    }

    #endregion

    #region Painter

    /// <summary>
    /// Zeichnet das IQ Puzzle Spielfeld
    /// </summary>
    public class A3_LEA_IQPuzzlePaint : A3_LEA_BaseIQPuzzlePaint
    {
        private const double CELL_SIZE = 40;
        private const double OFFSET_X = 20;
        private const double OFFSET_Y = 20;
        private const double PIECE_PREVIEW_Y = 250;
        
        private double _winAnimationScale = 0;
        private bool _isWinAnimationActive = false;
        private bool _winAnimationComplete = false;

        public override string Name => "A3 LEA IQ Puzzler Pro Paint";

        public override void PaintIQPuzzleField(Canvas canvas, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces, IA3_LEA_IQPuzzlePiece selectedPiece)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            // Zeichne Level-Buttons ZUERST (vor allem anderen, damit sie klickbar sind)
            DrawLevelButtons(canvas);

            // Zeichne Spielfeld-Gitter
            DrawGrid(canvas, field);

            // Zeichne platzierte Stücke
            DrawPlacedPieces(canvas, field);

            // Debug: Zeichne roten Punkt an aktueller Mausposition (nur im Spielfeld!)
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
            if (rules != null && rules.MouseX >= 0 && rules.MouseY >= 0 && 
                rules.MouseX < field.Width && rules.MouseY < field.Height)
            {
                DrawMousePositionIndicator(canvas, rules.MouseX, rules.MouseY);
            }

            // Zeichne Live-Vorschau an Mausposition (wenn Teil ausgewählt ist und im Spielfeld)
            var effectiveSelectedPiece = selectedPiece ?? rules?.SelectedPieceForPainting;
            if (rules != null && effectiveSelectedPiece != null && rules.MouseX >= 0 && rules.MouseY >= 0 &&
                rules.MouseX < field.Width && rules.MouseY < field.Height)
            {
                DrawPlacementPreview(canvas, field, effectiveSelectedPiece, rules.MouseX, rules.MouseY);
            }

            // Zeichne verfügbare Stücke unten (mit aktuellem Rotations-Status!)
            DrawAvailablePieces(canvas, availablePieces, effectiveSelectedPiece);

            // Zeichne Anleitung
            DrawInstructions(canvas);
            
            // Prüfe ob Puzzle gelöst ist und zeige Win-Animation
            if (field.IsFull())
            {
                if (!_isWinAnimationActive)
                {
                    _isWinAnimationActive = true;
                    _winAnimationScale = 0;
                    _winAnimationComplete = false;
                }
                
                // Animiere Scale nur einmal von 0 bis 1.0 (ohne Überschwingen)
                if (!_winAnimationComplete)
                {
                    _winAnimationScale += 0.08;
                    if (_winAnimationScale >= 1.0)
                    {
                        _winAnimationScale = 1.0;
                        _winAnimationComplete = true;
                    }
                }
                
                DrawWinAnimation(canvas);
            }
            else
            {
                _isWinAnimationActive = false;
                _winAnimationComplete = false;
                _winAnimationScale = 0;
            }
        }
        
        private void DrawWinAnimation(Canvas canvas)
        {
            double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 640;
            double canvasHeight = canvas.ActualHeight > 0 ? canvas.ActualHeight : 480;
            
            double centerX = canvasWidth / 2;
            double centerY = canvasHeight / 2;
            
            // Halbtransparenter Hintergrund
            var overlay = new Rectangle
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Fill = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
            };
            canvas.Children.Add(overlay);
            
            // "WIN!" Text mit Animation - GRÖSSERE SCHRIFT
            var winText = new TextBlock
            {
                Text = "WIN!",
                FontSize = 150 * _winAnimationScale,  // Erhöht von 80 auf 150
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)), // Gold
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 15,
                    ShadowDepth = 8,
                    Opacity = 0.9
                }
            };
            
            // Zentriere den Text
            winText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = winText.DesiredSize.Width;
            double textHeight = winText.DesiredSize.Height;
            
            Canvas.SetLeft(winText, centerX - textWidth / 2);
            Canvas.SetTop(winText, centerY - textHeight / 2 - 40);
            canvas.Children.Add(winText);
            
            // "🎉 Puzzle Gelöst 🎉" Text darunter - auf Deutsch mit Party-Emojis
            var subText = new TextBlock
            {
                Text = "🎉 Puzzle Gelöst 🎉",
                FontSize = 32,  // Auch etwas größer gemacht
                FontWeight = FontWeights.Normal,
                Foreground = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.8
                }
            };
            
            subText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double subTextWidth = subText.DesiredSize.Width;
            
            Canvas.SetLeft(subText, centerX - subTextWidth / 2);
            Canvas.SetTop(subText, centerY + 50);
            canvas.Children.Add(subText);
        }

        private void DrawGrid(Canvas canvas, IA3_LEA_IQPuzzleField field)
        {
            var gridBrush = new SolidColorBrush(Colors.Gray);

            // Vertikale Linien
            for (int x = 0; x <= field.Width; x++)
            {
                var line = new Line
                {
                    X1 = OFFSET_X + x * CELL_SIZE,
                    Y1 = OFFSET_Y,
                    X2 = OFFSET_X + x * CELL_SIZE,
                    Y2 = OFFSET_Y + field.Height * CELL_SIZE,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);
            }

            // Horizontale Linien
            for (int y = 0; y <= field.Height; y++)
            {
                var line = new Line
                {
                    X1 = OFFSET_X,
                    Y1 = OFFSET_Y + y * CELL_SIZE,
                    X2 = OFFSET_X + field.Width * CELL_SIZE,
                    Y2 = OFFSET_Y + y * CELL_SIZE,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);
            }
        }

        private void DrawPlacedPieces(Canvas canvas, IA3_LEA_IQPuzzleField field)
        {
            var occupied = field.GetOccupiedCells();
            var groupedByPiece = occupied.GroupBy(cell => cell.pieceId);

            foreach (var pieceGroup in groupedByPiece)
            {
                int pieceId = pieceGroup.Key;
                Color color = GetColorForPieceId(pieceId);
                var brush = new SolidColorBrush(color);

                foreach (var cell in pieceGroup)
                {
                    var rect = new Rectangle
                    {
                        Width = CELL_SIZE - 2,
                        Height = CELL_SIZE - 2,
                        Fill = brush,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(rect, OFFSET_X + cell.x * CELL_SIZE + 1);
                    Canvas.SetTop(rect, OFFSET_Y + cell.y * CELL_SIZE + 1);
                    canvas.Children.Add(rect);
                }
            }
        }

        private void DrawMousePositionIndicator(Canvas canvas, int gridX, int gridY)
        {
            // Zeichne einen roten Punkt in der Mitte der Zelle
            double pixelX = OFFSET_X + gridX * CELL_SIZE + CELL_SIZE / 2;
            double pixelY = OFFSET_Y + gridY * CELL_SIZE + CELL_SIZE / 2;

            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1
            };
            Canvas.SetLeft(dot, pixelX - 4);
            Canvas.SetTop(dot, pixelY - 4);
            canvas.Children.Add(dot);
        }

        private void DrawAvailablePieces(Canvas canvas, List<IA3_LEA_IQPuzzlePiece> availablePieces, IA3_LEA_IQPuzzlePiece selectedPiece)
        {
            double x = OFFSET_X;
            double y = PIECE_PREVIEW_Y;

            var label = new TextBlock
            {
                Text = "Available Pieces (Click to select, R to rotate, F to flip):",
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y - 20);
            canvas.Children.Add(label);

            foreach (var piece in availablePieces)
            {
                bool isSelected = selectedPiece != null && selectedPiece.Id == piece.Id;
                
                // WICHTIG: Wenn das Teil ausgewählt ist, zeige die AKTUELLE Rotation (selectedPiece),
                // sonst zeige das Original-Teil
                var pieceToShow = isSelected ? selectedPiece : piece;
                
                DrawPiecePreview(canvas, pieceToShow, x, y, isSelected);
                x += 60;

                if (x > 400)
                {
                    x = OFFSET_X;
                    y += 80;
                }
            }
        }

        private void DrawPiecePreview(Canvas canvas, IA3_LEA_IQPuzzlePiece piece, double x, double y, bool isSelected)
        {
            const double previewCellSize = 12;

            // Rahmen wenn ausgewählt
            if (isSelected)
            {
                var border = new Rectangle
                {
                    Width = piece.Width * previewCellSize + 6,
                    Height = piece.Height * previewCellSize + 6,
                    Stroke = Brushes.Gold,
                    StrokeThickness = 3
                };
                Canvas.SetLeft(border, x - 3);
                Canvas.SetTop(border, y - 3);
                canvas.Children.Add(border);
            }

            // Zeichne Stück
            var brush = new SolidColorBrush(piece.Color);
            for (int py = 0; py < piece.Height; py++)
            {
                for (int px = 0; px < piece.Width; px++)
                {
                    if (piece.Shape[py, px] == 1)
                    {
                        var rect = new Rectangle
                        {
                            Width = previewCellSize - 1,
                            Height = previewCellSize - 1,
                            Fill = brush,
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.5
                        };
                        Canvas.SetLeft(rect, x + px * previewCellSize);
                        Canvas.SetTop(rect, y + py * previewCellSize);
                        canvas.Children.Add(rect);
                    }
                }
            }
        }

        private void DrawPlacementPreview(Canvas canvas, IA3_LEA_IQPuzzleField field, IA3_LEA_IQPuzzlePiece selectedPiece, int gridX, int gridY)
        {
            // Prüfe ob Platzierung gültig wäre
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
            bool canPlace = rules != null && rules.CanPlacePiece(selectedPiece, gridX, gridY);

            // Zeichne halbtransparente Vorschau in Grün (gültig) oder Rot (ungültig)
            var previewColor = canPlace ? Color.FromArgb(100, 0, 255, 0) : Color.FromArgb(100, 255, 0, 0);
            var brush = new SolidColorBrush(previewColor);

            for (int py = 0; py < selectedPiece.Height; py++)
            {
                for (int px = 0; px < selectedPiece.Width; px++)
                {
                    if (selectedPiece.Shape[py, px] == 1)
                    {
                        int fieldX = gridX + px;
                        int fieldY = gridY + py;

                        if (field.IsValidPosition(fieldX, fieldY))
                        {
                            var rect = new Rectangle
                            {
                                Width = CELL_SIZE - 2,
                                Height = CELL_SIZE - 2,
                                Fill = brush,
                                Stroke = canPlace ? Brushes.LimeGreen : Brushes.Red,
                                StrokeThickness = 3,
                                StrokeDashArray = new DoubleCollection { 4, 2 }
                            };
                            Canvas.SetLeft(rect, OFFSET_X + fieldX * CELL_SIZE + 1);
                            Canvas.SetTop(rect, OFFSET_Y + fieldY * CELL_SIZE + 1);
                            canvas.Children.Add(rect);
                        }
                    }
                }
            }
        }

        private void DrawInstructions(Canvas canvas)
        {
            var text = new TextBlock
            {
                Text = "LEFT CLICK: Select piece | LEFT CLICK on grid: Place piece | RIGHT CLICK: Remove | R: Rotate | F: Flip",
                FontSize = 10,
                Foreground = Brushes.DarkGray
            };
            Canvas.SetLeft(text, OFFSET_X);
            Canvas.SetTop(text, 450);
            canvas.Children.Add(text);
        }

        private void DrawLevelButtons(Canvas canvas)
        {
            // Easy Button (Level 1)
            double buttonX1 = 500;
            double buttonY = 30;
            double buttonWidth = 100;
            double buttonHeight = 40;
            
            // Button 1 - Hintergrund
            var buttonRect1 = new Rectangle
            {
                Width = buttonWidth,
                Height = buttonHeight,
                Fill = new SolidColorBrush(Color.FromRgb(100, 200, 100)),
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 2,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(buttonRect1, buttonX1);
            Canvas.SetTop(buttonRect1, buttonY);
            canvas.Children.Add(buttonRect1);
            
            // Button 1 - Text
            var buttonText1 = new TextBlock
            {
                Text = "Easy",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(buttonText1, buttonX1 + 25);
            Canvas.SetTop(buttonText1, buttonY + 8);
            canvas.Children.Add(buttonText1);

            // Challenge Button (Level 51)
            double buttonY2 = 100;
        
            
            // Button 2 - Hintergrund
            var buttonRect2 = new Rectangle
            {
                Width = buttonWidth,
                Height = buttonHeight,
                Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                Stroke = Brushes.DarkOrange,
                StrokeThickness = 2,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(buttonRect2, buttonX1);
            Canvas.SetTop(buttonRect2, buttonY2);
            canvas.Children.Add(buttonRect2);
            
            // Button 2 - Text
            var buttonText2 = new TextBlock
            {
                Text = "Challenge",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(buttonText2, buttonX1 + 5);
            Canvas.SetTop(buttonText2, buttonY2 + 10);
            canvas.Children.Add(buttonText2);
        }

        private Color GetColorForPieceId(int pieceId)
        {
            var allPieces = A3_LEA_IQPuzzlePieceFactory.CreateAllPieces();
            var piece = allPieces.FirstOrDefault(p => p.Id == pieceId);
            return piece != null ? piece.Color : Colors.Gray;
        }
    }

    #endregion

    #region Human Player

    /// <summary>
    /// Menschlicher IQ Puzzle Spieler mit Maussteuerung
    /// </summary>
    public class A3_LEA_IQPuzzleHumanPlayer : A3_LEA_BaseHumanIQPuzzlePlayer
    {
        private int _playerNumber = 1;
        private IA3_LEA_IQPuzzlePiece _selectedPiece = null;

        public override string Name => "A3 LEA IQ Puzzle Human";
        public override int PlayerNumber => _playerNumber;
        public override IA3_LEA_IQPuzzlePiece SelectedPiece
        {
            get => _selectedPiece;
            set => _selectedPiece = value;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public override IGamePlayer Clone()
        {
            return new A3_LEA_IQPuzzleHumanPlayer();
        }

        public override void OnMouseMoved(System.Windows.Input.MouseEventArgs e)
        {
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
            if (rules != null)
            {
                // Mausrad-Rotation ZUERST behandeln (vor Position-Update)
                if (e is System.Windows.Input.MouseWheelEventArgs)
                {
                    var wheelEvent = e as System.Windows.Input.MouseWheelEventArgs;
                    if (wheelEvent != null && _selectedPiece != null)
                    {
                        if (wheelEvent.Delta > 0)
                        {
                            // Mausrad nach oben = Drehen
                            _selectedPiece = _selectedPiece.Rotate();
                            rules.SelectedPieceForPainting = _selectedPiece;
                        }
                        else if (wheelEvent.Delta < 0)
                        {
                            // Mausrad nach unten = Rückwärts drehen (3x vorwärts)
                            _selectedPiece = _selectedPiece.Rotate().Rotate().Rotate();
                            rules.SelectedPieceForPainting = _selectedPiece;
                        }
                    }
                    // Bei Mausrad-Event: Position NICHT updaten, behalte aktuelle Position
                    return;
                }

                // Position-Update nur bei normalen Mouse-Move Events
                var mousePos = e.GetPosition(e.Source as System.Windows.IInputElement);
                
                // Berechne Gitterkoordinaten mit OFFSET_X=20, OFFSET_Y=20, CELL_SIZE=40
                int gridX = (int)((mousePos.X - 30) / 40);
                int gridY = (int)((mousePos.Y - 30) / 40);
                
                // Nur setzen, wenn innerhalb des Spielfelds (0-10 für X, 0-4 für Y)
                if (gridX >= 0 && gridX < 11 && gridY >= 0 && gridY < 5)
                {
                    rules.MouseX = gridX;
                    rules.MouseY = gridY;
                }
                else
                {
                    rules.MouseX = -1;
                    rules.MouseY = -1;
                }
            }
        }

        public override IA3_LEA_IQPuzzleMove GetMove(IMoveSelection selection, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces)
        {
            if (selection == null || field == null) return null;

            if (selection is IClickSelection)
            {
                var click = (IClickSelection)selection;
                
                // Prüfe ob auf den "Easy" Button geklickt wurde (X: 500-600, Y: 30-70)
                if (click.ChangedButton == 0 && // Linksklick
                    click.XClickPos >= 500 && click.XClickPos <= 600 &&
                    click.YClickPos >= 30 && click.YClickPos <= 70)
                {
                    // Lade Level 1
                    var levelRules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                    if (levelRules != null)
                    {
                        levelRules.LoadChallenge(1);

                        // Zurücksetzen der Auswahl
                        _selectedPiece = null;
                        levelRules.SelectedPieceForPainting = null;
                    }
                    return null; // Kein Move, nur Button-Klick
                }

                // Prüfe ob auf den "Challenge" Button geklickt wurde (X: 500-600, Y: 100-140)
                if (click.ChangedButton == 0 && // Linksklick
                    click.XClickPos >= 500 && click.XClickPos <= 600 &&
                    click.YClickPos >= 100 && click.YClickPos <= 140)
                {
                    // Lade Level 51
                    var levelRules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                    if (levelRules != null)
                    {
                        levelRules.LoadChallenge(51);

                        // Zurücksetzen der Auswahl
                        _selectedPiece = null;
                        levelRules.SelectedPieceForPainting = null;
                    }
                    return null; // Kein Move, nur Button-Klick
                }
                
                var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;

                // Linksklick
                if (click.ChangedButton == 0) // Left button
                {
                    // Prüfe ob in Piece-Auswahl geklickt wurde (unten)
                    if (click.YClickPos > 250)
                    {
                        // Die verfügbaren Teile werden in Reihen gezeichnet (7 Spalten pro Reihe)
                        int colsPerRow = 7; // entspricht der Zeichnungslogik (OFFSET_X=20, Schritt=60, wrap bei >400)
                        int col = (int)((click.XClickPos - 20) / 60);
                        int row = (int)((click.YClickPos - 250) / 80);

                        if (col >= 0 && row >= 0)
                        {
                            int pieceIndex = row * colsPerRow + col;
                            if (pieceIndex >= 0 && pieceIndex < availablePieces.Count)
                            {
                                _selectedPiece = availablePieces[pieceIndex];

                                // Speichere Auswahl in Rules für Painter
                                if (rules != null)
                                {
                                    rules.SelectedPieceForPainting = _selectedPiece;
                                }

                                return null; // Nur Auswahl, kein Zug
                            }
                        }
                    }
                    // Klick auf Spielfeld
                    else
                    {
                        int x = (int)((click.XClickPos - 20) / 40);
                        int y = (int)((click.YClickPos - 20) / 40);

                        if (field.IsValidPosition(x, y))
                        {
                            // Wenn bereits ein Teil ausgewählt ist, versuche zu platzieren
                            if (_selectedPiece != null)
                            {
                                if (rules != null && rules.CanPlacePiece(_selectedPiece, x, y))
                                {
                                    var move = new A3_LEA_IQPuzzleMove(_selectedPiece, x, y, _playerNumber);

                                    // Reset nach Platzierung
                                    _selectedPiece = null;
                                    if (rules != null) rules.SelectedPieceForPainting = null;

                                    return move;
                                }
                            }
                            else
                            {
                                // Wenn kein Teil ausgewählt ist und auf ein bereits gesetztes Teil geklickt wurde,
                                // dann dieses Teil aufnehmen (entfernen) und zur erneuten Platzierung auswählen.
                                int pieceId = field[x, y];
                                if (pieceId > 0 && rules != null)
                                {
                                    var piece = rules.PlacedPieces.FirstOrDefault(p => p.Id == pieceId);
                                    if (piece != null)
                                    {
                                        // Entferne das Stück vom Feld und markiere es als ausgewählt
                                        rules.RemovePiece(piece);
                                        _selectedPiece = piece;
                                        rules.SelectedPieceForPainting = _selectedPiece;
                                    }
                                }
                            }
                        }
                    }
                }
                // Rechtsklick: Flip selected piece
                else if (click.ChangedButton == 1) // Right button
                {
                    if (_selectedPiece != null)
                    {
                        _selectedPiece = _selectedPiece.Flip();

                        if (rules != null)
                        {
                            rules.SelectedPieceForPainting = _selectedPiece;
                        }
                    }
                }
            }
            else if (selection is IKeySelection)
            {
                var key = (IKeySelection)selection;
                var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                
                // R: Rotate selected piece
                if (key.Key == System.Windows.Input.Key.R && _selectedPiece != null)
                {
                    _selectedPiece = _selectedPiece.Rotate();
                    
                    if (rules != null)
                    {
                        rules.SelectedPieceForPainting = _selectedPiece;
                    }
                }
                // F: Flip selected piece
                else if (key.Key == System.Windows.Input.Key.F && _selectedPiece != null)
                {
                    _selectedPiece = _selectedPiece.Flip();
                    
                    if (rules != null)
                    {
                        rules.SelectedPieceForPainting = _selectedPiece;
                    }
                }
                // ESC: Auswahl abbrechen
                else if (key.Key == System.Windows.Input.Key.Escape && rules != null)
                {
                    _selectedPiece = null;
                    rules.SelectedPieceForPainting = null;
                }
                // H: Hint (platziert direkt)
                else if (key.Key == System.Windows.Input.Key.H && rules != null)
                {
                    var hint = rules.GetHint();
                    if (hint != null)
                    {
                        return hint;
                    }
                }
                // S: Solve
                else if (key.Key == System.Windows.Input.Key.S && rules != null)
                {
                    rules.SolvePuzzle();
                }
            }

            return null;
        }
    }

    #endregion
}
