using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A5_SnakeField : IGameField
    {
        public const int FIELD_SIZE = 20;
        private int[,] _field;
        public List<Position> Snake { get; private set; }
        public Position Direction { get; set; }
        private System.Windows.Threading.DispatcherTimer gameTimer;
        private bool isGameRunning = false;

        public class Position
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Position(int x, int y)
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
            _field = new int[FIELD_SIZE, FIELD_SIZE];
            Snake = new List<Position>();
            InitializeSnake();
            
            // Timer für automatische Bewegung
            gameTimer = new System.Windows.Threading.DispatcherTimer();
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Interval = TimeSpan.FromMilliseconds(200); // Geschwindigkeit der Schlange
            gameTimer.Start();
            isGameRunning = true;
        }

        private void InitializeSnake()
        {
            // Start in der vertikalen Mitte, ganz links, nach rechts bewegend
            int startY = FIELD_SIZE / 2;  // Mitte der Höhe
            int startX = 0;               // Ganz links
            Snake = new List<Position>();
            Snake.Add(new Position(startX, startY));
            Direction = new Position(1, 0); // Start nach rechts
            UpdateField();
        }

        private void UpdateField()
        {
            // Feld zurücksetzen
            Array.Clear(_field, 0, _field.Length);
            
            // Schlange zeichnen
            foreach (var segment in Snake)
            {
                if (segment.X >= 0 && segment.X < FIELD_SIZE && 
                    segment.Y >= 0 && segment.Y < FIELD_SIZE)
                {
                    _field[segment.X, segment.Y] = 1;
                }
            }
        }

        public void MoveSnake()
        {
            var head = Snake[0];
            // Berechne die neue Position des Kopfes basierend auf der aktuellen Richtung
            var newHead = new Position(
                x: head.X + Direction.X,  // X-Position: links/rechts
                y: head.Y + Direction.Y   // Y-Position: oben/unten
            );
            
            // Überprüfe Kollision mit Wänden
            if (newHead.X < 0 || newHead.X >= FIELD_SIZE || 
                newHead.Y < 0 || newHead.Y >= FIELD_SIZE)
            {
                return; // Bewegung nicht möglich
            }

            // Füge neuen Kopf hinzu
            Snake.Insert(0, newHead);
            // Entferne das letzte Segment
            Snake.RemoveAt(Snake.Count - 1);
            
            UpdateField();
        }

        public void ChangeDirection(int dx, int dy)
        {
            // Verhindere 180-Grad-Drehungen (keine Umkehr erlaubt)
            if (!IsOppositeDirection(dx, dy))
            {
                Direction = new Position(dx, dy);
            }
        }

        private bool IsOppositeDirection(int dx, int dy)
        {
            // Prüfe, ob die neue Richtung genau entgegengesetzt zur aktuellen ist
            return (dx != 0 && dx == -Direction.X) || (dy != 0 && dy == -Direction.Y);
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < FIELD_SIZE && y >= 0 && y < FIELD_SIZE;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameRunning)
            {
                MoveSnake();
            }
        }

        public int GetPosition(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return _field[x, y];
            }
            return -1;
        }
    }
}