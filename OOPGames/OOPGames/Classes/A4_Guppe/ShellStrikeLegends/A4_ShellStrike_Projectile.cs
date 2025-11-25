using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class A4_ShellStrike_Projectile
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double VX { get; private set; }
        public double VY { get; private set; }
        public bool Active { get; set; } = true;

        public A4_ShellStrike_Projectile(double x, double y, double vx, double vy)
        {
            X = x; Y = y; VX = vx; VY = vy;
        }

        public void Tick(double gravity, double floorY, double maxX)
        {
            if (!Active) return;
            X += VX;
            Y += VY;
            VY += gravity;
            if (Y >= floorY) Active = false;
            if (X < 0 || X > maxX) Active = false;
        }

        public void Draw(Canvas canvas)
        {
            if (!Active) return;
            Ellipse shot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Black };
            Canvas.SetLeft(shot, X - 3);
            Canvas.SetTop(shot, Y - 3);
            canvas.Children.Add(shot);
        }
    }
}
