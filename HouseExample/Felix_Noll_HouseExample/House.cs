using System.Diagnostics;

namespace HouseExample
{
    public class Haus
    {
        float _laenge = 5;
        float _breite = 10;
        float _hoehe = 20;
        public float GetLaenge()
        {
            return _laenge;
        }
        public float laenge
        {
            get { return _laenge * 1000; }
            set
            {
                Debug.Assert(value > 0, "Länge muss größer als 0 sein");
                _laenge =value;

            }
        }
        public float breite
        {
            get { return _laenge; }
            set { _breite = value; }
        }
        public float hoehe
        {
            get { return _hoehe; }
            set { _hoehe = value; }
        }
        public void SetLaenge(float laenge)
        {
            _laenge = laenge;
        }
        public void SetHoehe(float hoehe)
        {
            _hoehe = hoehe;
        }
        public void SetBreite(float breite)
        {
            _breite = breite;
        }
        public float GetBreite()
        {
            return _breite;
        }
        public float GetFlaeche()
        {
            return _breite * _laenge;
        }
        public float GetVolumen()
        {
            return _breite * _laenge * _hoehe;
        }
        public string FurchtbareExeptFunktion()
        {
            return "das ist ein Fehler";
        }

    }
}
