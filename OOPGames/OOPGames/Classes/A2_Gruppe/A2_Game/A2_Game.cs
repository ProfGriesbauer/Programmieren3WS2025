public class Game
{
    public Board Board { get; }
    public Player[] Players { get; }
    public int CurrentPlayerId { get; private set; }
    public int TurnNumber { get; private set; } = 1;

    public Game(int width, int height)
    {
        Board = new Board(width, height);
        Players = new[] { new Player(0), new Player(1) };
        CurrentPlayerId = 0;

        SetupDefaultLayout();
    }

    private void SetupDefaultLayout()
    {
        // hier baust du deine Startbasis, Objective, Boost-Felder usw. auf dem Board
    }

    private Player CurrentPlayer => Players[CurrentPlayerId];

    // --- Rundenphasen ---
    public void StartTurn()
    {
        var p = CurrentPlayer;
        p.ResetTempForNewTurn();
        ApplyPersistentBoosts(p);
        ApplyIncome(p);
        ApplyTempBoosts(p);
    }

    public void EndTurn()
    {
        ResolveCaptures();
        if (CheckWin(out int winner))
        {
            // hier Meldung an Framework: Spielende + Gewinner
            return;
        }

        CurrentPlayerId = CurrentPlayerId == 0 ? 1 : 0;
        TurnNumber++;
        StartTurn();
    }
}
