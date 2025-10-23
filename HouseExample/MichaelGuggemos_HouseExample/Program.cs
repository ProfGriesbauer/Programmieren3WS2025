// Converted top-level statements into a callable method to avoid multiple entry points
using System;
namespace MichaelGuggemos_HouseExample
{
	public static class ProgramHelper
	{
		public static void Run()
		{
			HouseExample.House instHaus = new HouseExample.House();
			instHaus.ToString();
			Console.WriteLine("Hello, World!");
			instHaus.Laenge = 20;

			instHaus.Breite = 30;
			Console.WriteLine(instHaus.Breite + " " + instHaus.Laenge);
			Console.WriteLine("Fläche: " + instHaus.BerechneFlaeche());
		}
	}
}