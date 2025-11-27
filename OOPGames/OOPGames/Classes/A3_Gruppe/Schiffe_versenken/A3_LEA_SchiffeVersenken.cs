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
        
        // Animation für versenkte Schiffe
        public Dictionary<int, double> SunkShipAnimationTime = new Dictionary<int, double>();
        public Dictionary<int, System.DateTime> SunkShipTimestamp = new Dictionary<int, System.DateTime>();

        // Phase: 1 = Player1 setup, 2 = Player2 setup, 3 = Playing
        private int _phase = 1;
        public int Phase { get { return _phase; } set { _phase = value; } }
        public bool IsSetupPhase => Phase == 1 || Phase == 2;
        public int CurrentSetupPlayer => Phase == 1 ? 1 : (Phase == 2 ? 2 : 0);

        public int PlacedShipsCount1 => _ships.Count(s => s.X >= 0 && s.Y >= 0);
        public int PlacedShipsCount2 => _ships2.Count(s => s.X >= 0 && s.Y >= 0);
        public bool AllShipsPlaced1 => PlacedShipsCount1 == _ships.Count;
        public bool AllShipsPlaced2 => PlacedShipsCount2 == _ships2.Count;

        // Separate visibility control for each player's ships in Phase 3
        public bool ShowShipsPlayer1 { get; set; } = false;
        public bool ShowShipsPlayer2 { get; set; } = false;
        // Whether the last move grants an extra turn (e.g., hit in Battleship)
        public bool LastMoveGivesExtraTurn { get; private set; } = false;

        // Track current player during Phase 3 (playing phase)
        public int CurrentPlayerNumber { get; set; } = 1;

        // Gewinner-Animation
        public int WinnerPlayer { get; private set; } = 0;
        public System.DateTime? WinnerTimestamp { get; private set; } = null;
        public double WinnerAnimationTime => WinnerTimestamp.HasValue ? (System.DateTime.Now - WinnerTimestamp.Value).TotalSeconds : 0;

        // Maus-Tracking (wie IQ-Puzzle)
        public int MouseX { get; set; } = -1;
        public int MouseY { get; set; } = -1;
        
        // Manuell ausgewähltes Schiff (überschreibt automatische Auswahl)
        public A3_LEA_Ship ManuallySelectedShip { get; set; } = null;
        
        // Hilfsmethode: Gibt das aktuelle Schiff zurück (manuell ausgewählt oder nächstes unplatziertes)
        public A3_LEA_Ship GetCurrentShip(List<A3_LEA_Ship> shipsList)
        {
            // Wenn manuell ein Schiff ausgewählt wurde (auch platzierte), verwende es
            if (ManuallySelectedShip != null && shipsList.Contains(ManuallySelectedShip))
            {
                return ManuallySelectedShip;
            }
            // Ansonsten: nächstes unplatziertes Schiff
            return shipsList.FirstOrDefault(s => s.X < 0 && s.Y < 0);
        }

        public A3_LEA_SchiffeRules()
        {
            // Spieler 1 (IDs 1-5) - Initialisiere mit -1 für 'nicht platziert'
            _ships.Add(new A3_LEA_Ship(1, 4) { X = -1, Y = -1 });
            // add 5-cell ship
            _ships.Add(new A3_LEA_Ship(2, 5) { X = -1, Y = -1 });
            _ships.Add(new A3_LEA_Ship(3, 3) { X = -1, Y = -1 });
            _ships.Add(new A3_LEA_Ship(4, 3) { X = -1, Y = -1 });
            _ships.Add(new A3_LEA_Ship(5, 2) { X = -1, Y = -1 });
            // Spieler 2 (IDs 101-105 für eindeutige Animation-Keys)
            _ships2.Add(new A3_LEA_Ship(101, 4) { X = -1, Y = -1 });
            // add 5-cell ship for player 2
            _ships2.Add(new A3_LEA_Ship(102, 5) { X = -1, Y = -1 });
            _ships2.Add(new A3_LEA_Ship(103, 3) { X = -1, Y = -1 });
            _ships2.Add(new A3_LEA_Ship(104, 3) { X = -1, Y = -1 });
            _ships2.Add(new A3_LEA_Ship(105, 2) { X = -1, Y = -1 });
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

        public void RemoveShipFromField(A3_LEA_Ship ship)
        {
            if (ship.X < 0 || ship.Y < 0) return; // Schiff ist nicht platziert
            var targetField = _ships.Contains(ship) ? _field : _field2;
            for (int i = 0; i < ship.Size; i++)
            {
                if (ship.IsHorizontal)
                    targetField[ship.X + i, ship.Y] = 0;
                else
                    targetField[ship.X, ship.Y + i] = 0;
            }
            ship.X = -1;
            ship.Y = -1;
        }

        public override void PlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal)
        {
            if (!CanPlaceShip(ship, x, y, horizontal)) return;
            // Entferne Schiff zuerst vom alten Platz falls es schon platziert war
            RemoveShipFromField(ship);
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
                    
                    // Wenn Schiff versenkt wurde, starte Animation
                    if (ship.Hits == ship.Size && !SunkShipTimestamp.ContainsKey(ship.Id))
                    {
                        SunkShipTimestamp[ship.Id] = System.DateTime.Now;
                        SunkShipAnimationTime[ship.Id] = 0;
                    }
                }
                return true;
            }
            return false;
        }

        public override int CheckIfPLayerWon()
        {
            // Wenn alle Schiffe von Spieler1 versenkt sind -> Spieler2 gewinnt (return 2)
            if (_ships.All(s => s.Hits == s.Size))
            {
                if (WinnerPlayer == 0)
                {
                    WinnerPlayer = 2;
                    WinnerTimestamp = System.DateTime.Now;
                }
                return 2;
            }
            // Wenn alle Schiffe von Spieler2 versenkt sind -> Spieler1 gewinnt (return 1)
            if (_ships2.All(s => s.Hits == s.Size))
            {
                if (WinnerPlayer == 0)
                {
                    WinnerPlayer = 1;
                    WinnerTimestamp = System.DateTime.Now;
                }
                return 1;
            }
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
                ship.X = -1;
                ship.Y = -1;
                ship.IsHorizontal = true;
                ship.HitCells = new bool[ship.Size];
            }
            foreach (var ship in _ships2)
            {
                ship.X = -1;
                ship.Y = -1;
                ship.IsHorizontal = true;
                ship.HitCells = new bool[ship.Size];
            }
            Phase = 1;
            MouseX = -1;
            MouseY = -1;
            SunkShipAnimationTime.Clear();
            SunkShipTimestamp.Clear();
            WinnerPlayer = 0;
            WinnerTimestamp = null;
            CurrentPlayerNumber = 1;
        }

        // IGameRules2 Implementation
        public void StartedGameCall()
        {
            ClearField();
        }

        public void TickGameCall()
        {
            // Update Animation für versenkte Schiffe
            var keysToUpdate = SunkShipAnimationTime.Keys.ToList();
            foreach (var shipId in keysToUpdate)
            {
                if (SunkShipTimestamp.ContainsKey(shipId))
                {
                    var elapsed = (System.DateTime.Now - SunkShipTimestamp[shipId]).TotalSeconds;
                    SunkShipAnimationTime[shipId] = elapsed;
                }
            }
        }

        public override void DoMove(IPlayMove move)
        {
            if (move is IA3_LEA_SchiffeMove m)
            {
                // reset extra-turn flag by default; rules will set it for hits
                LastMoveGivesExtraTurn = false;

                if (IsSetupPhase)
                {
                    // Only accept moves from the player who is currently in the setup phase
                    if (m.PlayerNumber != CurrentSetupPlayer)
                    {
                        // ignore moves from the wrong player during setup
                        return;
                    }

                    // Special 'advance phase' move triggered by Next-button in the UI
                    if (m.X == -1 && m.Y == -1)
                    {
                        if (Phase == 1) Phase = 2;
                        else if (Phase == 2) Phase = 3;
                        // ensure MainWindow will switch the active player after this move
                        LastMoveGivesExtraTurn = false;
                        // reset mouse & manual selection
                        ManuallySelectedShip = null;
                        MouseX = -1; MouseY = -1;
                        return;
                    }

                    // Setup-Phase: Aktuelles Schiff platzieren (manuell ausgewählt oder nächstes)
                    var shipsList = CurrentSetupPlayer == 1 ? _ships : _ships2;
                    var currentShip = GetCurrentShip(shipsList);
                    if (currentShip != null)
                    {
                        bool horizontal = currentShip.IsHorizontal;
                        if (CanPlaceShip(currentShip, m.X, m.Y, horizontal))
                        {
                            PlaceShip(currentShip, m.X, m.Y, horizontal);
                            // Zurücksetzen der manuellen Auswahl nach Platzierung
                            ManuallySelectedShip = null;
                            // Keep the same player during setup after a successful placement
                            LastMoveGivesExtraTurn = true;
                        }
                    }
                    MouseX = -1;
                    MouseY = -1;
                }
                else
                {
                    // Playing-Phase: Schießen (auf das gegnerische Feld)
                    // Beim allerersten Schuss: Setze CurrentPlayerNumber auf den aktiven Spieler
                    if (_shots.Count == 0 && _shots2.Count == 0)
                    {
                        CurrentPlayerNumber = m.PlayerNumber;
                    }
                    
                    bool hit = ShootAtForPlayer(m.PlayerNumber, m.X, m.Y);
                    // grant extra turn on hit
                    LastMoveGivesExtraTurn = hit;
                    
                    // Wechsle sofort zum anderen Spieler bei Fehlschuss
                    if (!hit)
                    {
                        CurrentPlayerNumber = m.PlayerNumber == 1 ? 2 : 1;
                    }
                    else
                    {
                        // Bei Treffer bleibt der aktuelle Spieler
                        CurrentPlayerNumber = m.PlayerNumber;
                    }
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
            canvas.Background = new SolidColorBrush(Colors.White);
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

                // Zeichne realistischen Wasser-Hintergrund für Spielfeld
                var waterRect = new Rectangle 
                { 
                    Width = targetField.Width * cellSize, 
                    Height = targetField.Height * cellSize,
                    Fill = new LinearGradientBrush(
                        Color.FromRgb(65, 105, 225),  // Royal Blue
                        Color.FromRgb(30, 144, 255),  // Dodger Blue
                        90)
                };
                Canvas.SetLeft(waterRect, baseX);
                Canvas.SetTop(waterRect, baseY);
                Canvas.SetZIndex(waterRect, 0);
                canvas.Children.Add(waterRect);

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
                    if (ship.X >= 0 && ship.Y >= 0)
                    {
                        DrawWarship(canvas, ship, baseX, baseY, cellSize, ship.IsHorizontal);
                    }
                }

                // preview: Zeige das aktuelle Schiff (manuell ausgewählt oder nächstes)
                var nextShip = rules.GetCurrentShip(shipsList);
                if (nextShip != null && rules.MouseX >= 0 && rules.MouseY >= 0)
                {
                    var ship = nextShip;
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
                double shipPreviewY = baseY + targetField.Height * cellSize + 80;
                double shipPreviewX = baseX;
                const double shipCellSize = 12;
                const double shipStep = 80;
                var shipLabel = new TextBlock { Text = "Verfügbare Schiffe:", FontSize = 12, FontWeight = System.Windows.FontWeights.Bold };
                Canvas.SetLeft(shipLabel, shipPreviewX);
                Canvas.SetTop(shipLabel, shipPreviewY - 25);
                canvas.Children.Add(shipLabel);
                var currentShip = rules.GetCurrentShip(shipsList);
                for (int idx = 0; idx < shipsList.Count; idx++)
                {
                    var s = shipsList[idx];
                    double x = shipPreviewX + idx * shipStep;
                    bool isPlaced = s.X >= 0 && s.Y >= 0;
                    bool isCurrent = s == currentShip;
                    
                    // Grüne Umrandung für platzierte Schiffe
                    if (isPlaced)
                    {
                        var border = new Rectangle { Width = s.Size * shipCellSize + 6, Height = shipCellSize + 6, Stroke = Brushes.LimeGreen, StrokeThickness = 3, RadiusX = 3, RadiusY = 3 };
                        Canvas.SetLeft(border, x - 3);
                        Canvas.SetTop(border, shipPreviewY - 3);
                        canvas.Children.Add(border);
                    }
                    // Goldene Umrandung für das aktuelle (nächste) Schiff
                    else if (isCurrent)
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
                    var buttonText = new TextBlock { Text = "Weiter", FontSize = 14, FontWeight = System.Windows.FontWeights.Bold, Foreground = Brushes.White };
                    Canvas.SetLeft(buttonText, buttonX + 30); Canvas.SetTop(buttonText, buttonY + 10); canvas.Children.Add(buttonText);
                }

                var nextShipInfo = rules.GetCurrentShip(shipsList);
                string labelText = nextShipInfo != null 
                    ? $"Spieler {rules.CurrentSetupPlayer}: Platziere Schiff ({nextShipInfo.Size} Felder) - R: Drehen" 
                    : $"Spieler {rules.CurrentSetupPlayer}: Alle Schiffe platziert";
                var label = new TextBlock { Text = labelText, FontSize = 13, Foreground = Brushes.Black };
                Canvas.SetLeft(label, baseX);
                Canvas.SetTop(label, baseY + targetField.Height * cellSize + 30);
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
            
            // Wasser-Hintergrund für Player 1 Feld
            var water1 = new Rectangle 
            { 
                Width = f1.Width * smallCell, 
                Height = f1.Height * smallCell,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(65, 105, 225),
                    Color.FromRgb(30, 144, 255),
                    90)
            };
            Canvas.SetLeft(water1, topBaseX);
            Canvas.SetTop(water1, topBaseY);
            Canvas.SetZIndex(water1, 0);
            canvas.Children.Add(water1);
            
            // ULTRA-REALISTISCHES WASSER für Player 1
            double time1 = System.DateTime.Now.TimeOfDay.TotalSeconds;
            
            // Tiefenschichten
            for (int layer = 0; layer < 2; layer++)
            {
                var depth = new Rectangle
                {
                    Width = f1.Width * smallCell,
                    Height = f1.Height * smallCell,
                    Fill = new LinearGradientBrush(
                        Color.FromArgb((byte)(20 + layer * 10), 0, 0, 80),
                        Color.FromArgb((byte)(25 + layer * 12), 0, 40, 120),
                        45 + layer * 45)
                };
                Canvas.SetLeft(depth, topBaseX);
                Canvas.SetTop(depth, topBaseY);
                Canvas.SetZIndex(depth, 1 + layer);
                canvas.Children.Add(depth);
            }
            
            // Wellen-Schattierungen (identisch zu Setup)
            for (int y = 0; y < f1.Height; y++)
            {
                for (int x = 0; x < f1.Width; x++)
                {
                    double wavePattern = System.Math.Sin(x * 0.8 + y * 0.6 + time1 * 0.3) * 0.5 + 0.5;
                    if (wavePattern > 0.6)
                    {
                        var waveShade = new Rectangle
                        {
                            Width = smallCell,
                            Height = smallCell,
                            Fill = new SolidColorBrush(Color.FromArgb((byte)(wavePattern * 15), 0, 50, 100))
                        };
                        Canvas.SetLeft(waveShade, topBaseX + x * smallCell);
                        Canvas.SetTop(waveShade, topBaseY + y * smallCell);
                        Canvas.SetZIndex(waveShade, 5);
                        canvas.Children.Add(waveShade);
                    }
                }
            }
            
            // Kaustik-Lichtmuster (identisch zu Setup)
            for (int c = 0; c < 100; c++)
            {
                double causticPhase = (time1 * 0.3 + c * 0.3) % 3.0;
                int cx = c % 10;
                int cy = c / 10;
                double offsetX = System.Math.Sin(causticPhase * 2) * smallCell * 0.4;
                double offsetY = System.Math.Cos(causticPhase * 2) * smallCell * 0.4;
                
                var causticPath = new System.Windows.Shapes.Path
                {
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection
                        {
                            new PathFigure
                            {
                                StartPoint = new System.Windows.Point(cx * smallCell + offsetX, cy * smallCell + offsetY),
                                Segments = new PathSegmentCollection
                                {
                                    new BezierSegment
                                    {
                                        Point1 = new System.Windows.Point(cx * smallCell + smallCell * 0.3 + offsetX, cy * smallCell + smallCell * 0.2 + offsetY),
                                        Point2 = new System.Windows.Point(cx * smallCell + smallCell * 0.6 + offsetX, cy * smallCell + smallCell * 0.5 + offsetY),
                                        Point3 = new System.Windows.Point(cx * smallCell + smallCell * 0.8 + offsetX, cy * smallCell + smallCell * 0.3 + offsetY)
                                    }
                                }
                            }
                        }
                    },
                    Stroke = new LinearGradientBrush(
                        Color.FromArgb(50, 255, 255, 200),
                        Color.FromArgb(10, 200, 255, 255),
                        0),
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(causticPath, topBaseX);
                Canvas.SetTop(causticPath, topBaseY);
                Canvas.SetZIndex(causticPath, 8);
                canvas.Children.Add(causticPath);
            }
            
            // Strömungswirbel (identisch zu Setup - 10x10 Raster)
            for (int w = 0; w < 100; w++)
            {
                double whirlPhase = (time1 * 0.15 + w * 0.1) % 4.0;
                int wx = w % 10;
                int wy = w / 10;
                double whirlRadius = smallCell * (0.8 + System.Math.Sin(whirlPhase) * 0.3);
                
                var whirl = new Ellipse
                {
                    Width = whirlRadius,
                    Height = whirlRadius,
                    Stroke = new SolidColorBrush(Color.FromArgb(20, 100, 180, 255)),
                    StrokeThickness = 1.5,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(0, 0, 0, 0),
                        Color.FromArgb(10, 50, 120, 200))
                };
                Canvas.SetLeft(whirl, topBaseX + wx * smallCell + smallCell * 0.5 - whirlRadius / 2);
                Canvas.SetTop(whirl, topBaseY + wy * smallCell + smallCell * 0.5 - whirlRadius / 2);
                Canvas.SetZIndex(whirl, 6);
                canvas.Children.Add(whirl);
            }
            
            for (int x = 0; x <= f1.Width; x++) canvas.Children.Add(new Line { X1 = topBaseX + x * smallCell, Y1 = topBaseY, X2 = topBaseX + x * smallCell, Y2 = topBaseY + f1.Height * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            for (int y = 0; y <= f1.Height; y++) canvas.Children.Add(new Line { X1 = topBaseX, Y1 = topBaseY + y * smallCell, X2 = topBaseX + f1.Width * smallCell, Y2 = topBaseY + y * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            var title1 = new TextBlock { Text = "Spielfeld Spieler 1", FontWeight = System.Windows.FontWeights.Bold }; Canvas.SetLeft(title1, topBaseX); Canvas.SetTop(title1, topBaseY - 18); canvas.Children.Add(title1);
            
            // Orange border around Player 1 field if it's their turn to shoot (Player 2 shoots at Player 1's field)
            // Nur anzeigen wenn mindestens ein Schuss abgegeben wurde
            if ((rules.Shots.Count > 0 || rules.Shots2.Count > 0) && rules.CurrentPlayerNumber == 2)
            {
                var orangeBorder1 = new Rectangle 
                { 
                    Width = f1.Width * smallCell, 
                    Height = f1.Height * smallCell,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 4
                };
                Canvas.SetLeft(orangeBorder1, topBaseX);
                Canvas.SetTop(orangeBorder1, topBaseY);
                Canvas.SetZIndex(orangeBorder1, 101);
                canvas.Children.Add(orangeBorder1);
            }
            // draw ships as warship models if visible for Player 1
            if (rules.ShowShipsPlayer1)
            {
                foreach (var s in rules.Ships)
                {
                    if (s.X >= 0 && s.Y >= 0 && s.Hits < s.Size) // Nur nicht-versenkte Schiffe
                    {
                        DrawWarship(canvas, s, topBaseX, topBaseY, smallCell, s.IsHorizontal);
                    }
                }
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

            // draw sunk ships for player1 with explosion animation
            foreach (var s in rules.Ships)
            {
                if (s.Hits == s.Size)
                {
                    double animTime = rules.SunkShipAnimationTime.ContainsKey(s.Id) ? rules.SunkShipAnimationTime[s.Id] : 999;
                    
                    // Phase 1 (0-1.5s): Feuerbälle auf getroffenen Feldern
                    if (animTime < 1.5)
                    {
                        for (int i = 0; i < s.Size; i++)
                        {
                            int sx = s.IsHorizontal ? s.X + i : s.X;
                            int sy = s.IsHorizontal ? s.Y : s.Y + i;
                            
                            // Feuerball-Animation mit mehreren Schichten
                            double explosionPhase = (animTime * 2) % 1.0; // Pulsiert
                            double baseSize = smallCell * 0.8;
                            double pulseSize = baseSize * (1.0 + explosionPhase * 0.3);
                            
                            // Äußerer Feuerring (Orange)
                            var outerFire = new Ellipse 
                            { 
                                Width = pulseSize, 
                                Height = pulseSize,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb((byte)(200 - explosionPhase * 100), 255, 100, 0),
                                    Color.FromArgb((byte)(100 - explosionPhase * 60), 255, 50, 0))
                            };
                            Canvas.SetLeft(outerFire, topBaseX + sx * smallCell + smallCell / 2 - pulseSize / 2);
                            Canvas.SetTop(outerFire, topBaseY + sy * smallCell + smallCell / 2 - pulseSize / 2);
                            Canvas.SetZIndex(outerFire, 95);
                            canvas.Children.Add(outerFire);
                            
                            // Innerer Kern (Gelb-Weiß)
                            var innerCore = new Ellipse 
                            { 
                                Width = pulseSize * 0.5, 
                                Height = pulseSize * 0.5,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb(255, 255, 255, 200),
                                    Color.FromArgb((byte)(220 - explosionPhase * 100), 255, 200, 0))
                            };
                            Canvas.SetLeft(innerCore, topBaseX + sx * smallCell + smallCell / 2 - pulseSize * 0.25);
                            Canvas.SetTop(innerCore, topBaseY + sy * smallCell + smallCell / 2 - pulseSize * 0.25);
                            Canvas.SetZIndex(innerCore, 96);
                            canvas.Children.Add(innerCore);
                            
                            // Rauch-Partikel
                            var smoke = new Ellipse 
                            { 
                                Width = smallCell * 0.6, 
                                Height = smallCell * 0.6,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb((byte)(60 + explosionPhase * 40), 50, 50, 50),
                                    Color.FromArgb(0, 30, 30, 30))
                            };
                            Canvas.SetLeft(smoke, topBaseX + sx * smallCell + smallCell / 2 - smallCell * 0.3);
                            Canvas.SetTop(smoke, topBaseY + sy * smallCell + smallCell / 2 - smallCell * 0.3 - animTime * 5);
                            Canvas.SetZIndex(smoke, 94);
                            canvas.Children.Add(smoke);
                        }
                    }
                    // Phase 2 (1.5s+): Zeige Kriegsschiff mit Fade-In
                    else if (animTime >= 1.5 && animTime < 2.5)
                    {
                        // Fade-In des Schiffes
                        double fadeAlpha = (animTime - 1.5) / 1.0; // 0 bis 1
                        fadeAlpha = System.Math.Min(1.0, fadeAlpha);
                        
                        DrawWarship(canvas, s, topBaseX, topBaseY, smallCell, s.IsHorizontal);
                        
                        // Weißer Fade-Overlay
                        if (fadeAlpha < 1.0)
                        {
                            for (int i = 0; i < s.Size; i++)
                            {
                                int sx = s.IsHorizontal ? s.X + i : s.X;
                                int sy = s.IsHorizontal ? s.Y : s.Y + i;
                                var fadeOverlay = new Rectangle 
                                { 
                                    Width = smallCell - 1, 
                                    Height = smallCell - 1, 
                                    Fill = new SolidColorBrush(Color.FromArgb((byte)((1.0 - fadeAlpha) * 180), 255, 255, 255))
                                };
                                Canvas.SetLeft(fadeOverlay, topBaseX + sx * smallCell + 1);
                                Canvas.SetTop(fadeOverlay, topBaseY + sy * smallCell + 1);
                                Canvas.SetZIndex(fadeOverlay, 98);
                                canvas.Children.Add(fadeOverlay);
                            }
                        }
                    }
                    // Phase 3 (2.5s+): Schiff vollständig sichtbar
                    else
                    {
                        DrawWarship(canvas, s, topBaseX, topBaseY, smallCell, s.IsHorizontal);
                    }
                }
            }

            // draw bottom (Player2)
            var f2 = rules.SchiffeField2;
            
            // Wasser-Hintergrund für Player 2 Feld
            var water2 = new Rectangle 
            { 
                Width = f2.Width * smallCell, 
                Height = f2.Height * smallCell,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(65, 105, 225),
                    Color.FromRgb(30, 144, 255),
                    90)
            };
            Canvas.SetLeft(water2, bottomBaseX);
            Canvas.SetTop(water2, bottomBaseY);
            Canvas.SetZIndex(water2, 0);
            canvas.Children.Add(water2);
            
            // ULTRA-REALISTISCHES WASSER für Player 2
            double time2 = System.DateTime.Now.TimeOfDay.TotalSeconds + 1.5; // Phasenverschiebung
            
            // Tiefenschichten
            for (int layer = 0; layer < 2; layer++)
            {
                var depth = new Rectangle
                {
                    Width = f2.Width * smallCell,
                    Height = f2.Height * smallCell,
                    Fill = new LinearGradientBrush(
                        Color.FromArgb((byte)(20 + layer * 10), 0, 0, 80),
                        Color.FromArgb((byte)(25 + layer * 12), 0, 40, 120),
                        45 + layer * 45)
                };
                Canvas.SetLeft(depth, bottomBaseX);
                Canvas.SetTop(depth, bottomBaseY);
                Canvas.SetZIndex(depth, 1 + layer);
                canvas.Children.Add(depth);
            }
            
            // Wellen-Schattierungen (identisch zu Setup)
            for (int y = 0; y < f2.Height; y++)
            {
                for (int x = 0; x < f2.Width; x++)
                {
                    double wavePattern = System.Math.Sin(x * 0.8 + y * 0.6 + time2 * 0.3) * 0.5 + 0.5;
                    if (wavePattern > 0.6)
                    {
                        var waveShade = new Rectangle
                        {
                            Width = smallCell,
                            Height = smallCell,
                            Fill = new SolidColorBrush(Color.FromArgb((byte)(wavePattern * 15), 0, 50, 100))
                        };
                        Canvas.SetLeft(waveShade, bottomBaseX + x * smallCell);
                        Canvas.SetTop(waveShade, bottomBaseY + y * smallCell);
                        Canvas.SetZIndex(waveShade, 5);
                        canvas.Children.Add(waveShade);
                    }
                }
            }
            
            // Kaustik-Lichtmuster (identisch zu Setup)
            for (int c = 0; c < 100; c++)
            {
                double causticPhase = (time2 * 0.3 + c * 0.3) % 3.0;
                int cx = c % 10;
                int cy = c / 10;
                double offsetX = System.Math.Sin(causticPhase * 2) * smallCell * 0.4;
                double offsetY = System.Math.Cos(causticPhase * 2) * smallCell * 0.4;
                
                var causticPath = new System.Windows.Shapes.Path
                {
                    Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection
                        {
                            new PathFigure
                            {
                                StartPoint = new System.Windows.Point(cx * smallCell + offsetX, cy * smallCell + offsetY),
                                Segments = new PathSegmentCollection
                                {
                                    new BezierSegment
                                    {
                                        Point1 = new System.Windows.Point(cx * smallCell + smallCell * 0.3 + offsetX, cy * smallCell + smallCell * 0.2 + offsetY),
                                        Point2 = new System.Windows.Point(cx * smallCell + smallCell * 0.6 + offsetX, cy * smallCell + smallCell * 0.5 + offsetY),
                                        Point3 = new System.Windows.Point(cx * smallCell + smallCell * 0.8 + offsetX, cy * smallCell + smallCell * 0.3 + offsetY)
                                    }
                                }
                            }
                        }
                    },
                    Stroke = new LinearGradientBrush(
                        Color.FromArgb(50, 255, 255, 200),
                        Color.FromArgb(10, 200, 255, 255),
                        0),
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(causticPath, bottomBaseX);
                Canvas.SetTop(causticPath, bottomBaseY);
                Canvas.SetZIndex(causticPath, 8);
                canvas.Children.Add(causticPath);
            }
            
            // Strömungswirbel (identisch zu Setup - 10x10 Raster)
            for (int w = 0; w < 100; w++)
            {
                double whirlPhase = (time2 * 0.15 + w * 0.1) % 4.0;
                int wx = w % 10;
                int wy = w / 10;
                double whirlRadius = smallCell * (0.8 + System.Math.Sin(whirlPhase) * 0.3);
                
                var whirl = new Ellipse
                {
                    Width = whirlRadius,
                    Height = whirlRadius,
                    Stroke = new SolidColorBrush(Color.FromArgb(20, 100, 180, 255)),
                    StrokeThickness = 1.5,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(0, 0, 0, 0),
                        Color.FromArgb(10, 50, 120, 200))
                };
                Canvas.SetLeft(whirl, bottomBaseX + wx * smallCell + smallCell * 0.5 - whirlRadius / 2);
                Canvas.SetTop(whirl, bottomBaseY + wy * smallCell + smallCell * 0.5 - whirlRadius / 2);
                Canvas.SetZIndex(whirl, 6);
                canvas.Children.Add(whirl);
            }
            
            for (int x = 0; x <= f2.Width; x++) canvas.Children.Add(new Line { X1 = bottomBaseX + x * smallCell, Y1 = bottomBaseY, X2 = bottomBaseX + x * smallCell, Y2 = bottomBaseY + f2.Height * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            for (int y = 0; y <= f2.Height; y++) canvas.Children.Add(new Line { X1 = bottomBaseX, Y1 = bottomBaseY + y * smallCell, X2 = bottomBaseX + f2.Width * smallCell, Y2 = bottomBaseY + y * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            var title2 = new TextBlock { Text = "Spielfeld Spieler 2", FontWeight = System.Windows.FontWeights.Bold }; Canvas.SetLeft(title2, bottomBaseX); Canvas.SetTop(title2, bottomBaseY - 18); canvas.Children.Add(title2);
            
            // Orange border around Player 2 field if it's their turn to shoot (Player 1 shoots at Player 2's field)
            // Nur anzeigen wenn mindestens ein Schuss abgegeben wurde
            if ((rules.Shots.Count > 0 || rules.Shots2.Count > 0) && rules.CurrentPlayerNumber == 1)
            {
                var orangeBorder2 = new Rectangle 
                { 
                    Width = f2.Width * smallCell, 
                    Height = f2.Height * smallCell,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 4
                };
                Canvas.SetLeft(orangeBorder2, bottomBaseX);
                Canvas.SetTop(orangeBorder2, bottomBaseY);
                Canvas.SetZIndex(orangeBorder2, 101);
                canvas.Children.Add(orangeBorder2);
            }
            
            // Player indicator boxes on the right side
            double indicatorX = bottomBaseX + f2.Width * smallCell + 30;
            double indicatorY1 = topBaseY + 50;
            double indicatorY2 = topBaseY + 120;
            double indicatorWidth = 120;
            double indicatorHeight = 50;
            
            // Player 1 indicator box
            bool hasShots = rules.Shots.Count > 0 || rules.Shots2.Count > 0;
            var player1Box = new Rectangle
            {
                Width = indicatorWidth,
                Height = indicatorHeight,
                Fill = Brushes.LightGray,
                Stroke = (hasShots && rules.CurrentPlayerNumber == 1) ? Brushes.Orange : Brushes.Black,
                StrokeThickness = (hasShots && rules.CurrentPlayerNumber == 1) ? 4 : 2
            };
            Canvas.SetLeft(player1Box, indicatorX);
            Canvas.SetTop(player1Box, indicatorY1);
            canvas.Children.Add(player1Box);
            
            var player1Text = new TextBlock
            {
                Text = "Spieler 1",
                FontSize = 16,
                FontWeight = (hasShots && rules.CurrentPlayerNumber == 1) ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(player1Text, indicatorX + 20);
            Canvas.SetTop(player1Text, indicatorY1 + 15);
            canvas.Children.Add(player1Text);
            
            // Player 2 indicator box
            var player2Box = new Rectangle
            {
                Width = indicatorWidth,
                Height = indicatorHeight,
                Fill = Brushes.LightGray,
                Stroke = (hasShots && rules.CurrentPlayerNumber == 2) ? Brushes.Orange : Brushes.Black,
                StrokeThickness = (hasShots && rules.CurrentPlayerNumber == 2) ? 4 : 2
            };
            Canvas.SetLeft(player2Box, indicatorX);
            Canvas.SetTop(player2Box, indicatorY2);
            canvas.Children.Add(player2Box);
            
            var player2Text = new TextBlock
            {
                Text = "Spieler 2",
                FontSize = 16,
                FontWeight = (hasShots && rules.CurrentPlayerNumber == 2) ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(player2Text, indicatorX + 20);
            Canvas.SetTop(player2Text, indicatorY2 + 15);
            canvas.Children.Add(player2Text);
            
            // Gewinner-Animation (Kriegsschiff-Theme)
            if (rules.WinnerPlayer > 0)
            {
                double animTime = rules.WinnerAnimationTime;
                double popScale = animTime < 0.5 ? (animTime / 0.5) : 1.0; // Pop-up in 0.5 Sekunden
                
                // Halbtransparenter Ozean-Hintergrund
                var overlay = new Rectangle
                {
                    Width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800,
                    Height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600,
                    Fill = new LinearGradientBrush(
                        Color.FromArgb(200, 20, 40, 80),   // Dunkelblau oben
                        Color.FromArgb(200, 10, 70, 120),  // Ozeanblau unten
                        90)
                };
                Canvas.SetLeft(overlay, 0);
                Canvas.SetTop(overlay, 0);
                Canvas.SetZIndex(overlay, 200);
                canvas.Children.Add(overlay);
                
                double centerX = (canvas.ActualWidth > 0 ? canvas.ActualWidth : 800) / 2;
                double centerY = (canvas.ActualHeight > 0 ? canvas.ActualHeight : 600) / 2;
                
                // Großes Kriegsschiff diagonal im Hintergrund (5 Felder lang wie im Spiel)
                double shipCellSize = 60;
                double rotationAngle = 45; // Diagonal
                
                // Rotations-Transformation für das Schiff
                var shipGroup = new System.Windows.Controls.Canvas();
                Canvas.SetLeft(shipGroup, centerX);
                Canvas.SetTop(shipGroup, centerY);
                Canvas.SetZIndex(shipGroup, 201);
                
                var rotateTransform = new RotateTransform(rotationAngle);
                shipGroup.RenderTransform = rotateTransform;
                shipGroup.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                
                // Zeichne ultra-realistisches 5-Felder Kriegsschiff
                for (int seg = 0; seg < 5; seg++)
                {
                    double segX = -150 + seg * shipCellSize;
                    double segY = 0;
                    
                    // Schiffsrumpf (dunkelgrau mit Metallglanz)
                    var hull = new Rectangle
                    {
                        Width = shipCellSize - 4,
                        Height = shipCellSize * 0.7,
                        Fill = new LinearGradientBrush(
                            Color.FromRgb(80, 90, 100),
                            Color.FromRgb(50, 55, 60),
                            90)
                    };
                    Canvas.SetLeft(hull, segX);
                    Canvas.SetTop(hull, segY - shipCellSize * 0.35);
                    shipGroup.Children.Add(hull);
                    
                    // Deck (helleres Grau)
                    var deck = new Rectangle
                    {
                        Width = shipCellSize - 4,
                        Height = shipCellSize * 0.15,
                        Fill = new SolidColorBrush(Color.FromRgb(100, 110, 120))
                    };
                    Canvas.SetLeft(deck, segX);
                    Canvas.SetTop(deck, segY - shipCellSize * 0.35);
                    shipGroup.Children.Add(deck);
                    
                    // Fenster/Details
                    if (seg % 2 == 0)
                    {
                        var window = new Rectangle
                        {
                            Width = 8,
                            Height = 6,
                            Fill = new SolidColorBrush(Color.FromArgb(150, 255, 255, 150))
                        };
                        Canvas.SetLeft(window, segX + shipCellSize / 2 - 4);
                        Canvas.SetTop(window, segY - 5);
                        shipGroup.Children.Add(window);
                    }
                }
                
                // Kommandoturm (auf mittlerem Segment)
                var commandTower = new Rectangle
                {
                    Width = shipCellSize * 0.6,
                    Height = shipCellSize * 0.8,
                    Fill = new LinearGradientBrush(
                        Color.FromRgb(90, 100, 110),
                        Color.FromRgb(60, 65, 70),
                        90)
                };
                Canvas.SetLeft(commandTower, -30);
                Canvas.SetTop(commandTower, -shipCellSize * 0.75);
                shipGroup.Children.Add(commandTower);
                
                // Radar/Antenne
                var radar = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = new SolidColorBrush(Color.FromRgb(150, 160, 170))
                };
                Canvas.SetLeft(radar, -7.5);
                Canvas.SetTop(radar, -shipCellSize * 0.85);
                shipGroup.Children.Add(radar);
                
                // Geschütztürme an Bug und Heck
                for (int gunPos = 0; gunPos < 2; gunPos++)
                {
                    double gunX = gunPos == 0 ? -120 : 90;
                    var gunTurret = new Ellipse
                    {
                        Width = 25,
                        Height = 25,
                        Fill = new RadialGradientBrush(
                            Color.FromRgb(70, 75, 80),
                            Color.FromRgb(40, 45, 50))
                    };
                    Canvas.SetLeft(gunTurret, gunX - 12.5);
                    Canvas.SetTop(gunTurret, -12.5);
                    shipGroup.Children.Add(gunTurret);
                    
                    // Kanonenrohr
                    var cannon = new Rectangle
                    {
                        Width = 30,
                        Height = 6,
                        Fill = new SolidColorBrush(Color.FromRgb(60, 65, 70))
                    };
                    Canvas.SetLeft(cannon, gunX + (gunPos == 0 ? -35 : 12.5));
                    Canvas.SetTop(cannon, -3);
                    shipGroup.Children.Add(cannon);
                }
                
                canvas.Children.Add(shipGroup);
                
                // Gewinner-Box mit Marine-Theme (über dem Schiff)
                double boxWidth = 450 * popScale;
                double boxHeight = 220 * popScale;
                
                // Navy-blauer Rahmen mit Gold-Akzenten
                var winnerBox = new Rectangle
                {
                    Width = boxWidth,
                    Height = boxHeight,
                    Fill = new LinearGradientBrush(
                        Color.FromRgb(25, 50, 100),   // Marine Blau
                        Color.FromRgb(15, 35, 70),    // Dunkles Marine Blau
                        90),
                    Stroke = new LinearGradientBrush(
                        Color.FromRgb(255, 215, 0),   // Gold
                        Color.FromRgb(218, 165, 32),  // Dunkleres Gold
                        0),
                    StrokeThickness = 6,
                    RadiusX = 15,
                    RadiusY = 15
                };
                Canvas.SetLeft(winnerBox, centerX - boxWidth / 2);
                Canvas.SetTop(winnerBox, centerY - boxHeight / 2);
                Canvas.SetZIndex(winnerBox, 205);
                canvas.Children.Add(winnerBox);
                
                // Militär-Streifen (Gold)
                for (int stripe = 0; stripe < 2; stripe++)
                {
                    var goldStripe = new Rectangle
                    {
                        Width = boxWidth - 12,
                        Height = 3,
                        Fill = Brushes.Gold
                    };
                    Canvas.SetLeft(goldStripe, centerX - boxWidth / 2 + 6);
                    Canvas.SetTop(goldStripe, centerY - boxHeight / 2 + 12 + stripe * (boxHeight - 27));
                    Canvas.SetZIndex(goldStripe, 206);
                    canvas.Children.Add(goldStripe);
                }
                
                // Gewinner-Text (Militär-Stil)
                var siegText = new TextBlock
                {
                    Text = "⚓ SIEG! ⚓",
                    FontSize = 32 * popScale,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = Brushes.Gold,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Width = boxWidth
                };
                Canvas.SetLeft(siegText, centerX - boxWidth / 2);
                Canvas.SetTop(siegText, centerY - 70 * popScale);
                Canvas.SetZIndex(siegText, 207);
                canvas.Children.Add(siegText);
                
                var playerText = new TextBlock
                {
                    Text = $"Spieler {rules.WinnerPlayer}",
                    FontSize = 48 * popScale,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = Brushes.White,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Width = boxWidth
                };
                Canvas.SetLeft(playerText, centerX - boxWidth / 2);
                Canvas.SetTop(playerText, centerY - 25 * popScale);
                Canvas.SetZIndex(playerText, 207);
                canvas.Children.Add(playerText);
                
                var winText = new TextBlock
                {
                    Text = "hat gewonnen!",
                    FontSize = 28 * popScale,
                    FontWeight = System.Windows.FontWeights.Normal,
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 220, 255)),
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Width = boxWidth
                };
                Canvas.SetLeft(winText, centerX - boxWidth / 2);
                Canvas.SetTop(winText, centerY + 30 * popScale);
                Canvas.SetZIndex(winText, 207);
                canvas.Children.Add(winText);
            }
            // draw ships as warship models if visible for Player 2
            if (rules.ShowShipsPlayer2)
            {
                foreach (var s in rules.Ships2)
                {
                    if (s.X >= 0 && s.Y >= 0 && s.Hits < s.Size) // Nur nicht-versenkte Schiffe
                    {
                        DrawWarship(canvas, s, bottomBaseX, bottomBaseY, smallCell, s.IsHorizontal);
                    }
                }
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

            // draw sunk ships for player2 with explosion animation
            foreach (var s in rules.Ships2)
            {
                if (s.Hits == s.Size)
                {
                    double animTime = rules.SunkShipAnimationTime.ContainsKey(s.Id) ? rules.SunkShipAnimationTime[s.Id] : 999;
                    
                    // Phase 1 (0-1.5s): Feuerbälle auf getroffenen Feldern
                    if (animTime < 1.5)
                    {
                        for (int i = 0; i < s.Size; i++)
                        {
                            int sx = s.IsHorizontal ? s.X + i : s.X;
                            int sy = s.IsHorizontal ? s.Y : s.Y + i;
                            
                            // Feuerball-Animation mit mehreren Schichten
                            double explosionPhase = (animTime * 2) % 1.0; // Pulsiert
                            double baseSize = smallCell * 0.8;
                            double pulseSize = baseSize * (1.0 + explosionPhase * 0.3);
                            
                            // Äußerer Feuerring (Orange)
                            var outerFire = new Ellipse 
                            { 
                                Width = pulseSize, 
                                Height = pulseSize,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb((byte)(200 - explosionPhase * 100), 255, 100, 0),
                                    Color.FromArgb((byte)(100 - explosionPhase * 60), 255, 50, 0))
                            };
                            Canvas.SetLeft(outerFire, bottomBaseX + sx * smallCell + smallCell / 2 - pulseSize / 2);
                            Canvas.SetTop(outerFire, bottomBaseY + sy * smallCell + smallCell / 2 - pulseSize / 2);
                            Canvas.SetZIndex(outerFire, 95);
                            canvas.Children.Add(outerFire);
                            
                            // Innerer Kern (Gelb-Weiß)
                            var innerCore = new Ellipse 
                            { 
                                Width = pulseSize * 0.5, 
                                Height = pulseSize * 0.5,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb(255, 255, 255, 200),
                                    Color.FromArgb((byte)(220 - explosionPhase * 100), 255, 200, 0))
                            };
                            Canvas.SetLeft(innerCore, bottomBaseX + sx * smallCell + smallCell / 2 - pulseSize * 0.25);
                            Canvas.SetTop(innerCore, bottomBaseY + sy * smallCell + smallCell / 2 - pulseSize * 0.25);
                            Canvas.SetZIndex(innerCore, 96);
                            canvas.Children.Add(innerCore);
                            
                            // Rauch-Partikel
                            var smoke = new Ellipse 
                            { 
                                Width = smallCell * 0.6, 
                                Height = smallCell * 0.6,
                                Fill = new RadialGradientBrush(
                                    Color.FromArgb((byte)(60 + explosionPhase * 40), 50, 50, 50),
                                    Color.FromArgb(0, 30, 30, 30))
                            };
                            Canvas.SetLeft(smoke, bottomBaseX + sx * smallCell + smallCell / 2 - smallCell * 0.3);
                            Canvas.SetTop(smoke, bottomBaseY + sy * smallCell + smallCell / 2 - smallCell * 0.3 - animTime * 5);
                            Canvas.SetZIndex(smoke, 94);
                            canvas.Children.Add(smoke);
                        }
                    }
                    // Phase 2 (1.5s+): Zeige Kriegsschiff mit Fade-In
                    else if (animTime >= 1.5 && animTime < 2.5)
                    {
                        // Fade-In des Schiffes
                        double fadeAlpha = (animTime - 1.5) / 1.0; // 0 bis 1
                        fadeAlpha = System.Math.Min(1.0, fadeAlpha);
                        
                        DrawWarship(canvas, s, bottomBaseX, bottomBaseY, smallCell, s.IsHorizontal);
                        
                        // Weißer Fade-Overlay
                        if (fadeAlpha < 1.0)
                        {
                            for (int i = 0; i < s.Size; i++)
                            {
                                int sx = s.IsHorizontal ? s.X + i : s.X;
                                int sy = s.IsHorizontal ? s.Y : s.Y + i;
                                var fadeOverlay = new Rectangle 
                                { 
                                    Width = smallCell - 1, 
                                    Height = smallCell - 1, 
                                    Fill = new SolidColorBrush(Color.FromArgb((byte)((1.0 - fadeAlpha) * 180), 255, 255, 255))
                                };
                                Canvas.SetLeft(fadeOverlay, bottomBaseX + sx * smallCell + 1);
                                Canvas.SetTop(fadeOverlay, bottomBaseY + sy * smallCell + 1);
                                Canvas.SetZIndex(fadeOverlay, 98);
                                canvas.Children.Add(fadeOverlay);
                            }
                        }
                    }
                    // Phase 3 (2.5s+): Schiff vollständig sichtbar
                    else
                    {
                        DrawWarship(canvas, s, bottomBaseX, bottomBaseY, smallCell, s.IsHorizontal);
                    }
                }
            }

            var info = new TextBlock { Text = "Spielphase: Klicke auf das gegnerische Feld zum Schießen", FontSize = 13, Foreground = Brushes.Black };
            Canvas.SetLeft(info, OFFSET_X);
            Canvas.SetTop(info, bottomBaseY + f2.Height * smallCell + 10);
            canvas.Children.Add(info);

            // Button für Player 1 Schiffe
            double btn1X = topBaseX + f1.Width * smallCell + 20;
            double btn1Y = topBaseY;
            double btnW = 120;
            double btnH = 28;
            var btn1Rect = new Rectangle { Width = btnW, Height = btnH, Fill = new SolidColorBrush(Color.FromRgb(50, 100, 150)), Stroke = Brushes.Black, StrokeThickness = 1, RadiusX = 4, RadiusY = 4 };
            Canvas.SetLeft(btn1Rect, btn1X); Canvas.SetTop(btn1Rect, btn1Y); canvas.Children.Add(btn1Rect);
            var btn1Text = new TextBlock { Text = rules.ShowShipsPlayer1 ? "Schiffe 1 Verbergen" : "Schiffe 1 Anzeigen", FontSize = 10, Foreground = Brushes.White, FontWeight = System.Windows.FontWeights.Bold, Width = btnW, TextAlignment = System.Windows.TextAlignment.Center };
            Canvas.SetLeft(btn1Text, btn1X); Canvas.SetTop(btn1Text, btn1Y + 7); canvas.Children.Add(btn1Text);

            // Button für Player 2 Schiffe
            double btn2X = bottomBaseX + f2.Width * smallCell + 20;
            double btn2Y = bottomBaseY;
            var btn2Rect = new Rectangle { Width = btnW, Height = btnH, Fill = new SolidColorBrush(Color.FromRgb(150, 50, 50)), Stroke = Brushes.Black, StrokeThickness = 1, RadiusX = 4, RadiusY = 4 };
            Canvas.SetLeft(btn2Rect, btn2X); Canvas.SetTop(btn2Rect, btn2Y); canvas.Children.Add(btn2Rect);
            var btn2Text = new TextBlock { Text = rules.ShowShipsPlayer2 ? "Schiffe 2 Verbergen" : "Schiffe 2 Anzeigen", FontSize = 10, Foreground = Brushes.White, FontWeight = System.Windows.FontWeights.Bold, Width = btnW, TextAlignment = System.Windows.TextAlignment.Center };
            Canvas.SetLeft(btn2Text, btn2X); Canvas.SetTop(btn2Text, btn2Y + 7); canvas.Children.Add(btn2Text);
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
                    Canvas.SetZIndex(shadow, 10);
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
                Canvas.SetZIndex(waterDisplacement, 10);
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
                Canvas.SetZIndex(bowWave, 10);
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
                    Canvas.SetZIndex(sprayDrop, 10);
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
                Canvas.SetZIndex(sternWave, 10);
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
                    Canvas.SetZIndex(wake, 10);
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
                    Canvas.SetZIndex(foamBubble, 11);
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
                
                Canvas.SetZIndex(hull, 11);
                canvas.Children.Add(hull);

                // Seitliche Schatten/Details
                var sideDetail1 = new Line
                {
                    X1 = startX + cellSize * 0.3, Y1 = startY + yOffset + 5,
                    X2 = startX + shipWidth - cellSize * 0.2, Y2 = startY + yOffset + 5,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail1, 12);
                canvas.Children.Add(sideDetail1);

                var sideDetail2 = new Line
                {
                    X1 = startX + cellSize * 0.3, Y1 = startY + yOffset + shipHeight - 5,
                    X2 = startX + shipWidth - cellSize * 0.2, Y2 = startY + yOffset + shipHeight - 5,
                    Stroke = darkHullColor, StrokeThickness = 1.5
                };
                Canvas.SetZIndex(sideDetail2, 12);
                canvas.Children.Add(sideDetail2);

                // Deck-Mittellinie
                var deckLine = new Line
                {
                    X1 = startX + cellSize * 0.25, Y1 = startY + yOffset + shipHeight / 2,
                    X2 = startX + shipWidth - cellSize * 0.15, Y2 = startY + yOffset + shipHeight / 2,
                    Stroke = highlightColor, StrokeThickness = 2
                };
                Canvas.SetZIndex(deckLine, 12);
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
                    Canvas.SetZIndex(plate, 12);
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
                        Canvas.SetZIndex(rivetHighlight, 15);
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
                    Canvas.SetZIndex(anchor, 14);
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
                        Canvas.SetZIndex(lifeboat, 16);
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
                    Canvas.SetZIndex(bridge, 16);
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
                        Canvas.SetZIndex(windowFrame, 17);
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
                        Canvas.SetZIndex(interiorLight, 17);
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
                        Canvas.SetZIndex(window, 18);
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
                        Canvas.SetZIndex(windowReflection, 19);
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
                    Canvas.SetZIndex(chimney, 17);
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
                        Canvas.SetZIndex(smoke, 18);
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
                    Canvas.SetZIndex(heatDistortion, 16);
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
                    Canvas.SetZIndex(spotlightGlow, 18);
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
                    Canvas.SetZIndex(spotlight, 19);
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
                    Canvas.SetZIndex(lightBeam, 17);
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
                    Canvas.SetZIndex(antenna2, 18);
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
                    Canvas.SetZIndex(signalLight, 19);
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
                    Canvas.SetZIndex(signalGlow, 18);
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
                        Canvas.SetZIndex(electricSpark, 19);
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
                        Canvas.SetZIndex(boatShadow, 15);
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
                        Canvas.SetZIndex(boatHighlight, 17);
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
                    Canvas.SetZIndex(platformShadow, 15);
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
                    Canvas.SetZIndex(gunPlatform1, 16);
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
                    Canvas.SetZIndex(gunBase1, 17);
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
                    Canvas.SetZIndex(turretShine, 18);
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
                        Canvas.SetZIndex(gunBarrel, 18);
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
                    Canvas.SetZIndex(gunBase2, 16);
                    canvas.Children.Add(gunBase2);

                    var gunBarrel2 = new Rectangle
                    {
                        Width = cellSize * 0.15, Height = 2.5,
                        Fill = darkHullColor
                    };
                    Canvas.SetLeft(gunBarrel2, gun2X - cellSize * 0.2);
                    Canvas.SetTop(gunBarrel2, gun1Y - 1.25);
                    Canvas.SetZIndex(gunBarrel2, 17);
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
                    Canvas.SetZIndex(radarMast, 17);
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
                    Canvas.SetZIndex(radar, 18);
                    canvas.Children.Add(radar);

                    // Radar-Scan-Linie (grün)
                    var radarLine = new Line
                    {
                        X1 = radarX + cellSize * 0.08, Y1 = radarY + cellSize * 0.065,
                        X2 = radarX + cellSize * 0.14, Y2 = radarY + cellSize * 0.03,
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
                        StrokeThickness = 1.5
                    };
                    Canvas.SetZIndex(radarLine, 19);
                    canvas.Children.Add(radarLine);

                    // Pulsierender Radar-Glow)
                    var radarGlowOuter = new Ellipse
                    {
                        Width = 5, Height = 5,
                        Fill = new RadialGradientBrush(
                            Color.FromArgb(60, 100, 255, 100),
                            Color.FromArgb(0, 100, 255, 100))
                    };
                    Canvas.SetLeft(radarGlowOuter, radarX + cellSize * 0.105);
                    Canvas.SetTop(radarGlowOuter, radarY + cellSize * 0.005);
                    Canvas.SetZIndex(radarGlowOuter, 20);
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
                    Canvas.SetZIndex(radarGlow, 21);
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
                        Canvas.SetZIndex(radarWave, 19);
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
                    Canvas.SetZIndex(flagpole, 17);
                    canvas.Children.Add(flagpole);

                    // Wehende Flagge mit Schatten
                    var flagShadow = new System.Windows.Shapes.Polygon
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
                    };
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 6.5, startY + yOffset + shipHeight * 0.35 + 0.5));
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 2.5, startY + yOffset + shipHeight * 0.37 + 0.5));
                    flagShadow.Points.Add(new System.Windows.Point(startX + shipWidth - 6.5, startY + yOffset + shipHeight * 0.39 + 0.5));
                    Canvas.SetZIndex(flagShadow, 17);
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
                    Canvas.SetZIndex(flag, 18);
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
            if (rules != null && rules.IsSetupPhase)
            {
                var mousePos = e.GetPosition(null);
                int gridX = (int)((mousePos.X - 30) / 40);
                int gridY = (int)((mousePos.Y - 30) / 40);
                rules.MouseX = gridX;
                rules.MouseY = gridY;
            }

            // Mausrad-Rotation für das aktuelle Schiff
            if (e is System.Windows.Input.MouseWheelEventArgs wheelEvent && rules?.IsSetupPhase == true)
            {
                var shipsList = rules.CurrentSetupPlayer == 1 ? rules.Ships : rules.Ships2;
                var ship = rules.GetCurrentShip(shipsList);
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

                    // Start/Next button region - PRÜFE ZUERST
                    bool canProceed = (rules.Phase == 1 && rules.AllShipsPlaced1) || (rules.Phase == 2 && rules.AllShipsPlaced2);
                    if (canProceed)
                    {
                        double buttonX = baseOffset + shipsList.Count * shipStep + 20;
                        double buttonY = shipPreviewY - 5;
                        double buttonWidth = 120;
                        double buttonHeight = 40;
                        // Großzügige Toleranz für Button
                        if (click.XClickPos >= buttonX - 5 && click.XClickPos <= buttonX + buttonWidth + 5 && 
                            click.YClickPos >= buttonY - 5 && click.YClickPos <= buttonY + buttonHeight + 5)
                        {
                            // Instead of directly changing the phase here, return a special move
                            // that signals the rules to advance the phase. This ensures MainWindow
                            // processes the move and will switch the active player accordingly.
                            return new A3_LEA_SchiffeMove(-1, -1, activePlayer);
                        }
                    }

                    // Click on selection bar: Manuelles Auswählen eines Schiffs
                    // Nur wenn Klick eindeutig im Schiffsleisten-Bereich ist
                    if (click.YClickPos >= shipPreviewY - 10 && click.YClickPos <= shipPreviewY + 40)
                    {
                        double shipPreviewX = baseOffset;
                        double clickOffsetX = click.XClickPos - shipPreviewX;
                        
                        // Prüfe ob Klick im horizontalen Bereich der Schiffsleiste ist
                        if (clickOffsetX >= 0 && clickOffsetX < shipsList.Count * shipStep)
                        {
                            // Berechne welches Schiff angeklickt wurde
                            int shipClicked = (int)(clickOffsetX / shipStep);
                            
                            if (shipClicked >= 0 && shipClicked < shipsList.Count)
                            {
                                var ship = shipsList[shipClicked];
                                // Alle Schiffe können ausgewählt werden
                                rules.ManuallySelectedShip = ship;
                                // Wenn Schiff bereits platziert war, entferne es vom Feld
                                if (ship.X >= 0 && ship.Y >= 0)
                                {
                                    rules.RemoveShipFromField(ship);
                                }
                                return null; // Schiff wurde ausgewählt, keine weitere Aktion
                            }
                        }
                    }

                    // Click on field to place or pick up ship
                    // Prüfe ob Klick im Spielfeld-Bereich ist (ohne oberen Rand zu beschneiden)
                    double fieldRight = baseOffset + field.Width * cellSize;
                    double fieldBottom = baseOffset + field.Height * cellSize;
                    
                    if (click.XClickPos >= baseOffset && click.XClickPos <= fieldRight &&
                        click.YClickPos >= baseOffset && click.YClickPos <= fieldBottom)
                    {
                        int gridX = (int)((click.XClickPos - baseOffset) / cellSize);
                        int gridY = (int)((click.YClickPos - baseOffset) / cellSize);
                        
                        if (rules.Phase == 1 || rules.Phase == 2)
                        {
                            var targetField = activePlayer == 1 ? rules.SchiffeField : rules.SchiffeField2;
                            if (targetField.IsValidPosition(gridX, gridY))
                            {
                                // Prüfe ob an dieser Stelle ein Schiff ist
                                int shipIdAtCell = targetField[gridX, gridY];
                                if (shipIdAtCell > 0)
                                {
                                    // Finde das Schiff und wähle es aus (aufnehmen)
                                    var clickedShip = shipsList.FirstOrDefault(s => s.Id == shipIdAtCell);
                                    if (clickedShip != null)
                                    {
                                        rules.ManuallySelectedShip = clickedShip;
                                        rules.RemoveShipFromField(clickedShip);
                                        return null; // Schiff wurde aufgenommen
                                    }
                                }
                                else
                                {
                                    // Leere Zelle: Platziere aktuelles Schiff
                                    return new A3_LEA_SchiffeMove(gridX, gridY, activePlayer);
                                }
                            }
                        }
                    }
                }

                // Playing phase: clicks are shots on opponent's field (fields are stacked vertically)
                if (rules != null && !rules.IsSetupPhase)
                {
                    double smallCell = 24;
                    double topBaseY = 20;
                    double bottomBaseY = 20 + field.Height * smallCell + 40;
                    
                    // Button für Player 1 Schiffe (mit 10px Toleranz)
                    double btn1X = baseOffset + field.Width * smallCell + 20;
                    double btn1Y = topBaseY;
                    double btnW = 120;
                    double btnH = 28;
                    if (click.XClickPos >= btn1X - 10 && click.XClickPos <= btn1X + btnW + 10 && 
                        click.YClickPos >= btn1Y - 10 && click.YClickPos <= btn1Y + btnH + 10)
                    {
                        rules.ShowShipsPlayer1 = !rules.ShowShipsPlayer1;
                        return null;
                    }
                    
                    // Button für Player 2 Schiffe (mit 10px Toleranz)
                    double btn2X = baseOffset + field.Width * smallCell + 20;
                    double btn2Y = bottomBaseY;
                    if (click.XClickPos >= btn2X - 10 && click.XClickPos <= btn2X + btnW + 10 && 
                        click.YClickPos >= btn2Y - 10 && click.YClickPos <= btn2Y + btnH + 10)
                    {
                        rules.ShowShipsPlayer2 = !rules.ShowShipsPlayer2;
                        return null;
                    }
                    // Player 1 shoots on bottom (player2 field), Player2 shoots on top (player1 field)
                    if (_playerNumber == 1)
                    {
                        double fieldRight = baseOffset + field.Width * smallCell;
                        double fieldBottom = bottomBaseY + field.Height * smallCell;
                        
                        if (click.XClickPos >= baseOffset && click.XClickPos <= fieldRight &&
                            click.YClickPos >= bottomBaseY && click.YClickPos <= fieldBottom)
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
                        double fieldRight = baseOffset + field.Width * smallCell;
                        double fieldTop = topBaseY + field.Height * smallCell;
                        
                        if (click.XClickPos >= baseOffset && click.XClickPos <= fieldRight &&
                            click.YClickPos >= topBaseY && click.YClickPos <= fieldTop)
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
                }
            }
            else if (selection is IKeySelection keySelection)
            {
                if (rules != null && rules.IsSetupPhase)
                {
                    // ESC-Key: Abwählen des aktuellen Schiffs (zurück in die Leiste)
                    if (keySelection.Key == System.Windows.Input.Key.Escape)
                    {
                        if (rules.ManuallySelectedShip != null)
                        {
                            // Schiff zurück in Auswahlleiste legen (unplatziert)
                            rules.ManuallySelectedShip = null;
                            return null;
                        }
                    }
                    // R-Key: Rotation des nächsten unplatzierten Schiffs
                    if (keySelection.Key == System.Windows.Input.Key.R)
                    {
                        var shipsList = rules.CurrentSetupPlayer == 1 ? rules.Ships : rules.Ships2;
                        var currentShip = rules.GetCurrentShip(shipsList);
                        if (currentShip != null)
                        {
                            currentShip.IsHorizontal = !currentShip.IsHorizontal;
                        }
                    }
                }
            }
            return null;
        }
    }

    //Computer Player
    public class A3_LEA_ComputerSchiffePlayer : A3_LEA_BaseComputerSchiffePlayer
    {
        private int _playerNumber = 2;
        public override string Name => "A3 LEA Schiffe Computer";
        public override int PlayerNumber => _playerNumber;
        public override void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;
        public override IGamePlayer Clone() => new A3_LEA_ComputerSchiffePlayer();

        public IA3_LEA_SchiffeMove GetMove(IMoveSelection selection, IA3_LEA_SchiffeField field)
        {
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
            if (rules != null)
            {
                if (!rules.IsSetupPhase && rules.Phase == 3 && rules.CurrentSetupPlayer == _playerNumber)
                {
                    // Computer is in playing phase and it's his turn: shoot at random field
                    Random rand = new Random();
                    int x, y;
                    do
                    {
                        x = rand.Next(0, field.Width);
                        y = rand.Next(0, field.Height);
                    } while (rules.Shots2.Contains((x, y))); // avoid shooting the same cell twice
                    return new A3_LEA_SchiffeMove(x, y, _playerNumber);
                }
            }
            return null;
        }

        public override IA3_LEA_SchiffeMove GetMove(IA3_LEA_SchiffeField field)
        {
            var rules = OOPGamesManager.Singleton.ActiveRules as A3_LEA_SchiffeRules;
            if (rules == null) return null;

            // Setup phase placement for computer (player 2)
            if (rules.IsSetupPhase && rules.CurrentSetupPlayer == _playerNumber)
            {
                // Determine which ships list belongs to this computer
                var shipsList = _playerNumber == 1 ? rules.Ships : rules.Ships2;
                var ship = rules.GetCurrentShip(shipsList);
                if (ship != null)
                {
                    Random rand = new Random();
                    int attempts = 0;
                    while (attempts < 200)
                    {
                        bool horizontal = rand.Next(0, 2) == 0;
                        int maxX = horizontal ? field.Width - ship.Size : field.Width - 1;
                        int maxY = horizontal ? field.Height - 1 : field.Height - ship.Size;
                        int x = rand.Next(0, maxX + 1);
                        int y = rand.Next(0, maxY + 1);
                        if (rules.CanPlaceShip(ship, x, y, horizontal))
                        {
                            // Return placement move
                            return new A3_LEA_SchiffeMove(x, y, _playerNumber);
                        }
                        attempts++;
                    }
                }
                return null;
            }

            // Playing phase: computer should shoot at random unshot cell
            if (!rules.IsSetupPhase && rules.Phase == 3)
            {
                // choose the correct shots list based on shooter
                var shotsList = _playerNumber == 1 ? rules.Shots2 : rules.Shots;
                Random rand = new Random();
                int x, y;
                int tries = 0;
                do
                {
                    x = rand.Next(0, field.Width);
                    y = rand.Next(0, field.Height);
                    tries++;
                    if (tries > 500) break; // fail-safe
                } while (shotsList.Contains((x, y)));

                // If we found a valid cell, return a shooting move
                if (tries <= 500 && field.IsValidPosition(x, y))
                    return new A3_LEA_SchiffeMove(x, y, _playerNumber);
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

        
    }
}
