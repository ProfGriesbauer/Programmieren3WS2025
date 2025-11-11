namespace OOPGames
{
    public static class A5_Main
    {
        public static void Register(OOPGamesManager manager)
        {
            var snakeRules = new A5_SnakeRules();
            var snakePaint = new A5_SnakePaint();
            var humanPlayer = new A5_SnakeHumanPlayer();
            humanPlayer.SetPlayerNumber(1);

            manager.RegisterRules(snakeRules);
            manager.RegisterPainter(snakePaint);
            manager.RegisterPlayer(humanPlayer);
        }
    }
}