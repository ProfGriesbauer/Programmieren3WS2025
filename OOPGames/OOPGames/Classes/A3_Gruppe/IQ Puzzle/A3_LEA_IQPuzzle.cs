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

            // Piece 1: L-Shape (4 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(1, "L-Shape", Colors.Red, new int[,]
            {
                { 1, 0 },
                { 1, 0 },
                { 1, 1 }
            }));

            // Piece 2: T-Shape (4 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(2, "T-Shape", Colors.Orange, new int[,]
            {
                { 1, 1, 1 },
                { 0, 1, 0 }
            }));

            // Piece 3: Z-Shape (4 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(3, "Z-Shape", Colors.Yellow, new int[,]
            {
                { 1, 1, 0 },
                { 0, 1, 1 }
            }));

            // Piece 4: Square (4 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(4, "Square", Colors.Green, new int[,]
            {
                { 1, 1 },
                { 1, 1 }
            }));

            // Piece 5: Long L (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(5, "Long-L", Colors.Cyan, new int[,]
            {
                { 1, 0 },
                { 1, 0 },
                { 1, 0 },
                { 1, 1 }
            }));

            // Piece 6: W-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(6, "W-Shape", Colors.Blue, new int[,]
            {
                { 1, 0, 0 },
                { 1, 1, 0 },
                { 0, 1, 1 }
            }));

            // Piece 7: P-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(7, "P-Shape", Colors.Purple, new int[,]
            {
                { 1, 1 },
                { 1, 1 },
                { 1, 0 }
            }));

            // Piece 8: Y-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(8, "Y-Shape", Colors.Magenta, new int[,]
            {
                { 0, 1 },
                { 1, 1 },
                { 0, 1 },
                { 0, 1 }
            }));

            // Piece 9: U-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(9, "U-Shape", Colors.Pink, new int[,]
            {
                { 1, 0, 1 },
                { 1, 1, 1 }
            }));

            // Piece 10: I-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(10, "I-Shape", Colors.Brown, new int[,]
            {
                { 1 },
                { 1 },
                { 1 },
                { 1 },
                { 1 }
            }));

            // Piece 11: V-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(11, "V-Shape", Colors.LightBlue, new int[,]
            {
                { 1, 0, 0 },
                { 1, 0, 0 },
                { 1, 1, 1 }
            }));

            // Piece 12: Plus-Shape (5 cells)
            pieces.Add(new A3_LEA_IQPuzzlePiece(12, "Plus", Colors.LightGreen, new int[,]
            {
                { 0, 1, 0 },
                { 1, 1, 1 },
                { 0, 1, 0 }
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

        public A3_LEA_IQPuzzleRules()
        {
            _field = new A3_LEA_IQPuzzleField();
            _allPieces = A3_LEA_IQPuzzlePieceFactory.CreateAllPieces();
            _placedPieces = new List<IA3_LEA_IQPuzzlePiece>();
            _piecePlacements = new Dictionary<int, (int x, int y)>();
            _currentDifficulty = 1;
            SelectedPieceForPainting = null;
            LoadChallenge(1);
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

            // Einfache Challenge: Platziere ein paar Stücke vor
            // (In einer echten Implementierung würden hier 120 vordefinierte Challenges sein)
            if (challengeNumber == 1)
            {
                // Starter Challenge: Nur wenige Stücke übrig
                PlacePiece(_allPieces[0], 0, 0);  // L-Shape
                PlacePiece(_allPieces[3], 3, 0);  // Square
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

        public override string Name => "A3 LEA IQ Puzzler Pro Paint";

        public override void PaintIQPuzzleField(Canvas canvas, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces, IA3_LEA_IQPuzzlePiece selectedPiece)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            // Zeichne Spielfeld-Gitter
            DrawGrid(canvas, field);

            // Zeichne platzierte Stücke
            DrawPlacedPieces(canvas, field);

            // Zeichne Platzierungs-Vorschau (wenn Position gesetzt ist)
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
            // Fallback: Wenn selectedPiece vom BasePainter null ist, verwende Rules-Property
            var effectiveSelectedPiece = selectedPiece ?? rules?.SelectedPieceForPainting;
            if (rules != null && effectiveSelectedPiece != null && rules.PreviewX >= 0 && rules.PreviewY >= 0)
            {
                DrawPlacementPreview(canvas, field, effectiveSelectedPiece, rules.PreviewX, rules.PreviewY);
            }

            // Zeichne verfügbare Stücke unten (mit aktuellem Rotations-Status!)
            DrawAvailablePieces(canvas, availablePieces, effectiveSelectedPiece);

            // Zeichne Anleitung
            DrawInstructions(canvas);
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
                Text = "LEFT CLICK: Select piece/position | ENTER: Place piece | RIGHT CLICK: Remove | R: Rotate | F: Flip | ESC: Cancel",
                FontSize = 10,
                Foreground = Brushes.DarkGray
            };
            Canvas.SetLeft(text, OFFSET_X);
            Canvas.SetTop(text, 450);
            canvas.Children.Add(text);
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

        public override IA3_LEA_IQPuzzleMove GetMove(IMoveSelection selection, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces)
        {
            if (selection == null || field == null) return null;

            if (selection is IClickSelection)
            {
                var click = (IClickSelection)selection;
                
                // Linksklick: Stück auswählen ODER Vorschau-Position setzen
                if (click.ChangedButton == 0) // Left button
                {
                    // Prüfe ob in Piece-Auswahl geklickt wurde
                    if (click.YClickPos > 250)
                    {
                        int pieceIndex = (int)((click.XClickPos - 20) / 60);
                        if (pieceIndex >= 0 && pieceIndex < availablePieces.Count)
                        {
                            _selectedPiece = availablePieces[pieceIndex];
                            
                            // Speichere Auswahl auch in den Rules für den Painter
                            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                            if (rules != null)
                            {
                                rules.SelectedPieceForPainting = _selectedPiece;
                                // Reset Preview-Position bei neuer Auswahl
                                rules.PreviewX = -1;
                                rules.PreviewY = -1;
                            }
                            
                            return null; // Nur Auswahl, kein Zug
                        }
                    }
                    // Setze Vorschau-Position (noch nicht platzieren!)
                    else if (_selectedPiece != null)
                    {
                        int x = (int)((click.XClickPos - 20) / 40);
                        int y = (int)((click.YClickPos - 20) / 40);

                        // Speichere Preview-Position in Rules (auch wenn außerhalb, dann sieht man rot)
                        var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                        if (rules != null)
                        {
                            rules.PreviewX = x;
                            rules.PreviewY = y;
                            // Stelle sicher, dass der Painter ein ausgewähltes Teil hat
                            if (rules.SelectedPieceForPainting == null)
                            {
                                rules.SelectedPieceForPainting = _selectedPiece;
                            }
                        }
                        
                        return null; // Nur Preview, kein Zug
                    }
                }
                // Rechtsklick: Stück entfernen
                else if (click.ChangedButton == 1) // Right button
                {
                    int x = (int)((click.XClickPos - 20) / 40);
                    int y = (int)((click.YClickPos - 20) / 40);

                    if (field.IsValidPosition(x, y))
                    {
                        int pieceId = field[x, y];
                        if (pieceId > 0)
                        {
                            var allPieces = A3_LEA_IQPuzzlePieceFactory.CreateAllPieces();
                            var piece = allPieces.FirstOrDefault(p => p.Id == pieceId);
                            if (piece != null)
                            {
                                return new A3_LEA_IQPuzzleMove(piece, x, y, _playerNumber, true);
                            }
                        }
                    }
                }
            }
            else if (selection is IKeySelection)
            {
                var key = (IKeySelection)selection;
                var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_IQPuzzleRules;
                
                // ENTER: Platziere das Teil an der Vorschau-Position
                if (key.Key == System.Windows.Input.Key.Enter && _selectedPiece != null && rules != null)
                {
                    if (rules.PreviewX >= 0 && rules.PreviewY >= 0)
                    {
                        // Prüfe ob Platzierung gültig ist
                        if (rules.CanPlacePiece(_selectedPiece, rules.PreviewX, rules.PreviewY))
                        {
                            var move = new A3_LEA_IQPuzzleMove(_selectedPiece, rules.PreviewX, rules.PreviewY, _playerNumber);
                            
                            // Reset nach Platzierung
                            rules.PreviewX = -1;
                            rules.PreviewY = -1;
                            _selectedPiece = null;
                            rules.SelectedPieceForPainting = null;
                            
                            return move;
                        }
                    }
                }
                // ESC: Abbrechen / Vorschau löschen
                else if (key.Key == System.Windows.Input.Key.Escape && rules != null)
                {
                    rules.PreviewX = -1;
                    rules.PreviewY = -1;
                    _selectedPiece = null;
                    rules.SelectedPieceForPainting = null;
                }
                // R: Rotate selected piece
                else if (key.Key == System.Windows.Input.Key.R && _selectedPiece != null)
                {
                    _selectedPiece = _selectedPiece.Rotate();
                    
                    // Update auch in Rules
                    if (rules != null)
                    {
                        rules.SelectedPieceForPainting = _selectedPiece;
                    }
                }
                // F: Flip selected piece
                else if (key.Key == System.Windows.Input.Key.F && _selectedPiece != null)
                {
                    _selectedPiece = _selectedPiece.Flip();
                    
                    // Update auch in Rules
                    if (rules != null)
                    {
                        rules.SelectedPieceForPainting = _selectedPiece;
                    }
                }
                // H: Hint
                else if (key.Key == System.Windows.Input.Key.H)
                {
                    if (rules != null)
                    {
                        var hint = rules.GetHint();
                        if (hint != null)
                        {
                            _selectedPiece = hint.Piece;
                            rules.SelectedPieceForPainting = _selectedPiece;
                            rules.PreviewX = hint.X;
                            rules.PreviewY = hint.Y;
                            return null; // Zeige nur Preview, platziere nicht automatisch
                        }
                    }
                }
                // S: Solve
                else if (key.Key == System.Windows.Input.Key.S)
                {
                    if (rules != null)
                    {
                        rules.SolvePuzzle();
                    }
                }
            }

            return null;
        }
    }

    #endregion
}
