using System;

namespace OOPGames
{
    public class A4_ShellStrike_Field : IGameField
    {
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is A4_ShellStrike_Painter;
        }
    }
}
