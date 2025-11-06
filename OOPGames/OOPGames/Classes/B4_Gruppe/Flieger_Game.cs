using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPGames
{
    public class B4_XXX : Form
    {
        Timer gameTimer = new Timer();
        List<Rectangle> lasers = new List<Rectangle>();
        Rectangle player = new Rectangle(200, 350, 40, 20);
        int playerSpeed = 8;
        int laserSpeed = 6;
        Random rand = new Random();
        bool gameOver = false;
        bool leftPressed = false;
        bool rightPressed = false;

        public B4_XXX()
        {
            this.Text = "Schwarz-Weiß Flugzeugspiel";
            this.ClientSize = new Size(400, 400);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            gameTimer.Interval = 30; // ~33 FPS
            gameTimer.Tick += Update;
            gameTimer.Start();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            this.Paint += Draw;
        }

        void Update(object sender, EventArgs e)
        {
            if (gameOver) return;

            // Spielerbewegung
            if (leftPressed && player.X > 0)
                player.X -= playerSpeed;
            if (rightPressed && player.X < ClientSize.Width - player.Width)
                player.X += playerSpeed;

            // Neue Laser hinzufügen
            if (rand.Next(20) == 0)
            {
                int x = rand.Next(0, ClientSize.Width - 10);
                lasers.Add(new Rectangle(x, 0, 5, 20));
            }

            // Laser bewegen
            for (int i = 0; i < lasers.Count; i++)
            {
                var l = lasers[i];
                l.Y += laserSpeed;
                lasers[i] = l;
            }

            // Laser entfernen, die unten raus sind
            lasers.RemoveAll(l => l.Y > ClientSize.Height);

            // Kollision prüfen
            foreach (var l in lasers)
            {
                if (l.IntersectsWith(player))
                {
                    gameOver = true;
                    gameTimer.Stop();
                }
            }

            Invalidate();
        }

        void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Flugzeug zeichnen
            g.FillRectangle(Brushes.Black, player);

            // Laser zeichnen
            foreach (var l in lasers)
                g.FillRectangle(Brushes.Black, l);

            if (gameOver)
            {
                g.DrawString("GAME OVER", new Font("Arial", 20), Brushes.Black, 100, 150);
            }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) leftPressed = true;
            if (e.KeyCode == Keys.Right) rightPressed = true;

            if (e.KeyCode == Keys.Space && gameOver)
            {
                lasers.Clear();
                player.X = 200;
                gameOver = false;
                gameTimer.Start();
            }
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) leftPressed = false;
            if (e.KeyCode == Keys.Right) rightPressed = false;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new B4_XXX());
        }
    }
}


