// Converted top-level statements into a callable method to avoid multiple entry points
using System;
using System.ComponentModel;
using GeileVilla;

namespace SebastiansHaus
{
    public static class ProgramHelper
    {
        public static void Run()
        {
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
            GeileVilla.Raum Wohnzimmer = new GeileVilla.Raum("Wohnzimmer", 5, 4, 2, "Blau");
            Console.WriteLine(Wohnzimmer);
            GeileVilla.Raum Küche = new GeileVilla.Raum("Küche", 3, 4, 1, "Weiß");
            Console.WriteLine(Küche);
            GeileVilla.Raum Esszimmer = new GeileVilla.Raum("Esszimmer", 4, 4, 1, "Gelb");
            Console.WriteLine(Esszimmer);
            GeileVilla.Raum Bad = new GeileVilla.Raum("Bad", 2, 3, 0, "Weiß");
            Console.WriteLine(Bad);
        }
    }
}