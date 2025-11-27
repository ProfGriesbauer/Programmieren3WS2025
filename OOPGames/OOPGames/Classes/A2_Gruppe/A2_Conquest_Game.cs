using System;
using System.Linq;
using System.Text;

namespace OOPGames
{
    public class Game
    {
        public Troop[] Troops { get; private set; } = new Troop[0];

        private const int TargetOffset = 3;

        public Field Field { get; }
        public Player[] Players { get; }

        public int CurrentPlayerId { get; private set; } = 0;
        public int TurnNumber { get; private set; } = 1;

        public Game(int width, int height)
        {
            Field = new Field(width, height);
            Players = new[]
            {
                new Player(0),
                new Player(1)
            };

            SetupDefaultLayout();
        }

        public Player CurrentPlayer => Players[CurrentPlayerId];

        private void SetupDefaultLayout()
        {
            // 1) Home-Bases (Ecken, NICHT eroberbar)
            var home0 = Field.GetTile(0, 0);
            home0.OwnerID = 0;
            home0.IsBase = true;
            home0.IsHomeBase = true;
            home0.ResourceYield = 2;

            var home1 = Field.GetTile(Field.Width - 1, Field.Height - 1);
            home1.OwnerID = 1;
            home1.IsBase = true;
            home1.IsHomeBase = true;
            home1.ResourceYield = 2;

            // 2) Target-Bases (Siegziele, eroberbar) relativ zur gegnerischen Home-Base um (5,5)
            var targetFor1 = Field.GetTile(TargetOffset, TargetOffset); // liegt nahe Home0 -> Ziel für Spieler 1
            targetFor1.OwnerID = 0;               // gehört anfangs Spieler 0
            targetFor1.IsBase = true;
            targetFor1.IsTargetBase = true;
            targetFor1.DefenseLevel = 30;
            targetFor1.ResourceYield = 2;

            var targetFor0 = Field.GetTile(Field.Width - 1 - TargetOffset, Field.Height - 1 - TargetOffset); // nahe Home1 -> Ziel für Spieler 0
            targetFor0.OwnerID = 1;               // gehört anfangs Spieler 1
            targetFor0.IsBase = true;
            targetFor0.IsTargetBase = true;
            targetFor0.DefenseLevel = 30;
            targetFor0.ResourceYield = 2;

            // 3) Optional: ein paar Boosts “relativ” verteilen
            int x1 = Field.Width / 3;
            int x2 = (2 * Field.Width) / 3;
            int y1 = Field.Height / 3;
            int y2 = (2 * Field.Height) / 3;

            Field.GetTile(x1, y1).BoostOnTile = BoostType.ExtraCapacity;
            Field.GetTile(x2, y2).BoostOnTile = BoostType.FasterCapture;
            Field.GetTile(x1, y2).BoostOnTile = BoostType.ExtraAP;
            Field.GetTile(x2, y1).BoostOnTile = BoostType.AreaJammer;
             PlaceOwnedTile(0, 1, 0);
            PlaceOwnedTile(0, 0, 1);

            // Player 1 Home: (W-1,H-1) -> Troops auf (W-2,H-1) und (W-1,H-2)
            PlaceOwnedTile(1, Field.Width - 2, Field.Height - 1);
            PlaceOwnedTile(1, Field.Width - 1, Field.Height - 2);

            Troops = new[]
            {
                new Troop(0, 1, 0),
                new Troop(0, 0, 1),
                new Troop(1, Field.Width - 2, Field.Height - 1),
                new Troop(1, Field.Width - 1, Field.Height - 2)
            };
            
        }
        private void PlaceOwnedTile(int ownerId, int x, int y)
        {
            var t = Field.GetTile(x, y);
            t.OwnerID = ownerId;
            // optional: t.ResourceYield = 1; // Standard lassen
        }


        // ===== Rundenablauf =====

        public void StartTurn()
        {
            var p = CurrentPlayer;
            p.ResetTempForNewTurn();

            ApplyIncome(p);
            ApplyCapacityBoosts(p);
            ApplyTempBoosts(p);
        }

        public void EndTurn()
        {
            ResolveCaptures();

            if (CheckWin(out int winner))
            {
                // hier später: Meldung ans Framework
                return;
            }

            CurrentPlayerId = (CurrentPlayerId == 0) ? 1 : 0;
            TurnNumber++;

            StartTurn();
        }

        // ===== Wirtschaft & Boosts =====

        private void ApplyIncome(Player p)
        {
            int income = Field.AllTiles()
                .Where(t => t.OwnerID == p.Id)
                .Sum(t => t.ResourceYield);

            p.AddResources(income);
        }

        private void ApplyCapacityBoosts(Player p)
        {
            int baseCap = 2;
            int boost = Field.AllTiles()
                .Count(t => t.OwnerID == p.Id &&
                            t.BoostOnTile == BoostType.ExtraCapacity);

            p.SetCapacityBasePlusBoost(baseCap, boost);
        }

        private void ApplyTempBoosts(Player p)
        {
            bool ownsAPBoost = Field.AllTiles()
                .Any(t => t.OwnerID == p.Id &&
                          t.BoostOnTile == BoostType.ExtraAP);

            if (ownsAPBoost)
            {
                p.AddTempAP(1);
            }
        }

        // ===== Aktionen / Eroberung =====

        public bool TryStartCapture(int x, int y, int cost = 5)
        {
            var tile = Field.GetTile(x, y);
            var p = CurrentPlayer;

            if (!tile.CanBeCapturedBy(p, Field)) return false;
            if (!p.TrySpendResources(cost)) return false;
            if (!p.TryReserveCaptureSlot()) return false;

            tile.CapturingPlayerID = p.Id;
            tile.IsBeingContested = true;
            return true;
        }

        public void ResolveCaptures()
        {
            foreach (var tile in Field.AllTiles().Where(t => t.IsBeingContested))
            {
                var attacker = Players[tile.CapturingPlayerID];
                int ownerBefore = tile.OwnerID;

                tile.AdvanceCapture(attacker, Field);

                if (!tile.IsBeingContested && tile.OwnerID == attacker.Id)
                {
                    attacker.ReleaseCaptureSlot();
                    // hier könnte man Boost-Effekte beim Einnehmen triggern
                }
            }
        }

       public bool CheckWin(out int winnerId)
    {
        // Spieler 0 gewinnt, wenn er die Target-Base von Spieler 1 erobert hat
        var targetFor0 = Field.GetTile(Field.Width - 1 - TargetOffset, Field.Height - 1 - TargetOffset);
        if (targetFor0.IsTargetBase && targetFor0.OwnerID == 0)
        {
            winnerId = 0;
            return true;
        }

        // Spieler 1 gewinnt, wenn er die Target-Base von Spieler 0 erobert hat
        var targetFor1 = Field.GetTile(TargetOffset, TargetOffset);
        if (targetFor1.IsTargetBase && targetFor1.OwnerID == 1)
        {
            winnerId = 1;
            return true;
        }

        winnerId = -1;
        return false;
    }



        // Debug-Ansicht im Text
        public string RenderAscii()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Field.Height; y++)
            {
                for (int x = 0; x < Field.Width; x++)
                {
                    var t = Field.GetTile(x, y);
                    char c = t.OwnerID switch
                    {
                        -1 => '.',
                        0  => 'A',
                        1  => 'B',
                        _  => '?'
                    };
                    if (t.IsObjective) c = char.ToLower(c);
                    sb.Append(c).Append(' ');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
