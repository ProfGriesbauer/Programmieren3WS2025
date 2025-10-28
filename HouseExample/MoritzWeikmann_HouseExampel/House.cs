using System.Diagnostics;

namespace HouseExample
{

    public class Haus
    {
        // Beispiel-Raum (falls Du ihn später nutzen willst)
        Raum room1 = new Raum("Wohnzimmer", 5.0f, 4.0f);
        public float _laenge = 5;
        public float _breite = 10;

        // Nominale (konfigurierte) Verbrauchswerte - intern festgehalten
        private readonly float _nominalWasserM3PerH = 0.5f; // m3/h
        private readonly float _nominalStromA = 10.0f; // Ampere
        private readonly float _nominalHeizungsKW = 5.0f; // kW

        // Aktuelle (tatsächliche) Verbrauchswerte, die sich ändern wenn Zustände aus sind
        private float _currentWasserM3PerH;
        private float _currentStromA;
        private float _currentHeizungsKW;

        // Interne Zustände
        private bool _heizungAn = false;
        private bool _hauptstromAn = false;
        public Haus()
        {
            // initiale aktuelle Werte = nominal (wenn Hauptstrom an wäre)
            _currentWasserM3PerH = _nominalWasserM3PerH;
            _currentStromA = _nominalStromA;
            _currentHeizungsKW = 0f; // Heizung standardmäßig aus
        }

        // Eigenschaften und Methoden

        public float Laenge
        {
            get { return _laenge; }
            set
            {
                Debug.Assert(value >= 0, "Laenge muss positiv sein.");
                _laenge = value;
            }
        }

        public float Breite
        {
            get { return _breite; }
            set { _breite = value; }
        }

        public float Flaeche
        {
            get { return Laenge * Breite; }
        }
        public float GetLaenge()
        {
            return _laenge;
        }
        
        // Öffentliche, nur-lesbare Eigenschaften für aktuelle Verbräuche
        public float AktuellerWasserverbrauchM3ProH => _hauptstromAn ? _currentWasserM3PerH : 0f;
        public float AktuellerStromA => _hauptstromAn ? _currentStromA : 0f;
        public float AktuelleHeizungsLeistungKW => _heizungAn ? _currentHeizungsKW : 0f;

        // Property für Hauptstromschalter (setzt Verbräuche auf 0 wenn AUS)
        public bool HauptstromAn
        {
            get => _hauptstromAn;
            set
            {
                _hauptstromAn = value;
                if (!_hauptstromAn)
                {
                    // Bei Stromausfall fällt alles auf 0
                    _currentStromA = 0f;
                    // Heizung geht automatisch aus, weil keine Energie
                    _heizungAn = false;
                    _currentHeizungsKW = 0f;
                    _currentWasserM3PerH = 0f;
                }
                else
                {
                    // Bei wieder eingeschaltetem Hauptstrom: aktuelle Werte auf nominal
                    _currentStromA = _nominalStromA;
                    _currentWasserM3PerH = _nominalWasserM3PerH;
                    // Heizung bleibt aus, bis explizit eingeschaltet
                }
            }
        }

        // Property für Heizung: darf nur eingeschaltet werden wenn Hauptstrom an
        public bool HeizungAn
        {
            get => _heizungAn;
            set
            {
                if (value && !_hauptstromAn)
                {
                    // Wenn versucht wird Heizung einzuschalten, aber kein Strom da ist,
                    // bleibt sie aus. Alternativ könnte hier eine Exception geworfen werden.
                    _heizungAn = false;
                    _currentHeizungsKW = 0f;
                }
                else
                {
                    _heizungAn = value;
                    _currentHeizungsKW = _heizungAn ? _nominalHeizungsKW : 0f;
                }
            }
        }

        // Methode: Berechne verbrauchte Wassermenge über eine Zeitdauer in Stunden
        // durationHours kann auch Bruchteile (z.B. 0.5) sein.
        public float BerechneWasserverbrauch(float durationHours)
        {
            if (durationHours < 0) throw new ArgumentOutOfRangeException(nameof(durationHours));
            // Wenn kein Hauptstrom, dann aktueller Verbrauch 0
            float rate = AktuellerWasserverbrauchM3ProH;
            return rate * durationHours;
        }

        // Beispielmethode aus dem ursprünglichen Projekt (hält eine Ausnahme-Demo)
        public void BeispielExeption()
        {
            try
            {
                int a = 5;
                int b = 0;
                int c = a / b; // löst DivideByZeroException aus
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Fehler: Division durch Null ist nicht erlaubt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ein unerwarteter Fehler ist aufgetreten: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Ende des Ausnahmebehandlungsbeispiels.");
            }
        }
    }
}

