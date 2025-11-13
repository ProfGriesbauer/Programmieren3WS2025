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
    // Spielerklasse
    public class CrossyPlayer
    {
        public string Name { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Alive { get; set; }

        public CrossyPlayer(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
            Alive = true;
        }

        public void Move(string direction)
        {
            if (direction == "UP") Y++;
            else if (direction == "LEFT") X--;
            else if (direction == "RIGHT") X++;
            // Keine Abwärtsbewegung wie im Original!
        }
    }

    // Painterklasse
    public class CrossyPainter
    {
        private int width;
        private int height;

        public CrossyPainter(int boardWidth, int boardHeight)
        {
            width = boardWidth;
            height = boardHeight;
        }

        public void Paint(List<CrossyPlayer> players)
        {
            Console.Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var player = players.FirstOrDefault(p => p.X == x && p.Y == y && p.Alive);
                    if (player != null)
                        Console.Write(player.Name[0]);
                    else
                        Console.Write(".");
                }
                Console.WriteLine();
            }
        }
    }

    // Regelklasse
    public class CrossyRules
    {
        public List<(int, int)> Cars = new List<(int, int)> { (3,3), (5,6), (7,5) };
        public int BoardWidth { get; }
        public int BoardHeight { get; }

        public CrossyRules(int boardWidth, int boardHeight)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
        }

        public void Check(List<CrossyPlayer> players)
        {
            foreach (var player in players)
            {
                // Kollision mit Autos?
                foreach (var (carX, carY) in Cars)
                {
                    if (player.X == carX && player.Y == carY)
                    {
                        player.Alive = false;
                        Console.WriteLine($"{player.Name} wurde überfahren!");
                    }
                }
                // Spielfeldgrenzen
                if (player.X < 0 || player.X >= BoardWidth || player.Y < 0 || player.Y >= BoardHeight)
                {
                    player.Alive = false;
                    Console.WriteLine($"{player.Name} hat das Spielfeld verlassen!");
                }
                // Siegbedingung (z.B. Y ganz oben)
                if (player.Y == BoardHeight - 1)
                {
                    Console.WriteLine($"{player.Name} hat gewonnen!");
                    player.Alive = false;
                }
            }
        }
    }

    // Hauptspielklasse
    public class CrossyRoadsGame
    {
        private List<CrossyPlayer> players;
        private CrossyPainter painter;
        private CrossyRules rules;
        private int boardWidth = 10;
        private int boardHeight = 10;

        public CrossyRoadsGame()
        {
            players = new List<CrossyPlayer>
            {
                new CrossyPlayer("P1", 1, 0),
                new CrossyPlayer("P2", 2, 0)
            };
            painter = new CrossyPainter(boardWidth, boardHeight);
            rules = new CrossyRules(boardWidth, boardHeight);
        }

        public void Run()
        {
            while (players.Any(p => p.Alive))
            {
                painter.Paint(players);
                foreach (var player in players.Where(p => p.Alive))
                {
                    Console.WriteLine($"{player.Name} - Bereichere dich: (UP/LEFT/RIGHT)");
                    var move = Console.ReadLine()?.ToUpper();
                    player.Move(move);
                }
                rules.Check(players);
                System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("Spiel beendet!");
        }
    }

    // Main-Einstiegspunkt
    class Program
    {
        static void Main(string[] args)
        {
            var game = new CrossyRoadsGame();
            game.Run();
        }
    }

    public class CrossyRoads
    {
        public void Register()
        {
            OOPGamesManager.Singleton.RegisterPainter(new CroosyPainter());
            OOPGamesManager.Singleton.RegisterRules(new CrossyRules());
            OOPGamesManager.Singleton.RegisterPlayer(new CrossyPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new CrossyPainter());
        }
    }

}
