public interface IRoom
{
    float Laenge { get; set; }
    float Breite { get; set; }
    float BerechneFlaeche { get; }
}


public class Raum
{
    public float _laenge;
    public float _breite;

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

