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

        // Phase: 1 = Player1 setup, 2 = Player2 setup, 3 = Playing
        private int _phase = 1;
        public int Phase { get { return _phase; } set { _phase = value; } }
        public bool IsSetupPhase => Phase == 1 || Phase == 2;
        public int CurrentSetupPlayer => Phase == 1 ? 1 : (Phase == 2 ? 2 : 0);

        public int PlacedShipsCount1 => _ships.Count(s => s.X > 0 || s.Y > 0);
        public int PlacedShipsCount2 => _ships2.Count(s => s.X > 0 || s.Y > 0);
        public bool AllShipsPlaced1 => PlacedShipsCount1 == _ships.Count;
        public bool AllShipsPlaced2 => PlacedShipsCount2 == _ships2.Count;

        // Ausgewähltes Schiff (vom aktiven Spieler)
        public A3_LEA_Ship SelectedShip { get; set; } = null;

        // Maus-Tracking (wie IQ-Puzzle)
        public int MouseX { get; set; } = -1;
        public int MouseY { get; set; } = -1;

        public A3_LEA_SchiffeRules()
        {
            // Spieler 1
            _ships.Add(new A3_LEA_Ship(1, 4));
            _ships.Add(new A3_LEA_Ship(2, 3));
            _ships.Add(new A3_LEA_Ship(3, 3));
            _ships.Add(new A3_LEA_Ship(4, 2));
            // Spieler 2 (eigene Objekte)
            _ships2.Add(new A3_LEA_Ship(1, 4));
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
                    if (targetField[x + i, y] != 0) return false;
            }
            else
            {
                if (y + size > targetField.Height) return false;
                for (int i = 0; i < size; i++)
                    if (targetField[x, y + i] != 0) return false;
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
                    ShootAtForPlayer(m.PlayerNumber, m.X, m.Y);
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
                        for (int i = 0; i < ship.Size; i++)
                        {
                            int sx = ship.IsHorizontal ? ship.X + i : ship.X;
                            int sy = ship.IsHorizontal ? ship.Y : ship.Y + i;
                            var rect = new Rectangle { Width = cellSize - 2, Height = cellSize - 2, Fill = Brushes.Gray, Stroke = Brushes.Black, StrokeThickness = 1 };
                            Canvas.SetLeft(rect, baseX + sx * cellSize + 1);
                            Canvas.SetTop(rect, baseY + sy * cellSize + 1);
                            canvas.Children.Add(rect);
                        }
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
                foreach (var s in shipsList)
                {
                    double x = shipPreviewX + (s.Id - 1) * shipStep;
                    bool isSelected = rules.SelectedShip != null && rules.SelectedShip == s;
                    bool isPlaced = s.X > 0 || s.Y > 0;
                    if (isSelected)
                    {
                        var border = new Rectangle { Width = s.Size * shipCellSize + 6, Height = shipCellSize + 6, Stroke = Brushes.Gold, StrokeThickness = 3 };
                        Canvas.SetLeft(border, x - 3);
                        Canvas.SetTop(border, shipPreviewY - 3);
                        canvas.Children.Add(border);
                    }
                    var shipBrush = new SolidColorBrush(isPlaced ? Colors.DarkGray : Colors.LightGray);
                    for (int i = 0; i < s.Size; i++) { var rect = new Rectangle { Width = shipCellSize - 1, Height = shipCellSize - 1, Fill = shipBrush, Stroke = Brushes.Black, StrokeThickness = 0.5 }; Canvas.SetLeft(rect, x + i * shipCellSize); Canvas.SetTop(rect, shipPreviewY); canvas.Children.Add(rect); }
                }

                // Start-Button for progression
                bool showButton = (rules.Phase == 1 && rules.AllShipsPlaced1) || (rules.Phase == 2 && rules.AllShipsPlaced2);
                if (showButton)
                {
                    double buttonX = shipPreviewX + 4 * shipStep + 20;
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
            foreach (var cell in f1.GetOccupiedCells()) { var rect = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = Brushes.Gray, Stroke = Brushes.Black, StrokeThickness = 1 }; Canvas.SetLeft(rect, topBaseX + cell.x * smallCell + 1); Canvas.SetTop(rect, topBaseY + cell.y * smallCell + 1); canvas.Children.Add(rect); }
            foreach (var s in rules.Ships) for (int i = 0; i < s.Size; i++) if (s.HitCells[i]) { int sx = s.IsHorizontal ? s.X + i : s.X; int sy = s.IsHorizontal ? s.Y : s.Y + i; var e = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Red }; Canvas.SetLeft(e, topBaseX + sx * smallCell + smallCell / 2 - 3); Canvas.SetTop(e, topBaseY + sy * smallCell + smallCell / 2 - 3); canvas.Children.Add(e); }

            // draw bottom (Player2)
            var f2 = rules.SchiffeField2;
            for (int x = 0; x <= f2.Width; x++) canvas.Children.Add(new Line { X1 = bottomBaseX + x * smallCell, Y1 = bottomBaseY, X2 = bottomBaseX + x * smallCell, Y2 = bottomBaseY + f2.Height * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            for (int y = 0; y <= f2.Height; y++) canvas.Children.Add(new Line { X1 = bottomBaseX, Y1 = bottomBaseY + y * smallCell, X2 = bottomBaseX + f2.Width * smallCell, Y2 = bottomBaseY + y * smallCell, Stroke = Brushes.Black, StrokeThickness = 1 });
            var title2 = new TextBlock { Text = "Player 2 Field", FontWeight = System.Windows.FontWeights.Bold }; Canvas.SetLeft(title2, bottomBaseX); Canvas.SetTop(title2, bottomBaseY - 18); canvas.Children.Add(title2);
            foreach (var cell in f2.GetOccupiedCells()) { var rect = new Rectangle { Width = smallCell - 1, Height = smallCell - 1, Fill = Brushes.Gray, Stroke = Brushes.Black, StrokeThickness = 1 }; Canvas.SetLeft(rect, bottomBaseX + cell.x * smallCell + 1); Canvas.SetTop(rect, bottomBaseY + cell.y * smallCell + 1); canvas.Children.Add(rect); }
            foreach (var s in rules.Ships2) for (int i = 0; i < s.Size; i++) if (s.HitCells[i]) { int sx = s.IsHorizontal ? s.X + i : s.X; int sy = s.IsHorizontal ? s.Y : s.Y + i; var e = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Red }; Canvas.SetLeft(e, bottomBaseX + sx * smallCell + smallCell / 2 - 3); Canvas.SetTop(e, bottomBaseY + sy * smallCell + smallCell / 2 - 3); canvas.Children.Add(e); }

            var info = new TextBlock { Text = "Playing: click opponent field to shoot", FontSize = 14, Foreground = Brushes.Black };
            Canvas.SetLeft(info, OFFSET_X);
            Canvas.SetTop(info, bottomBaseY + f2.Height * smallCell + 10);
            canvas.Children.Add(info);
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
                int gridX = (int)((mousePos.X - 20) / 40);
                int gridY = (int)((mousePos.Y - 20) / 40);
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
                        double buttonX = baseOffset + 4 * shipStep + 20;
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
                    // Player 1 shoots on bottom (player2 field), Player2 shoots on top (player1 field)
                    if (_playerNumber == 1)
                    {
                        if (click.YClickPos >= bottomBaseY && click.YClickPos < bottomBaseY + field.Height * smallCell)
                        {
                            int gx = (int)((click.XClickPos - baseOffset) / smallCell);
                            int gy = (int)((click.YClickPos - bottomBaseY) / smallCell);
                            if (rules.SchiffeField2.IsValidPosition(gx, gy)) return new A3_LEA_SchiffeMove(gx, gy, _playerNumber);
                        }
                    }
                    else // player 2
                    {
                        if (click.YClickPos >= topBaseY && click.YClickPos < topBaseY + field.Height * smallCell)
                        {
                            int gx = (int)((click.XClickPos - baseOffset) / smallCell);
                            int gy = (int)((click.YClickPos - topBaseY) / smallCell);
                            if (rules.SchiffeField.IsValidPosition(gx, gy)) return new A3_LEA_SchiffeMove(gx, gy, _playerNumber);
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
    }
}
