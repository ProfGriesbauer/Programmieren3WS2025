using System.Diagnostics;

namespace HouseExample
{
    public interface IRoom
    {
        string Name { get; }
        int FensterAnzahl { get; }

        double BerechneFlaeche();
    }

    public interface ISetableRoom
    {
        string Name { get; }
        int FensterAnzahl { get; set; }

        double BerechneFlaeche();
    }

    public class NassRaum : Raum, IRoom, IComparable
    {
        int anzahlDuschen;

        public NassRaum(string name, double flaeche, int fensterAnzahl, int anzahlDuschen) : base(name, flaeche, fensterAnzahl)
        {
            this.anzahlDuschen = anzahlDuschen;
        }

        public override bool Equals(object? obj)
        {
            if (obj is NassRaum)
            {
                NassRaum otherNassRaum = (NassRaum)obj;
                return this.Flaeche == otherNassRaum.Flaeche &&
                       this.FensterAnzahl == otherNassRaum.FensterAnzahl &&
                       this.anzahlDuschen == otherNassRaum.anzahlDuschen;
            }
            return false;
        }

        public new int FensterAnzahl { get; set; }

        public new double BerechneFlaeche()
        {
            // Implementiere die spezifische Logik für Nassräume
            return base.BerechneFlaeche() * 1.1; // Beispiel: Nassräume haben 10% mehr Fläche
        }

        public int CompareTo(NassRaum? other)
        {
            //benutze den Namen zum Vergleichen
            if (other == null) return 1;
            return this.Name.CompareTo(other.Name);
        }

        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;
            if (obj is NassRaum otherNassRaum)
            {
                return CompareTo(otherNassRaum);
            }
            else
            {
                throw new ArgumentException("Objekt ist kein NassRaum");
            }
        }

        public static bool operator <(NassRaum a, NassRaum b)
        {
            if (ReferenceEquals(a, null)) return !ReferenceEquals(b, null);
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(NassRaum a, NassRaum b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.CompareTo(b) > 0;
        }

        public new string Name
        {
            get
            {
                return "Normaler Nassraum";
            }
        }

        public int AnzahlDuschen { get => anzahlDuschen; }
    }

    public class ArbeitsRaum : Raum, IRoom, ISetableRoom
    {
        public ArbeitsRaum(string name, double flaeche, int fensterAnzahl) : base(name, flaeche, fensterAnzahl)
        {
        }

        public new string Name
        {
            get
            {
                return "Normaler Arbeitsraum";
            }
        }

        int ISetableRoom.FensterAnzahl { get => FensterAnzahl; set => base.fensterAnzahl = value; }
    }

    public class Raum : IRoom
    {
        private double flaeche;
        protected int fensterAnzahl;

        public double BerechneFlaeche()
        {
            return flaeche;
        }

        public string Name
        {
            get
            {
                return "Normaler Raum";
            }
        }
        public double Flaeche { get => flaeche; set => flaeche = value; }
        public int FensterAnzahl { get; }

        public Raum(string name, double flaeche, int fensterAnzahl)
        {
            Flaeche = flaeche;
            FensterAnzahl = fensterAnzahl;
        }

        public override string ToString()
        {
            return $"Raum: {Name}, Fläche: {Flaeche} m², Fenster: {FensterAnzahl}";
        }
    }
}