// See https://aka.ms/new-console-template for more information
HouseExample.Haus instHaus = new HouseExample.Haus();
instHaus.ToString();
Console.WriteLine("Hello, World!");
instHaus.Laenge = 20;
Console.WriteLine(instHaus.Breite + " " + instHaus.Laenge);
Console.WriteLine("Fläche: " + instHaus.BerechneFlaeche());
try
{
    // Beispiel: Setze einen ungültigen Wert, um einen Fehler zu provozieren
    instHaus.Laenge = -1;
    Console.WriteLine("Neue Fläche: " + instHaus.BerechneFlaeche());
}
catch (Exception ex)
{
    Console.WriteLine("Fehler aufgetreten: " + ex.Message);
}
Console.WriteLine(instHaus.Breite + " " + instHaus.Laenge);
