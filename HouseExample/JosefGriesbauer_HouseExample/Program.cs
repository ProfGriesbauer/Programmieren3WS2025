// See https://aka.ms/new-console-template for more information
using HouseExample;

HouseExample.Haus instHaus = new HouseExample.Haus();
HouseExample.Haus instHaus2 = null;
instHaus.ToString();
Console.WriteLine("Hello, World!");
instHaus.Laenge = 20;

try
{
    instHaus2.FurchtbareExceptionFunktion();
}
catch (Exception ex)
{
    Console.WriteLine("Fehler aufgetreten: " + ex.Message);
}

instHaus.Rooms.Add(new ArbeitsRaum("Neuer Arbeitsraum", 12.5, 2));
instHaus.Room1.Name = "Neuer Name für Arbeitsraum";

foreach (IRoom room in instHaus.Rooms)
{
    Console.WriteLine(room.Name + " " + room.BerechneFlaeche() + " " + room.FensterAnzahl);
}

Console.WriteLine(instHaus.Room1.Name + " " + instHaus.Laenge);
Console.WriteLine("Fläche: " + instHaus.BerechneFlaeche());
