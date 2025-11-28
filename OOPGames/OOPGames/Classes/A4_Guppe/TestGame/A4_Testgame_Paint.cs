using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    // Simple and readable painter for the TestGame.
    // Draws a rough terrain and two rectangular players on top.
    public class A4_Testgame_Paint : IPaintGame
    {
        public string Name { get { return "A4_Testgame_Paint_Simple"; } }

        // Main paint method called by the framework.
        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            // --- setup canvas ---
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));

            // Use canvas size, fallback to defaults if not ready
            double width = canvas.ActualWidth; if (width <= 0) width = 800;
            double height = canvas.ActualHeight; if (height <= 0) height = 600;

            int w = Math.Max(1, (int)width);
            int h = Math.Max(1, (int)height);

            // --- generate a simple terrain
            // We keep it tiny and easy: start at 75% height and make small steps.
            int[] terrain = new int[w];
            var rnd = new Random(123); // fixed seed so result is stable and easy to understand
            int y = (int)(h * 0.75);
            int minY = (int)(h * 0.35);
            int maxY = (int)(h * 0.95);
            for (int x = 0; x < w; x++)
            {
                if (x > 0)
                {
                    // small step left or right
                    y += rnd.Next(-2, 3);
                    if (y < minY) y = minY;
                    if (y > maxY) y = maxY;
                }
                terrain[x] = y;
            }

            // --- draw terrain as a filled polygon ---
            var brush = new SolidColorBrush(Color.FromRgb(139, 69, 19));
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(new Point(0, h), true, true);
                ctx.LineTo(new Point(0, terrain[0]), true, false);
                for (int x = 1; x < w; x++) ctx.LineTo(new Point(x, terrain[x]), true, false);
                ctx.LineTo(new Point(w - 1, h), true, false);
            }
            geom.Freeze();
            canvas.Children.Add(new Path { Data = geom, Fill = brush });

            // --- draw players (if field provides them) ---
            if (currentField is A4_Testgame_Field f && f.Players != null)
            {
                double rectW = Math.Max(12, w * 0.03);
                double rectH = rectW * 1.5;
                foreach (var p in f.Players)
                {
                    double fx = Math.Max(0.0, Math.Min(1.0, p.XFrac));
                    double px = fx * (w - 1);
                    int ix = (int)Math.Round(px);
                    if (ix < 0) ix = 0; if (ix >= w) ix = w - 1;
                    double ground = terrain[ix];

                    var rect = new Rectangle
                    {
                        Width = rectW,
                        Height = rectH,
                        Fill = (p.PlayerNumber == 1) ? Brushes.DodgerBlue : Brushes.Crimson,
                    };
                    Canvas.SetLeft(rect, px - rectW / 2);
                    Canvas.SetTop(rect, ground - rectH);
                    Canvas.SetZIndex(rect, 20);
                    canvas.Children.Add(rect);
                }
            }
        }
    }
}
