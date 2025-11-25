interface IBoostEffect
{
    void ApplyOngoing(Game game, Player owner, Tile sourceTile);
    void OnCaptured(Game game, Player owner, Tile capturedTile);
}
