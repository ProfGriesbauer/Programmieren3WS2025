using System;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    // Represents a single playing piece in "Mensch ärgere dich nicht"
    public class B1_MAN_Piece
    {
        // Unique id for the piece (0..3 per player)
        public int Id { get; }

        // Owner player number (1..4)
        public int Owner { get; }

        // Position on the main track: -1 = in base/start area, 0..39 = track index, 100..103 = home slots per player
        public int Position { get; private set; }

        public B1_MAN_Piece(int id, int owner)
        {
            Id = id;
            Owner = owner;
            Position = -1; // initially in base
        }

        public bool IsInBase => Position == -1;
        public bool IsOnTrack => Position >= 0 && Position < 40;
        public bool IsInHome => Position >= 100;

        public void SetPosition(int pos)
        {
            Position = pos;
        }

        public override string ToString()
        {
            return $"P{Owner}-{Id}@{Position}";
        }
    }
}
