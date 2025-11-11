using System;

namespace OOPGames
{
    public class A5_SnakeRules : IGameRules2
    {
        public string Name => "A5 Rules Snake";
        public IGameField CurrentField => _field;
        public bool MovesPossible => true;

        private readonly A5_SnakeField _field;

        public A5_SnakeRules()
        {
            _field = new A5_SnakeField();
        }

        public void DoMove(IPlayMove move)
        {
            if (!(move is A5_SnakeMove snakeMove)) return;

            switch (snakeMove.Direction)
            {
                case "W": _field.ChangeDirection(0, -1); break;
                case "S": _field.ChangeDirection(0, 1); break;
                case "A": _field.ChangeDirection(-1, 0); break;
                case "D": _field.ChangeDirection(1, 0); break;
            }
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