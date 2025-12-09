namespace OOPGames
{
  
    /// Repräsentiert eine Position im 2D-Raum mit Pixel-Genauigkeit
  
    public class PixelPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        
        // BewegungRichtungswechselsrichtung für Rotation (optional)
        public double DirX { get; set; }
        public double DirY { get; set; }

        public PixelPosition(double x, double y)
        {
            X = x;
            Y = y;
            DirX = 1; // Default: nach rechts
            DirY = 0;
        }

        
        /// Prüft ob diese Richtung entgegengesetzt zu einer anderen ist
        
        public bool IsOpposite(PixelPosition other)
        {
            return (X != 0 && X == -other.X) || (Y != 0 && Y == -other.Y);
        }
        
       
        /// Berechnet den Rotationswinkel in Grad basierend auf der Bewegungsrichtung
       
        public double GetRotationAngle()
        {
            if (DirX > 0) return 0;    // Rechts
            if (DirX < 0) return 180;  // Links
            if (DirY > 0) return 90;   // Unten
            if (DirY < 0) return 270;  // Oben
            return 0;
        }
    }
}
