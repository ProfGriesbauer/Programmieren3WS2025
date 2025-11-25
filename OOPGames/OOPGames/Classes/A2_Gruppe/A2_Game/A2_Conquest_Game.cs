using System;
using System.Linq;
using System.Text;

namespace OOPGames
{
    public class Game
    {
        public Field Board { get; }
        public Player[] Players { get; }

        public int CurrentPlayerId { get; private set; } = 0;
        public int TurnNumber { get; private set; } = 1;

        public Game(int width, int height)
        {
            Board = new Field(width, height);
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
            // Startbasis links
            var baseTile = Board.GetTile(0, Board.Height / 2);
            baseTile.OwnerID = 0;
            baseTile.IsBase = true;
            baseTile.ResourceYield = 2;

            // Objective rechts
            var objTile = Board.GetTile(Board.Width - 1, Board.Height / 2);
            objTile.OwnerID = 1;
            objTile.IsObjective = true;
            objTile.DefenseLevel = 20;
            objTile.ResourceYield = 2;

            // ein paar Boost-Felder als Beispiel
            if (Board.Width > 4 && Board.Height > 2)
            {
                Board.GetTile(2, 1).BoostOnTile = BoostType.ExtraCapacity;
                Board.GetTile(3, 3).BoostOnTile = BoostType.FasterCapture;
                Board.GetTile(1, 3).BoostOnTile = BoostType.ExtraAP;
                Board.GetTile(4, 1).BoostOnTile = BoostType.AreaJammer;
            }
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
            int income = Board.AllTiles()
                .Where(t => t.OwnerID == p.Id)
                .Sum(t => t.ResourceYield);

            p.AddResources(income);
        }

        private void ApplyCapacityBoosts(Player p)
        {
            int baseCap = 2;
            int boost = Board.AllTiles()
                .Count(t => t.OwnerID == p.Id &&
                            t.BoostOnTile == BoostType.ExtraCapacity);

            p.SetCapacityBasePlusBoost(baseCap, boost);
        }

        private void ApplyTempBoosts(Player p)
        {
            bool ownsAPBoost = Board.AllTiles()
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
            var tile = Board.GetTile(x, y);
            var p = CurrentPlayer;

            if (!tile.CanBeCapturedBy(p, Board)) return false;
            if (!p.TrySpendResources(cost)) return false;
            if (!p.TryReserveCaptureSlot()) return false;

            tile.CapturingPlayerID = p.Id;
            tile.IsBeingContested = true;
            return true;
        }

        public void ResolveCaptures()
        {
            foreach (var tile in Board.AllTiles().Where(t => t.IsBeingContested))
            {
                var attacker = Players[tile.CapturingPlayerID];
                int ownerBefore = tile.OwnerID;

                tile.AdvanceCapture(attacker, Board);

                if (!tile.IsBeingContested && tile.OwnerID == attacker.Id)
                {
                    attacker.ReleaseCaptureSlot();
                    // hier könnte man Boost-Effekte beim Einnehmen triggern
                }
            }
        }

        public bool CheckWin(out int winnerId)
        {
            foreach (var t in Board.AllTiles().Where(t => t.IsObjective))
            {
                if (t.OwnerID != -1)
                {
                    winnerId = t.OwnerID;
                    return true;
                }
            }

            winnerId = -1;
            return false;
        }

        // Debug-Ansicht im Text
        public string RenderAscii()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Board.Height; y++)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    var t = Board.GetTile(x, y);
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
