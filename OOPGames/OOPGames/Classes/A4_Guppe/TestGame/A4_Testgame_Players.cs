using System;

namespace OOPGames
{
    // Minimal human player: does not produce moves but satisfies interface
    public class A4_Testgame_HumanPlayer : IHumanGamePlayer
    {
        int _playerNumber = 0;
        public string Name { get { return "A4_Testgame_HumanPlayer"; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public int PlayerNumber { get { return _playerNumber; } }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return true;
        }

        public IGamePlayer Clone()
        {
            return new A4_Testgame_HumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            // no moves in this dummy player
            return null;
        }
    }

    // Minimal computer player: no logic, satisfies interface
    public class A4_Testgame_ComputerPlayer : IComputerGamePlayer
    {
        int _playerNumber = 0;
        public string Name { get { return "A4_Testgame_ComputerPlayer"; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public int PlayerNumber { get { return _playerNumber; } }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return true;
        }

        public IGamePlayer Clone()
        {
            return new A4_Testgame_ComputerPlayer();
        }

        public IPlayMove GetMove(IGameField field)
        {
            return null;
        }
    }
}
