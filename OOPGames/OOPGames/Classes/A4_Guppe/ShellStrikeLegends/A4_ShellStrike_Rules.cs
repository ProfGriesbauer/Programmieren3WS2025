using System;

namespace OOPGames
{
    public class A4_ShellStrike_Rules : IGameRules
    {
        private readonly A4_ShellStrike_Field _field = new A4_ShellStrike_Field();

        public string Name => "A4 ShellStrikeLegends Rules";
        public IGameField CurrentField => _field;

        // No moves for now; just a static field to display
        public bool MovesPossible => false;

        public void DoMove(IPlayMove move)
        {
            // No-op: static field
        }

        public void ClearField()
        {
            // Nothing to clear for a static terrain preview
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }
    }
}
