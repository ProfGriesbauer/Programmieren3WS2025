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


Raum arbeit = new ArbeitsRaum("Neuer Arbeitsraum", 12.5, 2);
NassRaum nass = new NassRaum("Neuer Nassraum", 8.0, 1, 2);
NassRaum nass2 = new NassRaum("Noch ein Nassraum", 9.0, 1, 3);
Raum normal = new Raum("Normaler Raum", 20.0, 3);
instHaus.Rooms.Add(arbeit);
instHaus.Rooms.Add(nass);
instHaus.Rooms.Add(nass2);
instHaus.Rooms.Add(normal);

if (nass < nass2)
{
    Console.WriteLine("Die beiden Nassräume sind gleich.");
}
else
{
    Console.WriteLine("Die beiden Nassräume sind unterschiedlich.");
}

nass.BerechneFlaeche();
Console.WriteLine("Anzahl Duschen im Nassraum: " + ((NassRaum)nass).AnzahlDuschen);

foreach (IRoom room in instHaus.Rooms)
{
    Console.WriteLine(room.Name + " " + room.BerechneFlaeche() + " " + room.FensterAnzahl);
}

// Zähle die unterschiedlichen Raumtypen mit "is"
int arbeitsRaumCount = 0;
int nassRaumCount = 0;
int normalRaumCount = 0;
foreach (IRoom room in instHaus.Rooms)
{
    if (room is ArbeitsRaum)
    {
        arbeitsRaumCount++;
    }
    else if (room is NassRaum)
    {
        nassRaumCount++;
    }
    //Normale räume werden auch gezählt
    if (room is Raum)
    {
        // count normal rooms
        normalRaumCount++;
    }
}

Console.WriteLine(instHaus.Room1.Name + " " + instHaus.Laenge);
Console.WriteLine("Fläche: " + instHaus.BerechneFlaeche());
