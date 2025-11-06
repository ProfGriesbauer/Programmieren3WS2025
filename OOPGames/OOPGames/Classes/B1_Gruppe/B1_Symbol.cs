using System;

namespace OOPGames.B1_Gruppe
{
    // Basisklasse für ein Symbol (Kreuz oder Kreis)
    public abstract class B1_Symbol
    {
        public string Name { get; }

        protected B1_Symbol(string name)
        {
            Name = name;
        }

        // Ein kleines Darstellungszeichen, nützlich für Tests/Console
        public abstract char CharRepresentation { get; }

        public override string ToString() => Name;
    }
}
