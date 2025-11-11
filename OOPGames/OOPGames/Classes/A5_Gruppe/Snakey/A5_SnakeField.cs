using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A5_SnakeField : IGameField
    {
        // Spielfeld in Pixeln
        public const int FIELD_WIDTH = 600;   // Pixel
        public const int FIELD_HEIGHT = 600;  // Pixel
        public const int SNAKE_SIZE = 40;     // Größe eines Schlangensegments in Pixeln (doppelt so groß)
        
        public List<PixelPosition> Snake { get; private set; }
        public PixelPosition Direction { get; set; }
        private System.Windows.Threading.DispatcherTimer gameTimer;
        private bool isGameRunning = false;
        private const double SPEED = 10.0; // Pixel pro Frame (5x schneller)

        public class PixelPosition
        {
            public double X { get; set; }
            public double Y { get; set; }

            public PixelPosition(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A5_SnakePaint;
        }

        public A5_SnakeField()
        {
            Snake = new List<PixelPosition>();
            InitializeSnake();
            
            // Timer für flüssige Bewegung - läuft schneller für pixel-basierte Updates
            gameTimer = new System.Windows.Threading.DispatcherTimer();
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ca. 60 FPS
            gameTimer.Start();
            isGameRunning = true;
        }

        private void InitializeSnake()
        {
            // Start in der Mitte des Spielfelds
            double startX = FIELD_WIDTH / 2.0;
            double startY = FIELD_HEIGHT / 2.0;
            
            Snake = new List<PixelPosition>();
            // Starte nur mit dem Kopf
            Snake.Add(new PixelPosition(startX, startY));
            
            Direction = new PixelPosition(1, 0); // Start nach rechts
        }

        public void MoveSnake()
        {
            if (Snake.Count == 0) return;
            
            var head = Snake[0];
            
            // Berechne neue Kopfposition mit kleinen Pixel-Schritten
            var newHead = new PixelPosition(
                x: head.X + (Direction.X * SPEED),
                y: head.Y + (Direction.Y * SPEED)
            );
            
            // Überprüfe Kollision mit Wänden
            if (newHead.X < 0 || newHead.X >= FIELD_WIDTH || 
                newHead.Y < 0 || newHead.Y >= FIELD_HEIGHT)
            {
                // Game Over - für jetzt einfach zurücksetzen
                InitializeSnake();
                return;
            }

            // Füge neuen Kopf hinzu
            Snake.Insert(0, newHead);
            
            // Entferne das letzte Segment (Snake behält konstante Länge)
            Snake.RemoveAt(Snake.Count - 1);
        }

        public void ChangeDirection(double dx, double dy)
        {
            // Verhindere 180-Grad-Drehungen (keine Umkehr erlaubt)
            if (!IsOppositeDirection(dx, dy))
            {
                Direction = new PixelPosition(dx, dy);
            }
        }

        private bool IsOppositeDirection(double dx, double dy)
        {
            // Prüfe, ob die neue Richtung genau entgegengesetzt zur aktuellen ist
            return (dx != 0 && dx == -Direction.X) || (dy != 0 && dy == -Direction.Y);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameRunning)
            {
                MoveSnake();
            }
        }

        // Legacy-Methode für Kompatibilität mit IGameField - nicht mehr verwendet
        public int GetPosition(int x, int y)
        {
            return 0;
        }
    }
}