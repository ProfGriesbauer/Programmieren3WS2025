using System;
using System.Linq;

namespace OOPGames
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }

        public int OwnerID { get; set; } = -1; // -1 neutral, 0/1 players

        // Basissystem
        public bool IsHomeBase { get; set; } = false;   // Ecke, NICHT eroberbar
        public bool IsTargetBase { get; set; } = false; // Zielbasis, eroberbar, win-condition
        public bool IsBase { get; set; } = false;       // nur fürs Zeichnen/Markieren

        public int ResourceYield { get; set; } = 1;
        public BoostType BoostOnTile { get; set; } = BoostType.None;

        // Capture
        public int CaptureTarget { get; set; } = 100;
        public int CaptureProgress { get; set; } = 0;
        public int CapturingPlayerID { get; set; } = -1;
        public bool IsBeingContested { get; set; } = false;

        public Tile(int x, int y) { X = x; Y = y; }

        // Troop-gesteuert: Keine Adjacency-Regel, nur Basen schützen
        public bool CanBeCapturedBy(Player player)
        {
            if (IsHomeBase) return false;          // Homebase niemals
            if (OwnerID == player.Id) return false;
            if (IsBeingContested) return false;
            return true;
        }

        // Eroberungsrate NUR durch Boosts beeinflusst
        public int ComputeCaptureRate(Player attacker, Field field)
        {
            int rate = attacker.CaptureRate;
            rate += BoostSystem.GetCaptureRateDelta(attacker, this, field);
            return System.Math.Max(1, rate);
        }

        public void AdvanceCapture(Player attacker, Field field)
        {
            CaptureProgress += ComputeCaptureRate(attacker, field);

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
