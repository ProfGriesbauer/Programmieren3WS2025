using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class A5_SnakeRules : IGameRules2
    {
        public string Name => "A5 Snake Game";
        private A5_SnakeField _field;

        public A5_SnakeRules()
        {
            _field = new A5_SnakeField();
        }

        public IGameField CurrentField => _field;

        public bool MovesPossible => true; // Immer möglich zu bewegen

        public void DoMove(IPlayMove move)
        {
            if (move is A5_SnakeMove snakeMove)
            {
                switch (snakeMove.Direction)
                {
                    case "W":
                        _field.ChangeDirection(0, -1);  // Nach oben
                        break;
                    case "S":
                        _field.ChangeDirection(0, 1);   // Nach unten
                        break;
                    case "A":
                        _field.ChangeDirection(-1, 0);  // Nach links
                        break;
                    case "D":
                        _field.ChangeDirection(1, 0);   // Nach rechts
                        break;
                }
            }
        }

        public void ClearField()
        {
            _field = new A5_SnakeField();
        }

        public int CheckIfPLayerWon()
        {
            return -1; // Noch keine Gewinnbedingung implementiert
        }

        public void StartedGameCall()
        {
            ClearField();
        }

        public void TickGameCall()
        {
            _field.MoveSnake(); // Bewege die Schlange bei jedem Tick
        }
    }

    public class A5_SnakeMove : IPlayMove
    {
        public string Direction { get; set; }
        public int PlayerNumber { get; set; }
    }
}