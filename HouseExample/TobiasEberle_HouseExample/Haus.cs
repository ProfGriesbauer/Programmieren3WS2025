namespace HouseExample
{
    public class Haus
    {
        float _laenge =10;
        float _breite = 5;

        public float Laenge
        {
            get { return _laenge; }
            set { _laenge = value; }
        }
        public float getLaenge()
        {
            return _laenge;
        }

        public float getBreite()
        {
            return _breite;
        }


    }
}