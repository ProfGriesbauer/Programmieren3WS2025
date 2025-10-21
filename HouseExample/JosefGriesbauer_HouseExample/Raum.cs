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

    public class NassRaum : Raum, IRoom
    {
        int anzahlDuschen;

        public NassRaum(string name, double flaeche, int fensterAnzahl, int anzahlDuschen) : base(name, flaeche, fensterAnzahl)
        {
            this.anzahlDuschen = anzahlDuschen;
        }

        public new int FensterAnzahl { get; set; }

        public new double BerechneFlaeche()
        {
            // Implementiere die spezifische Logik für Nassräume
            return base.BerechneFlaeche() * 1.1; // Beispiel: Nassräume haben 10% mehr Fläche
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