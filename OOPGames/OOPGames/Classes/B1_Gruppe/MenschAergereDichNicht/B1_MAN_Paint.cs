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

        private const double FIELD_SIZE = 40;     // Größere Felder
        private const double PIECE_SIZE = 32;     // Größere Spielfiguren
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

        // Zeichne 2x2 Startfelder, ausgerichtet an Grid-Zentren (gridSize übergeben)
        private void DrawStartFields(Canvas canvas, double x, double y, double gridSize, Brush color)
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
                        FIELD_SIZE);
                }
            }
        }

        private void DrawHomeFields(Canvas canvas, double startX, double startY, Brush color, int direction)
        {
            // Zielfelder in einer Linie
            for (int i = 0; i < 4; i++)
            {
                double x = startX;
                double y = startY + i * (FIELD_SIZE + FIELD_GAP) * direction;
                DrawField(canvas, x, y, color, Brushes.Black, FIELD_SIZE);
            }
        }

        public string Name => "B1_MAN_Paint";

        public void PaintGameField(Canvas canvas, OOPGames.IGameField currentField)
        {
            canvas.Children.Clear();
            if (!(currentField is B1_MAN_Board board)) return;

            // Spielbrett-Grundmaße
            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 550;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 550;
            double size = Math.Min(w, h) - 60; // Mehr Rand für bessere Zentrierung
            double gridSize = size / 11;  // 11x11 Grid wie im Original
            const double CORNER_PADDING = 20; // Abstand der 2x2-Felder vom Rand
            // Zentriere das Brett innerhalb des Canvas
            double boardLeft = (w - size) / 2.0;
            double boardTop = (h - size) / 2.0;

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
            DrawStartFields(canvas, boardLeft + CORNER_PADDING, boardTop + CORNER_PADDING, gridSize, PlayerColors[2]);                           // Gelb oben links
            DrawStartFields(canvas, boardLeft + size - CORNER_PADDING - 2*gridSize, boardTop + CORNER_PADDING, gridSize, PlayerColors[3]);      // Grün oben rechts
            DrawStartFields(canvas, boardLeft + CORNER_PADDING, boardTop + size - CORNER_PADDING - 2*gridSize, gridSize, PlayerColors[1]);      // Schwarz unten links
            DrawStartFields(canvas, boardLeft + size - CORNER_PADDING - 2*gridSize, boardTop + size - CORNER_PADDING - 2*gridSize, gridSize, PlayerColors[0]); // Rot unten rechts

            // Laufstrecke: einfache quadratische Ring-Anordnung auf der äußeren 11x11-Perimeterlinie.
            // Das erzeugt genau 40 Felder (Perimeter eines 11×11 Gitters): wir laufen im Uhrzeigersinn
            // beginnend unten links (x=0,y=10) → rechts → oben → links zurück.
            Point[] trackPoints = new Point[40];
            var perim = new System.Collections.Generic.List<(int x, int y)>();
            // bottom edge: x=0..10, y=10
            for (int x = 0; x <= 10; x++) perim.Add((x, 10));
            // right edge: x=10, y=9..0
            for (int y = 9; y >= 0; y--) perim.Add((10, y));
            // top edge: x=9..0, y=0
            for (int x = 9; x >= 0; x--) perim.Add((x, 0));
            // left edge: x=0, y=1..9
            for (int y = 1; y <= 9; y++) perim.Add((0, y));

            // Sanity: perim sollte 40 Elemente haben
            if (perim.Count != 40)
            {
                // fallback: fill mit ersten 40 Gitterzellen falls unerwartet
                perim.Clear();
                for (int x = 0; x <= 10; x++) for (int y = 0; y <= 10; y++) if (perim.Count < 40) perim.Add((x, y));
            }

            // Falte die äußeren und mittleren Punkte nach innen, damit sie an den Linien des + anliegen
            for (int i = 0; i < perim.Count; i++)
            {
                var v = perim[i];
                // Unten: x=0..6 -> falte zur 9er Linie (außer x=5 ist Zentrum)
                if (v.y == 10 && v.x <= 6 && v.x != 5) perim[i] = (v.x + 1, 9);
                // Unten: x=4..10 -> falte zur 9er Linie (außer x=5 ist Zentrum)
                else if (v.y == 10 && v.x >= 4) perim[i] = (v.x - 1, 9);
                
                // Rechts: y=4..10 -> falte zur 9er Linie (außer y=5 ist Zentrum)
                else if (v.x == 10 && v.y >= 4) perim[i] = (9, v.y - 1);
                // Rechts: y=0..6 -> falte zur 1er Linie (außer y=5 ist Zentrum)
                else if (v.x == 10 && v.y <= 6 && v.y != 5) perim[i] = (9, v.y + 1);
                
                // Oben: x=4..10 -> falte zur 1er Linie (außer x=5 ist Zentrum)
                else if (v.y == 0 && v.x >= 4) perim[i] = (v.x - 1, 1);
                // Oben: x=0..6 -> falte zur 1er Linie (außer x=5 ist Zentrum)
                else if (v.y == 0 && v.x <= 6 && v.x != 5) perim[i] = (v.x + 1, 1);
                
                // Links: y=0..6 -> falte zur 1er Linie (außer y=5 ist Zentrum)
                else if (v.x == 0 && v.y <= 6 && v.y != 5) perim[i] = (1, v.y + 1);
                // Links: y=4..10 -> falte zur 9er Linie (außer y=5 ist Zentrum)
                else if (v.x == 0 && v.y >= 4) perim[i] = (1, v.y - 1);
            }

            for (int i = 0; i < 40; i++)
            {
                var p = perim[i];
                trackPoints[i] = new Point(boardLeft + p.x * gridSize, boardTop + p.y * gridSize);
            }

            // Zeichne Laufstrecke
            for (int i = 0; i < B1_MAN_Board.TrackLength; i++)
            {
                DrawField(canvas, trackPoints[i].X, trackPoints[i].Y, Brushes.White, Brushes.Black, FIELD_SIZE);

                // Zeichne Figur wenn vorhanden
                var piece = board.GetPieceAt(i);
                if (piece != null)
                {
                    DrawField(canvas,
                        trackPoints[i].X,
                        trackPoints[i].Y,
                        PlayerColors[piece.Owner - 1],
                        Brushes.Black,
                        PIECE_SIZE);
                }
            }

            // (Verbindungen entfernt auf Wunsch des Benutzers)

            // Zielfelder als 4er-Reihen, die ein "+" bilden. Das Zentrum (5,5) bleibt frei.
            // Rot (unten nach oben) -> y = 9,8,7,6
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + 5 * gridSize;
                double y = boardTop + (9 - i) * gridSize;
                DrawField(canvas, x, y, PlayerColors[0], Brushes.Black, FIELD_SIZE);
            }

            // Schwarz (rechts nach links) -> x = 9,8,7,6
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + (9 - i) * gridSize;
                double y = boardTop + 5 * gridSize;
                DrawField(canvas, x, y, PlayerColors[1], Brushes.Black, FIELD_SIZE);
            }

            // Gelb (oben nach unten) -> y = 1,2,3,4
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + 5 * gridSize;
                double y = boardTop + (1 + i) * gridSize;
                DrawField(canvas, x, y, PlayerColors[2], Brushes.Black, FIELD_SIZE);
            }

            // Grün (links nach rechts) -> x = 1,2,3,4
            for (int i = 0; i < 4; i++)
            {
                double x = boardLeft + (1 + i) * gridSize;
                double y = boardTop + 5 * gridSize;
                DrawField(canvas, x, y, PlayerColors[3], Brushes.Black, FIELD_SIZE);
            }

            // Zeichne Figuren in Zielfeldern und Basis
            foreach (var player in board.Players)
            {
                // Basis-Figuren (2x2), positioniert an den neuen Start-Anchor-Punkten
                var basePieces = player.Pieces.Where(p => p.IsInBase).ToList();
                for (int i = 0; i < basePieces.Count; i++)
                {
                    // Mappe i auf 2x2 Grid
                    int colIndex = i % 2;
                    int rowIndex = i / 2;
                    double baseAnchorX = 0, baseAnchorY = 0;
                    switch (player.PlayerNumber)
                    {
                        case 1: // Rot -> unten links (Grid 2,8)
                            baseAnchorX = boardLeft + 2 * gridSize;
                            baseAnchorY = boardTop + 8 * gridSize;
                            break;
                        case 2: // Schwarz -> unten rechts (Grid 8,8)
                            baseAnchorX = boardLeft + 8 * gridSize;
                            baseAnchorY = boardTop + 8 * gridSize;
                            break;
                        case 3: // Gelb -> oben rechts (Grid 8,2)
                            baseAnchorX = boardLeft + 8 * gridSize;
                            baseAnchorY = boardTop + 2 * gridSize;
                            break;
                        case 4: // Grün -> oben links (Grid 2,2)
                            baseAnchorX = boardLeft + 2 * gridSize;
                            baseAnchorY = boardTop + 2 * gridSize;
                            break;
                    }
                    double baseX = baseAnchorX + colIndex * gridSize;
                    double baseY = baseAnchorY + rowIndex * gridSize;
                    DrawField(canvas, baseX, baseY, PlayerColors[player.PlayerNumber - 1], Brushes.Black, PIECE_SIZE);
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
                            homeX = boardLeft + 5 * gridSize;
                            homeY = boardTop + (9 - homeIndex) * gridSize;
                            break;
                        case 2: // Schwarz (rechts nach links) -> x = 9,8,7,6
                            homeX = boardLeft + (9 - homeIndex) * gridSize;
                            homeY = boardTop + 5 * gridSize;
                            break;
                        case 3: // Gelb (oben nach unten) -> y = 1,2,3,4
                            homeX = boardLeft + 5 * gridSize;
                            homeY = boardTop + (1 + homeIndex) * gridSize;
                            break;
                        case 4: // Grün (links nach rechts) -> x = 1,2,3,4
                            homeX = boardLeft + (1 + homeIndex) * gridSize;
                            homeY = boardTop + 5 * gridSize;
                            break;
                    }
                    DrawField(canvas, homeX, homeY, PlayerColors[player.PlayerNumber - 1], Brushes.Black, PIECE_SIZE);
                }
            }

            // Spielstatus-Text
            TextBlock status = new TextBlock
            {
                Text = "Würfeln und auf ein gültiges Feld klicken",
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(status, 20);
            Canvas.SetTop(status, 20 + size + 10);
            canvas.Children.Add(status);
        }
    }
}