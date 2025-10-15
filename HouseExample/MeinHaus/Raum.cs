using System;

namespace GeileVilla
{
    public class Raum
    {
        public string Name { get; }
        public float Laenge { get; }
        public float Breite { get; }
        public int Fenster { get; }
        public string Wandfarbe { get; }

        public float Fläche => Laenge * Breite;

        public Raum(string name, float laenge, float breite, int fenster = 0, string wandfarbe = "Weiß")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name darf nicht leer sein.", nameof(name));
            if (laenge <= 0)
                throw new ArgumentOutOfRangeException(nameof(laenge), "Laenge muss > 0 sein.");
            if (breite <= 0)
                throw new ArgumentOutOfRangeException(nameof(breite), "Breite muss > 0 sein.");
            if (fenster < 0)
                throw new ArgumentOutOfRangeException(nameof(fenster), "Fenster darf nicht negativ sein.");

            Name = name.Trim();
            Laenge = laenge;
            Breite = breite;
            Fenster = fenster;
            Wandfarbe = string.IsNullOrWhiteSpace(wandfarbe) ? "Weiß" : wandfarbe.Trim();
        }

        public override string ToString()
        {
            return $"{Name}: {Laenge}m x {Breite}m = {Fläche:0.##} m², Fenster: {Fenster}, Farbe: {Wandfarbe}";
        }
    }
}