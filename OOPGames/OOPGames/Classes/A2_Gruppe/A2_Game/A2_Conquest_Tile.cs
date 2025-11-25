
using System.Linq;
using System;

namespace OOPGames{
    public class Tile
    {
        
        public int X { get; }
            public int Y { get; }

            /// -1 = neutral, 0 = Spieler, 1 = Gegner
            public int OwnerID { get; set; } = -1;

            public bool IsObjective { get; set; }    // Ziel im Gegnergebiet
            public bool IsBase { get; set; }         // Startbasis

            public int ResourceYield { get; set; } = 1;
            public BoostType BoostOnTile { get; set; } = BoostType.None;

            /// Erschwert die Einnahme
            public int DefenseLevel { get; set; } = 0;

            /// Zielwert, der erreicht werden muss (Capture-Progress)
            public int CaptureTarget { get; set; } = 100;
            public int CaptureProgress { get; set; } = 0;
            public int CapturingPlayerID { get; set; } = -1;
            public bool IsBeingContested { get; set; } = false;

            public Tile(int x, int y)
            {
                X = x;
                Y = y;
            }



        public bool CanBeCapturedBy(Player player, Field field)
        {
            if (OwnerID == player.Id) return false;
            if (IsBase && OwnerID != -1) return false; // optional: Base direkt sperren?

            bool isAdjacent = field.GetNeighbours4(this).Any(n => n.OwnerID == player.Id);
            return isAdjacent;
        }

        public int ComputeCaptureRate(Player attacker, Field field)
        {
            int ownedNeighbours = field
                .GetNeighbours4(this)
                .Count(n => n.OwnerID == attacker.Id);

            int rate = attacker.CaptureRate +
                    ownedNeighbours * attacker.AdjacencyBonusPerNeighbour;

            // Boosts/Jammer
            bool hasFastBoost = field.GetNeighbours4(this)
                .Any(n => n.OwnerID == attacker.Id &&
                        n.BoostOnTile == BoostType.FasterCapture);

            if (hasFastBoost) rate += 10;

            bool hasEnemyJammer = field.GetNeighbours4(this)
                .Any(n => n.OwnerID != -1 &&
                        n.OwnerID != attacker.Id &&
                        n.BoostOnTile == BoostType.AreaJammer);

            if (hasEnemyJammer) rate -= 10;

            rate = Math.Max(1, rate - DefenseLevel / 5);
            return rate;
        }

        public void AdvanceCapture(Player attacker, Field field)
        {
            int rate = ComputeCaptureRate(attacker, field);
            CaptureProgress += rate;

            if (CaptureProgress >= CaptureTarget)
            {
                OwnerID = attacker.Id;
                CaptureProgress = 0;
                IsBeingContested = false;
                CapturingPlayerID = -1;
            }
        }
    }
}