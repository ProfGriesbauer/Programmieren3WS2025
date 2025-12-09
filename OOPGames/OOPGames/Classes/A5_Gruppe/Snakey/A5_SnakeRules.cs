using System;

namespace OOPGames
{
    public class A5_SnakeRules : IGameRules2
    {
        public string Name { get; }
        public IGameField CurrentField => _field;
        public bool MovesPossible => true;

        private readonly A5_SnakeField _field;
        private readonly bool _isTwoPlayerMode;

        public A5_SnakeRules(bool twoPlayerMode = false)
        {
            _isTwoPlayerMode = twoPlayerMode;
            _field = new A5_SnakeField(twoPlayerMode);
            Name = twoPlayerMode ? "A5 Snake (Multiplayer)" : "A5 Snake (Singleplayer)";
        }

        public void DoMove(IPlayMove move)
        {
            if (!(move is A5_SnakeMove snakeMove)) return;

            int playerNumber = snakeMove.PlayerNumber;

            // Alle Richtungsänderungen werden sofort gepuffert, nicht blockiert
            switch (snakeMove.Direction)
            {
                case "W": _field.ChangeDirection(0, -1, playerNumber); break;
                case "S": _field.ChangeDirection(0, 1, playerNumber); break;
                case "A": _field.ChangeDirection(-1, 0, playerNumber); break;
                case "D": _field.ChangeDirection(1, 0, playerNumber); break;
                case "SPACE": 
                    // If game over screen is shown, acknowledge it and prepare for restart
                    if (!_field.IsGameRunning && _field.GameOverScreenShown == false)
                    {
                        _field.AcknowledgeGameOver();
                    }
                    else
                    {
                        _field.StartGame();
                    }
                    break;
            }
            
            // Nicht blockieren - Framework kann sofort nächsten Move verarbeiten
        }

        public void ClearField() { }
        public void StartedGameCall() { }
        public void TickGameCall() { }

        public int CheckIfPLayerWon()
        {
            return -1; // Snake has no classic win condition
        }
    }

    public class A5_SnakeMove : IPlayMove
    {
        public string Direction { get; set; }
        public int PlayerNumber { get; set; }
    }
}