using System.Diagnostics;


namespace HouseExample
{
    
    public class Haus
    {
        Raum room1 = new Raum("Wohnzimmer", 30.5, 2);
        ArbeitsRaum arbeitsRaum = new ArbeitsRaum("Arbeitszimmer", 15.0, 1);
        NassRaum nassRaum = new NassRaum();

        List<IRoom> rooms = new List<IRoom>();

        float _laenge = 5;
        float _breite = 10;

        public Haus()
        {
            room1.FensterAnzahl = 3;
            nassRaum.FensterAnzahl = 1;

            rooms.Add(room1);
            rooms.Add(arbeitsRaum);
            rooms.Add(nassRaum);
        }
        
        public IRoom Room1 { get => arbeitsRaum; }

        public List<IRoom> Rooms { get => rooms; }

        //Eigenschaft/Property
        public float Laenge
        {
            get { return _laenge; }
            set
            {
                Debug.Assert(value > 0, "Die Länge muss größer als 0 sein");
                _laenge = value;
            }
        }

        public float Breite
        {
            get { return _breite; }
            set { _breite = value; }
        }

        public float BerechneFlaeche()
        {
            return _laenge * _breite;
        }

        public void FurchtbareExceptionFunktion()
        {
            throw new Exception("Das ist eine furchtbare Exception");
        }
    }
}