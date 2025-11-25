namespace OOPGames
{
    public static class A5_Main
    {
        public static void Register(OOPGamesManager manager)
        {
            // Gemeinsamer Painter für beide Modi
            var snakePaint = new A5_SnakePaint();
            manager.RegisterPainter(snakePaint);
            
            // Player (global für alle Rules)
            var humanPlayer1 = new A5_SnakeHumanPlayer();
            humanPlayer1.SetPlayerNumber(1);
            var humanPlayer2 = new A5_SnakeHumanPlayer2();
            humanPlayer2.SetPlayerNumber(2);
            
            manager.RegisterPlayer(humanPlayer1);
            manager.RegisterPlayer(humanPlayer2);
            
            // Singleplayer-Variante
            var snakeRules = new A5_SnakeRules(false);
            manager.RegisterRules(snakeRules);
            
            // 2-Player-Variante
            var snakeRules2P = new A5_SnakeRules(true);
            manager.RegisterRules(snakeRules2P);
        }
    }
}