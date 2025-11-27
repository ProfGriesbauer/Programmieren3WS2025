using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class A2_ConquestPainter : IPaintGame2
    {
        public string Name => "A2_Conquest_Painter";

        // Muss zum HumanPlayer-Mapping passen:
        public const int BoardLeft = 20;
        public const int BoardTop = 20;
        public const int CellSize = 60;

        public void PaintGameField(Canvas canvas, IGameField currentField)
            => Draw(canvas, currentField as A2_ConquestGameField);

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
            => Draw(canvas, currentField as A2_ConquestGameField);

        private void Draw(Canvas canvas, A2_ConquestGameField gf)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            if (gf?.Game?.Field == null) return;

            var field = gf.Game.Field;

            // Grid + Tiles
            for (int y = 0; y < field.Height; y++)
            {
                for (int x = 0; x < field.Width; x++)
                {
                    var t = field.GetTile(x, y);

                    var rect = new Rectangle
                    {
                        Width = CellSize - 2,
                        Height = CellSize - 2,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Fill = OwnerBrush(t.OwnerID),
                        RadiusX = 6,
                        RadiusY = 6
                    };

                    Canvas.SetLeft(rect, BoardLeft + x * CellSize + 1);
                    Canvas.SetTop(rect, BoardTop + y * CellSize + 1);
                    canvas.Children.Add(rect);

                    // Objective: kleiner Kreis
                    if (t.IsObjective)
                    {
                        var circ = new Ellipse
                        {
                            Width = CellSize * 0.35,
                            Height = CellSize * 0.35,
                            Stroke = Brushes.Gold,
                            StrokeThickness = 3,
                            Fill = Brushes.Transparent
                        };
                        Canvas.SetLeft(circ, BoardLeft + x * CellSize + CellSize * 0.325);
                        Canvas.SetTop(circ, BoardTop + y * CellSize + CellSize * 0.325);
                        canvas.Children.Add(circ);
                    }

                    // Contested: Fortschritt als Text
                    if (t.IsBeingContested)
                    {
                        var tb = new TextBlock
                        {
                            Text = $"{Math.Min(100, t.CaptureProgress)}%",
                            Foreground = Brushes.Black
                        };
                        Canvas.SetLeft(tb, BoardLeft + x * CellSize + 6);
                        Canvas.SetTop(tb, BoardTop + y * CellSize + 6);
                        canvas.Children.Add(tb);
                    }

                    // Boost: kleines "B"
                    if (t.BoostOnTile != BoostType.None)
                    {
                        var tb = new TextBlock
                        {
                            Text = "B",
                            Foreground = Brushes.DarkGreen,
                            FontWeight = System.Windows.FontWeights.Bold
                        };
                        Canvas.SetLeft(tb, BoardLeft + x * CellSize + CellSize - 16);
                        Canvas.SetTop(tb, BoardTop + y * CellSize + 4);
                        canvas.Children.Add(tb);
                    }
                }
            }
            // Troops zeichnen (kleine Kreise)
            foreach (var troop in gf.Game.Troops)
            {
                var circ = new Ellipse
                {
                    Width = CellSize * 0.35,
                    Height = CellSize * 0.35,
                    StrokeThickness = 2,
                    Stroke = Brushes.Black,
                    Fill = troop.OwnerId == 0 ? Brushes.DarkRed : Brushes.DarkBlue
                };

                Canvas.SetLeft(circ, BoardLeft + troop.X * CellSize + CellSize * 0.325);
                Canvas.SetTop(circ, BoardTop + troop.Y * CellSize + CellSize * 0.325);
                canvas.Children.Add(circ);
            }

        }

        private Brush OwnerBrush(int ownerId)
        {
            return ownerId switch
            {
                -1 => Brushes.LightGray,
                0 => Brushes.IndianRed,
                1 => Brushes.LightSteelBlue,
                _ => Brushes.Gray
            };
        }
    }
}
