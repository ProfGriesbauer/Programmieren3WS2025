using System;

namespace OOpGames
{
    public class Player
    {
        public int Id { get; }

        public int Resources { get; private set; }

        public int BaseAP { get; set; } = 1;
        public int TempBonusAP { get; private set; } = 0;
        public int TotalAP => BaseAP + TempBonusAP;

        public int CaptureRate { get; set; } = 20;
        public int CapacityMax { get; private set; } = 2;
        public int CapacityUsed { get; private set; } = 0;

        public int AdjacencyBonusPerNeighbour { get; set; } = 3;

        public Player(int id)
        {
            Id = id;
        }

        // --- Ressourcen / AP / Kapazität ---

        public void AddResources(int amount)
        {
            Resources += amount;
        }

        public bool TrySpendResources(int amount)
        {
            if (Resources < amount) return false;
            Resources -= amount;
            return true;
        }

        public void ResetTempForNewTurn()
        {
            TempBonusAP = 0;
        }

        public void AddTempAP(int amount)
        {
            TempBonusAP += amount;
        }

        public void SetCapacityBasePlusBoost(int baseCapacity, int capacityBoost)
        {
            CapacityMax = baseCapacity + capacityBoost;
            if (CapacityUsed > CapacityMax)
            {
                CapacityUsed = CapacityMax;
            }
        }

        public bool TryReserveCaptureSlot()
        {
            if (CapacityUsed >= CapacityMax) return false;
            CapacityUsed++;
            return true;
        }

        public void ReleaseCaptureSlot()
        {
            if (CapacityUsed > 0) CapacityUsed--;
        }
    }
}
