using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    public class B1_MAN_Paint : OOPGames.IPaintGame
    {
        private static readonly Brush BackgroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 204)); // Hellgelb wie Original
        private static readonly Brush[] PlayerColors = new Brush[] {
            Brushes.Red,        // Spieler 1 (Rot)
            Brushes.Black,      // Spieler 2 (Schwarz)
            Brushes.Yellow,     // Spieler 3 (Gelb)
            Brushes.Green       // Spieler 4 (Grün)
        };

        // FIELD_SIZE wird dynamisch basierend auf gridSize berechnet, um Überlappungen zu vermeiden
        private const double FIELD_SIZE_RATIO = 0.75;  // 75% der gridSize für Felder
        private const double PIECE_SIZE_RATIO = 0.65;  // 65% der gridSize für Spielfiguren
        private const double FIELD_GAP = 2;       // Kleinerer Abstand zwischen Feldern
        private const double BASE_GAP = 4;        // Abstand zwischen Basis-Feldern

        private void DrawField(Canvas canvas, double x, double y, Brush fill, Brush stroke, double size)
        {
            Ellipse field = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1
            };
            Canvas.SetLeft(field, x - size/2);
            Canvas.SetTop(field, y - size/2);
            canvas.Children.Add(field);
        }

        private void DrawFieldWithLabel(Canvas canvas, double x, double y, Brush fill, Brush stroke, double size, string label)
        {
            Ellipse field = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1
            };
            Canvas.SetLeft(field, x - size/2);
            Canvas.SetTop(field, y - size/2);
            canvas.Children.Add(field);

            // Bestimme Textfarbe: Schwarz für helle Farben (Rot, Gelb, Grün), Weiß für dunkle (Schwarz)
            Brush textColor = Brushes.White;
            if (fill == Brushes.Red || fill == Brushes.Yellow || fill == Brushes.Green)
            {
                textColor = Brushes.Black;
            }

            // Zeichne Label zentriert
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = size * 0.6,
                FontWeight = FontWeights.Bold,
                Foreground = textColor,
                TextAlignment = TextAlignment.Center
            };
            
            labelText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(labelText, x - labelText.DesiredSize.Width / 2);
            Canvas.SetTop(labelText, y - labelText.DesiredSize.Height / 2);
            canvas.Children.Add(labelText);
        }

        private void DrawPiece(Canvas canvas, double x, double y, Brush color, double size, int pieceNumber)
        {
            // Zeichne Kreis
            Ellipse piece = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(piece, x - size/2);
            Canvas.SetTop(piece, y - size/2);
            canvas.Children.Add(piece);

            // Bestimme Textfarbe: Schwarz für helle Farben (Rot, Gelb, Grün), Weiß für dunkle (Schwarz)
            Brush textColor = Brushes.White;
            if (color == Brushes.Red || color == Brushes.Yellow || color == Brushes.Green)
            {
                textColor = Brushes.Black;
            }

            // Zeichne Nummer zentriert
            TextBlock number = new TextBlock
            {
                Text = pieceNumber.ToString(),
                FontSize = size * 0.6,
                FontWeight = FontWeights.Bold,
                Foreground = textColor,
                TextAlignment = TextAlignment.Center
            };
            
            // Measure the text size for proper centering
            number.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(number, x - number.DesiredSize.Width / 2);
            Canvas.SetTop(number, y - number.DesiredSize.Height / 2);
            canvas.Children.Add(number);
        }

        // Zeichne 2x2 Startfelder, ausgerichtet an Grid-Zentren (gridSize übergeben)
        private void DrawStartFields(Canvas canvas, double x, double y, double gridSize, Brush color, double fieldSize)
        {
            // x,y sind die Grid-Zentren-Startposition (z.B. 20 + 2*gridSize)
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    DrawField(canvas,
                        x + col * gridSize,
                        y + row * gridSize,
                        color,
                        Brushes.Black,
                        fieldSize);
                }
            }
        }

        private void DrawHomeFields(Canvas canvas, double startX, double startY, Brush color, int direction, double fieldSize)
        {
            // Zielfelder in einer Linie
            for (int i = 0; i < 4; i++)
            {
                double x = startX;
                double y = startY + i * (fieldSize + FIELD_GAP) * direction;
                DrawField(canvas, x, y, color, Brushes.Black, fieldSize);
            }
        }
        
        // Get color for special field type
        private Brush GetSpecialFieldColor(SpecialFieldType type)
        {
            switch (type)
            {
                case SpecialFieldType.Skip:
                    return Brushes.LightCoral;      // Rot - Aussetzen
                case SpecialFieldType.RollAgain:
                    return Brushes.LightGreen;      // Grün - Nochmal würfeln
                case SpecialFieldType.Forward2:
                    return Brushes.LightBlue;       // Blau - 2 vor
                case SpecialFieldType.Backward2:
                    return Brushes.Orange;          // Orange - 2 zurück
                case SpecialFieldType.BackToStart:
                    return Brushes.Purple;          // Lila - Zurück zum Start
                default:
                    return Brushes.White;
            }
        }
        
        // Get symbol for special field type
        private string GetSpecialFieldSymbol(SpecialFieldType type)
        {
            switch (type)
            {
                case SpecialFieldType.Skip:
                    return "X";         // Aussetzen
                case SpecialFieldType.RollAgain:
                    return "↻";         // Nochmal würfeln
                case SpecialFieldType.Forward2:
                    return "↑2";        // 2 vor
                case SpecialFieldType.Backward2:
                    return "↓2";        // 2 zurück
                case SpecialFieldType.BackToStart:
                    return "⌂";         // Zurück zum Start
                default:
                    return "";
            }
        }

        public string Name => "B1_MAN_Paint";

        public void PaintGameField(Canvas canvas, OOPGames.IGameField currentField)
        {
            canvas.Children.Clear();
            if (!(currentField is B1_MAN_Board board)) return;


            // Spielbrett-Grundmaße: nutze die gesamte verfügbare Fläche
            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 550;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 550;
            
            // Reserviere unten Platz für Würfel und Figurenauswahl (ca. 120px)
            const double BOTTOM_CONTROL_HEIGHT = 120;
            double availableHeight = h - BOTTOM_CONTROL_HEIGHT;
            
            // Nutze maximale Fläche, aber quadratisch für das Spielfeld
            double size = Math.Min(w - 40, availableHeight - 40); // Kleine Ränder (20px pro Seite)
            double gridSize = size / 11;  // 11x11 Grid wie im Original
            const double CORNER_PADDING = 20; // Abstand der 2x2-Felder vom Rand
            
            // Berechne Feldgrößen dynamisch basierend auf gridSize, um Überlappungen zu vermeiden
            double fieldSize = gridSize * FIELD_SIZE_RATIO;
            double pieceSize = gridSize * PIECE_SIZE_RATIO;
            
            // Zentriere das Brett horizontal, platziere es vertikal oben mit kleinem Rand
            double boardLeft = (w - size) / 2.0;
            double boardTop = 20; // Kleiner Abstand oben
            
            // Berechne Offset, damit Grid-Zentren korrekt auf Pixel-Zentren fallen
            // Das 11x11 Grid hat Positionen 0..10, also 11 Felder
            // Felder sollen auf den Grid-Zentren liegen: gridSize/2 + i*gridSize
            double gridOffset = gridSize / 2.0;

            // Spielbrett-Hintergrund
            Rectangle boardRect = new Rectangle() {
                Width = size,
                Height = size,
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = BackgroundColor
            };
            Canvas.SetLeft(boardRect, boardLeft);
            Canvas.SetTop(boardRect, boardTop);
            canvas.Children.Add(boardRect);

            // Startfelder (2x2) in den Ecken mit Padding
            // Die 2x2 Felder sollten zentriert auf den Grid-Positionen 0,1 (bzw. 9,10) sein
            // Nutze gridOffset für korrekte Positionierung wie bei Track-Feldern
            DrawStartFields(canvas, boardLeft + gridOffset, boardTop + gridOffset, gridSize, PlayerColors[3], fieldSize);                           // Grün oben links
            DrawStartFields(canvas, boardLeft + gridOffset + 9 * gridSize, boardTop + gridOffset, gridSize, PlayerColors[2], fieldSize);      // gelb oben rechts
            DrawStartFields(canvas, boardLeft + gridOffset, boardTop + gridOffset + 9 * gridSize, gridSize, PlayerColors[0], fieldSize);      // rot unten links
            DrawStartFields(canvas, boardLeft + gridOffset + 9 * gridSize, boardTop + gridOffset + 9 * gridSize, gridSize, PlayerColors[1], fieldSize); // Schwarz unten rechts

            // Laufstrecke: 40 Felder die ein Kreuz-Muster bilden
            // Korrigiert nach Pfeilen: rechte Seite muss auf x=6 sein, nicht x=10
            Point[] trackPoints = new Point[40];
            var track = new System.Collections.Generic.List<(int x, int y)>();
            
            // Linker Arm horizontal (y=4): Start bei (0,4) nach rechts
            track.Add((0, 4)); track.Add((1, 4)); track.Add((2, 4)); track.Add((3, 4)); track.Add((4, 4));  // 0-4
            
            // Oberer linker Arm vertikal (x=4): von (4,3) nach oben
            track.Add((4, 3)); track.Add((4, 2)); track.Add((4, 1)); track.Add((4, 0));  // 5-8
            
            // Obere Mitte horizontal (y=0): nach rechts
            track.Add((5, 0)); track.Add((6, 0));  // 9-10 - Eingang Grün
            
            // Oberer rechter Bereich vertikal (x=6): von (6,1) nach unten
            track.Add((6, 1)); track.Add((6, 2)); track.Add((6, 3)); track.Add((6, 4));  // 11-14
            
            // Rechter Arm vertikal (x=6): weiter nach unten - NICHT x=10!
            track.Add((6, 5)); track.Add((6, 6)); track.Add((6, 7)); track.Add((6, 8)); track.Add((6, 9)); // 15-19
            track.Add((6, 10));  // 20 - Ecke
            
            // Untere Seite horizontal (y=10): von (5,10) nach links
            track.Add((5, 10)); track.Add((4, 10));  // 21-22
            
            // Linker unterer Arm vertikal (x=4): von (4,9) nach oben
            track.Add((4, 9)); track.Add((4, 8)); track.Add((4, 7)); track.Add((4, 6));  // 23-26 - Eingang Rot
            
            // Untere Mitte horizontal (y=6): von (3,6) nach links
            track.Add((3, 6)); track.Add((2, 6)); track.Add((1, 6)); track.Add((0, 6));  // 27-30
            
            // Linker Arm zurück vertikal (x=0): von (0,5) nach oben zum Start
            track.Add((0, 5));  // 31
            
            // Das sind nur 32 Felder, brauchen aber 40
            // Nach den Pfeilen im Bild: es fehlen noch Felder auf den äußeren Kanten
            
            track.Clear();
            
            // NEUE Version nach Pfeil-Analyse:
            // Die Pfeile zeigen: rechte Felder von x=10 nach x=6 verschieben
            
            // Linker Arm (y=4)
            track.Add((0, 4)); track.Add((1, 4)); track.Add((2, 4)); track.Add((3, 4)); track.Add((4, 4));  // 0-4
            
            // Oberer linker Arm (x=4)
            track.Add((4, 3)); track.Add((4, 2)); track.Add((4, 1)); track.Add((4, 0));  // 5-8
            
            // Obere Mitte (y=0)
            track.Add((5, 0)); track.Add((6, 0));  // 9-10
            
            // Oberer rechter Bereich (x=6)
            track.Add((6, 1)); track.Add((6, 2)); track.Add((6, 3)); track.Add((6, 4)); track.Add((6, 5));  // 11-15
            
            // Rechter Arm (x=6) - nach den Pfeilen!
            track.Add((6, 6)); track.Add((6, 7)); track.Add((6, 8)); track.Add((6, 9)); track.Add((6, 10)); // 16-20
            
            // Untere rechte Ecke (y=10)
            track.Add((5, 10)); track.Add((4, 10));  // 21-22
            
            // Linker unterer Arm (x=4)
            track.Add((4, 9)); track.Add((4, 8)); track.Add((4, 7)); track.Add((4, 6));  // 32-35
            
            // Untere Mitte (y=6)
            track.Add((3, 6)); track.Add((2, 6)); track.Add((1, 6)); track.Add((0, 6));  // 36-39
            
            // Immer noch nur 32... Die Pfeile zeigen auch Felder an den äußeren Ecken
            // Analysiere nochmal: es müssen noch 8 Felder sein auf y=10 und x=10
            
            track.Clear();
            
            // FINALE Version mit allen 40 Feldern - KORRIGIERTE REIHENFOLGE:
            // Start bei Grün (0,4) und dann im Uhrzeigersinn rum
            
            // Grün startet hier: (0,4) - Index 0
            track.Add((0, 4)); track.Add((1, 4)); track.Add((2, 4)); track.Add((3, 4)); track.Add((4, 4));  // 0-4
            
            // Nach oben (x=4)
            track.Add((4, 3)); track.Add((4, 2)); track.Add((4, 1)); track.Add((4, 0));  // 5-8
            
            // Nach rechts oben (y=0) - Gelb startet bei Index 10: (6,0)
            track.Add((5, 0)); track.Add((6, 0));  // 9-10 (Gelb Start bei 10)
            
            // Nach unten (x=6)
            track.Add((6, 1)); track.Add((6, 2)); track.Add((6, 3)); track.Add((6, 4)); track.Add((6, 5));  // 11-15
            track.Add((6, 6)); track.Add((6, 7)); track.Add((6, 8)); track.Add((6, 9)); track.Add((6, 10)); // 16-20
            
            // Nach rechts unten (y=10)
            track.Add((7, 10)); track.Add((8, 10)); track.Add((9, 10)); track.Add((10, 10)); // 21-24
            
            // Nach oben rechts (x=10) - Schwarz startet bei Index 25: (10,6) aber sollte bei 24 starten...
            // Schwarz sollte bei (10,6) starten, nicht bei (10,10)!
            // Ich muss die Reihenfolge anders machen
            
            track.Clear();
            
            // Track mit 40 Feldern im Uhrzeigersinn, passend zum visuellen Kreuz-Layout:
            // Start bei Grün (0,4) - Index 0
            
            track.Add((0, 4));  // 0 - Grün Start (A)
            track.Add((1, 4)); track.Add((2, 4)); track.Add((3, 4)); track.Add((4, 4));  // 1-4
            track.Add((4, 3)); track.Add((4, 2)); track.Add((4, 1)); track.Add((4, 0));  // 5-8
            track.Add((5, 0)); track.Add((6, 0));  // 9-10 (Gelb Start (A) bei 10)
            track.Add((6, 1)); track.Add((6, 2)); track.Add((6, 3)); track.Add((6, 4));  // 11-14
            track.Add((7, 4)); track.Add((8, 4)); track.Add((9, 4)); track.Add((10, 4));  // 15-18
            track.Add((10, 5)); track.Add((10, 6));  // 19-20 (Schwarz Start (A) bei 20)
            track.Add((9, 6)); track.Add((8, 6)); track.Add((7, 6)); track.Add((6, 6));  // 21-24
            track.Add((6, 7)); track.Add((6, 8)); track.Add((6, 9)); track.Add((6, 10));  // 25-28
            track.Add((5, 10)); track.Add((4, 10));  // 29-30 (Rot Start (A) bei 30)
            track.Add((4, 9)); track.Add((4, 8)); track.Add((4, 7)); track.Add((4, 6));  // 31-34
            track.Add((3, 6)); track.Add((2, 6)); track.Add((1, 6)); track.Add((0, 6));  // 35-38
            track.Add((0, 5));  // 39
            
            // WICHTIG: Track muss genau 40 Felder haben, dann wird (0,5) durch Modulo erreicht
            // Aber für die Anzeige müssen wir (0,5) explizit zeichnen!

            for (int i = 0; i < 40; i++)
            {
                var p = track[i % track.Count];
                trackPoints[i] = new Point(boardLeft + gridOffset + p.x * gridSize, boardTop + gridOffset + p.y * gridSize);
            }

            // Definiere Startfelder mit ihren Farben und Grid-Koordinaten
            var startFields = new System.Collections.Generic.Dictionary<(int x, int y), (Brush color, string label)>
            {
                { (0, 4), (PlayerColors[3], "A") },   // Grün
                { (4, 10), (PlayerColors[0], "A") },  // Rot
                { (10, 6), (PlayerColors[1], "A") },  // Schwarz
                { (6, 0), (PlayerColors[2], "A") }    // Gelb
            };

            // Zeichne Laufstrecke
            for (int i = 0; i < B1_MAN_Board.TrackLength; i++)
            {
                var p = track[i];
                
                // Prüfe ob dieses Feld ein Startfeld ist
                if (startFields.ContainsKey(p))
                {
                    var (color, label) = startFields[p];
                    DrawFieldWithLabel(canvas, trackPoints[i].X, trackPoints[i].Y, color, Brushes.Black, fieldSize, label);
                }
                else
                {
                    // Check if this is a special field in advanced mode
                    var specialType = board.GetSpecialFieldType(i);
                    if (board.AdvancedMode && specialType != SpecialFieldType.None)
                    {
                        // Draw special field with appropriate color and symbol
                        Brush specialColor = GetSpecialFieldColor(specialType);
                        string specialSymbol = GetSpecialFieldSymbol(specialType);
                        DrawFieldWithLabel(canvas, trackPoints[i].X, trackPoints[i].Y, specialColor, Brushes.Black, fieldSize, specialSymbol);
                    }
                    else
                    {
                        DrawField(canvas, trackPoints[i].X, trackPoints[i].Y, Brushes.White, Brushes.Black, fieldSize);
                    }
                }

                // Zeichne Figur wenn vorhanden
                var piece = board.GetPieceAt(i);
                if (piece != null)
                {
                    DrawPiece(canvas,
                        trackPoints[i].X,
                        trackPoints[i].Y,
                        PlayerColors[piece.Owner - 1],
                        pieceSize,
                        piece.Id + 1);
                }
            }
            
            // Zusätzliches Feld bei (0,5) - muss manuell gezeichnet werden
            double field_0_5_x = boardLeft + gridOffset + 0 * gridSize;
            double field_0_5_y = boardTop + gridOffset + 5 * gridSize;
            DrawField(canvas, field_0_5_x, field_0_5_y, Brushes.White, Brushes.Black, fieldSize);
            
            // Prüfe ob Figur auf Position 39 (= (0,5)) ist
            var piece_39 = board.GetPieceAt(39);
            if (piece_39 != null)
            {
                DrawPiece(canvas,
                    field_0_5_x,
                    field_0_5_y,
                    PlayerColors[piece_39.Owner - 1],
                    pieceSize,
                    piece_39.Id + 1);
            }

            // (Verbindungen entfernt auf Wunsch des Benutzers)

            // Zielfelder als 4er-Reihen, die ein "+" bilden. Das Zentrum (5,5) bleibt frei.
            // Rot (unten nach oben) -> y = 9,8,7,6
            string[] labels = { "a", "b", "c", "d" };
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + gridOffset + 5 * gridSize;
                double y = boardTop + gridOffset + (9 - i) * gridSize;
                DrawFieldWithLabel(canvas, x, y, PlayerColors[0], Brushes.Black, fieldSize, labels[i]);
            }

            // Schwarz (rechts nach links) -> x = 9,8,7,6
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + gridOffset + (9 - i) * gridSize;
                double y = boardTop + gridOffset + 5 * gridSize;
                DrawFieldWithLabel(canvas, x, y, PlayerColors[1], Brushes.Black, fieldSize, labels[i]);
            }

            // Gelb (oben nach unten) -> y = 1,2,3,4
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + gridOffset + 5 * gridSize;
                double y = boardTop + gridOffset + (1 + i) * gridSize;
                DrawFieldWithLabel(canvas, x, y, PlayerColors[2], Brushes.Black, fieldSize, labels[i]);
            }

            // Grün (links nach rechts) -> x = 1,2,3,4
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + gridOffset + (1 + i) * gridSize;
                double y = boardTop + gridOffset + 5 * gridSize;
                DrawFieldWithLabel(canvas, x, y, PlayerColors[3], Brushes.Black, fieldSize, labels[i]);
            }

            // Zeichne Figuren in Zielfeldern und Basis
            foreach (var player in board.Players)
            {
                // Basis-Figuren (2x2), zentriert auf den Startfeldern
                var basePieces = player.Pieces.Where(p => p.IsInBase).ToList();
                for (int i = 0; i < basePieces.Count; i++)
                {
                    // Mappe i auf 2x2 Grid
                    int colIndex = i % 2;
                    int rowIndex = i / 2;
                    double baseAnchorX = 0, baseAnchorY = 0;
                    switch (player.PlayerNumber)
                    {
                        case 1: // Rot -> unten links
                            baseAnchorX = boardLeft + gridOffset;
                            baseAnchorY = boardTop + gridOffset + 9 * gridSize;
                            break;
                        case 2: // Schwarz -> unten rechts
                            baseAnchorX = boardLeft + gridOffset + 9 * gridSize;
                            baseAnchorY = boardTop + gridOffset + 9 * gridSize;
                            break;
                        case 3: // Gelb -> oben rechts
                            baseAnchorX = boardLeft + gridOffset + 9 * gridSize;
                            baseAnchorY = boardTop + gridOffset;
                            break;
                        case 4: // Grün -> oben links
                            baseAnchorX = boardLeft + gridOffset;
                            baseAnchorY = boardTop + gridOffset;
                            break;
                    }
                    double baseX = baseAnchorX + colIndex * gridSize;
                    double baseY = baseAnchorY + rowIndex * gridSize;
                    DrawPiece(canvas, baseX, baseY, PlayerColors[player.PlayerNumber - 1], pieceSize, basePieces[i].Id + 1);
                }

                // Zielfeld-Figuren
                var homePieces = player.Pieces.Where(p => p.IsInHome).ToList();
                foreach (var piece in homePieces)
                {
                    int homeIndex = piece.Position - board.HomeBaseForPlayer(player.PlayerNumber);
                    double homeX = 0, homeY = 0;
                    switch (player.PlayerNumber)
                    {
                        case 1: // Rot (unten nach oben) -> y = 9,8,7,6
                            homeX = boardLeft + gridOffset + 5 * gridSize;
                            homeY = boardTop + gridOffset + (9 - homeIndex) * gridSize;
                            break;
                        case 2: // Schwarz (rechts nach links) -> x = 9,8,7,6
                            homeX = boardLeft + gridOffset + (9 - homeIndex) * gridSize;
                            homeY = boardTop + gridOffset + 5 * gridSize;
                            break;
                        case 3: // Gelb (oben nach unten) -> y = 1,2,3,4
                            homeX = boardLeft + gridOffset + 5 * gridSize;
                            homeY = boardTop + gridOffset + (1 + homeIndex) * gridSize;
                            break;
                        case 4: // Grün (links nach rechts) -> x = 1,2,3,4
                            homeX = boardLeft + gridOffset + (1 + homeIndex) * gridSize;
                            homeY = boardTop + gridOffset + 5 * gridSize;
                            break;
                    }
                    DrawPiece(canvas, homeX, homeY, PlayerColors[player.PlayerNumber - 1], pieceSize, piece.Id + 1);
                }
            }

            // Spielstatus-Text im unteren Bereich
            string statusText = "";
            if (!board.Dice.HasBeenRolled)
            {
                if (board.MustSkipNextTurn)
                {
                    statusText = $"Spieler {board.CurrentPlayer}: AUSSETZEN! (Spezialfeld)";
                }
                else
                {
                    statusText = $"Spieler {board.CurrentPlayer}: Klicke auf den Würfel";
                }
            }
            else
            {
                statusText = $"Spieler {board.CurrentPlayer}: Würfel = {board.Dice.CurrentValue} - Wähle Figur 1-4 zum Bewegen";
                
                if (board.CanRollAgain)
                {
                    statusText += " (Nochmal würfeln!)";
                }
            }
            
            TextBlock status = new TextBlock
            {
                Text = statusText,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            status.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(status, w/2 - status.DesiredSize.Width/2);
            Canvas.SetTop(status, boardTop + size + 20);
            canvas.Children.Add(status);
            
            // Berechne zentrierte Position für Controls
            double controlsY = boardTop + size + 50;
            double diceSize = 70;
            double buttonSize = 50;
            double buttonSpacing = 10;
            
            // Gesamtbreite: Spieler-Anzeige + Würfel + Abstand + 4 Buttons mit Spacing
            double playerIndicatorSize = 40;
            double totalWidth = playerIndicatorSize + 20 + diceSize + 30 + (4 * buttonSize) + (3 * buttonSpacing);
            double controlsStartX = (w - totalWidth) / 2;
            
            // Spieler-Anzeige (farbiger Kreis) links vom Würfel
            double playerIndicatorX = controlsStartX;
            double playerIndicatorY = controlsY + (diceSize - playerIndicatorSize) / 2;
            
            Ellipse playerIndicator = new Ellipse
            {
                Width = playerIndicatorSize,
                Height = playerIndicatorSize,
                Fill = PlayerColors[board.CurrentPlayer - 1],
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            Canvas.SetLeft(playerIndicator, playerIndicatorX);
            Canvas.SetTop(playerIndicator, playerIndicatorY);
            canvas.Children.Add(playerIndicator);
            
            // Spielernummer im Kreis
            TextBlock playerNumberText = new TextBlock
            {
                Text = $"P{board.CurrentPlayer}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            };
            playerNumberText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(playerNumberText, playerIndicatorX + playerIndicatorSize/2 - playerNumberText.DesiredSize.Width/2);
            Canvas.SetTop(playerNumberText, playerIndicatorY + playerIndicatorSize/2 - playerNumberText.DesiredSize.Height/2);
            canvas.Children.Add(playerNumberText);
            
            // Würfel zeichnen - rechts von der Spieler-Anzeige
            double diceX = playerIndicatorX + playerIndicatorSize + 20;
            
            // Würfel-Hintergrund
            Rectangle diceRect = new Rectangle
            {
                Width = diceSize,
                Height = diceSize,
                Fill = board.Dice.HasBeenRolled ? Brushes.LightGray : Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 3,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(diceRect, diceX);
            Canvas.SetTop(diceRect, controlsY);
            
            // Würfel nur klickbar wenn noch nicht gewürfelt
            // WICHTIG: Speichere auch hier den aktuellen Spieler beim Zeichnen!
            int diceDrawingPlayer = board.CurrentPlayer;
            
            if (!board.Dice.HasBeenRolled)
            {
                diceRect.MouseDown += (sender, e) => {
                    e.Handled = true; // Verhindere Event-Bubbling
                    
                    // Frühe Checks BEVOR IsProcessing gesetzt wird
                    if (board.IsProcessing) return;
                    if (board.Dice.HasBeenRolled) return; // Doppelcheck
                    if (board.CurrentPlayer != diceDrawingPlayer) return; // KRITISCH: Nur wenn noch derselbe Spieler!
                    
                    // Jetzt erst IsProcessing setzen
                    board.IsProcessing = true;
                    
                    try
                    {
                        // Würfeln - NUR würfeln, keine Figurenbewegung!
                        board.Dice.Roll();
                        board.SelectedPiece = null;
                        
                        // Prüfe ob der Spieler 3-Würfel-Regel braucht
                        var currentPlayerObj = board.Players.FirstOrDefault(p => p.PlayerNumber == board.CurrentPlayer);
                        if (currentPlayerObj != null)
                        {
                            bool allInBase = currentPlayerObj.Pieces.All(pc => pc.IsInBase);
                            bool anyOnTrack = currentPlayerObj.Pieces.Any(pc => pc.IsOnTrack);
                            bool anyInBase = currentPlayerObj.Pieces.Any(pc => pc.IsInBase);
                            
                            bool needs3Rolls = false;
                            
                            if (allInBase)
                            {
                                needs3Rolls = true;
                            }
                            else if (!anyOnTrack && anyInBase)
                            {
                                // Prüfe ob alle Haus-Figuren auf Endpositionen sind
                                int baseHome = board.HomeBaseForPlayer(board.CurrentPlayer);
                                var homePieces = currentPlayerObj.Pieces.Where(pc => pc.IsInHome).ToList();
                                
                                bool allHomeOnFinalPositions = true;
                                
                                if (homePieces.Count > 0)
                                {
                                    var sortedHome = homePieces.OrderByDescending(pc => pc.Position).ToList();
                                    
                                    // Die hinterste Figur muss auf Slot 3 (d) sein
                                    if (sortedHome[0].Position != baseHome + 3)
                                    {
                                        allHomeOnFinalPositions = false;
                                    }
                                    else if (sortedHome.Count >= 2 && sortedHome[1].Position != baseHome + 2)
                                    {
                                        allHomeOnFinalPositions = false;
                                    }
                                    else if (sortedHome.Count >= 3 && sortedHome[2].Position != baseHome + 1)
                                    {
                                        allHomeOnFinalPositions = false;
                                    }
                                }
                                
                                needs3Rolls = allHomeOnFinalPositions;
                            }
                            
                            if (needs3Rolls)
                            {
                                // Spieler braucht 3 Versuche
                                if (board.RollAttemptsRemaining == 0)
                                {
                                    // Erster Wurf -> 3 Versuche geben
                                    board.RollAttemptsRemaining = 3;
                                }
                                
                                board.RollAttemptsRemaining--;
                                
                                if (board.Dice.CurrentValue == 6)
                                {
                                    // 6 gewürfelt -> Versuche zurücksetzen, Spieler kann ziehen
                                    board.RollAttemptsRemaining = 0;
                                }
                                else if (board.RollAttemptsRemaining > 0)
                                {
                                    // Keine 6, aber noch Versuche übrig -> Würfel zurücksetzen für nächsten Versuch
                                    board.Dice.Reset();
                                }
                                else
                                {
                                    // Keine 6 nach 3 Versuchen -> Turn beenden
                                    board.EndTurn();
                                }
                            }
                        }
                        
                        // Neuzeichnen
                        PaintGameField(canvas, board);
                    }
                    finally
                    {
                        board.IsProcessing = false;
                    }
                };
            }
            canvas.Children.Add(diceRect);
            
            // Würfel-Wert anzeigen - zeige LastRolledValue wenn bei 3-Versuche-Regel
            string diceText = "?";
            if (board.Dice.HasBeenRolled)
            {
                diceText = board.Dice.CurrentValue.ToString();
            }
            else if (board.RollAttemptsRemaining > 0 && board.RollAttemptsRemaining < 3 && board.Dice.LastRolledValue > 0)
            {
                // Zeige letzten Wurf während 3-Versuche-Regel
                diceText = board.Dice.LastRolledValue.ToString();
            }
            
            TextBlock diceValue = new TextBlock
            {
                Text = diceText,
                FontSize = 40,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false  // Klicks fallen durch zum Rectangle
            };
            diceValue.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(diceValue, diceX + diceSize/2 - diceValue.DesiredSize.Width/2);
            Canvas.SetTop(diceValue, controlsY + diceSize/2 - diceValue.DesiredSize.Height/2);
            canvas.Children.Add(diceValue);
            
            // Zeige Würfelversuche wenn Spieler alle Figuren in der Basis hat
            if (board.RollAttemptsRemaining > 0)
            {
                TextBlock attemptsText = new TextBlock
                {
                    Text = $"Versuche: {board.RollAttemptsRemaining}/3",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center
                };
                attemptsText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(attemptsText, diceX + diceSize/2 - attemptsText.DesiredSize.Width/2);
                Canvas.SetTop(attemptsText, controlsY + diceSize + 5);
                canvas.Children.Add(attemptsText);
            }
            
            // Figurenauswahl-Buttons (1-4) - nur sichtbar und klickbar wenn gewürfelt wurde
            if (board.Dice.HasBeenRolled)
            {
                double buttonsStartX = diceX + diceSize + 30;
                double buttonsY = controlsY + diceSize/2 - buttonSize/2;
                
                // Speichere den aktuellen Spieler beim Zeichnen für eindeutige Identifikation
                int drawingPlayer = board.CurrentPlayer;
                
                // Prüfe ob irgendeine Figur bewegt werden kann
                bool anyMovePossible = false;
                for (int i = 0; i < 4; i++)
                {
                    var checkPiece = board.GetPlayerPiece(board.CurrentPlayer, i);
                    if (checkPiece != null && board.CanMovePiece(checkPiece, board.Dice.CurrentValue))
                    {
                        anyMovePossible = true;
                        break;
                    }
                }
                
                for (int i = 0; i < 4; i++)
                {
                    int pieceIndex = i; // Kopie für Lambda - wichtig!
                    
                    double btnX = buttonsStartX + i * (buttonSize + buttonSpacing);
                    
                    // Prüfe ob diese Figur bewegt werden kann
                    var checkPiece = board.GetPlayerPiece(board.CurrentPlayer, pieceIndex);
                    bool canMove = checkPiece != null && board.CanMovePiece(checkPiece, board.Dice.CurrentValue);
                    
                    // Button-Hintergrund - ausgegraut wenn nicht bewegbar
                    Rectangle btnRect = new Rectangle
                    {
                        Width = buttonSize,
                        Height = buttonSize,
                        Fill = canMove ? PlayerColors[board.CurrentPlayer - 1] : Brushes.Gray,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        RadiusX = 5,
                        RadiusY = 5,
                        Opacity = canMove ? 1.0 : 0.4
                    };
                    Canvas.SetLeft(btnRect, btnX);
                    Canvas.SetTop(btnRect, buttonsY);
                    
                    // Nur klickbar machen wenn bewegbar
                    if (canMove)
                    {
                        btnRect.MouseDown += (sender, e) => {
                        e.Handled = true; // Verhindere Event-Bubbling und Doppelklicks
                        
                        // Sofort IsProcessing prüfen - wenn schon verarbeitet wird, KOMPLETT abbrechen
                        if (board.IsProcessing) return;
                        
                        // WICHTIG: Frühe Checks BEVOR wir IsProcessing setzen!
                        if (board.CurrentPlayer != drawingPlayer) return;
                        if (!board.Dice.HasBeenRolled) return;
                        
                        // Jetzt erst IsProcessing setzen, da alle Vorbedingungen erfüllt sind
                        board.IsProcessing = true;
                        
                        try
                        {
                            // Verwende GetPlayerPiece mit gespeichertem drawingPlayer für eindeutige Identifikation
                            var clickPiece = board.GetPlayerPiece(drawingPlayer, pieceIndex);
                            if (clickPiece == null) return;
                            
                            // Prüfe nochmal ob der aktuelle Würfelwert noch gültig ist
                            int diceValue = board.Dice.CurrentValue;
                            if (diceValue <= 0 || diceValue > 6) return;
                            
                            // Figur auswählen und Bewegung ausführen
                            board.SelectedPiece = clickPiece;
                            bool moved = board.TryMoveSelectedPiece();
                            
                            // Prüfe ob eine 6 gewürfelt wurde -> Spieler darf nochmal würfeln
                            bool rolledSix = (diceValue == 6);
                            
                            // Prüfe ob Spezialfeld "Nochmal würfeln" aktiv ist (Advanced Mode)
                            bool canRollAgain = board.CanRollAgain;
                            
                            if (rolledSix || canRollAgain)
                            {
                                // Bei 6 oder RollAgain: Würfel zurücksetzen für nächsten Wurf, Spieler bleibt
                                board.Dice.FullReset();
                                board.SelectedPiece = null;
                                
                                // Reset special field flag if it was used
                                if (canRollAgain)
                                {
                                    board.CanRollAgain = false;
                                }
                            }
                            else
                            {
                                // Keine 6 und kein RollAgain: Spielerwechsel (berücksichtigt Skip automatisch in EndTurn)
                                board.EndTurn();
                            }
                            
                            // Neuzeichnen
                            PaintGameField(canvas, board);
                        }
                        finally
                        {
                            // IMMER zurücksetzen, sonst ist das Spiel blockiert
                            board.IsProcessing = false;
                        }
                    };
                    }
                    canvas.Children.Add(btnRect);
                    
                    // Button-Text (Figurnummer) - auch ausgegraut wenn nicht bewegbar
                    TextBlock btnText = new TextBlock
                    {
                        Text = (i + 1).ToString(),
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Foreground = canMove ? 
                            ((board.CurrentPlayer - 1 == 0 || board.CurrentPlayer - 1 == 2 || board.CurrentPlayer - 1 == 3) ? Brushes.Black : Brushes.White) 
                            : Brushes.DarkGray,
                        TextAlignment = TextAlignment.Center,
                        IsHitTestVisible = false,  // Text blockiert keine Klicks
                        Opacity = canMove ? 1.0 : 0.4
                    };
                    btnText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(btnText, btnX + buttonSize/2 - btnText.DesiredSize.Width/2);
                    Canvas.SetTop(btnText, buttonsY + buttonSize/2 - btnText.DesiredSize.Height/2);
                    canvas.Children.Add(btnText);
                }
                
                // "Zug beenden" Button - nur anzeigen wenn kein Zug möglich ist
                if (!anyMovePossible)
                {
                    double skipBtnWidth = 120;
                    double skipBtnHeight = 40;
                    double skipBtnX = buttonsStartX + 4 * (buttonSize + buttonSpacing) + 20;
                    double skipBtnY = controlsY + diceSize/2 - skipBtnHeight/2;
                    
                    Rectangle skipBtnRect = new Rectangle
                    {
                        Width = skipBtnWidth,
                        Height = skipBtnHeight,
                        Fill = Brushes.Orange,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        RadiusX = 5,
                        RadiusY = 5
                    };
                    Canvas.SetLeft(skipBtnRect, skipBtnX);
                    Canvas.SetTop(skipBtnRect, skipBtnY);
                    
                    skipBtnRect.MouseDown += (sender, e) => {
                        e.Handled = true;
                        if (board.IsProcessing) return;
                        if (board.CurrentPlayer != drawingPlayer) return;
                        if (!board.Dice.HasBeenRolled) return;
                        
                        board.IsProcessing = true;
                        try
                        {
                            // Spielerwechsel ohne Bewegung
                            board.EndTurn();
                            PaintGameField(canvas, board);
                        }
                        finally
                        {
                            board.IsProcessing = false;
                        }
                    };
                    canvas.Children.Add(skipBtnRect);
                    
                    TextBlock skipBtnText = new TextBlock
                    {
                        Text = "Zug beenden",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center,
                        IsHitTestVisible = false
                    };
                    skipBtnText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(skipBtnText, skipBtnX + skipBtnWidth/2 - skipBtnText.DesiredSize.Width/2);
                    Canvas.SetTop(skipBtnText, skipBtnY + skipBtnHeight/2 - skipBtnText.DesiredSize.Height/2);
                    canvas.Children.Add(skipBtnText);
                }
            }
        }
    }
}