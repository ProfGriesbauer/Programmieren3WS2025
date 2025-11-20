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
            DrawStartFields(canvas, boardLeft + CORNER_PADDING, boardTop + CORNER_PADDING, gridSize, PlayerColors[3], fieldSize);                           // Grün oben links
            DrawStartFields(canvas, boardLeft + size - CORNER_PADDING - gridSize, boardTop + CORNER_PADDING, gridSize, PlayerColors[2], fieldSize);      // gelb oben rechts
            DrawStartFields(canvas, boardLeft + CORNER_PADDING, boardTop + size - CORNER_PADDING - gridSize, gridSize, PlayerColors[0], fieldSize);      // rot unten links
            DrawStartFields(canvas, boardLeft + size - CORNER_PADDING - gridSize, boardTop + size - CORNER_PADDING - gridSize, gridSize, PlayerColors[1], fieldSize); // Schwarz unten rechts

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
            track.Add((4, 9)); track.Add((4, 8)); track.Add((4, 7)); track.Add((4, 6));  // 23-26
            
            // Untere Mitte (y=6)
            track.Add((3, 6)); track.Add((2, 6)); track.Add((1, 6)); track.Add((0, 6));  // 27-30
            
            // Zurück zum Start (x=0)
            track.Add((0, 5));  // 31
            
            // Immer noch nur 32... Die Pfeile zeigen auch Felder an den äußeren Ecken
            // Analysiere nochmal: es müssen noch 8 Felder sein auf y=10 und x=10
            
            track.Clear();
            
            // FINALE Version mit allen 40 Feldern:
            // Linker Arm (y=4)
            track.Add((0, 4)); track.Add((1, 4)); track.Add((2, 4)); track.Add((3, 4)); track.Add((4, 4));  // 0-4
            
            // Oberer linker Arm (x=4)
            track.Add((4, 3)); track.Add((4, 2)); track.Add((4, 1)); track.Add((4, 0));  // 5-8
            
            // Obere Mitte (y=0)
            track.Add((5, 0)); track.Add((6, 0));  // 9-10
            
            // Oberer rechter Bereich (x=6) - eingeklappt!
            track.Add((6, 1)); track.Add((6, 2)); track.Add((6, 3)); track.Add((6, 4)); track.Add((6, 5));  // 11-15
            
            // Rechter Arm (x=6)
            track.Add((6, 6)); track.Add((6, 7)); track.Add((6, 8)); track.Add((6, 9)); track.Add((6, 10)); // 16-20
            
            // Zusätzliche Felder bei y=4 (horizontal)
            track.Add((7, 4)); track.Add((8, 4)); track.Add((9, 4)); // 21-23
            
            // Untere rechte Ecke - blau markierte Felder GELÖSCHT
            // ENTFERNT: track.Add((7, 10)); track.Add((8, 10)); track.Add((9, 10)); track.Add((10, 10));
            
            // Rechte Seite nach oben (x=10) - (10,3) GELÖSCHT!
            track.Add((10, 6)); track.Add((10, 5)); track.Add((10, 4)); // 24-26 (nur noch 3 Felder)
            
            // Zusätzliche Felder bei y=6 (horizontal)
            track.Add((9, 6)); track.Add((8, 6)); track.Add((7, 6)); // 27-29
            
            // Zurück zur Mitte (y=10)
            track.Add((5, 10)); track.Add((4, 10));  // 30-31
            
            // Linker unterer Arm (x=4)
            track.Add((4, 9)); track.Add((4, 8)); track.Add((4, 7)); track.Add((4, 6));  // 32-35
            
            // Untere Mitte (y=6)
            track.Add((3, 6)); track.Add((2, 6)); track.Add((1, 6)); track.Add((0, 6));  // 36-39
            
            // WICHTIG: Track muss genau 40 Felder haben, dann wird (0,5) durch Modulo erreicht
            // Aber für die Anzeige müssen wir (0,5) explizit zeichnen!

            for (int i = 0; i < 40; i++)
            {
                var p = track[i % track.Count];
                trackPoints[i] = new Point(boardLeft + gridOffset + p.x * gridSize, boardTop + gridOffset + p.y * gridSize);
            }

            // Zeichne Laufstrecke
            for (int i = 0; i < B1_MAN_Board.TrackLength; i++)
            {
                DrawField(canvas, trackPoints[i].X, trackPoints[i].Y, Brushes.White, Brushes.Black, fieldSize);

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
                        case 1: // Rot -> unten links, Start bei CORNER_PADDING
                            baseAnchorX = boardLeft + CORNER_PADDING;
                            baseAnchorY = boardTop + size - CORNER_PADDING - gridSize;
                            break;
                        case 2: // Schwarz -> unten rechts
                            baseAnchorX = boardLeft + size - CORNER_PADDING - gridSize;
                            baseAnchorY = boardTop + size - CORNER_PADDING - gridSize;
                            break;
                        case 3: // Gelb -> oben rechts
                            baseAnchorX = boardLeft + size - CORNER_PADDING - gridSize;
                            baseAnchorY = boardTop + CORNER_PADDING;
                            break;
                        case 4: // Grün -> oben links
                            baseAnchorX = boardLeft + CORNER_PADDING;
                            baseAnchorY = boardTop + CORNER_PADDING;
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
                            homeX = boardLeft + gridOffset + gridSize;
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

            // Spielstatus-Text im unteren Bereich (reserviert für Würfel und Figurenauswahl)
            TextBlock status = new TextBlock
            {
                Text = "Würfeln und auf ein gültiges Feld klicken",
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(status, 20);
            Canvas.SetTop(status, boardTop + size + 20); // Unter dem Spielfeld
            canvas.Children.Add(status);
            
            // Platzhalter für Würfel-Animation (kann später implementiert werden)
            // Bereich: Y = boardTop + size + 50, Höhe ca. 70px
        }
    }
}