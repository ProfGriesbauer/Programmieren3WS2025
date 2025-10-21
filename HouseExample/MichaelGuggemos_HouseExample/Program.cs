// See https://aka.ms/new-console-template for more information
HouseExample.House instHaus = new HouseExample.House();
instHaus.ToString();
Console.WriteLine("Hello, World!");
instHaus.Laenge = 20;


instHaus.Breite = 30;
Console.WriteLine(instHaus.Breite + " " + instHaus.Laenge);
Console.WriteLine("Fläche: " + instHaus.BerechneFlaeche());