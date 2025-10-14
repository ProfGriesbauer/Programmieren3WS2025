using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace GeileVilla
{
    public class Villa
    {
        float _laenge = 20;
        float _breite = 10;
        readonly List<Raum> _raeume = new();

        public float GetLänge()
        {
            return _laenge;
        }

        public void SetLänge(float laenge)
        {
            if (laenge > 0)
            {
                _laenge = laenge;
            }
        }

        //Eigenschaft
        public float laenge
        {
            get { return _laenge; }
            set
            {
                Debug.Assert(value > 0, "Länge muss größer als 0 sein");
                if (value > 0)
                {
                    _laenge = value;
                }
            }
        }
        public float Breite
        {
            get { return _breite; }
            set { if (value > 0) { _breite = value; } }
        }
        public float Fläche
        {
            get { return laenge * Breite; }
        }

        // Räume-API
        public IReadOnlyList<Raum> Raeume => _raeume;

        public void AddRaum(Raum raum)
        {
            if (raum == null) throw new ArgumentNullException(nameof(raum));
            _raeume.Add(raum);
        }

        public bool RemoveRaum(string name)
        {
            var r = _raeume.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (r == null) return false;
            return _raeume.Remove(r);
        }

        public float GesamtflächeRaeume => _raeume.Sum(r => r.Fläche);

        public float FreieFläche => MathF.Max(0, Fläche - GesamtflächeRaeume);

        public Raum AddSchönerRaum(
            string name = "Wohnzimmer",
            float laenge = 5.0f,
            float breite = 4.0f,
            int fenster = 2,
            string wandfarbe = "Warmweiß")
        {
            var r = new Raum(name, laenge, breite, fenster, wandfarbe);
            AddRaum(r);
            return r;
        }

        public void Furchtbareexception()
        {
            throw new Exception("Du Dussel");
        }
    }
}

