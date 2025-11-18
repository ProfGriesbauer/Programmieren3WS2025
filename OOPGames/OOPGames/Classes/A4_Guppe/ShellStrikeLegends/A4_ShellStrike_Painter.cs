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

            // Floor (bottom half)
            var floor = new Rectangle
            {
                Width = w,
                Height = h / 2,
                Fill = new SolidColorBrush(Color.FromRgb(244, 164, 96)) // SandyBrown
            };
            Canvas.SetLeft(floor, 0);
            Canvas.SetTop(floor, h / 2);
            canvas.Children.Add(floor);

            // Hill (flat ellipse in center)
            double hillWidth = w / 3.0;
            double hillHeight = h / 8.0;
            double hillX = (w - hillWidth) / 2.0;
            double hillY = (h / 2.0) - (hillHeight / 2.0);
            var hill = new Ellipse
            {
                Width = hillWidth,
                Height = hillHeight,
                Fill = new SolidColorBrush(Color.FromRgb(205, 133, 63)) // Peru
            };
            Canvas.SetLeft(hill, hillX);
            Canvas.SetTop(hill, hillY);
            canvas.Children.Add(hill);
        }
    }
}
