using System;

namespace OOPGames
{
    // Minimal rules implementation to allow painting without gameplay
    public class A4_Testgame_Rules : IGameRules
    {
        A4_Testgame_Field _field = new A4_Testgame_Field();

        public string Name { get { return "A4_Testgame_Rules_Dummy"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible { get { return false; } }

        public void DoMove(IPlayMove move)
        {
            // no-op
        }

        public void ClearField()
        {
            // no-op
        }

        public int CheckIfPLayerWon()
        {
            return -1; // no winner
        }
    }

    public class A4_Testgame_Field : IGameField
    {
        public bool CanBePaintedBy(IPaintGame painter)
        {
            // accept any painter so the canvas can be painted
            return true;
        }
    }
}
