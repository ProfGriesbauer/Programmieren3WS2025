namespace OOPGames{

    public enum BoostType
        {
            None,
            ExtraCapacity,   // +1 CapacityMax solange dieses Feld dir gehört
            ExtraIncome,     // + Ressourcen pro Runde
            FasterCapture,   // schnellere Einnahmen auf benachbarten Feldern
            ExtraAP,         // 1x in einer Runde 2 Felder setzen (zusätzliche Aktion)
            ShieldUp,        // höherer Verteidigungswert (DefenseLevel)
            AreaJammer       // verlangsamt gegnerische Einnahmen in Nachbarschaft
        }

    public static class BoostSystem
    {
        public static void ApplyOngoing(Game game, Player player)
        {
            foreach (var tile in game.Board.AllTiles()
                        .Where(t => t.OwnerID == player.Id))
            {
                IBoostEffect effect = ToEffect(tile.BoostOnTile);
                effect?.ApplyOngoing(game, player, tile);
            }
        }

        public static void OnTileCaptured(Game game, Player player, Tile tile)
        {
            IBoostEffect effect = ToEffect(tile.BoostOnTile);
            effect?.OnCaptured(game, player, tile);
        }

        private static IBoostEffect? ToEffect(BoostType type)
        {
            return type switch
            {
                BoostType.ExtraCapacity => new ExtraCapacityBoost(),
                // BoostType.ExtraAP => neue Klasse ExtraApBoost usw.
                _ => null
            };
        }
    }

    //ab hier kommen die einzielen Boost-Effekt-Klassen
    public class ExtraCapacityBoost : IBoostEffect
    {
        public void ApplyOngoing(Game game, Player owner, Tile tile)
        {
            // z.B. +1 auf CapacityMax pro Feld
            owner.SetCapacityBasePlusBoost(baseCap: 2,
                boost: game.Board.AllTiles()
                    .Count(t => t.OwnerID == owner.Id && t.BoostOnTile == BoostType.ExtraCapacity));
        }

        public void OnCaptured(Game game, Player owner, Tile tile)
        {
            // keine einmalige Aktion nötig
        }
    }
}
