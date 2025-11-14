using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Direction for Froggo moves   
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public interface IOOPGame
    {
        void Start();
        void MovePlayer(Direction direction);
        bool CheckCollision();
        void Render();
    }

    public class FroggoGame : IOOPGame
    {
        private int playerX = 0;
        private int playerY = 0;
        private int[,] map = new int[10,10];

        public void Start()
        {
            playerX = 5;
            playerY = 9;
            // Beispielhafte Initialisierung
            for (int i = 0; i < 10; i++) map[3,i] = 1; // Hindernisreihe
        }

        public void MovePlayer(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: if (playerY > 0) playerY--; break;
                case Direction.Down: if (playerY < 9) playerY++; break;
                case Direction.Left: if (playerX > 0) playerX--; break;
                case Direction.Right: if (playerX < 9) playerX++; break;
            }
        }

        public bool CheckCollision()
        {
            return map[playerX, playerY] == 1;
        }

        public void Render()
        {
            for(int y = 0; y < 10; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    if (x == playerX && y == playerY)
                        Console.Write("F ");
                    else if (map[x, y] == 1)
                        Console.Write("X ");
                    else
                        Console.Write(". ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    // Beispielhafte Nutzung
    public class Register
    {
        public static void Main()
        {
            IOOPGame game = new FroggoGame();
            game.Start();
            game.Render();
            game.MovePlayer(Direction.Up);
            game.Render();
        }
    }


}
