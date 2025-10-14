namespace HouseExample
{
    public class House
    {
        public float _laenge;
        public float _breite;

        private float _stromverbrauch;      // in Ampere (A)
        private float _wasserverbrauch;     // in m³/h
        private float _heizleistung;        // in kW


        private bool _heizungAn = false;
        private bool _hauptstromAn = false;

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

        // Zustand des Hauptstromschalters
        public bool HauptstromAn
        {
            get { return _hauptstromAn; }
            set
            {
                if (value == false)
                {
                    _stromverbrauch = 0;
                    _heizungAn = false;
                    _heizleistung = 0;
                }
                _hauptstromAn = value;
            }
        }

        // Zustand der Heizung
        public bool HeizungAn
        {
            get { return _heizungAn; }
            set
            {
                if (value && !_hauptstromAn)
                {
                    // Heizung kann nicht eingeschaltet werden, wenn kein Strom
                    _heizungAn = false;
                    _heizleistung = 0;
                }
                else if (!value)
                {
                    _heizungAn = false;
                    _heizleistung = 0;
                }
                else
                {
                    _heizungAn = true;
                }
            }
        }

        public float Stromverbrauch
        {
            get { return _stromverbrauch; }
        }

        public float Wasserverbrauch
        {
            get { return _wasserverbrauch; }
        }

        public float Heizleistung
        {
            get { return _heizleistung; }
        }

        public float BerechneFlaeche()
        {
            return _laenge * _breite;
        }

        // Methode zur Berechnung der verbrauchten Wassermenge über einen Zeitraum (in Stunden)
        public float BerechneWassermenge(float stunden)
        {
            return _wasserverbrauch * stunden;
        }
    }
}

