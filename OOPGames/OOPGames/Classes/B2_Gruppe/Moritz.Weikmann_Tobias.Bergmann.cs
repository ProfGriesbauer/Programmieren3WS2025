using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPGames
{
    public class B2_XXX
    {
        // Einfaches Beispiel: eine statische Methode, die eine Nachricht zurückgibt.
        public static string GetGreeting(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Hallo, Welt!";
            return $"Hallo, {name}!";
        }

        // Optional: eine Methode, die die Nachricht auf die Konsole schreibt.
        public static void PrintGreeting(string name = null)
        {
            Console.WriteLine(GetGreeting(name ?? string.Empty));
        }
    }
}
