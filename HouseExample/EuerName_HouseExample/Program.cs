// See https://aka.ms/new-console-template for more information
HouseExample.Haus insthaus = new HouseExample.Haus();
insthaus.ToString();
int meineZahl = 5;
Console.WriteLine("Hello, World!");
Console.WriteLine(insthaus.GetBreite() +" "+ insthaus.GetLaenge());
Console.WriteLine("Fläche: " + insthaus.GetFlaeche());

try
{
    float test = 5.0f / 0.0f;
}
catch (Exception ex)
{
    Console.WriteLine("fehler aufgetreten" + ex.Message);
}
Console.WriteLine(insthaus.GetBreite() + " " + insthaus.GetLaenge());
Console.WriteLine("Fl#che: " + insthaus.BerechneFlaeche());