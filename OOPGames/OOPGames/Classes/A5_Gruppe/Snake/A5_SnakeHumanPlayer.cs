using System;
using System.Windows.Input;

namespace OOPGames
{
    public class A5_SnakeHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber = 1;

        public string Name => "A5 Player Snake";

        public int PlayerNumber => _playerNumber;

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A5_SnakeRules;
        }

        public IGamePlayer Clone()
        {
            A5_SnakeHumanPlayer clone = new A5_SnakeHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IKeySelection keySelection)
            {
                A5_SnakeMove move = new A5_SnakeMove { PlayerNumber = PlayerNumber };
                
                switch (keySelection.Key)
                {
                    case Key.W:
                        move.Direction = "W";
                        return move;
                    case Key.S:
                        move.Direction = "S";
                        return move;
                    case Key.A:
                        move.Direction = "A";
                        return move;
                    case Key.D:
                        move.Direction = "D";
                        return move;
                }
            }
            
            return null;
        }
    }
}