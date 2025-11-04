using System;

namespace OOPGames.B1_Gruppe
{
    public class B1_Player
    {
        public int Id { get; }
        public string Name { get; set; }
        public B1_Symbol Symbol { get; }

        public B1_Player(int id, string name, B1_Symbol symbol)
        {
            Id = id;
            Name = name ?? $"Player{Id}";
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public override string ToString() => $"{Name} ({Symbol})";
    }
}
