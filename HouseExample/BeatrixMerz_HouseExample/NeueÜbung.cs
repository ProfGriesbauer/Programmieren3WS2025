namespace Huan
{
    public class Nutte
    {
        float _alter = 25;
        float _groesse = 1.7f;
        float _gewicht = 65;
        float _rücken = 70;
        float _titten = 75;
        float _arsch = 80;
        float _sympathie = 90;
        string _name = "Ayleen";

        public float alter
        {
            get { return _alter; }
            set { _alter = value; }
        }
        public float groesse
        {
            get { return _groesse; }
            set { _groesse = value; }
        }

        public float gewicht
        {
            get { return _gewicht; }
            set { _gewicht = value; }
        }
        public float rücken
        {
            get { return _rücken; }
            set { _rücken = value; }
        }
        public float titten
        {
            get { return _titten; }
            set { _titten = value; }
        }
        public float arsch
        {
            get { return _arsch; }
            set { _arsch = value; }
        }
        public float sympathie
        {
            get { return _sympathie; }
            set { _sympathie = value; }
        }
        public float Attraktivität()
        {
            return (alter * groesse * gewicht * rücken * titten * arsch * sympathie) / 7;
        }
        public float Preis()
        {
            return (alter + groesse + gewicht + rücken + titten + arsch + sympathie) / 7;
        }
        public string AlleWerte()
        {
            return $"Alter: {alter}\nGröße: {groesse}\nGewicht: {gewicht}\nRücken: {rücken}\nTitten: {titten}\nArsch: {arsch}\nSympathie: {sympathie}";
        }
    
        public string NuttenWerte()
        {
            return $"Attraktivität: {Attraktivität()}\nPreis: {Preis()} Euro \nAlle Werte:\n{AlleWerte()} \nName: {_name}";
        }
    }
}