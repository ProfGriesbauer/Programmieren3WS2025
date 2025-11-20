using System.Collections.Generic;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    public class B1_MAN_Player
    {
        public int PlayerNumber { get; }
        public string Name { get; set; }
        public bool IsComputer { get; set; }

        public List<B1_MAN_Piece> Pieces { get; } = new List<B1_MAN_Piece>();

        public B1_MAN_Player(int playerNumber, string name, bool isComputer=false)
        {
            PlayerNumber = playerNumber;
            Name = name;
            IsComputer = isComputer;

            for (int i = 0; i < 4; i++)
            {
                Pieces.Add(new B1_MAN_Piece(i, playerNumber));
            }
        }
    }
}
