using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPGames
{
    /// <summary>
    /// Interface für Labyrinth-Spielzustand.
    /// Ermöglicht Zugriff auf Field, Timer und Game-Status ohne zirkuläre Referenz.
    /// Der Painter braucht nur dieses Interface, nicht die konkrete Rules-Klasse.
    /// </summary>
    public interface IB2_MazeGameState
    {
        /// Das Spielfeld
        IGameField Field { get; }
        
        /// Verbleibende Countdown-Zeit in Sekunden (0 wenn vorbei)
        double RemainingCountdown { get; }
        
        /// Ist der Countdown noch aktiv?
        bool IsCountdownActive { get; }
        
        /// Ist das Spiel beendet? (Spieler hat Ziel erreicht)
        bool GameEnded { get; }
    }
}
