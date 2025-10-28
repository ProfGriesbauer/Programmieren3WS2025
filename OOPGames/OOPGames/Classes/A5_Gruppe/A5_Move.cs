using System;

namespace OOPGames
{
    /// <summary>
    /// Die Spielzug-Klasse: Repräsentiert einen einzelnen Zug im Spiel
    /// </summary>
    public class A5_Move : IX_TicTacToeMove
    {
        // Properties für die Position des Zugs und den Spieler, der den Zug macht
        public int Row { get; set; }          // Zeile des Spielfelds (0-2)
        public int Column { get; set; }       // Spalte des Spielfelds (0-2)
        public int PlayerNumber { get; set; } // Spielernummer (1 oder 2)
    }
}