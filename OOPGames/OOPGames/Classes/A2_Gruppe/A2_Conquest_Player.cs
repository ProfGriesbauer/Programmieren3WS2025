using System;

namespace OOPGames
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
        public int TempCapacityBonus { get; private set; } = 0;
        private bool _extraApGrantedThisTurn = false;           

        public Player(int id) => Id = id;

        public void AddResources(int amount) => Resources += amount;

        public bool TrySpendResources(int amount)
        {
            if (Resources < amount) return false;
            Resources -= amount;
            return true;
        }

        public void ResetTempForNewTurn()
        {
            TempBonusAP = 0;
            TempCapacityBonus = 0;
            _extraApGrantedThisTurn = false;
        }

        public void AddTempCapacity(int amount) => TempCapacityBonus += amount;

        public void GrantExtraApOnce(int amount)
        {
            if (_extraApGrantedThisTurn) return;
            TempBonusAP += amount;
            _extraApGrantedThisTurn = true;
        }
        
        public void AddTempAP(int amount) => TempBonusAP += amount;

        public void SetCapacityBasePlusBoost(int baseCapacity, int capacityBoost)
        {
            CapacityMax = baseCapacity + capacityBoost;
            if (CapacityUsed > CapacityMax) CapacityUsed = CapacityMax;
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
