using System;

namespace HouseExample
{
    /// <summary>
    /// Repräsentiert ein Haus mit Länge und Breite.
    /// </summary>
    public class Haus
    {
        private float _laenge = 5;
        private float _breite = 10;

        /// <summary>
        /// Erstellt ein neues Haus. Standardwerte: Länge=5, Breite=10.
        /// </summary>
        public Haus(float laenge = 5, float breite = 10)
        {
            Laenge = laenge;
            Breite = breite;
        }

        /// <summary>
        /// Länge des Hauses (in Metern). Darf nicht negativ sein.
        /// </summary>
        public float Laenge
        {
            get => _laenge;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Laenge), "Laenge darf nicht negativ sein.");
                _laenge = value;
            }
        }

        /// <summary>
        /// Breite des Hauses (in Metern). Darf nicht negativ sein.
        /// </summary>
        public float Breite
        {
            get => _breite;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Breite), "Breite darf nicht negativ sein.");
                _breite = value;
            }
        }

        /// <summary>
        /// Validiert die Dimensionen und wirft bei ungültigen Werten eine Ausnahme.
        /// </summary>
        public void ValidateDimensions()
        {
            // Zugriff auf die Properties löst die Setter-Validierung nicht aus,
            // daher werfen wir explizit, wenn Werte ungültig sind.
            if (Laenge < 0) throw new ArgumentOutOfRangeException(nameof(Laenge), "Laenge darf nicht negativ sein.");
            if (Breite < 0) throw new ArgumentOutOfRangeException(nameof(Breite), "Breite darf nicht negativ sein.");
        }

        /// <summary>
        /// Berechnet die Fläche des Hauses.
        /// </summary>
        public float BerechneFlaeche() => Laenge * Breite;

        /// <summary>
        /// Liefert eine lesbare Darstellung des Hauses.
        /// </summary>
        public override string ToString()
        {
            return $"Haus: {Laenge} x {Breite} (Fläche: {BerechneFlaeche()})";
        }
    }
}

