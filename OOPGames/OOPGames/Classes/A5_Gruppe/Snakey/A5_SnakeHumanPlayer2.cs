using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Zweiter Human Player für Snake - nutzt Pfeiltasten statt WASD
    /// </summary>
    public class A5_SnakeHumanPlayer2 : IHumanGamePlayer
    {
        public string Name => "A5 Human Player 2 Snake";
        public int PlayerNumber { get; private set; } = 2;

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is A5_SnakeRules;
        }

        public IGamePlayer Clone()
        {
            return new A5_SnakeHumanPlayer2 { PlayerNumber = this.PlayerNumber };
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
                Key.Up => "W",
                Key.Down => "S",
                Key.Left => "A",
                Key.Right => "D",
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
