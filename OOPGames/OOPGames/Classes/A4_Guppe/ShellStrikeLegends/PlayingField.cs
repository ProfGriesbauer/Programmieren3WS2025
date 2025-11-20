using System;
using System.Drawing;

namespace OOPGames.Classes.A4_Guppe.ShellStrikeLegends
{
    public class PlayingField
    {
        public int Width { get; }
        public int Height { get; }

        public PlayingField(int width, int height)
        {
            Width = width;
            Height = height;
        }

        // Draws a floor with a small flat hill in the middle
        public void Draw(Graphics g)
        {
            // Draw base floor
            g.FillRectangle(Brushes.SandyBrown, 0, Height / 2, Width, Height / 2);

            // Draw hill (simple flat ellipse in the center)
            int hillWidth = Width / 3;
            int hillHeight = Height / 8;
            int hillX = (Width - hillWidth) / 2;
            int hillY = (Height / 2) - (hillHeight / 2);
            g.FillEllipse(Brushes.Peru, hillX, hillY, hillWidth, hillHeight);
        }
    }
}
