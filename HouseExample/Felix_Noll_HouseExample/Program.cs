// See https://aka.ms/new-console-template for more information
using System.Security.AccessControl;

HouseExample.Haus instHaus = new HouseExample.Haus();
instHaus.ToString();
int meineZahl = 5;
Console.WriteLine("Hello, World!");
Console.WriteLine(meineZahl);
Console.WriteLine(instHaus.GetBreite());
Console.WriteLine("Fläche: "+ instHaus.GetFlaeche());
instHaus.SetLaenge(140);
Console.WriteLine("NeueFläche: " + instHaus.GetFlaeche());
instHaus.SetBreite(30);
instHaus.SetHoehe(50);
instHaus.SetLaenge(10);
Console.WriteLine("Neuen Fläche: " + instHaus.GetFlaeche());
Console.WriteLine("Neues Volumen: " + instHaus.GetVolumen());

float test = 5.0f;
try
{

    instHaus.FurchtbareExeptFunktion();
}
catch (Exception ex)
{
    Console.WriteLine("Fehler Aufgetreten: " + ex.Message);
}
test = test / 0.0f;
Console.WriteLine(test);
IRoom room1 = new Raum("Wohnzimmer", 30, true);
room1.BeschreibeRaum();

List<Raum> hausRaeume = new List<Raum>();
hausRaeume.Add(new Raum("Wohnzimmer", 30, true));   

foreach (var raum in hausRaeume)
{
    raum.BeschreibeRaum();
}

object o = new object();
o.e