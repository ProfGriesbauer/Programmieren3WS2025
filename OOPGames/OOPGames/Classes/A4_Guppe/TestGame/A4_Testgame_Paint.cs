using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace OOPGames
{
    // Simple painter for testing: draws a solid triangle centered in the canvas
    public class A4_Testgame_Paint : IPaintGame
    {
        public string Name { get { return "A4_Testgame_Paint_Triangle"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            // clear canvas and set a neutral background
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0) w = 800;
            if (h <= 0) h = 600;

            // triangle size and center
            double size = Math.Min(w, h) * 0.25; // quarter of smallest dimension
            double cx = w / 2.0;
            double cy = h / 2.0;

            // create an upright isosceles triangle
            Polygon tri = new Polygon();
            tri.Points = new PointCollection()
            {
                new Point(cx, cy - size),            // top
                new Point(cx - size, cy + size),    // bottom-left
                new Point(cx + size, cy + size)     // bottom-right
            };
            tri.Fill = new SolidColorBrush(Color.FromRgb(200, 80, 40));
            tri.Stroke = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            tri.StrokeThickness = 3.0;

            // position and add
            Canvas.SetLeft(tri, 0);
            Canvas.SetTop(tri, 0);
            canvas.Children.Add(tri);
        }
    }
}
