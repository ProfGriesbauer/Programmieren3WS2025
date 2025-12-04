using System;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    // Represents a dice for "Mensch ärgere dich nicht"
    public class B1_MAN_Dice
    {
        private static Random _random = new Random();
        
        // Current dice value (1-6), or 0 if not rolled yet
        public int CurrentValue { get; private set; }
        
        // True if dice has been rolled this turn
        public bool HasBeenRolled { get; private set; }
        
        // Letzter Wert für Anzeige bei 3-Würfel-Regel
        public int LastRolledValue { get; private set; }

        public B1_MAN_Dice()
        {
            CurrentValue = 0;
            HasBeenRolled = false;
            LastRolledValue = 0;
        }

        // Roll the dice and return the result (1-6)
        public int Roll()
        {
            CurrentValue = _random.Next(1, 7); // 1 to 6 inclusive
            LastRolledValue = CurrentValue;
            HasBeenRolled = true;
            return CurrentValue;
        }

        // Reset dice for next turn
        public void Reset()
        {
            CurrentValue = 0;
            HasBeenRolled = false;
            // LastRolledValue bleibt erhalten für Anzeige
        }
        
        // Vollständiger Reset (z.B. bei EndTurn)
        public void FullReset()
        {
            CurrentValue = 0;
            HasBeenRolled = false;
            LastRolledValue = 0;
        }

        public override string ToString()
        {
            return HasBeenRolled ? $"Dice: {CurrentValue}" : "Dice: not rolled";
        }
    }
}
