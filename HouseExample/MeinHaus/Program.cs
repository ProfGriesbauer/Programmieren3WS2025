// See https://aka.ms/new-console-template for more information
using System.ComponentModel;
using GeileVilla;
Console.WriteLine("Hello, World!");
GeileVilla.Villa Villa1 = new GeileVilla.Villa();
Console.WriteLine(Villa1.laenge + Villa1.Breite);
Console.WriteLine(Villa1.Fläche);
// Call Furchtbareexception() on Villa1 if it exists
Villa1.laenge = 300;
try
{
    Villa1.Furchtbareexception();
}
catch (Exception ex)
{
    Console.WriteLine($"Isch Kaputt ah: {ex.Message}");
}