// See https://aka.ms/new-console-template for more information
HouseExample.Haus insthaus = new HouseExample.Haus();
insthaus.ToString();
int meineZahl = 5;

Console.WriteLine("Hello, World!");
Console.WriteLine(insthaus.GetBreite() + " " + insthaus.GetLaenge());

insthaus.SetLaenge(786);

Console.WriteLine(insthaus.GetLaenge());