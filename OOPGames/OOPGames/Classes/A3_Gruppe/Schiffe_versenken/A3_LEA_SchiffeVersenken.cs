using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // 10x10 Spielfeld
    public class A3_LEA_SchiffeField : IA3_LEA_SchiffeField
    {
        private int[,] _grid = new int[10, 10];
        public int Width => 10;
        public int Height => 10;
        public int this[int x, int y]
        {
            get => IsValidPosition(x, y) ? _grid[x, y] : -1;
            set { if (IsValidPosition(x, y)) _grid[x, y] = value; }
        }
        public bool IsValidPosition(int x, int y) => x >= 0 && x < 10 && y >= 0 && y < 10;
        public List<(int x, int y, int shipId)> GetOccupiedCells()
        {
            var occ = new List<(int x, int y, int shipId)>();
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    if (_grid[x, y] > 0) occ.Add((x, y, _grid[x, y]));
            return occ;
        }
        public bool CanBePaintedBy(IPaintGame painter) => painter is IA3_LEA_SchiffePaint;
    }

    // Schiffeversenken-Rules
    public class A3_LEA_SchiffeRules : A3_LEA_BaseSchiffeRules, IGameRules2
    {
        // Zwei Felder: jeweils ein Feld pro Spieler
        private A3_LEA_SchiffeField _field = new A3_LEA_SchiffeField();
        private A3_LEA_SchiffeField _field2 = new A3_LEA_SchiffeField();
        private List<A3_LEA_Ship> _ships = new List<A3_LEA_Ship>();
        private List<A3_LEA_Ship> _ships2 = new List<A3_LEA_Ship>();
        private List<(int x, int y)> _shots = new List<(int x, int y)>();
        private List<(int x, int y)> _shots2 = new List<(int x, int y)>();
        // Expose shots so painter and player input can check for misses/duplicates
        public List<(int x, int y)> Shots => _shots;
        public List<(int x, int y)> Shots2 => _shots2;

        // Phase: 1 = Player1 setup, 2 = Player2 setup, 3 = Playing
        private int _phase = 1;
        public int Phase { get { return _phase; } set { _phase = value; } }
        public bool IsSetupPhase => Phase == 1 || Phase == 2;
        public int CurrentSetupPlayer => Phase == 1 ? 1 : (Phase == 2 ? 2 : 0);

        public int PlacedShipsCount1 => _ships.Count(s => s.X > 0 || s.Y > 0);
        public int PlacedShipsCount2 => _ships2.Count(s => s.X > 0 || s.Y > 0);
        public bool AllShipsPlaced1 => PlacedShipsCount1 == _ships.Count;
        public bool AllShipsPlaced2 => PlacedShipsCount2 == _ships2.Count;

        // Whether ships are visible in Phase 3 (playing). Default: hidden.
        public bool ShowShipsPhase3 { get; set; } = false;
        // Whether the last move grants an extra turn (e.g., hit in Battleship)
        public bool LastMoveGivesExtraTurn { get; private set; } = false;

        // Ausgewähltes Schiff (vom aktiven Spieler)
        public A3_LEA_Ship SelectedShip { get; set; } = null;

        // Maus-Tracking (wie IQ-Puzzle)
        public int MouseX { get; set; } = -1;
        public int MouseY { get; set; } = -1;

        public A3_LEA_SchiffeRules()
        {
            // Spieler 1
            _ships.Add(new A3_LEA_Ship(1, 4));
            // add 5-cell ship
            _ships.Add(new A3_LEA_Ship(5, 5));
            _ships.Add(new A3_LEA_Ship(2, 3));
            _ships.Add(new A3_LEA_Ship(3, 3));
            _ships.Add(new A3_LEA_Ship(4, 2));
            // Spieler 2 (eigene Objekte)
            _ships2.Add(new A3_LEA_Ship(1, 4));
            // add 5-cell ship for player 2
            _ships2.Add(new A3_LEA_Ship(5, 5));
            _ships2.Add(new A3_LEA_Ship(2, 3));
            _ships2.Add(new A3_LEA_Ship(3, 3));
            _ships2.Add(new A3_LEA_Ship(4, 2));
        }

        public override IA3_LEA_SchiffeField SchiffeField => _field;
        public IA3_LEA_SchiffeField SchiffeField2 => _field2;
        public override List<A3_LEA_Ship> Ships => _ships;
        public List<A3_LEA_Ship> Ships2 => _ships2;
        public override IGameField CurrentField => _field;
        public override bool MovesPossible => _ships.Any(s => s.Hits < s.Size) || _ships2.Any(s => s.Hits < s.Size);
        public override string Name => "A3 LEA Schiffe Versenken";

        public override bool CanPlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal)
        {
            int size = ship.Size;
            // Bestimme Ziel-Feld basierend darauf, zu welcher Schiffsliste dieses Schiff gehört
            var targetField = _ships.Contains(ship) ? _field : _field2;
            if (horizontal)
            {
                if (x + size > targetField.Width) return false;
                for (int i = 0; i < size; i++)
                {
                    int cx = x + i;
                    int cy = y;
                    // cell itself must be free
                    if (targetField[cx, cy] != 0) return false;
                    // check surrounding cells to enforce at least one water cell between ships
                    for (int nx = cx - 1; nx <= cx + 1; nx++)
                        for (int ny = cy - 1; ny <= cy + 1; ny++)
                        {
                            if (!targetField.IsValidPosition(nx, ny)) continue;
                            // skip the cells that are part of this ship (they are being checked separately)
                            if (nx >= x && nx < x + size && ny == cy) continue;
                            if (targetField[nx, ny] != 0) return false;
                        }
                }
            }
            else
            {
                if (y + size > targetField.Height) return false;
                for (int i = 0; i < size; i++)
                {
                    int cx = x;
                    int cy = y + i;
                    // cell itself must be free
                    if (targetField[cx, cy] != 0) return false;
                    // check surrounding cells to enforce spacing
                    for (int nx = cx - 1; nx <= cx + 1; nx++)
                        for (int ny = cy - 1; ny <= cy + 1; ny++)
                        {
                            if (!targetField.IsValidPosition(nx, ny)) continue;
                            // skip the cells that are part of this ship
                            if (ny >= y && ny < y + size && nx == cx) continue;
                            if (targetField[nx, ny] != 0) return false;
                        }
                }
            }
            return true;
        }

        public override void PlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal)
        {
            if (!CanPlaceShip(ship, x, y, horizontal)) return;
            ship.X = x;
            ship.Y = y;
            ship.IsHorizontal = horizontal;
            var targetField = _ships.Contains(ship) ? _field : _field2;
            for (int i = 0; i < ship.Size; i++)
            {
                if (horizontal)
                    targetField[x + i, y] = ship.Id;
                else
                    targetField[x, y + i] = ship.Id;
            }
        }
        public override bool ShootAt(int x, int y)
        {
            // Standardmäßig schießt diese Methode auf Feld 1
            return ShootAtOnField(_field, _ships, _shots, x, y);
        }

        public bool ShootAtForPlayer(int shooter, int x, int y)
        {
            // shooter 1 schießt auf field2, shooter 2 schießt auf field1
            if (shooter == 1)
                return ShootAtOnField(_field2, _ships2, _shots2, x, y);
            else
                return ShootAtOnField(_field, _ships, _shots, x, y);
        }

        private bool ShootAtOnField(A3_LEA_SchiffeField targetField, List<A3_LEA_Ship> shipsList, List<(int x, int y)> shotsList, int x, int y)
        {
            if (!targetField.IsValidPosition(x, y) || shotsList.Contains((x, y)))
                return false;
            shotsList.Add((x, y));
            int shipId = targetField[x, y];
            if (shipId > 0)
            {
                var ship = shipsList.FirstOrDefault(s => s.Id == shipId);
                if (ship != null)
                {
                    int hitIndex = ship.IsHorizontal ? x - ship.X : y - ship.Y;
                    if (hitIndex >= 0 && hitIndex < ship.Size)
                        ship.HitCells[hitIndex] = true;
                }
                return true;
            }
            return false;
        }

        public override int CheckIfPLayerWon()
        {
            // Wenn alle Schiffe von Spieler1 versenkt sind -> Spieler2 gewinnt (return 2)
            if (_ships.All(s => s.Hits == s.Size)) return 2;
            // Wenn alle Schiffe von Spieler2 versenkt sind -> Spieler1 gewinnt (return 1)
            if (_ships2.All(s => s.Hits == s.Size)) return 1;
            return 0;
        }

        public override void ClearField()
        {
            _field = new A3_LEA_SchiffeField();
            _field2 = new A3_LEA_SchiffeField();
            _shots.Clear();
            _shots2.Clear();
            foreach (var ship in _ships)
            {
                ship.X = 0;
                ship.Y = 0;
                ship.IsHorizontal = true;
                ship.HitCells = new bool[ship.Size];
            }
            foreach (var ship in _ships2)
            {
                ship.X = 0;
                ship.Y = 0;
                ship.IsHorizontal = true;
                ship.HitCells = new bool[ship.Size];
            }
            Phase = 1;
            SelectedShip = null;
            MouseX = -1;
            MouseY = -1;
        }

        // IGameRules2 Implementation
        public void StartedGameCall()
        {
            ClearField();
        }

        public void TickGameCall()
        {
            // Für Tick-basierte Spiele, nicht benötigt für Schiffe versenken
        }

        public override void DoMove(IPlayMove move)
        {
            if (move is IA3_LEA_SchiffeMove m)
            {
                // reset extra-turn flag by default; rules will set it for hits
                LastMoveGivesExtraTurn = false;

                if (IsSetupPhase && SelectedShip != null)
                {
                    // Setup-Phase: Ausgewähltes Schiff platzieren
                    bool horizontal = SelectedShip.IsHorizontal;
                    if (CanPlaceShip(SelectedShip, m.X, m.Y, horizontal))
                    {
                        PlaceShip(SelectedShip, m.X, m.Y, horizontal);
                        SelectedShip = null;
                        MouseX = -1;
                        MouseY = -1;
                    }
                }
                else
                {
                    // Playing-Phase: Schießen (auf das gegnerische Feld)
                    bool hit = ShootAtForPlayer(m.PlayerNumber, m.X, m.Y);
                    // grant extra turn on hit
                    LastMoveGivesExtraTurn = hit;
                }
            }
        }
    }

    // Painter
    public class A3_LEA_SchiffePaint : A3_LEA_BaseSchiffePaint, IPaintGame2
    {
        private const double CELL_SIZE = 40;
        private const double OFFSET_X = 20;
        private const double OFFSET_Y = 20;

        public override string Name => "A3 LEA Schiffe Paint";

        public override void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is IA3_LEA_SchiffeField field)
            {
                var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
                var ships = rules?.Ships ?? new List<A3_LEA_Ship>();
                PaintSchiffeField(canvas, field, ships);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            // Nutze die gleiche Logik wie PaintGameField für Tick-basiertes Repainting
            PaintGameField(canvas, currentField);
        }

        public override void PaintSchiffeField(Canvas canvas, IA3_LEA_SchiffeField field, List<A3_LEA_Ship> ships)
        {
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Colors.LightBlue);
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
            if (rules == null) return;

            // Phase 1 and 2: show single field with selection bar for the active setup player
            if (rules.Phase == 1 || rules.Phase == 2)
            {
                // use large cells for setup
                double cellSize = CELL_SIZE;
                double baseX = OFFSET_X;
                double baseY = OFFSET_Y;
                var targetField = rules.Phase == 1 ? (IA3_LEA_SchiffeField)rules.SchiffeField : (IA3_LEA_SchiffeField)rules.SchiffeField2;
                var shipsList = rules.Phase == 1 ? rules.Ships : rules.Ships2;

                // draw grid
                for (int x = 0; x <= targetField.Width; x++)
                {
                    var line = new Line { X1 = baseX + x * cellSize, Y1 = baseY, X2 = baseX + x * cellSize, Y2 = baseY + targetField.Height * cellSize, Stroke = Brushes.Black, StrokeThickness = 1 };
                    canvas.Children.Add(line);
                }
                for (int y = 0; y <= targetField.Height; y++)
                {
                    var line = new Line { X1 = baseX, Y1 = baseY + y * cellSize, X2 = baseX + targetField.Width * cellSize, Y2 = baseY + y * cellSize, Stroke = Brushes.Black, StrokeThickness = 1 };
                    canvas.Children.Add(line);
                }

                // draw placed ships
                foreach (var ship in shipsList)
                {
                    if (ship.X > 0 && ship.Y > 0)
                    {
                        DrawWarship(canvas, ship, baseX, baseY, cellSize, ship.IsHorizontal);
                    }
                }

                // preview
                if (rules.SelectedShip != null && rules.MouseX >= 0 && rules.MouseY >= 0)
                {
                    var ship = rules.SelectedShip;
                    bool canPlace = rules.CanPlaceShip(ship, rules.MouseX, rules.MouseY, ship.IsHorizontal);
                    var previewBrush = new SolidColorBrush(canPlace ? Color.FromArgb(100, 0, 255, 0) : Color.FromArgb(100, 255, 0, 0));
                    for (int i = 0; i < ship.Size; i++)
                    {
                        int x = ship.IsHorizontal ? rules.MouseX + i : rules.MouseX;
                        int y = ship.IsHorizontal ? rules.MouseY : rules.MouseY + i;
                        if (targetField.IsValidPosition(x, y))
                        {
                            var rect = new Rectangle { Width = cellSize - 2, Height = cellSize - 2, Fill = previewBrush, Stroke = canPlace ? Brushes.LimeGreen : Brushes.Red, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 2 } };
                            Canvas.SetLeft(rect, baseX + x * cellSize + 1);
                            Canvas.SetTop(rect, baseY + y * cellSize + 1);
                            canvas.Children.Add(rect);
                        }
                    }
                }

                // selection bar
                double shipPreviewY = baseY + targetField.Height * cellSize + 50;
                double shipPreviewX = baseX;
                const double shipCellSize = 12;
                const double shipStep = 80;
                var shipLabel = new TextBlock { Text = "Available Ships:", FontSize = 12, FontWeight = System.Windows.FontWeights.Bold };
                Canvas.SetLeft(shipLabel, shipPreviewX);
                Canvas.SetTop(shipLabel, shipPreviewY - 25);
                canvas.Children.Add(shipLabel);
                for (int idx = 0; idx < shipsList.Count; idx++)
                {
                    var s = shipsList[idx];
                    double x = shipPreviewX + idx * shipStep;
                    bool isSelected = rules.SelectedShip != null && rules.SelectedShip == s;
                    bool isPlaced = s.X > 0 || s.Y > 0;
                    if (isSelected)
                    {
                        var border = new Rectangle { Width = s.Size * shipCellSize + 6, Height = shipCellSize + 6, Stroke = Brushes.Gold, StrokeThickness = 3, RadiusX = 3, RadiusY = 3 };
                        Canvas.SetLeft(border, x - 3);
                        Canvas.SetTop(border, shipPreviewY - 3);
                        canvas.Children.Add(border);
                    }
                    
                    // Zeichne Mini-Kriegsschiff
                    DrawMiniWarship(canvas, s, x, shipPreviewY, shipCellSize, true, isPlaced);
                }

                // Start-Button for progression
                bool showButton = (rules.Phase == 1 && rules.AllShipsPlaced1) || (rules.Phase == 2 && rules.AllShipsPlaced2);
                if (showButton)
                {
                    double buttonX = shipPreviewX + shipsList.Count * shipStep + 20;
                    double buttonY = shipPreviewY - 5;
                    double buttonWidth = 120;
                    double buttonHeight = 40;
                    var buttonRect = new Rectangle { Width = buttonWidth, Height = buttonHeight, Fill = new SolidColorBrush(Color.FromRgb(0, 150, 0)), Stroke = Brushes.DarkGreen, StrokeThickness = 2, RadiusX = 5, RadiusY = 5 };
                    Canvas.SetLeft(buttonRect, buttonX); Canvas.SetTop(buttonRect, buttonY); canvas.Children.Add(buttonRect);
                    var buttonText = new TextBlock { Text = "Next", FontSize = 14, FontWeight = System.Windows.FontWeights.Bold, Foreground = Brushes.White };
                    Canvas.SetLeft(buttonText, buttonX + 20); Canvas.SetTop(buttonText, buttonY + 10); canvas.Children.Add(buttonText);
                }

                var label = new TextBlock { Text = rules.IsSetupPhase ? (rules.SelectedShip != null ? $"Selected: {rules.SelectedShip.Size} cells - R: Rotate, Click on grid to place" : $"Player {rules.CurrentSetupPlayer}: Click a ship to select it") : "Playing: Click to shoot", FontSize = 14, Foreground = Brushes.Black };
                Canvas.SetLeft(label, baseX);
                Canvas.SetTop(label, baseY + targetField.Height * cellSize + 20);
                canvas.Children.Add(label);
                return;
            }

            // Phase 3: Playing - show both fields stacked vertically with smaller cells
            double smallCell = 24;
            double topBaseX = OFFSET_X;
            double topBaseY = OFFSET_Y;
            double bottomBaseX = OFFSET_X;
            double bottomBaseY = OFFSET_Y + field.Height * smallCell + 40;

            // draw top (Player1)
            var f1 = rules.SchiffeField;
            for (int x = 0; x <= f1.Width; x++) canvas.Children.Add(new Line { X1 = topBaseX + x * smallCell, Y1 = topBaseY, X2 = topBaseX + x * smallCell, Y2 = topBaseY + f1.Height * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            for (int y = 0; y <= f1.Height; y++) canvas.Children.Add(new Line { X1 = topBaseX, Y1 = topBaseY + y * smallCell, X2 = topBaseX + f1.Width * smallCell, Y2 = topBaseY + y * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            var title1 = new TextBlock { Text = "Player 1 Field", FontWeight = System.Windows.FontWeights.Bold }; Canvas.SetLeft(title1, topBaseX); Canvas.SetTop(title1, topBaseY - 18); canvas.Children.Add(title1);
            // draw occupied cells only if ships are visible in Phase 3
            if (rules.ShowShipsPhase3)
            {
                foreach (var cell in f1.GetOccupiedCells()) { var rect = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = Brushes.Gray, Stroke = Brushes.Black, StrokeThickness = 1 }; Canvas.SetLeft(rect, topBaseX + cell.x * smallCell + 1); Canvas.SetTop(rect, topBaseY + cell.y * smallCell + 1); canvas.Children.Add(rect); }
            }
            // draw hits on player1 ships
            foreach (var s in rules.Ships)
                for (int i = 0; i < s.Size; i++)
                    if (s.HitCells[i])
                    {
                        int sx = s.IsHorizontal ? s.X + i : s.X;
                        int sy = s.IsHorizontal ? s.Y : s.Y + i;
                        var e = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red, Stroke = Brushes.DarkRed, StrokeThickness = 1.5 };
                        Canvas.SetLeft(e, topBaseX + sx * smallCell + smallCell / 2 - 4);
                        Canvas.SetTop(e, topBaseY + sy * smallCell + smallCell / 2 - 4);
                        Canvas.SetZIndex(e, 100); // Hoher Z-Index damit Treffer IMMER vor Schiffen sind
                        canvas.Children.Add(e);
                    }
            // draw misses on player1 field
            foreach (var miss in rules.Shots)
            {
                // if miss (no ship at that cell)
                if (f1.IsValidPosition(miss.x, miss.y) && f1[miss.x, miss.y] == 0)
                {
                    var m = new Ellipse { Width = 6, Height = 6, Stroke = Brushes.CornflowerBlue, Fill = Brushes.White, StrokeThickness = 1.5 };
                    Canvas.SetLeft(m, topBaseX + miss.x * smallCell + smallCell / 2 - 3);
                    Canvas.SetTop(m, topBaseY + miss.y * smallCell + smallCell / 2 - 3);
                    Canvas.SetZIndex(m, 99);
                    canvas.Children.Add(m);
                }
            }

            // draw sunk ships for player1 as faded gray (visible even when ships are hidden)
            foreach (var s in rules.Ships)
            {
                if (s.Hits == s.Size)
                {
                    for (int i = 0; i < s.Size; i++)
                    {
                        int sx = s.IsHorizontal ? s.X + i : s.X;
                        int sy = s.IsHorizontal ? s.Y : s.Y + i;
                        if (!f1.IsValidPosition(sx, sy)) continue;
                        var faded = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = new SolidColorBrush(Color.FromArgb(140, 180, 180, 180)), Stroke = Brushes.DarkGray, StrokeThickness = 0.5 };
                        Canvas.SetLeft(faded, topBaseX + sx * smallCell + 1);
                        Canvas.SetTop(faded, topBaseY + sy * smallCell + 1);
                        canvas.Children.Add(faded);
                    }
                }
            }

            // draw bottom (Player2)
            var f2 = rules.SchiffeField2;
            for (int x = 0; x <= f2.Width; x++) canvas.Children.Add(new Line { X1 = bottomBaseX + x * smallCell, Y1 = bottomBaseY, X2 = bottomBaseX + x * smallCell, Y2 = bottomBaseY + f2.Height * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            for (int y = 0; y <= f2.Height; y++) canvas.Children.Add(new Line { X1 = bottomBaseX, Y1 = bottomBaseY + y * smallCell, X2 = bottomBaseX + f2.Width * smallCell, Y2 = bottomBaseY + y * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            var title2 = new TextBlock { Text = "Player 2 Field", FontWeight = System.Windows.FontWeights.Bold }; Canvas.SetLeft(title2, bottomBaseX); Canvas.SetTop(title2, bottomBaseY - 18); canvas.Children.Add(title2);
            if (rules.ShowShipsPhase3)
            {
                foreach (var cell in f2.GetOccupiedCells()) { var rect = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = Brushes.Gray, Stroke = Brushes.Black, StrokeThickness = 1 }; Canvas.SetLeft(rect, bottomBaseX + cell.x * smallCell + 1); Canvas.SetTop(rect, bottomBaseY + cell.y * smallCell + 1); canvas.Children.Add(rect); }
            }
            // draw hits on player2 ships
            foreach (var s in rules.Ships2)
                for (int i = 0; i < s.Size; i++)
                    if (s.HitCells[i])
                    {
                        int sx = s.IsHorizontal ? s.X + i : s.X;
                        int sy = s.IsHorizontal ? s.Y : s.Y + i;
                        var e = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red, Stroke = Brushes.DarkRed, StrokeThickness = 1.5 };
                        Canvas.SetLeft(e, bottomBaseX + sx * smallCell + smallCell / 2 - 4);
                        Canvas.SetTop(e, bottomBaseY + sy * smallCell + smallCell / 2 - 4);
                        Canvas.SetZIndex(e, 100); // Hoher Z-Index damit Treffer IMMER vor Schiffen sind
                        canvas.Children.Add(e);
                    }
            // draw misses on player2 field
            foreach (var miss in rules.Shots2)
            {
                if (f2.IsValidPosition(miss.x, miss.y) && f2[miss.x, miss.y] == 0)
                {
                    var m = new Ellipse { Width = 6, Height = 6, Stroke = Brushes.CornflowerBlue, Fill = Brushes.White, StrokeThickness = 1.5 };
                    Canvas.SetLeft(m, bottomBaseX + miss.x * smallCell + smallCell / 2 - 3);
                    Canvas.SetTop(m, bottomBaseY + miss.y * smallCell + smallCell / 2 - 3);
                    Canvas.SetZIndex(m, 99);
                    canvas.Children.Add(m);
                }
            }

            // draw sunk ships for player2 as faded gray (visible even when ships are hidden)
            foreach (var s in rules.Ships2)
            {
                if (s.Hits == s.Size)
                {
                    for (int i = 0; i < s.Size; i++)
                    {
                        int sx = s.IsHorizontal ? s.X + i : s.X;
                        int sy = s.IsHorizontal ? s.Y : s.Y + i;
                        if (!f2.IsValidPosition(sx, sy)) continue;
                        var faded = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = new SolidColorBrush(Color.FromArgb(140, 180, 180, 180)), Stroke = Brushes.DarkGray, StrokeThickness = 0.5 };
                        Canvas.SetLeft(faded, bottomBaseX + sx * smallCell + 1);
                        Canvas.SetTop(faded, bottomBaseY + sy * smallCell + 1);
                        canvas.Children.Add(faded);
                    }
                }
            }

            var info = new TextBlock { Text = "Playing: click opponent field to shoot", FontSize = 14, Foreground = Brushes.Black };
            Canvas.SetLeft(info, OFFSET_X);
            Canvas.SetTop(info, bottomBaseY + f2.Height * smallCell + 10);
            canvas.Children.Add(info);

            // Show/Hide Ships toggle button
            double shipsButtonX = topBaseX + f1.Width * smallCell + 20;
            double shipsButtonY = topBaseY;
            double shipsButtonW = 120;
            double shipsButtonH = 28;
            var btnRect = new Rectangle { Width = shipsButtonW, Height = shipsButtonH, Fill = new SolidColorBrush(Color.FromRgb(50, 50, 50)), Stroke = Brushes.Black, StrokeThickness = 1, RadiusX = 4, RadiusY = 4 };
            Canvas.SetLeft(btnRect, shipsButtonX); Canvas.SetTop(btnRect, shipsButtonY); canvas.Children.Add(btnRect);
            var btnText = new TextBlock { Text = rules.ShowShipsPhase3 ? "Hide Ships" : "Show Ships", FontSize = 12, Foreground = Brushes.White, FontWeight = System.Windows.FontWeights.Bold };
            Canvas.SetLeft(btnText, shipsButtonX + 12); Canvas.SetTop(btnText, shipsButtonY + 6); canvas.Children.Add(btnText);
        }

        private void DrawWarship(Canvas canvas, A3_LEA_Ship ship, double baseX, double baseY, double cellSize, bool horizontal)
        {
            // Realistische Kriegsschiff-Farben (Vogelperspektive)
            var hullColor = new LinearGradientBrush(
                Color.FromRgb(55, 65, 75),    // Dunkel
                Color.FromRgb(75, 85, 95),    // Heller
                90);
            var deckColor = new SolidColorBrush(Color.FromRgb(85, 95, 105)); // Deck
            var darkHullColor = new SolidColorBrush(Color.FromRgb(45, 55, 65)); // Schatten
            var detailColor = new SolidColorBrush(Color.FromRgb(35, 45, 55)); // Aufbauten
            var highlightColor = new SolidColorBrush(Color.FromRgb(100, 110, 120)); // Highlights
            var accentColor = new SolidColorBrush(Color.FromRgb(200, 80, 60)); // Rot für Details
            var orangeColor = new SolidColorBrush(Color.FromRgb(220, 140, 60)); // Orange für Rettungsboote
            
            int shipSize = ship.Size;
            double startX = baseX + ship.X * cellSize;
            double startY = baseY + ship.Y * cellSize;

            if (horizontal)
            {
                // Horizontales Kriegsschiff (von oben betrachtet)
                double shipWidth = cellSize * shipSize;
                double shipHeight = cellSize * 0.88;
                double yOffset = (cellSize - shipHeight) / 2;

                // Mehrschichtiges Schatten-System für Tiefenwahrnehmung
                for (int shadowLayer = 0; shadowLayer < 3; shadowLayer++)
                {
                    var shadow = new System.Windows.Shapes.Ellipse
                    {
                        Width = shipWidth * (0.85 + shadowLayer * 0.05),
                        Height = shipHeight * (0.6 + shadowLayer * 0.1),
                        Fill = new SolidColorBrush(Color.FromArgb((byte)(25 - shadowLayer * 8), 0, 0, 0))
                    };
                    Canvas.SetLeft(shadow, startX + shipWidth * 0.075 - shadowLayer * 2);
                    Canvas.SetTop(shadow, startY + yOffset + shipHeight * 0.2 + shadowLayer * 1);
                    Canvas.SetZIndex(shadow, 0);
                    canvas.Children.Add(shadow);
                }

                // Wasserverdrängung unter dem Schiff
                var waterDisplacement = new System.Windows.Shapes.Ellipse
                {
                    Width = shipWidth * 0.95,
                    Height = shipHeight * 0.85,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(40, 50, 120, 180),
                        Color.FromArgb(0, 50, 120, 180))
                };
                Canvas.SetLeft(waterDisplacement, startX + shipWidth * 0.025);
                Canvas.SetTop(waterDisplacement, startY + yOffset + shipHeight * 0.08);
                Canvas.SetZIndex(waterDisplacement, 0);
                canvas.Children.Add(waterDisplacement);

                // Bugwelle mit Gischt-Effekt
                var bowWave = new System.Windows.Shapes.Ellipse
                {
                    Width = cellSize * 0.35,
                    Height = shipHeight * 0.45,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(120, 240, 250, 255),
                        Color.FromArgb(40, 200, 220, 240)),
                    Stroke = new SolidColorBrush(Color.FromArgb(150, 220, 235, 245)),
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(bowWave, startX - cellSize * 0.18);
                Canvas.SetTop(bowWave, startY + yOffset + shipHeight * 0.28);
                Canvas.SetZIndex(bowWave, 0);
                canvas.Children.Add(bowWave);

                // Gischt-Spritzer am Bug
                for (int spray = 0; spray < 3; spray++)
                {
                    var sprayDrop = new System.Windows.Shapes.Ellipse
                    {
                        Width = 2 - spray * 0.5,
                        Height = 2 - spray * 0.5,
                        Fill = new SolidColorBrush(Color.FromArgb((byte)(150 - spray * 40), 255, 255, 255))
                    };
                    Canvas.SetLeft(sprayDrop, startX - cellSize * 0.22 - spray * 2);
                    Canvas.SetTop(sprayDrop, startY + yOffset + shipHeight * 0.3 + spray * 3);
                    Canvas.SetZIndex(sprayDrop, 0);
                    canvas.Children.Add(sprayDrop);
                }

                // Heckwelle mit Verwirbelung
                var sternWave = new System.Windows.Shapes.Ellipse
                {
                    Width = cellSize * 0.28,
                    Height = shipHeight * 0.38,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(90, 220, 235, 245),
                        Color.FromArgb(30, 200, 220, 240))
                };
                Canvas.SetLeft(sternWave, startX + shipWidth - cellSize * 0.12);
                Canvas.SetTop(sternWave, startY + yOffset + shipHeight * 0.31);
                Canvas.SetZIndex(sternWave, 0);
                canvas.Children.Add(sternWave);

                // Dynamische Kielwasser-Trails
                for (int wakeTrail = 0; wakeTrail < 5; wakeTrail++)
                {
                    var wake = new System.Windows.Shapes.Ellipse
                    {
                        Width = cellSize * (0.15 - wakeTrail * 0.02),
                        Height = shipHeight * (0.22 - wakeTrail * 0.03),
                        Fill = new RadialGradientBrush(
                            Color.FromArgb((byte)(70 - wakeTrail * 12), 200, 220, 240),
                            Color.FromArgb((byte)(20 - wakeTrail * 3), 180, 200, 220))
                    };
                    Canvas.SetLeft(wake, startX + shipWidth + cellSize * 0.08 + wakeTrail * 4);
                    Canvas.SetTop(wake, startY + yOffset + shipHeight * (0.35 + wakeTrail * 0.02));
                    Canvas.SetZIndex(wake, 0);
                    canvas.Children.Add(wake);
                }

                // Schaumkronen im Kielwasser
                for (int foam = 0; foam < 4; foam++)
                {
                    var foamBubble = new System.Windows.Shapes.Ellipse
                    {
                        Width = 1.5 - foam * 0.3,
                        Height = 1.5 - foam * 0.3,
                        Fill = new SolidColorBrush(Color.FromArgb((byte)(180 - foam * 35), 255, 255, 255))
                    };
                    Canvas.SetLeft(foamBubble, startX + shipWidth + 3 + foam * 2.5);
                    Canvas.SetTop(foamBubble, startY + yOffset + shipHeight * 0.4 + foam * 1.5);
                    Canvas.SetZIndex(foamBubble, 1);
                    canvas.Children.Add(foamBubble);
                }

                // Hauptrumpf - realistisch geformt
                var hull = new System.Windows.Shapes.Polygon
                {
                    Fill = hullColor,
                    Stroke = new SolidColorBrush(Color.FromRgb(25, 35, 45)),
                    StrokeThickness = 1.5
                };
                
                // Bug (sehr spitz, wie ein echtes Kriegsschiff)
                hull.Points.Add(new System.Windows.Point(startX + 1, startY + yOffset + shipHeight / 2));
                hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.2, startY + yOffset + 4));
                hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.35, startY + yOffset + 2));
                hull.Points.Add(new System.Windows.Point(startX + shipWidth - cellSize * 0.15, startY + yOffset + 2));
                // Heck (V-förmig für Propeller)
                hull.Points.Add(new System.Windows.Point(startX + shipWidth - 2, startY + yOffset + shipHeight * 0.3));
                hull.Points.Add(new System.Windows.Point(startX + shipWidth - cellSize * 0.1, startY + yOffset + shipHeight / 2));
                hull.Points.Add(new System.Windows.Point(startX + shipWidth - 2, startY + yOffset + shipHeight * 0.7));
                hull.Points.Add(new System.Windows.Point(startX + shipWidth - cellSize * 0.15, startY + yOffset + shipHeight - 2));
                hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.35, startY + yOffset + shipHeight - 2));
                hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.2, startY + yOffset + shipHeight - 4));
                
                Canvas.SetZIndex(hull, 1);
                canvas.Children.Add(hull);

                // Seitliche Schatten/Details
                var sideDetail1 = new Line
                {
                    X1 = startX + cellSize * 0.3, Y1 = startY + yOffset + 5,
                    X2 = startX + shipWidth - cellSize * 0.2, Y2 = startY + yOffset + 5,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail1, 2);
                canvas.Children.Add(sideDetail1);

                var sideDetail2 = new Line
                {
                    X1 = startX + cellSize * 0.3, Y1 = startY + yOffset + shipHeight - 5,
                    X2 = startX + shipWidth - cellSize * 0.2, Y2 = startY + yOffset + shipHeight - 5,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail2, 2);
                canvas.Children.Add(sideDetail2);

                // Deck-Mittellinie
                var deckLine = new Line
                {
                    X1 = startX + cellSize * 0.25, Y1 = startY + yOffset + shipHeight / 2,
                    X2 = startX + shipWidth - cellSize * 0.15, Y2 = startY + yOffset + shipHeight / 2,
                    Stroke = highlightColor, StrokeThickness = 2
                };
                Canvas.SetZIndex(deckLine, 2);
                canvas.Children.Add(deckLine);

                // Panzerplatten-Textur mit Nieten
                for (int i = 0; i < shipSize; i++)
                {
                    var plate = new Line
                    {
                        X1 = startX + cellSize * i + cellSize * 0.15,
                        Y1 = startY + yOffset + 8,
                        X2 = startX + cellSize * i + cellSize * 0.15,
                        Y2 = startY + yOffset + shipHeight - 8,
                        Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                        StrokeThickness = 0.8
                    };
                    Canvas.SetZIndex(plate, 2);
                    canvas.Children.Add(plate);

                    // 3D-Nieten auf den Panzerplatten
                    for (int n = 0; n < 3; n++)
                    {
                        // Nieten-Schatten
                        var rivetShadow = new System.Windows.Shapes.Ellipse
                        {
                            Width = 2, Height = 2,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0))
                        };
                        Canvas.SetLeft(rivetShadow, startX + cellSize * i + cellSize * 0.5 - 0.5);
                        Canvas.SetTop(rivetShadow, startY + yOffset + 12.5 + n * (shipHeight - 24) / 2);
                        Canvas.SetZIndex(rivetShadow, 3);
                        canvas.Children.Add(rivetShadow);

                        // Nieten-Basis
                        var rivet = new System.Windows.Shapes.Ellipse
                        {
                            Width = 2, Height = 2,
                            Fill = new RadialGradientBrush(
                                Color.FromRgb(110, 120, 130),
                                Color.FromRgb(70, 80, 90))
                        };
                        Canvas.SetLeft(rivet, startX + cellSize * i + cellSize * 0.5 - 1);
                        Canvas.SetTop(rivet, startY + yOffset + 12 + n * (shipHeight - 24) / 2);
                        Canvas.SetZIndex(rivet, 4);
                        canvas.Children.Add(rivet);

                        // Nieten-Highlight
                        var rivetHighlight = new System.Windows.Shapes.Ellipse
                        {
                            Width = 0.8, Height = 0.8,
                            Fill = new SolidColorBrush(Color.FromArgb(150, 180, 190, 200))
                        };
                        Canvas.SetLeft(rivetHighlight, startX + cellSize * i + cellSize * 0.5 - 0.7);
                        Canvas.SetTop(rivetHighlight, startY + yOffset + 12.2 + n * (shipHeight - 24) / 2);
                        Canvas.SetZIndex(rivetHighlight, 5);
                        canvas.Children.Add(rivetHighlight);
                    }
                }

                // Anker am Bug
                if (shipSize >= 3)
                {
                    var anchorChain = new Line
                    {
                        X1 = startX + cellSize * 0.2, Y1 = startY + yOffset + 8,
                        X2 = startX + cellSize * 0.2, Y2 = startY + yOffset + 12,
                        Stroke = new SolidColorBrush(Color.FromRgb(80, 90, 100)),
                        StrokeThickness = 1.5
                    };
                    Canvas.SetZIndex(anchorChain, 4);
                    canvas.Children.Add(anchorChain);

                    var anchor = new System.Windows.Shapes.Polygon
                    {
                        Fill = new SolidColorBrush(Color.FromRgb(60, 70, 80)),
                        Stroke = darkHullColor,
                        StrokeThickness = 0.5
                    };
                    anchor.Points.Add(new System.Windows.Point(startX + cellSize * 0.2, startY + yOffset + 12));
                    anchor.Points.Add(new System.Windows.Point(startX + cellSize * 0.18, startY + yOffset + 16));
                    anchor.Points.Add(new System.Windows.Point(startX + cellSize * 0.2, startY + yOffset + 15));
                    anchor.Points.Add(new System.Windows.Point(startX + cellSize * 0.22, startY + yOffset + 16));
                    Canvas.SetZIndex(anchor, 4);
                    canvas.Children.Add(anchor);
                }

                // Rettungsboote
                if (shipSize >= 3)
                {
                    for (int side = 0; side < 2; side++)
                    {
                        double boatY = startY + yOffset + (side == 0 ? 10 : shipHeight - 14);
                        double boatX = startX + shipWidth * 0.6;
                        
                        var lifeboat = new Ellipse
                        {
                            Width = cellSize * 0.12, Height = 4,
                            Fill = orangeColor,
                            Stroke = darkHullColor, StrokeThickness = 0.5
                        };
                        Canvas.SetLeft(lifeboat, boatX);
                        Canvas.SetTop(lifeboat, boatY);
                        Canvas.SetZIndex(lifeboat, 4);
                        canvas.Children.Add(lifeboat);
                    }
                }

                // Brücke/Kommandoturm (verbessert) (für Schiffe >= 3)
                if (shipSize >= 3)
                {
                    double bridgeX = startX + shipWidth * 0.42;
                    double bridgeY = startY + yOffset + shipHeight * 0.22;
                    double bridgeW = cellSize * 0.4;
                    double bridgeH = shipHeight * 0.56;
                    
                    // Brücken-Hauptstruktur
                    var bridge = new Rectangle
                    {
                        Width = bridgeW, Height = bridgeH,
                        Fill = new LinearGradientBrush(deckColor.Color, Color.FromRgb(70, 80, 90), 45),
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1, RadiusX = 2, RadiusY = 2
                    };
                    Canvas.SetLeft(bridge, bridgeX);
                    Canvas.SetTop(bridge, bridgeY);
                    Canvas.SetZIndex(bridge, 3);
                    canvas.Children.Add(bridge);

                    // Fenster/Kommandobrücke
                    // Fenster mit Glas-Effekt und Rahmen
                    for (int i = 0; i < 3; i++)
                    {
                        var windowFrame = new Rectangle
                        {
                            Width = bridgeW * 0.22, Height = 3,
                            Fill = darkHullColor
                        };
                        Canvas.SetLeft(windowFrame, bridgeX + bridgeW * 0.1 + i * bridgeW * 0.28);
                        Canvas.SetTop(windowFrame, bridgeY + bridgeH * 0.22);
                        Canvas.SetZIndex(windowFrame, 5);
                        canvas.Children.Add(windowFrame);

                        // Innenraumbeleuchtung hinter Fenster
                        var interiorLight = new Rectangle
                        {
                            Width = bridgeW * 0.2, Height = 2.5,
                            Fill = new LinearGradientBrush(
                                Color.FromArgb(100, 255, 240, 200),
                                Color.FromArgb(40, 255, 220, 150), 90)
                        };
                        Canvas.SetLeft(interiorLight, bridgeX + bridgeW * 0.11 + i * bridgeW * 0.28);
                        Canvas.SetTop(interiorLight, bridgeY + bridgeH * 0.24);
                        Canvas.SetZIndex(interiorLight, 5);
                        canvas.Children.Add(interiorLight);

                        var window = new Rectangle
                        {
                            Width = bridgeW * 0.18, Height = 2,
                            Fill = new LinearGradientBrush(
                                Color.FromArgb(200, 180, 220, 255),
                                Color.FromArgb(140, 150, 200, 240), 45)
                        };
                        Canvas.SetLeft(window, bridgeX + bridgeW * 0.12 + i * bridgeW * 0.28);
                        Canvas.SetTop(window, bridgeY + bridgeH * 0.25);
                        Canvas.SetZIndex(window, 6);
                        canvas.Children.Add(window);

                        // Fenster-Spiegelung
                        var windowReflection = new Line
                        {
                            X1 = bridgeX + bridgeW * 0.12 + i * bridgeW * 0.28,
                            Y1 = bridgeY + bridgeH * 0.25,
                            X2 = bridgeX + bridgeW * 0.12 + i * bridgeW * 0.28 + bridgeW * 0.18,
                            Y2 = bridgeY + bridgeH * 0.25,
                            Stroke = new SolidColorBrush(Color.FromArgb(150, 240, 250, 255)),
                            StrokeThickness = 0.5
                        };
                        Canvas.SetZIndex(windowReflection, 7);
                        canvas.Children.Add(windowReflection);
                    }

                    // Schornstein mit realistischem Rauch
                    var chimney = new Rectangle
                    {
                        Width = cellSize * 0.08, Height = bridgeH * 0.5,
                        Fill = new LinearGradientBrush(Color.FromRgb(90, 100, 110), Color.FromRgb(60, 70, 80), 90),
                        Stroke = darkHullColor, StrokeThickness = 0.8
                    };
                    Canvas.SetLeft(chimney, bridgeX + bridgeW + 2);
                    Canvas.SetTop(chimney, bridgeY - bridgeH * 0.2);
                    Canvas.SetZIndex(chimney, 5);
                    canvas.Children.Add(chimney);

                    // Volumetrischer Rauch mit Wirbeln
                    for (int s = 0; s < 5; s++)
                    {
                        var smoke = new Ellipse
                        {
                            Width = 3 + s * 0.8, Height = 2.5 + s * 0.6,
                            Fill = new RadialGradientBrush(
                                Color.FromArgb((byte)(50 - s * 8), 120, 120, 120),
                                Color.FromArgb((byte)(15 - s * 2), 80, 80, 80))
                        };
                        Canvas.SetLeft(smoke, bridgeX + bridgeW + 2 - s * 0.7 + (s % 2 == 0 ? 1 : -1));
                        Canvas.SetTop(smoke, bridgeY - bridgeH * 0.3 - s * 2.5);
                        Canvas.SetZIndex(smoke, 5);
                        canvas.Children.Add(smoke);
                    }

                    // Hitze-Verzerrung über Schornstein
                    var heatDistortion = new System.Windows.Shapes.Ellipse
                    {
                        Width = cellSize * 0.1,
                        Height = bridgeH * 0.3,
                        Fill = new LinearGradientBrush(
                            Color.FromArgb(15, 255, 200, 150),
                            Color.FromArgb(0, 255, 200, 150), 90)
                    };
                    Canvas.SetLeft(heatDistortion, bridgeX + bridgeW + 1);
                    Canvas.SetTop(heatDistortion, bridgeY - bridgeH * 0.4);
                    Canvas.SetZIndex(heatDistortion, 4);
                    canvas.Children.Add(heatDistortion);

                    // Scheinwerfer mit volumetrischem Licht
                    var spotlightGlow = new Ellipse
                    {
                        Width = 6, Height = 6,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(80, 255, 240, 180),
                            Color.FromArgb(0, 255, 240, 180))
                    };
                    Canvas.SetLeft(spotlightGlow, bridgeX + bridgeW * 0.1 - 1.5);
                    Canvas.SetTop(spotlightGlow, bridgeY + bridgeH * 0.7 - 1.5);
                    Canvas.SetZIndex(spotlightGlow, 6);
                    canvas.Children.Add(spotlightGlow);

                    var spotlight = new Ellipse
                    {
                        Width = 3, Height = 3,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(240, 255, 255, 230),
                            Color.FromArgb(150, 255, 235, 180))
                    };
                    Canvas.SetLeft(spotlight, bridgeX + bridgeW * 0.1);
                    Canvas.SetTop(spotlight, bridgeY + bridgeH * 0.7);
                    Canvas.SetZIndex(spotlight, 7);
                    canvas.Children.Add(spotlight);

                    // Lichtstrahl-Effekt
                    var lightBeam = new System.Windows.Shapes.Polygon
                    {
                        Fill = new LinearGradientBrush(
                            Color.FromArgb(30, 255, 255, 200),
                            Color.FromArgb(0, 255, 255, 200), 45)
                    };
                    lightBeam.Points.Add(new System.Windows.Point(bridgeX + bridgeW * 0.1 + 1.5, bridgeY + bridgeH * 0.7 + 1.5));
                    lightBeam.Points.Add(new System.Windows.Point(bridgeX + bridgeW * 0.1 - 3, bridgeY + bridgeH * 0.7 + 8));
                    lightBeam.Points.Add(new System.Windows.Point(bridgeX + bridgeW * 0.1 + 6, bridgeY + bridgeH * 0.7 + 8));
                    Canvas.SetZIndex(lightBeam, 5);
                    canvas.Children.Add(lightBeam);

                    // Mehrere Antennen
                    var antenna1 = new Line
                    {
                        X1 = bridgeX + bridgeW * 0.3, Y1 = bridgeY,
                        X2 = bridgeX + bridgeW * 0.3, Y2 = bridgeY - 5,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 130, 140)), StrokeThickness = 1
                    };
                    Canvas.SetZIndex(antenna1, 6);
                    canvas.Children.Add(antenna1);

                    var antenna2 = new Line
                    {
                        X1 = bridgeX + bridgeW * 0.7, Y1 = bridgeY,
                        X2 = bridgeX + bridgeW * 0.7, Y2 = bridgeY - 4,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 130, 140)), StrokeThickness = 0.8
                    };
                    Canvas.SetZIndex(antenna2, 6);
                    canvas.Children.Add(antenna2);

                    // Rotes Signallicht auf Antenne 1
                    var signalLight = new System.Windows.Shapes.Ellipse
                    {
                        Width = 2, Height = 2,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(220, 255, 50, 50),
                            Color.FromArgb(100, 200, 30, 30))
                    };
                    Canvas.SetLeft(signalLight, bridgeX + bridgeW * 0.3 - 1);
                    Canvas.SetTop(signalLight, bridgeY - 6);
                    Canvas.SetZIndex(signalLight, 7);
                    canvas.Children.Add(signalLight);

                    // Licht-Glow um Signal
                    var signalGlow = new System.Windows.Shapes.Ellipse
                    {
                        Width = 4, Height = 4,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(80, 255, 100, 100),
                            Color.FromArgb(0, 255, 100, 100))
                    };
                    Canvas.SetLeft(signalGlow, bridgeX + bridgeW * 0.3 - 2);
                    Canvas.SetTop(signalGlow, bridgeY - 7);
                    Canvas.SetZIndex(signalGlow, 6);
                    canvas.Children.Add(signalGlow);

                    // Elektrische Funken (klein)
                    for (int spark = 0; spark < 2; spark++)
                    {
                        var electricSpark = new Line
                        {
                            X1 = bridgeX + bridgeW * 0.7, Y1 = bridgeY - 4,
                            X2 = bridgeX + bridgeW * 0.7 + (spark == 0 ? 1 : -1), Y2 = bridgeY - 5,
                            Stroke = new SolidColorBrush(Color.FromArgb(180, 150, 200, 255)),
                            StrokeThickness = 0.5
                        };
                        Canvas.SetZIndex(electricSpark, 7);
                        canvas.Children.Add(electricSpark);
                    }
                }

                // Rettungsboote mit 3D-Effekt
                if (shipSize >= 3)
                {
                    for (int side = 0; side < 2; side++)
                    {
                        double boatY = startY + yOffset + (side == 0 ? 10 : shipHeight - 14);
                        double boatX = startX + shipWidth * 0.6;
                        
                        // Rettungsboot-Schatten
                        var boatShadow = new Ellipse
                        {
                            Width = cellSize * 0.13, Height = 4.5,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0))
                        };
                        Canvas.SetLeft(boatShadow, boatX + 0.5);
                        Canvas.SetTop(boatShadow, boatY + 0.5);
                        Canvas.SetZIndex(boatShadow, 3);
                        canvas.Children.Add(boatShadow);

                        var lifeboat = new Ellipse
                        {
                            Width = cellSize * 0.12, Height = 4,
                            Fill = new LinearGradientBrush(
                                Color.FromRgb(240, 160, 80),
                                Color.FromRgb(200, 120, 40), 90),
                            Stroke = darkHullColor, StrokeThickness = 0.5
                        };
                        Canvas.SetLeft(lifeboat, boatX);
                        Canvas.SetTop(lifeboat, boatY);
                        Canvas.SetZIndex(lifeboat, 4);
                        canvas.Children.Add(lifeboat);

                        // Highlight auf Rettungsboot
                        var boatHighlight = new Ellipse
                        {
                            Width = cellSize * 0.06, Height = 1.5,
                            Fill = new SolidColorBrush(Color.FromArgb(120, 255, 220, 150))
                        };
                        Canvas.SetLeft(boatHighlight, boatX + cellSize * 0.02);
                        Canvas.SetTop(boatHighlight, boatY + 0.8);
                        Canvas.SetZIndex(boatHighlight, 5);
                        canvas.Children.Add(boatHighlight);
                    }
                }

                // Geschütztürme (verbessert mit Doppelläufen und Plattformen)
                if (shipSize >= 4)
                {
                    // Vorderer Geschützturm
                    double gun1X = startX + shipWidth * 0.22;
                    double gun1Y = startY + yOffset + shipHeight * 0.5;
                    
                    // Turmplattform mit Schatten
                    var platformShadow = new Ellipse
                    {
                        Width = cellSize * 0.3, Height = cellSize * 0.3,
                        Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
                    };
                    Canvas.SetLeft(platformShadow, gun1X - cellSize * 0.15 + 1);
                    Canvas.SetTop(platformShadow, gun1Y - cellSize * 0.15 + 1);
                    Canvas.SetZIndex(platformShadow, 2);
                    canvas.Children.Add(platformShadow);

                    var gunPlatform1 = new Ellipse
                    {
                        Width = cellSize * 0.28, Height = cellSize * 0.28,
                        Fill = new RadialGradientBrush(
                            Color.FromRgb(70, 80, 90),
                            Color.FromRgb(35, 45, 55)),
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1.2
                    };
                    Canvas.SetLeft(gunPlatform1, gun1X - cellSize * 0.14);
                    Canvas.SetTop(gunPlatform1, gun1Y - cellSize * 0.14);
                    Canvas.SetZIndex(gunPlatform1, 3);
                    canvas.Children.Add(gunPlatform1);

                    var gunBase1 = new Ellipse
                    {
                        Width = cellSize * 0.18, Height = cellSize * 0.18,
                        Fill = new RadialGradientBrush(
                            Color.FromRgb(90, 100, 110),
                            Color.FromRgb(50, 60, 70)),
                        Stroke = darkHullColor, StrokeThickness = 1
                    };
                    Canvas.SetLeft(gunBase1, gun1X - cellSize * 0.09);
                    Canvas.SetTop(gunBase1, gun1Y - cellSize * 0.09);
                    Canvas.SetZIndex(gunBase1, 4);
                    canvas.Children.Add(gunBase1);

                    // Metallglanz auf Turm
                    var turretShine = new Ellipse
                    {
                        Width = cellSize * 0.08, Height = cellSize * 0.08,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(120, 200, 210, 220),
                            Color.FromArgb(0, 200, 210, 220))
                    };
                    Canvas.SetLeft(turretShine, gun1X - cellSize * 0.05);
                    Canvas.SetTop(turretShine, gun1Y - cellSize * 0.06);
                    Canvas.SetZIndex(turretShine, 5);
                    canvas.Children.Add(turretShine);

                    // Doppelläufe
                    for (int barrel = 0; barrel < 2; barrel++)
                    {
                        var gunBarrel = new Rectangle
                        {
                            Width = cellSize * 0.18, Height = 2,
                            Fill = new LinearGradientBrush(Color.FromRgb(40, 50, 60), Color.FromRgb(60, 70, 80), 0),
                            Stroke = darkHullColor, StrokeThickness = 0.5
                        };
                        Canvas.SetLeft(gunBarrel, gun1X - cellSize * 0.22);
                        Canvas.SetTop(gunBarrel, gun1Y - 2.5 + barrel * 3);
                        Canvas.SetZIndex(gunBarrel, 5);
                        canvas.Children.Add(gunBarrel);
                    }

                    // Hinterer Geschützturm
                    double gun2X = startX + shipWidth * 0.72;
                    
                    var gunBase2 = new Rectangle
                    {
                        Width = cellSize * 0.2, Height = cellSize * 0.2,
                        Fill = detailColor,
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1, RadiusX = 1, RadiusY = 1
                    };
                    Canvas.SetLeft(gunBase2, gun2X - cellSize * 0.1);
                    Canvas.SetTop(gunBase2, gun1Y - cellSize * 0.1);
                    Canvas.SetZIndex(gunBase2, 3);
                    canvas.Children.Add(gunBase2);

                    var gunBarrel2 = new Rectangle
                    {
                        Width = cellSize * 0.15, Height = 2.5,
                        Fill = darkHullColor
                    };
                    Canvas.SetLeft(gunBarrel2, gun2X - cellSize * 0.2);
                    Canvas.SetTop(gunBarrel2, gun1Y - 1.25);
                    Canvas.SetZIndex(gunBarrel2, 4);
                    canvas.Children.Add(gunBarrel2);
                }

                // Radaranlagen mit photorealistischen Effekten (für große Schiffe >= 5)
                if (shipSize >= 5)
                {
                    double radarX = startX + shipWidth * 0.54;
                    double radarY = startY + yOffset + shipHeight * 0.32;
                    
                    // Radar-Mast mit Metalleffekt
                    var radarMast = new Rectangle
                    {
                        Width = 1.5, Height = cellSize * 0.08,
                        Fill = new LinearGradientBrush(
                            Color.FromRgb(140, 150, 160),
                            Color.FromRgb(100, 110, 120), 0)
                    };
                    Canvas.SetLeft(radarMast, radarX + cellSize * 0.068);
                    Canvas.SetTop(radarMast, radarY);
                    Canvas.SetZIndex(radarMast, 5);
                    canvas.Children.Add(radarMast);

                    // Radar-Schüssel mit 3D-Gradient
                    var radar = new Ellipse
                    {
                        Width = cellSize * 0.16, Height = cellSize * 0.13,
                        Fill = new RadialGradientBrush(
                            Color.FromRgb(100, 110, 120),
                            Color.FromRgb(50, 60, 70)),
                        Stroke = new SolidColorBrush(Color.FromRgb(30, 40, 50)),
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(radar, radarX);
                    Canvas.SetTop(radar, radarY);
                    Canvas.SetZIndex(radar, 6);
                    canvas.Children.Add(radar);

                    // Radar-Scan-Linie (grün)
                    var radarLine = new Line
                    {
                        X1 = radarX + cellSize * 0.08, Y1 = radarY + cellSize * 0.065,
                        X2 = radarX + cellSize * 0.14, Y2 = radarY + cellSize * 0.03,
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
                        StrokeThickness = 1.5
                    };
                    Canvas.SetZIndex(radarLine, 7);
                    canvas.Children.Add(radarLine);

                    // Pulsierender Radar-Glow
                    var radarGlowOuter = new Ellipse
                    {
                        Width = 5, Height = 5,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(60, 100, 255, 100),
                            Color.FromArgb(0, 100, 255, 100))
                    };
                    Canvas.SetLeft(radarGlowOuter, radarX + cellSize * 0.105);
                    Canvas.SetTop(radarGlowOuter, radarY + cellSize * 0.005);
                    Canvas.SetZIndex(radarGlowOuter, 8);
                    canvas.Children.Add(radarGlowOuter);

                    var radarGlow = new Ellipse
                    {
                        Width = 2.5, Height = 2.5,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(220, 150, 255, 150),
                            Color.FromArgb(100, 100, 255, 100))
                    };
                    Canvas.SetLeft(radarGlow, radarX + cellSize * 0.118);
                    Canvas.SetTop(radarGlow, radarY + cellSize * 0.018);
                    Canvas.SetZIndex(radarGlow, 9);
                    canvas.Children.Add(radarGlow);

                    // Radar-Strahlungswellen
                    for (int wave = 0; wave < 2; wave++)
                    {
                        var radarWave = new Ellipse
                        {
                            Width = 9 + wave * 5, Height = 9 + wave * 5,
                            Stroke = new SolidColorBrush(Color.FromArgb((byte)(35 - wave * 12), 100, 255, 100)),
                            StrokeThickness = 0.8
                        };
                        Canvas.SetLeft(radarWave, radarX + cellSize * 0.085 - wave * 2.5);
                        Canvas.SetTop(radarWave, radarY - wave * 2.5);
                        Canvas.SetZIndex(radarWave, 7);
                        canvas.Children.Add(radarWave);
                    }

                    // Flaggenmast mit Metallglanz
                    var flagpole = new Rectangle
                    {
                        Width = 1, Height = 8,
                        Fill = new LinearGradientBrush(
                            Color.FromRgb(150, 160, 170),
                            Color.FromRgb(100, 110, 120), 0)
                    };
                    Canvas.SetLeft(flagpole, startX + shipWidth - 8);
                    Canvas.SetTop(flagpole, startY + yOffset + shipHeight * 0.35);
                    Canvas.SetZIndex(flagpole, 5);
                    canvas.Children.Add(flagpole);

                    // Wehende Flagge mit Schatten
                    var flagShadow = new System.Windows.Shapes.Polygon
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
                    };
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 6.5, startY + yOffset + shipHeight * 0.35 + 0.5));
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 2.5, startY + yOffset + shipHeight * 0.37 + 0.5));
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 6.5, startY + yOffset + shipHeight * 0.39 + 0.5));
                    Canvas.SetZIndex(flagShadow, 5);
                    canvas.Children.Add(flagShadow);

                    var flag = new System.Windows.Shapes.Polygon
                    {
                        Fill = new LinearGradientBrush(
                            Color.FromRgb(220, 90, 70),
                            Color.FromRgb(180, 60, 40), 45),
                        Stroke = new SolidColorBrush(Color.FromRgb(150, 50, 30)),
                        StrokeThickness = 0.5
                    };
                    flag.Points.Add(new System.Windows.Point(startX + shipWidth - 7, startY + yOffset + shipHeight * 0.35));
                    flag.Points.Add(new System.Windows.Point(startX + shipWidth - 3, startY + yOffset + shipHeight * 0.37));
                    flag.Points.Add(new System.Windows.Point(startX + shipWidth - 7, startY + yOffset + shipHeight * 0.39));
                    Canvas.SetZIndex(flag, 6);
                    canvas.Children.Add(flag);
                }
            }
            else
            {
                // Vertikales Kriegsschiff (von oben betrachtet)
                double shipHeight = cellSize * shipSize;
                double shipWidth = cellSize * 0.88;
                double xOffset = (cellSize - shipWidth) / 2;

                // Schatten und Wellen
                var shadow = new System.Windows.Shapes.Ellipse
                {
                    Width = shipWidth * 0.7,
                    Height = shipHeight * 0.9,
                    Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
                };
                Canvas.SetLeft(shadow, startX + xOffset + shipWidth * 0.2);
                Canvas.SetTop(shadow, startY + shipHeight * 0.05);
                Canvas.SetZIndex(shadow, 0);
                canvas.Children.Add(shadow);

                // Bugwelle (vertikal)
                var bowWave = new System.Windows.Shapes.Ellipse
                {
                    Width = shipWidth * 0.45,
                    Height = cellSize * 0.3,
                    Fill = new SolidColorBrush(Color.FromArgb(80, 200, 220, 240)),
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 180, 200, 220)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(bowWave, startX + xOffset + shipWidth * 0.25);
                Canvas.SetTop(bowWave, startY - cellSize * 0.15);
                Canvas.SetZIndex(bowWave, 0);
                canvas.Children.Add(bowWave);

                // Heckwelle (vertikal)
                var sternWave = new System.Windows.Shapes.Ellipse
                {
                    Width = shipWidth * 0.4,
                    Height = cellSize * 0.25,
                    Fill = new SolidColorBrush(Color.FromArgb(60, 200, 220, 240))
                };
                Canvas.SetLeft(sternWave, startX + xOffset + shipWidth * 0.28);
                Canvas.SetTop(sternWave, startY + shipHeight - cellSize * 0.1);
                Canvas.SetZIndex(sternWave, 0);
                canvas.Children.Add(sternWave);

                // Hauptrumpf - realistisch geformt
                var hull = new System.Windows.Shapes.Polygon
                {
                    Fill = hullColor,
                    Stroke = new SolidColorBrush(Color.FromRgb(25, 35, 45)),
                    StrokeThickness = 1.5
                };
                
                // Bug (sehr spitz nach oben)
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth / 2, startY + 1));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + 4, startY + cellSize * 0.2));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + 2, startY + cellSize * 0.35));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + 2, startY + shipHeight - cellSize * 0.15));
                // Heck (unten, V-förmig)
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth * 0.3, startY + shipHeight - 2));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth / 2, startY + shipHeight - cellSize * 0.1));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth * 0.7, startY + shipHeight - 2));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth - 2, startY + shipHeight - cellSize * 0.15));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth - 2, startY + cellSize * 0.35));
                hull.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth - 4, startY + cellSize * 0.2));
                
                Canvas.SetZIndex(hull, 1);
                canvas.Children.Add(hull);

                // Seitliche Schatten/Details
                var sideDetail1 = new Line
                {
                    X1 = startX + xOffset + 5, Y1 = startY + cellSize * 0.3,
                    X2 = startX + xOffset + 5, Y2 = startY + shipHeight - cellSize * 0.2,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail1, 2);
                canvas.Children.Add(sideDetail1);

                var sideDetail2 = new Line
                {
                    X1 = startX + xOffset + shipWidth - 5, Y1 = startY + cellSize * 0.3,
                    X2 = startX + xOffset + shipWidth - 5, Y2 = startY + shipHeight - cellSize * 0.2,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail2, 2);
                canvas.Children.Add(sideDetail2);

                // Panzerplatten mit Nieten (vertikal)
                for (int i = 0; i < shipSize; i++)
                {
                    var plate = new Line
                    {
                        X1 = startX + xOffset + 8, Y1 = startY + cellSize * i + cellSize * 0.15,
                        X2 = startX + xOffset + shipWidth - 8, Y2 = startY + cellSize * i + cellSize * 0.15,
                        Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                        StrokeThickness = 0.8
                    };
                    Canvas.SetZIndex(plate, 2);
                    canvas.Children.Add(plate);

                    // Nieten
                    for (int n = 0; n < 3; n++)
                    {
                        var rivet = new System.Windows.Shapes.Ellipse
                        {
                            Width = 1.5, Height = 1.5,
                            Fill = new SolidColorBrush(Color.FromRgb(90, 100, 110))
                        };
                        Canvas.SetLeft(rivet, startX + xOffset + 12 + n * (shipWidth - 24) / 2);
                        Canvas.SetTop(rivet, startY + cellSize * i + cellSize * 0.5 - 0.75);
                        Canvas.SetZIndex(rivet, 3);
                        canvas.Children.Add(rivet);
                    }
                }

                // Deck-Mittellinie
                var deckLine = new Line
                {
                    X1 = startX + xOffset + shipWidth / 2, Y1 = startY + cellSize * 0.25,
                    X2 = startX + xOffset + shipWidth / 2, Y2 = startY + shipHeight - cellSize * 0.15,
                    Stroke = highlightColor, StrokeThickness = 2
                };
                Canvas.SetZIndex(deckLine, 2);
                canvas.Children.Add(deckLine);

                // Anker am Bug (vertikal)
                if (shipSize >= 3)
                {
                    var anchorChain = new Line
                    {
                        X1 = startX + xOffset + 8, Y1 = startY + cellSize * 0.2,
                        X2 = startX + xOffset + 12, Y2 = startY + cellSize * 0.2,
                        Stroke = new SolidColorBrush(Color.FromRgb(80, 90, 100)),
                        StrokeThickness = 1.5
                    };
                    Canvas.SetZIndex(anchorChain, 4);
                    canvas.Children.Add(anchorChain);

                    var anchor = new System.Windows.Shapes.Polygon
                    {
                        Fill = new SolidColorBrush(Color.FromRgb(60, 70, 80)),
                        Stroke = darkHullColor,
                        StrokeThickness = 0.5
                    };
                    anchor.Points.Add(new System.Windows.Point(startX + xOffset + 12, startY + cellSize * 0.2));
                    anchor.Points.Add(new System.Windows.Point(startX + xOffset + 16, startY + cellSize * 0.18));
                    anchor.Points.Add(new System.Windows.Point(startX + xOffset + 15, startY + cellSize * 0.2));
                    anchor.Points.Add(new System.Windows.Point(startX + xOffset + 16, startY + cellSize * 0.22));
                    Canvas.SetZIndex(anchor, 4);
                    canvas.Children.Add(anchor);
                }

                // Brücke/Kommandoturm
                if (shipSize >= 3)
                {
                    double bridgeX = startX + xOffset + shipWidth * 0.22;
                    double bridgeY = startY + shipHeight * 0.42;
                    double bridgeW = shipWidth * 0.56;
                    double bridgeH = cellSize * 0.4;
                    
                    var bridge = new Rectangle
                    {
                        Width = bridgeW, Height = bridgeH,
                        Fill = new LinearGradientBrush(deckColor.Color, Color.FromRgb(70, 80, 90), 45),
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1, RadiusX = 2, RadiusY = 2
                    };
                    Canvas.SetLeft(bridge, bridgeX);
                    Canvas.SetTop(bridge, bridgeY);
                    Canvas.SetZIndex(bridge, 3);
                    canvas.Children.Add(bridge);

                    // Fenster mit Glas-Effekt
                    for (int i = 0; i < 3; i++)
                    {
                        var windowFrame = new Rectangle
                        {
                            Width = 3, Height = bridgeH * 0.22,
                            Fill = darkHullColor
                        };
                        Canvas.SetLeft(windowFrame, startX + xOffset + shipWidth / 2 - 1.5);
                        Canvas.SetTop(windowFrame, bridgeY + bridgeH * 0.12 + i * bridgeH * 0.28);
                        Canvas.SetZIndex(windowFrame, 5);
                        canvas.Children.Add(windowFrame);

                        var window = new Rectangle
                        {
                            Width = 2, Height = bridgeH * 0.18,
                            Fill = new SolidColorBrush(Color.FromArgb(180, 150, 180, 200))
                        };
                        Canvas.SetLeft(window, startX + xOffset + shipWidth / 2 - 1);
                        Canvas.SetTop(window, bridgeY + bridgeH * 0.14 + i * bridgeH * 0.28);
                        Canvas.SetZIndex(window, 6);
                        canvas.Children.Add(window);
                    }

                    // Scheinwerfer
                    var spotlight = new Ellipse
                    {
                        Width = 3, Height = 3,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(200, 255, 255, 220),
                            Color.FromArgb(100, 255, 220, 150))
                    };
                    Canvas.SetLeft(spotlight, bridgeX + bridgeW * 0.7);
                    Canvas.SetTop(spotlight, bridgeY + bridgeH * 0.1);
                    Canvas.SetZIndex(spotlight, 6);
                    canvas.Children.Add(spotlight);

                    // Antennen
                    var antenna1 = new Line
                    {
                        X1 = startX + xOffset + shipWidth / 2, Y1 = bridgeY,
                        X2 = startX + xOffset + shipWidth / 2 - 5, Y2 = bridgeY,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 130, 140)), StrokeThickness = 1
                    };
                    Canvas.SetZIndex(antenna1, 6);
                    canvas.Children.Add(antenna1);

                    var antenna2 = new Line
                    {
                        X1 = startX + xOffset + shipWidth / 2, Y1 = bridgeY + bridgeH * 0.3,
                        X2 = startX + xOffset + shipWidth / 2 + 4, Y2 = bridgeY + bridgeH * 0.3,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 130, 140)), StrokeThickness = 0.8
                    };
                    Canvas.SetZIndex(antenna2, 6);
                    canvas.Children.Add(antenna2);
                }

                // Geschütztürme
                if (shipSize >= 4)
                {
                    double gunX = startX + xOffset + shipWidth / 2;
                    
                    // Vorderer Geschützturm
                    double gun1Y = startY + shipHeight * 0.22;
                    
                    var gunBase1 = new Rectangle
                    {
                        Width = cellSize * 0.2, Height = cellSize * 0.2,
                        Fill = detailColor,
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1, RadiusX = 1, RadiusY = 1
                    };
                    Canvas.SetLeft(gunBase1, gunX - cellSize * 0.1);
                    Canvas.SetTop(gunBase1, gun1Y - cellSize * 0.1);
                    Canvas.SetZIndex(gunBase1, 3);
                    canvas.Children.Add(gunBase1);

                    var gunBarrel1 = new Rectangle
                    {
                        Width = 2.5, Height = cellSize * 0.15,
                        Fill = darkHullColor
                    };
                    Canvas.SetLeft(gunBarrel1, gunX - 1.25);
                    Canvas.SetTop(gunBarrel1, gun1Y - cellSize * 0.2);
                    Canvas.SetZIndex(gunBarrel1, 4);
                    canvas.Children.Add(gunBarrel1);

                    // Hinterer Geschützturm
                    double gun2Y = startY + shipHeight * 0.72;
                    
                    var gunBase2 = new Rectangle
                    {
                        Width = cellSize * 0.2, Height = cellSize * 0.2,
                        Fill = detailColor,
                        Stroke = new SolidColorBrush(Color.FromRgb(20, 30, 40)),
                        StrokeThickness = 1, RadiusX = 1, RadiusY = 1
                    };
                    Canvas.SetLeft(gunBase2, gunX - cellSize * 0.1);
                    Canvas.SetTop(gunBase2, gun2Y - cellSize * 0.1);
                    Canvas.SetZIndex(gunBase2, 3);
                    canvas.Children.Add(gunBase2);

                    var gunBarrel2 = new Rectangle
                    {
                        Width = 2.5, Height = cellSize * 0.15,
                        Fill = darkHullColor
                    };
                    Canvas.SetLeft(gunBarrel2, gunX - 1.25);
                    Canvas.SetTop(gunBarrel2, gun2Y - cellSize * 0.2);
                    Canvas.SetZIndex(gunBarrel2, 4);
                    canvas.Children.Add(gunBarrel2);
                }

                // Radaranlagen und Details
                if (shipSize >= 5)
                {
                    // Radar-Schüssel mit Mast
                    double radarX = startX + xOffset + shipWidth * 0.32;
                    double radarY = startY + shipHeight * 0.58;
                    
                    var radarMast = new Rectangle
                    {
                        Width = cellSize * 0.08, Height = 1.5,
                        Fill = new SolidColorBrush(Color.FromRgb(120, 130, 140))
                    };
                    Canvas.SetLeft(radarMast, radarX + cellSize * 0.02);
                    Canvas.SetTop(radarMast, radarY);
                    Canvas.SetZIndex(radarMast, 5);
                    canvas.Children.Add(radarMast);

                    var radar = new Ellipse
                    {
                        Width = cellSize * 0.13, Height = cellSize * 0.16,
                        Fill = new LinearGradientBrush(Color.FromRgb(80, 90, 100), Color.FromRgb(50, 60, 70), 135),
                        Stroke = new SolidColorBrush(Color.FromRgb(30, 40, 50)),
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(radar, radarX);
                    Canvas.SetTop(radar, radarY);
                    Canvas.SetZIndex(radar, 6);
                    canvas.Children.Add(radar);

                    // Radar-Scan-Linie
                    var radarLine = new Line
                    {
                        X1 = radarX + cellSize * 0.065, Y1 = radarY + cellSize * 0.08,
                        X2 = radarX + cellSize * 0.03, Y2 = radarY + cellSize * 0.14,
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 220, 100)),
                        StrokeThickness = 1.5
                    };
                    Canvas.SetZIndex(radarLine, 7);
                    canvas.Children.Add(radarLine);

                    // Radar-Glow
                    var radarGlow = new Ellipse
                    {
                        Width = 2, Height = 2,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(150, 100, 255, 100),
                            Color.FromArgb(0, 100, 255, 100))
                    };
                    Canvas.SetLeft(radarGlow, radarX + cellSize * 0.025);
                    Canvas.SetTop(radarGlow, radarY + cellSize * 0.13);
                    Canvas.SetZIndex(radarGlow, 8);
                    canvas.Children.Add(radarGlow);

                    // Deck-Beleuchtung (Lauflichter) für vertikale Schiffe
                    for (int deckLight = 0; deckLight < 3; deckLight++)
                    {
                        var runningLight = new System.Windows.Shapes.Ellipse
                        {
                            Width = 1.5, Height = 1.5,
                            Fill = new RadialGradientBrush(
                                Color.FromArgb(180, 100, 255, 100),
                                Color.FromArgb(60, 50, 200, 50))
                        };
                        Canvas.SetLeft(runningLight, startX + xOffset + shipWidth * 0.15);
                        Canvas.SetTop(runningLight, startY + shipHeight * (0.3 + deckLight * 0.2));
                        Canvas.SetZIndex(runningLight, 7);
                        canvas.Children.Add(runningLight);
                    }

                    // Atmosphärischer Nebel um das Schiff
                    var atmosphericFog = new System.Windows.Shapes.Ellipse
                    {
                        Width = shipWidth * 1.4,
                        Height = shipHeight * 1.1,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(0, 200, 220, 240),
                            Color.FromArgb(25, 180, 200, 220))
                    };
                    Canvas.SetLeft(atmosphericFog, startX + xOffset - shipWidth * 0.2);
                    Canvas.SetTop(atmosphericFog, startY - shipHeight * 0.05);
                    Canvas.SetZIndex(atmosphericFog, 0);
                    canvas.Children.Add(atmosphericFog);

                    // Flaggenmast am Heck
                    var flagpole = new Rectangle
                    {
                        Width = 8, Height = 1,
                        Fill = new SolidColorBrush(Color.FromRgb(120, 130, 140))
                    };
                    Canvas.SetLeft(flagpole, startX + xOffset + shipWidth * 0.35);
                    Canvas.SetTop(flagpole, startY + shipHeight - 8);
                    Canvas.SetZIndex(flagpole, 5);
                    canvas.Children.Add(flagpole);

                    // Wehende Flagge
                    var flag = new System.Windows.Shapes.Polygon
                    {
                        Fill = accentColor,
                        Stroke = new SolidColorBrush(Color.FromRgb(150, 60, 40)),
                        StrokeThickness = 0.5
                    };
                    flag.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth * 0.35, startY + shipHeight - 7));
                    flag.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth * 0.37, startY + shipHeight - 3));
                    flag.Points.Add(new System.Windows.Point(startX + xOffset + shipWidth * 0.39, startY + shipHeight - 7));
                    Canvas.SetZIndex(flag, 6);
                    canvas.Children.Add(flag);
                }
            }
        }

        private void DrawMiniWarship(Canvas canvas, A3_LEA_Ship ship, double startX, double startY, double cellSize, bool horizontal, bool isPlaced)
        {
            // Kleinere Version für die Auswahlleiste
            var hullColor = new SolidColorBrush(isPlaced ? Color.FromRgb(50, 50, 60) : Color.FromRgb(70, 70, 80));
            var deckColor = new SolidColorBrush(isPlaced ? Color.FromRgb(60, 60, 70) : Color.FromRgb(90, 90, 100));
            
            int shipSize = ship.Size;
            double shipWidth = cellSize * shipSize;
            double shipHeight = cellSize * 0.7;

            // Einfacher Rumpf für Mini-Version
            var hull = new System.Windows.Shapes.Polygon
            {
                Fill = hullColor,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5
            };
            
            // Bug spitz, Heck abgerundet
            hull.Points.Add(new System.Windows.Point(startX + 1, startY + shipHeight / 2));
            hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.3, startY + 1));
            hull.Points.Add(new System.Windows.Point(startX + shipWidth - cellSize * 0.2, startY + 1));
            hull.Points.Add(new System.Windows.Point(startX + shipWidth - 1, startY + shipHeight * 0.4));
            hull.Points.Add(new System.Windows.Point(startX + shipWidth - 1, startY + shipHeight * 0.6));
            hull.Points.Add(new System.Windows.Point(startX + shipWidth - cellSize * 0.2, startY + shipHeight - 1));
            hull.Points.Add(new System.Windows.Point(startX + cellSize * 0.3, startY + shipHeight - 1));
            
            canvas.Children.Add(hull);

            // Mini-Details nur für größere Schiffe
            if (shipSize >= 3)
            {
                // Kleine Brücke
                var bridge = new Rectangle
                {
                    Width = cellSize * 0.25,
                    Height = shipHeight * 0.3,
                    Fill = deckColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.3
                };
                Canvas.SetLeft(bridge, startX + shipWidth * 0.5 - cellSize * 0.125);
                Canvas.SetTop(bridge, startY + shipHeight * 0.25);
                canvas.Children.Add(bridge);
            }

            if (shipSize >= 4)
            {
                // Mini-Geschütztürme
                var gun1 = new Ellipse
                {
                    Width = cellSize * 0.15,
                    Height = cellSize * 0.15,
                    Fill = Brushes.DarkSlateGray,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.3
                };
                Canvas.SetLeft(gun1, startX + shipWidth * 0.25 - cellSize * 0.075);
                Canvas.SetTop(gun1, startY + shipHeight * 0.35);
                canvas.Children.Add(gun1);

                var gun2 = new Ellipse
                {
                    Width = cellSize * 0.15,
                    Height = cellSize * 0.15,
                    Fill = Brushes.DarkSlateGray,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.3
                };
                Canvas.SetLeft(gun2, startX + shipWidth * 0.75 - cellSize * 0.075);
                Canvas.SetTop(gun2, startY + shipHeight * 0.35);
                canvas.Children.Add(gun2);
            }
        }
    }

    // Human Player
    public class A3_LEA_HumanSchiffePlayer : A3_LEA_BaseHumanSchiffePlayer, IHumanGamePlayerWithMouse
    {
        private int _playerNumber = 1;
        public override string Name => "A3 LEA Schiffe Human";
        public override int PlayerNumber => _playerNumber;
        public override void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;
        public override IGamePlayer Clone() => new A3_LEA_HumanSchiffePlayer();

        public void OnMouseMoved(System.Windows.Input.MouseEventArgs e)
        {
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
            if (rules != null && rules.IsSetupPhase && rules.SelectedShip != null)
            {
                var mousePos = e.GetPosition(null);
                int gridX = (int)((mousePos.X - 30) / 40);
                int gridY = (int)((mousePos.Y - 30) / 40);
                rules.MouseX = gridX;
                rules.MouseY = gridY;
            }

            // Mausrad-Rotation
            if (e is System.Windows.Input.MouseWheelEventArgs wheelEvent && rules?.IsSetupPhase == true && rules?.SelectedShip != null)
            {
                var ship = rules.SelectedShip;
                if (ship != null)
                {
                    ship.IsHorizontal = !ship.IsHorizontal;
                }
            }
        }

        public override IA3_LEA_SchiffeMove GetMove(IMoveSelection selection, IA3_LEA_SchiffeField field)
        {
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
            
            if (selection is IClickSelection click)
            {
                // Common layout constants
                const double baseOffset = 20;
                const double cellSize = 40;
                const double shipStep = 80;
                double shipPreviewY = baseOffset + field.Height * cellSize + 50;

                // If in setup phase (player 1 or player 2)
                if (rules != null && rules.IsSetupPhase)
                {
                    // determine active player and lists
                    var activePlayer = rules.CurrentSetupPlayer;
                    var shipsList = activePlayer == 1 ? rules.Ships : rules.Ships2;

                    // Start/Next button region
                    bool canProceed = (rules.Phase == 1 && rules.AllShipsPlaced1) || (rules.Phase == 2 && rules.AllShipsPlaced2);
                    if (canProceed)
                    {
                        double buttonX = baseOffset + shipsList.Count * shipStep + 20;
                        double buttonY = shipPreviewY - 5;
                        double buttonWidth = 120;
                        double buttonHeight = 40;
                        if (click.XClickPos >= buttonX && click.XClickPos <= buttonX + buttonWidth && click.YClickPos >= buttonY && click.YClickPos <= buttonY + buttonHeight)
                        {
                            // advance phase
                            if (rules.Phase == 1) rules.Phase = 2;
                            else if (rules.Phase == 2) rules.Phase = 3;
                            // clear selection
                            rules.SelectedShip = null;
                            return null;
                        }
                    }

                    // Click on selection bar (under field)
                    if (click.YClickPos > shipPreviewY - 5 && click.YClickPos < shipPreviewY + 30)
                    {
                        double shipPreviewX = baseOffset;
                        int shipClicked = (int)((click.XClickPos - shipPreviewX) / shipStep);
                        if (shipClicked >= 0 && shipClicked < shipsList.Count)
                        {
                            var ship = shipsList[shipClicked];
                            if (ship.X == 0 && ship.Y == 0)
                            {
                                rules.SelectedShip = ship;
                            }
                        }
                        return null;
                    }

                    // Click on field to place: allow placement for the active setup player
                    int gridX = (int)((click.XClickPos - baseOffset) / cellSize);
                    int gridY = (int)((click.YClickPos - baseOffset) / cellSize);
                    if (rules.Phase == 1 || rules.Phase == 2)
                    {
                        var targetField = activePlayer == 1 ? rules.SchiffeField : rules.SchiffeField2;
                        if (targetField.IsValidPosition(gridX, gridY))
                        {
                            // Return a move that targets the active setup player (player who should receive the placement)
                            return new A3_LEA_SchiffeMove(gridX, gridY, activePlayer);
                        }
                    }
                    return null;
                }

                // Playing phase: clicks are shots on opponent's field (fields are stacked vertically)
                if (rules != null && !rules.IsSetupPhase)
                {
                    double smallCell = 24;
                    double topBaseY = 20;
                    double bottomBaseY = 20 + field.Height * smallCell + 40;
                    // Toggle button for showing/hiding ships in Phase3
                    double shipsButtonX = baseOffset + field.Width * smallCell + 20;
                    double shipsButtonY = topBaseY;
                    double shipsButtonW = 120;
                    double shipsButtonH = 28;
                    if (click.XClickPos >= shipsButtonX && click.XClickPos <= shipsButtonX + shipsButtonW && click.YClickPos >= shipsButtonY && click.YClickPos <= shipsButtonY + shipsButtonH)
                    {
                        rules.ShowShipsPhase3 = !rules.ShowShipsPhase3;
                        return null;
                    }
                    // Player 1 shoots on bottom (player2 field), Player2 shoots on top (player1 field)
                    if (_playerNumber == 1)
                    {
                        if (click.YClickPos >= bottomBaseY && click.YClickPos < bottomBaseY + field.Height * smallCell)
                        {
                            int gx = (int)((click.XClickPos - baseOffset) / smallCell);
                            int gy = (int)((click.YClickPos - bottomBaseY) / smallCell);
                            if (rules.SchiffeField2.IsValidPosition(gx, gy))
                            {
                                // don't allow shooting the same cell twice
                                if (rules.Shots2.Contains((gx, gy))) return null;
                                return new A3_LEA_SchiffeMove(gx, gy, _playerNumber);
                            }
                        }
                    }
                    else // player 2
                    {
                        if (click.YClickPos >= topBaseY && click.YClickPos < topBaseY + field.Height * smallCell)
                        {
                            int gx = (int)((click.XClickPos - baseOffset) / smallCell);
                            int gy = (int)((click.YClickPos - topBaseY) / smallCell);
                            if (rules.SchiffeField.IsValidPosition(gx, gy))
                            {
                                if (rules.Shots.Contains((gx, gy))) return null;
                                return new A3_LEA_SchiffeMove(gx, gy, _playerNumber);
                            }
                        }
                    }
                    return null;
                }
            }
            else if (selection is IKeySelection keySelection)
            {
                if (rules != null && rules.IsSetupPhase && rules.SelectedShip != null)
                {
                    // R-Key: Rotation
                    if (keySelection.Key == System.Windows.Input.Key.R)
                    {
                        rules.SelectedShip.IsHorizontal = !rules.SelectedShip.IsHorizontal;
                    }
                }
            }
            return null;
        }
    }

    // Move-Klasse
    public class A3_LEA_SchiffeMove : IA3_LEA_SchiffeMove
    {
        public int X { get; }
        public int Y { get; }
        public int PlayerNumber { get; }
        public MoveType MoveType => MoveType.click;

        public A3_LEA_SchiffeMove(int x, int y, int playerNumber)
        {
            X = x;
            Y = y;
            PlayerNumber = playerNumber;
        }

        // Simple computer player for Schiffe versenken (placed here like A3 TicTacToe)
        public class A3_LEA_ComputerSchiffePlayer : IComputerGamePlayer
        {
            private int _playerNumber = 0;
            private Random _rand = new Random();
            private Queue<(int x, int y)> _targetQueue = new Queue<(int x, int y)>();

            public string Name => "A3 LEA Computer (Random Hunt)";
            public int PlayerNumber => _playerNumber;

            public void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;

            public bool CanBeRuledBy(IGameRules rules) => rules is A3_LEA_SchiffeRules;

            public IGamePlayer Clone() => new A3_LEA_ComputerSchiffePlayer();

            public IPlayMove GetMove(IGameField field)
            {
                var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
                if (rules == null) return null;

                // SETUP phase: place all ships for this computer when it's its setup turn
                if (rules.IsSetupPhase)
                {
                    if (rules.CurrentSetupPlayer != _playerNumber) return null;

                    var shipsList = rules.CurrentSetupPlayer == 1 ? rules.Ships : rules.Ships2;
                    var targetField = rules.CurrentSetupPlayer == 1 ? rules.SchiffeField : rules.SchiffeField2;

                    var ship = shipsList.FirstOrDefault(s => s.X == 0 && s.Y == 0);
                    if (ship == null) return null;

                    // try random placements
                    ship.IsHorizontal = _rand.Next(2) == 0;
                    for (int tries = 0; tries < 1000; tries++)
                    {
                        int gx = _rand.Next(0, targetField.Width);
                        int gy = _rand.Next(0, targetField.Height);
                        if (rules.CanPlaceShip(ship, gx, gy, ship.IsHorizontal))
                            return new A3_LEA_SchiffeMove(gx, gy, _playerNumber);
                        if (tries % 8 == 0) ship.IsHorizontal = !ship.IsHorizontal;
                    }

                    // deterministic fallback
                    for (int y = 0; y < targetField.Height; y++)
                        for (int x = 0; x < targetField.Width; x++)
                            if (rules.CanPlaceShip(ship, x, y, ship.IsHorizontal))
                                return new A3_LEA_SchiffeMove(x, y, _playerNumber);

                    return null;
                }

                // PLAYING phase: shooting logic
                List<A3_LEA_Ship> oppShips = _playerNumber == 1 ? rules.Ships2 : rules.Ships;
                var oppShots = _playerNumber == 1 ? rules.Shots2 : rules.Shots;
                var oppField = _playerNumber == 1 ? rules.SchiffeField2 : rules.SchiffeField;

                // Hunt: enqueue neighbors of partially hit ships
                foreach (var s in oppShips)
                {
                    if (s.Hits > 0 && s.Hits < s.Size)
                    {
                        for (int i = 0; i < s.Size; i++)
                        {
                            if (!s.HitCells[i]) continue;
                            int sx = s.IsHorizontal ? s.X + i : s.X;
                            int sy = s.IsHorizontal ? s.Y : s.Y + i;
                            var neighbors = new (int x, int y)[] { (sx+1,sy),(sx-1,sy),(sx,sy+1),(sx,sy-1) };
                            foreach (var n in neighbors)
                            {
                                if (!oppField.IsValidPosition(n.x, n.y)) continue;
                                if (oppShots.Contains((n.x, n.y))) continue;
                                if (!_targetQueue.Contains(n)) _targetQueue.Enqueue(n);
                            }
                        }
                    }
                }

                while (_targetQueue.Count > 0)
                {
                    var t = _targetQueue.Dequeue();
                    if (!oppShots.Contains((t.x, t.y)) && oppField.IsValidPosition(t.x, t.y))
                        return new A3_LEA_SchiffeMove(t.x, t.y, _playerNumber);
                }

                // random shot on an unshot cell
                var free = new List<(int x, int y)>();
                for (int y = 0; y < oppField.Height; y++)
                    for (int x = 0; x < oppField.Width; x++)
                        if (!oppShots.Contains((x, y))) free.Add((x, y));

                if (free.Count == 0) return null;
                var pick = free[_rand.Next(free.Count)];
                return new A3_LEA_SchiffeMove(pick.x, pick.y, _playerNumber);
            }
        }
    }
}
