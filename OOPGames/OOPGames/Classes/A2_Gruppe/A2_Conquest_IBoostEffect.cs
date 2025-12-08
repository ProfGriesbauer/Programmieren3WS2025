namespace OOPGames
{
    public interface IBoostEffect
    {
        void ApplyOngoing(Game game, Player owner, Tile sourceTile);
        void OnCaptured(Game game, Player newOwner, Tile capturedTile);

        // Neu: Capture-Rate Modifikation (Standard: 0)
        int GetCaptureRateDelta(Player attacker, Tile contestedTile, Tile boostTile, Field field);
    }
}
