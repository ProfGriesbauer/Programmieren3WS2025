using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using OOPGames.Classes.A4_Guppe.ShellStrikeLegends;

namespace OOPGames.Classes.A4_Guppe.ShellStrikeLegends
{
    public class ShellStrikeLegendsFieldViewer : UserControl
    {
        private PlayingField _field;

        public ShellStrikeLegendsFieldViewer()
        {
            _field = new PlayingField(Config.FieldWidth, Config.FieldHeight);
            this.Width = Config.FieldWidth;
            this.Height = Config.FieldHeight;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // Use GDI+ Graphics from DrawingContext
            // For demonstration, draw using WPF brushes
            Rect floorRect = new Rect(0, this.Height / 2, this.Width, this.Height / 2);
            dc.DrawRectangle(Brushes.SandyBrown, null, floorRect);

            double hillWidth = this.Width / 3;
            double hillHeight = this.Height / 8;
            double hillX = (this.Width - hillWidth) / 2;
            double hillY = (this.Height / 2) - (hillHeight / 2);
            dc.DrawEllipse(Brushes.Peru, null, new Point(hillX + hillWidth / 2, hillY + hillHeight / 2), hillWidth / 2, hillHeight / 2);
        }
    }
}
