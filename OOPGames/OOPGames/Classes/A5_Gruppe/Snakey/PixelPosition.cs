namespace OOPGames
{
    /// <summary>
    /// Repräsentiert eine Position im 2D-Raum mit Pixel-Genauigkeit
    /// </summary>
    public class PixelPosition
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PixelPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Prüft ob diese Richtung entgegengesetzt zu einer anderen ist
        /// </summary>
        public bool IsOpposite(PixelPosition other)
        {
            return (X != 0 && X == -other.X) || (Y != 0 && Y == -other.Y);
        }
    }
}
