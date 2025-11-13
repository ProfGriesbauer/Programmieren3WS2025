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
        // Interface-Implementierungen (nutze OOPGames Schnittstellen!)
    public class CrossyPlayer : IPlayer
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Alive { get; set; } = true;

        public CrossyPlayer() => Name = "Crossy"; // Default-Konstruktor wird benötigt!

        public void MakeMove(string direction)
        {
            if (direction == "UP") Y++;
            else if (direction == "LEFT") X--;
            else if (direction == "RIGHT") X++;
        }

        public IPlayer Clone() => new CrossyPlayer { Name = this.Name, X = this.X, Y = this.Y, Alive = this.Alive };
    }

    public class CrossyPainter : IPainter
    {
        public void PaintGame(Canvas canvas, IGameField field)
        {
            // Hier Zeichenlogik für WPF-Canvas (statt Console)
            // Am besten: Canvas.Children.Clear(); und Rechtecke/Ellipsen zeichnen!
        }
        public string Name => "CrossyPainter";
    }

    public class CrossyRules : IRules
    {
        public List<(int, int)> Cars = new() { (3,3), (5,6), (7,5) };
        public void MakeMove(IGameField field, IPlayer[] players)
        {
            foreach (var player in players.Cast<CrossyPlayer>())
            {
                // Regeln prüfen (siehe ursprünglichen Code)
            }
        }
        public string Name => "CrossyRules";
    }

    public class CrossyGamePlugin : IGamePlugin // (Falls dein Framework Plugins nutzt)
    {
        public void Register()
        {
            OOPGamesManager.Singleton.RegisterPainter(new CrossyPainter());
            OOPGamesManager.Singleton.RegisterRules(new CrossyRules());
            OOPGamesManager.Singleton.RegisterPlayer(new CrossyPlayer()); // Spieler kann dann im UI ausgewählt werden
        }
    }

}
