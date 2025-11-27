namespace OOPGames
{
    public enum BoostType
    {
        None,
        ExtraCapacity,
        ExtraIncome,
        FasterCapture,
        ExtraAP,
        ShieldUp,
        AreaJammer
    }

    public static class BoostSystem
    {
        private static readonly IBoostEffect _none = new NoBoostEffect();
        private static readonly IBoostEffect _extraCapacity = new ExtraCapacityEffect();
        private static readonly IBoostEffect _extraIncome = new ExtraIncomeEffect();
        private static readonly IBoostEffect _fasterCapture = new FasterCaptureEffect();
        private static readonly IBoostEffect _extraAp = new ExtraApEffect();
        private static readonly IBoostEffect _shieldUp = new ShieldUpEffect();
        private static readonly IBoostEffect _areaJammer = new AreaJammerEffect();

        public static IBoostEffect ToEffect(BoostType type) => type switch
        {
            BoostType.ExtraCapacity => _extraCapacity,
            BoostType.ExtraIncome => _extraIncome,
            BoostType.FasterCapture => _fasterCapture,
            BoostType.ExtraAP => _extraAp,
            BoostType.ShieldUp => _shieldUp,
            BoostType.AreaJammer => _areaJammer,
            _ => _none
        };

        // Wird im StartTurn() aufgerufen: alle owned Boost-Tiles wirken
        public static void ApplyOngoing(Game game, Player owner)
        {
            foreach (var tile in game.Field.AllTiles())
            {
                if (tile.OwnerID != owner.Id) continue;
                ToEffect(tile.BoostOnTile).ApplyOngoing(game, owner, tile);
            }
        }

        // Wird nach erfolgreicher Eroberung eines Tiles aufgerufen
        public static void OnTileCaptured(Game game, Player newOwner, Tile capturedTile)
        {
            ToEffect(capturedTile.BoostOnTile).OnCaptured(game, newOwner, capturedTile);
        }

        // Capture-Rate Delta: contestedTile + seine 4 Nachbarn auswerten
        public static int GetCaptureRateDelta(Player attacker, Tile contestedTile, Field field)
        {
            int delta = 0;

            // Boost auf dem contestedTile selbst
            delta += ToEffect(contestedTile.BoostOnTile)
                .GetCaptureRateDelta(attacker, contestedTile, contestedTile, field);

            // Boosts auf Nachbarn
            foreach (var n in field.GetNeighbours4(contestedTile))
            {
                delta += ToEffect(n.BoostOnTile)
                    .GetCaptureRateDelta(attacker, contestedTile, n, field);
            }

            return delta;
        }
    }

    // ===== Boost-Implementierungen (je Typ eine Klasse) =====

    internal sealed class NoBoostEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile) { }
        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }
        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field) => 0;
    }

    internal sealed class ExtraCapacityEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile)
            => owner.AddTempCapacity(1);

        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field) => 0;
    }

    internal sealed class ExtraIncomeEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile)
            => owner.AddResources(1); // +1 pro ExtraIncome-Tile pro Turn

        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field) => 0;
    }

    internal sealed class ExtraApEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile)
            => owner.GrantExtraApOnce(1); // egal wie viele ExtraAP-Tiles: +1/Turn (wie bisher)

        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field) => 0;
    }

    internal sealed class FasterCaptureEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile) { }
        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field)
        {
            // +10 wenn ein eigener FasterCapture-Boost in der Nachbarschaft liegt
            if (boostTile.OwnerID == attacker.Id) return +10;
            return 0;
        }
    }

    internal sealed class AreaJammerEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile) { }
        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field)
        {
            // -10 wenn ein gegnerischer Jammer in der Nachbarschaft liegt
            if (boostTile.OwnerID != -1 && boostTile.OwnerID != attacker.Id) return -10;
            return 0;
        }
    }

    internal sealed class ShieldUpEffect : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile sourceTile) { }
        public void OnCaptured(Game game, Player newOwner, Tile capturedTile) { }

        public int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field)
        {
            // Schild hilft nur beim Verteidigen: contestedTile muss einen Besitzer haben
            int defenderId = contestedTile.OwnerID;
            if (defenderId == -1) return 0;

            // -10 wenn ein ShieldUp-Boost des Verteidigers neben dem contestedTile liegt
            if (boostTile.OwnerID == defenderId) return -10;
            return 0;
        }
    }
}
