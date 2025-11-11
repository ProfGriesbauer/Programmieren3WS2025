using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A5_Main
    {
        public static void Register(OOPGamesManager manager)
        {
            // Registriere Snake-Spiel Komponenten
            A5_SnakeRules snakeRules = new A5_SnakeRules();
            A5_SnakePaint snakePaint = new A5_SnakePaint();
            A5_SnakeHumanPlayer humanPlayer = new A5_SnakeHumanPlayer();
            humanPlayer.SetPlayerNumber(1);

            manager.RegisterRules(snakeRules);
            manager.RegisterPainter(snakePaint);
            manager.RegisterPlayer(humanPlayer);
        }
    }
}
