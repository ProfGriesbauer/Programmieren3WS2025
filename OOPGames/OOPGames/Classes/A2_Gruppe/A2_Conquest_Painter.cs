using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class A2_ConquestPainter : IPaintGame2
    {
        public string Name => "A2_Conquest_Painter";

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

            var game = gf.Game;
            var field = game.Field;

            // Tiles
            for (int y = 0; y < field.Height; y++)
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

                // Homebase hervorheben
                if (t.IsHomeBase)
                {
                    rect.StrokeThickness = 4;
                    rect.Stroke = Brushes.DimGray;
                }

                Canvas.SetLeft(rect, BoardLeft + x * CellSize + 1);
                Canvas.SetTop(rect, BoardTop + y * CellSize + 1);
                canvas.Children.Add(rect);

                // TargetBase: goldener Ring
                if (t.IsTargetBase)
                {
                    var ring = new Ellipse
                    {
                        Width = CellSize * 0.5,
                        Height = CellSize * 0.5,
                        Stroke = Brushes.Gold,
                        StrokeThickness = 4,
                        Fill = Brushes.Transparent
                    };
                    Canvas.SetLeft(ring, BoardLeft + x * CellSize + CellSize * 0.25);
                    Canvas.SetTop(ring, BoardTop + y * CellSize + CellSize * 0.25);
                    canvas.Children.Add(ring);
                }

                // Boost marker
                if (t.BoostOnTile != BoostType.None)
                {
                    var mark = new Ellipse
                    {
                        Width = CellSize * 0.2,
                        Height = CellSize * 0.2,
                        Fill = Brushes.DarkGreen,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(mark, BoardLeft + x * CellSize + CellSize - 18);
                    Canvas.SetTop(mark, BoardTop + y * CellSize + 6);
                    canvas.Children.Add(mark);
                }

                // Capture progress text
                if (t.IsBeingContested)
                {
                    var tb = new TextBlock
                    {
                        Text = $"{Math.Min(100, t.CaptureProgress)}%",
                        Foreground = Brushes.Black,
                        FontWeight = System.Windows.FontWeights.Bold
                    };
                    Canvas.SetLeft(tb, BoardLeft + x * CellSize + 6);
                    Canvas.SetTop(tb, BoardTop + y * CellSize + 6);
                    canvas.Children.Add(tb);
                }
            }

            // Troops
            foreach (var troop in new[] {
                game.GetTroop(0,0), game.GetTroop(0,1), game.GetTroop(1,0), game.GetTroop(1,1)
            })
            {
                if (troop == null) continue;

                double r = CellSize * 0.28;
                double cx = BoardLeft + troop.X * CellSize + CellSize / 2.0;
                double cy = BoardTop + troop.Y * CellSize + CellSize / 2.0;

                var e = new Ellipse
                {
                    Width = 2 * r,
                    Height = 2 * r,
                    Fill = troop.OwnerId == 0 ? Brushes.DarkRed : Brushes.DarkBlue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                // selected highlight
                if (game.SelectedTroopLocalIndex[troop.OwnerId] == troop.LocalIndex)
                {
                    e.Stroke = Brushes.Gold;
                    e.StrokeThickness = 5;
                }

                Canvas.SetLeft(e, cx - r);
                Canvas.SetTop(e, cy - r);
                canvas.Children.Add(e);
            }
        }

        private Brush OwnerBrush(int ownerId) => ownerId switch
        {
            -1 => Brushes.LightGray,
            0 => Brushes.IndianRed,
            1 => Brushes.LightSteelBlue,
            _ => Brushes.Gray
        };
    }
}
