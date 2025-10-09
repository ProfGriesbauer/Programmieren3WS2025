namespace HouseExample
{
    public class House
    {
        public float _laenge;
        public float _breite;

        /*public House(float laenge, float breite)
        {
            _laenge = laenge;
            _breite = breite;
        }*/

        public float Laenge
        {
            get { return _laenge; }
            set { _laenge = value; }
        }

        public float Breite
        {
            get { return _breite; }
            set { _breite = value; }    
        }

        public float BerechneFlaeche
        {
            get { return _laenge * _breite; }   
        }
    }
}

