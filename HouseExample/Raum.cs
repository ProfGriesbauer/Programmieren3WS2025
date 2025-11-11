using System;
namespace HouseExample
{
	public class Raum
	{
		private Haus _haus;
		private string _infoAusProgram;

		public Raum(Haus haus, string infoAusProgram)
		{
			_haus = haus;
			_infoAusProgram = infoAusProgram;
		}

		public void PrintInfo()
		{
			Console.WriteLine($"Raum im Haus mit Fläche: {_haus.BerechneFlaeche()} und Info: {_infoAusProgram}");
		}
	}
}
