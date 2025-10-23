using System;

namespace HouseExample
{
    public interface IRaum
    {
        string Name { get; set; }
        float Laenge { get; set; }
        float Breite { get; set; }
        bool LichtAn { get; set; }
        int Temperatur { get; set; }
        double BerechneFlaeche();
    }
    public class Nassraum : IRaum
    {
        public string Name { get; set; }
        public float Laenge { get; set; }
        public float Breite { get; set; }
        public bool LichtAn { get; set; }
        public int Temperatur { get; set; }

        public Nassraum(string name, float laenge, float breite)
        {
            Name = name;
            Laenge = laenge;
            Breite = breite;
            LichtAn = false;
            Temperatur = 20;
        }

        public double BerechneFlaeche()
        {
            return Laenge * Breite;
        }
    }
}
    // Move Raum class inside the HouseExample namespace


namespace HouseExample
{
    public class Raum : IRaum
    {
        // Felder
        private float _laenge;
        private float _breite;
        private bool _lichtAn;
        private int _temperatur;

        // Konstruktoren
        public Raum() : this("Unbenannt", 3.0f, 4.0f) { }

        public Raum(string name, float laenge, float breite)
        {
            Name = name;
            Laenge = laenge;
            Breite = breite;
            _lichtAn = false;
            _temperatur = 20;
        }

        // Eigenschaften
        public string Name { get; set; }

        public float Laenge
        {
            get => _laenge;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Laenge), "Laenge muss >= 0 sein.");
                _laenge = value;
            }
        }

        public float Breite
        {
            get => _breite;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Breite), "Breite muss >= 0 sein.");
                _breite = value;
            }
        }

        // Umsetzung der Interface-Properties
        public bool LichtAn
        {
            get => _lichtAn;
            set => _lichtAn = value;
        }

        public int Temperatur
        {
            get => _temperatur;
            set
            {
                if (value < -30 || value > 50) throw new ArgumentOutOfRangeException(nameof(Temperatur), "Temperatur außerhalb realistischer Werte.");
                _temperatur = value;
            }
        }

        // Berechnete Eigenschaft
        public double BerechneFlaeche()
        {
            return Laenge * Breite;
        }
    }
}
    
