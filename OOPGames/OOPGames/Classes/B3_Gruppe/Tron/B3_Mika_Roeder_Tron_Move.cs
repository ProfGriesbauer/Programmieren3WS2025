using System;

namespace OOPGames
{
    public enum B3TronDirection { Up, Right, Down, Left }

    // Move for Tron: a direction change request from a player
    public class B3_Mika_Roeder_Tron_Move : IPlayMove
    {
        public int PlayerNumber { get; private set; }
        public B3TronDirection Direction { get; private set; }

        public B3_Mika_Roeder_Tron_Move(int playerNumber, B3TronDirection dir)
        {
            PlayerNumber = playerNumber;
            Direction = dir;
        }
    }
}
