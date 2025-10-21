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
        string Name { get; set; }
        int FensterAnzahl { get; set; }

        double BerechneFlaeche();
    }

    public class NassRaum : IRoom
    {
        private string name = "Nassraum";

        public string Name { get => name; set => name = value; }
        public int FensterAnzahl { get; set; }

        public double BerechneFlaeche()
        {
            // Implementiere die spezifische Logik für Nassräume
            return 0;
        }
    }

    public class ArbeitsRaum : Raum, IRoom, ISetableRoom
    {
        public ArbeitsRaum(string name, double flaeche, int fensterAnzahl) : base(name, flaeche, fensterAnzahl)
        {
        }
    }

    public class Raum : IRoom
    {
        private string name;
        private double flaeche;
        private int fensterAnzahl;

        public double BerechneFlaeche()
        {
            return flaeche;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public double Flaeche { get => flaeche; set => flaeche = value; }
        public int FensterAnzahl { get; set; }

        public Raum(string name, double flaeche, int fensterAnzahl)
        {
            Name = name;
            Flaeche = flaeche;
            FensterAnzahl = fensterAnzahl;
        }

        public override string ToString()
        {
            return $"Raum: {Name}, Fläche: {Flaeche} m², Fenster: {FensterAnzahl}";
        }
    }
}