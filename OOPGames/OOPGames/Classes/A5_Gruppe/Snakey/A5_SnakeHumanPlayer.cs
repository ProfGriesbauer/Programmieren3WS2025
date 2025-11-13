using System.Windows.Input;

namespace OOPGames
{
    public class A5_SnakeHumanPlayer : IHumanGamePlayer
    {
        public string Name => "A5 Player Snake";
        public int PlayerNumber { get; private set; } = 1;

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A5_SnakeRules;
        }

        public IGamePlayer Clone()
        {
            return new A5_SnakeHumanPlayer { PlayerNumber = this.PlayerNumber };
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(selection is IKeySelection keySelection)) return null;

            string direction = keySelection.Key switch
            {
                Key.W => "W",
                Key.S => "S",
                Key.A => "A",
                Key.D => "D",
                Key.Space => "SPACE",
                _ => null
            };

            return direction != null ? new A5_SnakeMove 
            { 
                Direction = direction, 
                PlayerNumber = PlayerNumber 
            } : null;
        }
    }
}