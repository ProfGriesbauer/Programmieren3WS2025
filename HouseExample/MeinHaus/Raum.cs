using System;
using System.Diagnostics;

namespace GeileVilla
{
    
    public class Raum
    {
        private string _name = "Raum";
        private float _laenge;
        private float _breite;
        private int _fenster;
        private string _wandfarbe = "Weiß";
        //Klasseninvarianten

        public Raum(string name, float laenge, float breite, int fenster = 1, string? wandfarbe = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Raum" : name.Trim();
            Laenge = laenge;
            Breite = breite;
            Fenster = Math.Max(0, fenster);
            if (!string.IsNullOrWhiteSpace(wandfarbe))
                Wandfarbe = wandfarbe!;
        }

        public string Name
        {
            get => _name;
            set => _name = string.IsNullOrWhiteSpace(value) ? _name : value.Trim();
        }

        public float Laenge
        {
            get => _laenge;
            set
            {
                Debug.Assert(value > 0, "Länge muss größer als 0 sein");
                if (value > 0) _laenge = value;
            }
        }

        public float Breite
        {
            get => _breite;
            set
            {
                Debug.Assert(value > 0, "Breite muss größer als 0 sein");
                if (value > 0) _breite = value;
            }
        }

        public int Fenster
        {
            get => _fenster;
            set => _fenster = value < 0 ? 0 : value;
        }

        public string Wandfarbe
        {
            get => _wandfarbe;
            set => _wandfarbe = string.IsNullOrWhiteSpace(value) ? _wandfarbe : value.Trim();
        }

        public float Fläche => Laenge * Breite;

        public override string ToString()
            => $"{Name}: {Laenge}m x {Breite}m = {Fläche} m², Fenster: {Fenster}, Wandfarbe: {Wandfarbe}";
    }
}
