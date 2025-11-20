using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    public class A4_ShellStrike_Painter : IPaintGame
    {
        public string Name => "A4 ShellStrikeLegends Painter";

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(135, 206, 235)); // SkyBlue

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0) w = 800; // fallback
            if (h <= 0) h = 400; // fallback

            // Shared terrain color (Peru) for both floor and hill
            var terrainBrush = new SolidColorBrush(Color.FromRgb(205, 133, 63));

            // Floor (slimmer than before: 1/5 of total height)
            double floorH = h / 5.0;
            var floor = new Rectangle
            {
                Width = w,
                Height = floorH,
                Fill = terrainBrush
            };
            Canvas.SetLeft(floor, 0);
            Canvas.SetTop(floor, h - floorH);

            // Hill as a diamond (rhombus) centered horizontally
            double hillWidth = w / 3.0;      // wide, flat look
            double hillHeight = h / 10.0;    // lower height for flatter shape
            double hw = hillWidth / 2.0;
            double hh = hillHeight / 2.0;
            double cx = w / 2.0;             // center x
            double floorTop = h - floorH;    // y where the floor starts
            double cy = floorTop;            // place the diamond's horizontal line on the floor

            var diamond = new Polygon
            {
                Fill = terrainBrush,
                Points = new PointCollection
                {
                    new Point(cx - hw, cy),     // left on floor line
                    new Point(cx, cy - hh),     // top above floor
                    new Point(cx + hw, cy),     // right on floor line
                    new Point(cx, cy + hh)      // bottom below floor (will be covered by floor)
                }
            };
            // Draw hill first so the floor covers its bottom half -> looks like a hill protruding
            canvas.Children.Add(diamond);
            canvas.Children.Add(floor);
        }
    }
}
