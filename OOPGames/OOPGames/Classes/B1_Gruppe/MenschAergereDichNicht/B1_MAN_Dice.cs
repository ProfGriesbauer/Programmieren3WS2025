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

        public B1_MAN_Dice()
        {
            CurrentValue = 0;
            HasBeenRolled = false;
        }

        // Roll the dice and return the result (1-6)
        public int Roll()
        {
            CurrentValue = _random.Next(1, 7); // 1 to 6 inclusive
            HasBeenRolled = true;
            return CurrentValue;
        }

        // Reset dice for next turn
        public void Reset()
        {
            CurrentValue = 0;
            HasBeenRolled = false;
        }

        public override string ToString()
        {
            return HasBeenRolled ? $"Dice: {CurrentValue}" : "Dice: not rolled";
        }
    }
}
