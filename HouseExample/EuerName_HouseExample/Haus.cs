namespace HouseExample
{
    public class Haus
    {
        float _laenge = 5;
        float _breite = 10;

        public float GetLaenge()
        {
            return _laenge;
        }

        public void SetLaenge(float laenge)
        {
            _laenge = laenge;
        }
        public float GetBreite()
        {
            return _breite;
        }
        public float GetFlaeche()
        {
            return _laenge * _breite;
        }
    }
}