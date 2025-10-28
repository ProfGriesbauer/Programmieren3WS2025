using System;

namespace OOPGames
{
    /// <summary>
    /// Hauptklasse für die Registrierung aller Komponenten beim OOPGamesManager
    /// Diese Klasse ist verantwortlich für die Initialisierung und Registrierung aller Spielkomponenten
    /// </summary>
    public class A5_Register
    {
        public void Register()
        {
            // Erstelle die Instanzen
            A5_Paint painter = new A5_Paint();
            A5_Rules rules = new A5_Rules();
            A5_Player player = new A5_Player();

            // Registriere beim OOPGamesManager
            OOPGamesManager.Singleton.RegisterPainter(painter);
            OOPGamesManager.Singleton.RegisterRules(rules);
            OOPGamesManager.Singleton.RegisterPlayer(player);
        }
    }
}