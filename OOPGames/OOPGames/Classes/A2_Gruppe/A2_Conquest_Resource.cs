namespace OOPGames
{
    public class ConquestPlayerState
    {
        public int PlayerID { get; }

        public int Resources { get; set; } = 0;

        /// Aktionen pro Runde (z.B. 1 Feld in Beschlag nehmen)
        public int BaseAP { get; set; } = 1;
        public int TempBonusAP { get; set; } = 0;
        public int TotalAP => BaseAP + TempBonusAP;

        /// Geschwindigkeit der Einnahme
        public int CaptureRate { get; set; } = 20;

        /// Maximal gleichzeitig laufende Einnahmen
        public int CapacityMax { get; set; } = 2;
        public int CapacityUsed { get; set; } = 0;

        /// Belohnung für viele eigene Nachbarn
        public int AdjacencyBonusPerNeighbour { get; set; } = 3;

        public ConquestPlayerState(int id)
        {
            PlayerID = id;
        }

        public void ResetTempForNewTurn()
        {
            TempBonusAP = 0;
        }
    }
}
