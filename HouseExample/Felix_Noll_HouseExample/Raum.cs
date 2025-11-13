namespace HauseExample
{
    using System; 
}
public interface IRoom
{
    string Name { get; set; }
    int Groesse { get; set; } // in Quadratmetern
    bool HatFenster { get; set; }
    void BeschreibeRaum();
    double BerecheneFlaeche();
}

public class nassRaum : IRoom
{
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int Groesse { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool HatFenster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public double BerecheneFlaeche()
    {
        throw new NotImplementedException();
    }

    public void BeschreibeRaum()
    {
        throw new NotImplementedException();
    }
}

public class trockenRaum : IRoom
{
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int Groesse { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool HatFenster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public double BerecheneFlaeche()
    {
        throw new NotImplementedException();
    }

    public void BeschreibeRaum()
    {
        throw new NotImplementedException();
    }
}

public class Wohnzimmer : IRoom
{
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int Groesse { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool HatFenster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public double BerecheneFlaeche()
    {
        throw new NotImplementedException();
    }

    public void BeschreibeRaum()
    {
        throw new NotImplementedException();
    }
}

public class Raum : IRoom
{
    private string name = string.Empty;
    private int groesse;
    private bool hatFenster;
    private double flaeche;

    public string Name { get => name; set => name = value; }
    public int Groesse { get => groesse; set => groesse = value; } // in Quadratmetern
    public bool HatFenster { get => hatFenster; set => hatFenster = value; }
    public double Flaeche { get => flaeche; set => flaeche = value; } // in Quadratmetern
    public Raum(string name, int groesse, bool hatFenster)
    {
        Name = name;
        Groesse = groesse;
        HatFenster = hatFenster;
        Flaeche = groesse; // Annahme: Die Fläche entspricht der Größe in m²
    }

    public double BerecheneFlaeche()
    {
        return Flaeche; // Annahme: Die Fläche entspricht der Größe in m²
    }
    public void BeschreibeRaum()
    {
        string fensterInfo = HatFenster ? "hat Fenster" : "hat keine Fenster";
        Console.WriteLine($"Der Raum '{Name}' ist {Groesse} m² groß und {fensterInfo}.");
    }
}