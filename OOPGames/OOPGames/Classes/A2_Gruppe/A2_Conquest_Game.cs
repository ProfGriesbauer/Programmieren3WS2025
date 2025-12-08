using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public class Game
    {
        // === Settings ===
        private const int TargetOffset = 3;  // von x/y=5 auf 3 reduziert
        private const int CaptureCost = 5;   // Kosten, um Capture zu starten
        private const int BaseCapacity = 2;

        public Field Field { get; }
        public Player[] Players { get; }

        public int CurrentPlayerId { get; private set; } = 0;
        public int TurnNumber { get; private set; } = 1;

        // === Troops ===
        private readonly Troop[,] _troops = new Troop[2, 2]; // [playerId, localIndex]
        public int[] SelectedTroopLocalIndex { get; } = new[] { -1, -1 };

        public Game(int width, int height)
        {
            Field = new Field(width, height);
            Players = new[] { new Player(0), new Player(1) };

            SetupDefaultLayout();
            SpawnTroopsAndClaimStartTiles();

            StartTurn();
        }

        public Player CurrentPlayer => Players[CurrentPlayerId];

        public Troop GetTroop(int playerId, int localIndex) => _troops[playerId, localIndex];

        public Troop[] GetTroopsOf(int playerId) => new[] { _troops[playerId, 0], _troops[playerId, 1] };

        public bool TryGetTroopAt(int x, int y, out Troop troop)
        {
            foreach (var t in new[] { _troops[0, 0], _troops[0, 1], _troops[1, 0], _troops[1, 1] })
            {
                if (t != null && t.X == x && t.Y == y)
                {
                    troop = t;
                    return true;
                }
            }
            troop = null;
            return false;
        }

        public bool IsOccupiedByTroop(int x, int y) => TryGetTroopAt(x, y, out _);

        private static bool IsKingMove(int fx, int fy, int tx, int ty)
        {
            int dx = Math.Abs(tx - fx);
            int dy = Math.Abs(ty - fy);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }


        private (int x, int y) GetHomeCoord(int playerId)
    => playerId == 0 ? (0, 0) : (Field.Width - 1, Field.Height - 1);

        private bool HasConnectedPathFromHomeToAnyNeighbour(int playerId, Tile target)
        {
            // Welche Nachbarn gehören dem Spieler überhaupt?
            var ownedNeighbours = Field.GetNeighbours4(target)
                .Where(n => n.OwnerID == playerId)
                .ToList();

            if (ownedNeighbours.Count == 0) return false;

            var (hx, hy) = GetHomeCoord(playerId);

            var visited = new bool[Field.Width, Field.Height];
            var q = new Queue<(int x, int y)>();

            q.Enqueue((hx, hy));
            visited[hx, hy] = true;

            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                var cur = Field.GetTile(x, y);

                foreach (var n in Field.GetNeighbours4(cur))
                {
                    if (n.OwnerID != playerId) continue;
                    if (visited[n.X, n.Y]) continue;

                    visited[n.X, n.Y] = true;
                    q.Enqueue((n.X, n.Y));
                }
            }

            return ownedNeighbours.Any(n => visited[n.X, n.Y]);
        }

        // öffentlich, damit die KI die Regel ebenfalls sauber prüfen kann
        public bool MeetsTargetBasePreconditions(int playerId, Tile tile)
        {
            if (!tile.IsTargetBase) return true;

            // Regel 1: mind. 2 Nachbarn schon owned
            int ownedNeighbours = Field.GetNeighbours4(tile).Count(n => n.OwnerID == playerId);
            if (ownedNeighbours < 2) return false;

            // Regel 2: Verbindung zur Homebase über owned Tiles
            return HasConnectedPathFromHomeToAnyNeighbour(playerId, tile);
        }


        public bool IsLegalTroopMove(int playerId, int troopLocalIndex, int targetX, int targetY)
        {
            if (playerId != CurrentPlayerId) return false;

            var troop = _troops[playerId, troopLocalIndex];
            if (troop == null) return false;

            if (targetX < 0 || targetY < 0 || targetX >= Field.Width || targetY >= Field.Height) return false;
            if (!IsKingMove(troop.X, troop.Y, targetX, targetY)) return false;
            if (IsOccupiedByTroop(targetX, targetY)) return false;

            return true;
        }

        // === Turn flow ===
        public void StartTurn()
        {
            var p = CurrentPlayer;
            p.ResetTempForNewTurn();

            // Base income: Summe der yields
            int income = Field.AllTiles()
                .Where(t => t.OwnerID == p.Id)
                .Sum(t => t.ResourceYield);

            p.AddResources(income);

            // Boosts (OOP)
            BoostSystem.ApplyOngoing(this, p);

            // Capacity nach Boosts setzen
            p.SetCapacityBasePlusBoost(BaseCapacity, p.TempCapacityBonus);
        }


        public void EndTurn()
        {
            ResolveCaptures();

            if (CheckWin(out _))
                return;

            CurrentPlayerId = (CurrentPlayerId == 0) ? 1 : 0;
            TurnNumber++;

            StartTurn();
        }

        // === Economy / boosts ===
        private void ApplyIncome(Player p)
        {
            int income = Field.AllTiles()
                .Where(t => t.OwnerID == p.Id)
                .Sum(t => t.ResourceYield);

            // ExtraIncome-Boost: +1 pro Feld mit ExtraIncome
            int extra = Field.AllTiles()
                .Count(t => t.OwnerID == p.Id && t.BoostOnTile == BoostType.ExtraIncome);

            p.AddResources(income + extra);
        }

        private void ApplyCapacityBoosts(Player p)
        {
            int boost = Field.AllTiles()
                .Count(t => t.OwnerID == p.Id && t.BoostOnTile == BoostType.ExtraCapacity);

            p.SetCapacityBasePlusBoost(BaseCapacity, boost);
        }

        private void ApplyTempBoosts(Player p)
        {
            int apBoostCount = Field.AllTiles()
                .Count(t => t.OwnerID == p.Id && t.BoostOnTile == BoostType.ExtraAP);

            if (apBoostCount > 0)
                p.AddTempAP(1); // max +1 pro Turn (simpel)
        }

        // === Layout ===
        private void SetupDefaultLayout()
        {
            // Player 0 Homebase: (0,0) nicht eroberbar
            var home0 = Field.GetTile(0, 0);
            home0.OwnerID = 0;
            home0.IsBase = true;
            home0.IsHomeBase = true;
            home0.ResourceYield = 2;

            // Player 1 Homebase: (W-1,H-1) nicht eroberbar
            var home1 = Field.GetTile(Field.Width - 1, Field.Height - 1);
            home1.OwnerID = 1;
            home1.IsBase = true;
            home1.IsHomeBase = true;
            home1.ResourceYield = 2;

            // Target Base für Spieler 1 (liegt im Gebiet von Spieler 0 nahe (0,0))
            var targetFor1 = Field.GetTile(TargetOffset, TargetOffset);
            targetFor1.OwnerID = 0;
            targetFor1.IsBase = true;
            targetFor1.IsTargetBase = true;
            targetFor1.ResourceYield = 2;
            targetFor1.CaptureTarget = 120;

            // Target Base für Spieler 0 (liegt im Gebiet von Spieler 1 nahe (W-1,H-1))
            var targetFor0 = Field.GetTile(Field.Width - 1 - TargetOffset, Field.Height - 1 - TargetOffset);
            targetFor0.OwnerID = 1;
            targetFor0.IsBase = true;
            targetFor0.IsTargetBase = true;
            targetFor0.ResourceYield = 2;
            targetFor0.CaptureTarget = 120;

            // Ein paar Boosts relativ (optional)
            int x1 = Field.Width / 3;
            int x2 = (2 * Field.Width) / 3;
            int y1 = Field.Height / 3;
            int y2 = (2 * Field.Height) / 3;

            Field.GetTile(x1, y1).BoostOnTile = BoostType.ExtraCapacity;
            Field.GetTile(x2, y2).BoostOnTile = BoostType.FasterCapture;
            Field.GetTile(x1, y2).BoostOnTile = BoostType.ExtraAP;
            Field.GetTile(x2, y1).BoostOnTile = BoostType.AreaJammer;
        }

        private void SpawnTroopsAndClaimStartTiles()
        {
            // P0 Homebase (0,0) -> Troops auf (1,0) und (0,1)
            _troops[0, 0] = new Troop(0, 0, 1, 0);
            _troops[0, 1] = new Troop(0, 1, 0, 1);

            // P1 Homebase (W-1,H-1) -> Troops auf (W-2,H-1) und (W-1,H-2)
            _troops[1, 0] = new Troop(1, 0, Field.Width - 2, Field.Height - 1);
            _troops[1, 1] = new Troop(1, 1, Field.Width - 1, Field.Height - 2);

            // Felder unter Troops sind direkt erobert
            ClaimTileIfPossible(_troops[0, 0]);
            ClaimTileIfPossible(_troops[0, 1]);
            ClaimTileIfPossible(_troops[1, 0]);
            ClaimTileIfPossible(_troops[1, 1]);
        }

        private void ClaimTileIfPossible(Troop t)
        {
            if (t.X < 0 || t.Y < 0 || t.X >= Field.Width || t.Y >= Field.Height) return;
            var tile = Field.GetTile(t.X, t.Y);
            if (!tile.IsHomeBase) tile.OwnerID = t.OwnerId;
        }

        // === Troop Move ===
        public bool TryMoveTroop(int playerId, int troopLocalIndex, int targetX, int targetY)
        {
            if (!IsLegalTroopMove(playerId, troopLocalIndex, targetX, targetY)) return false;

            var troop = _troops[playerId, troopLocalIndex];

            // Capture am alten Feld abbrechen, falls Troop der Capturer war
            CancelCaptureIfLeaving(troop);

            troop.X = targetX;
            troop.Y = targetY;

            // Falls Feld nicht owned -> Capture starten
            TryStartCaptureAtTroopPosition(troop);

            SelectedTroopLocalIndex[playerId] = -1;
            return true;
        }

        private void CancelCaptureIfLeaving(Troop troop)
        {
            var oldTile = Field.GetTile(troop.X, troop.Y);
            if (oldTile.IsBeingContested && oldTile.CapturingPlayerID == troop.OwnerId)
            {
                oldTile.IsBeingContested = false;
                oldTile.CapturingPlayerID = -1;
                oldTile.CaptureProgress = 0;
                Players[troop.OwnerId].ReleaseCaptureSlot();
            }
        }

        private void TryStartCaptureAtTroopPosition(Troop troop)
        {
            var tile = Field.GetTile(troop.X, troop.Y);
            var p = Players[troop.OwnerId];

            if (!tile.CanBeCapturedBy(p)) return;

            // TargetBase nur dann eroberbar, wenn mind. 2 Nachbarn schon erobert sind
            if (tile.IsTargetBase)
            {
                int ownedNeighbours = Field.GetNeighbours4(tile).Count(n => n.OwnerID == p.Id);
                if (ownedNeighbours < 2) return;
            }

            if (!p.TrySpendResources(CaptureCost)) return;
            if (!p.TryReserveCaptureSlot()) return;

            if (!tile.CanBeCapturedBy(p)) return;

            // TargetBase: nur wenn Nachbarn+Connection erfüllt
            if (!MeetsTargetBasePreconditions(p.Id, tile)) return;


            tile.CapturingPlayerID = p.Id;
            tile.IsBeingContested = true;
        }


        // Capture läuft nur, solange Troop auf dem Tile steht
        public void ResolveCaptures()
        {
            foreach (var tile in Field.AllTiles().Where(t => t.IsBeingContested).ToList())
            {
                int capturer = tile.CapturingPlayerID;

                // Troop des Capturers muss auf dem Feld stehen
                if (!TryGetTroopAt(tile.X, tile.Y, out var troopOnTile) || troopOnTile.OwnerId != capturer)
                {
                    tile.IsBeingContested = false;
                    tile.CapturingPlayerID = -1;
                    tile.CaptureProgress = 0;
                    Players[capturer].ReleaseCaptureSlot();
                    continue;
                }

                var attacker = Players[capturer];
                bool wasContested = tile.IsBeingContested;

                tile.AdvanceCapture(attacker, Field);

                if (wasContested && !tile.IsBeingContested && tile.OwnerID == attacker.Id)
                {
                    attacker.ReleaseCaptureSlot();
                    BoostSystem.OnTileCaptured(this, attacker, tile);
                }
            }
        }

        // Winner: wer die gegnerische TargetBase besitzt
        public bool CheckWin(out int winnerId)
        {
            // Target nahe (0,0) gehört ursprünglich Player 0 -> wenn OwnerID==1 => Player 1 gewinnt
            var targetFor1 = Field.GetTile(TargetOffset, TargetOffset);
            if (targetFor1.IsTargetBase && targetFor1.OwnerID == 1)
            {
                winnerId = 1;
                return true;
            }

            // Target nahe (W-1,H-1) gehört ursprünglich Player 1 -> wenn OwnerID==0 => Player 0 gewinnt
            var targetFor0 = Field.GetTile(Field.Width - 1 - TargetOffset, Field.Height - 1 - TargetOffset);
            if (targetFor0.IsTargetBase && targetFor0.OwnerID == 0)
            {
                winnerId = 0;
                return true;
            }

            winnerId = -1;
            return false;
        }
    }
}
