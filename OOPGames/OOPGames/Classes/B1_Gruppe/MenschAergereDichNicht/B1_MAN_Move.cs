using System;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    // Move representation for Mensch Ärger dich nicht
    public class B1_MAN_Move : OOPGames.IPlayMove
    {
        public int PlayerNumber { get; }
        public int PieceId { get; }
        public int Dice { get; }

        public B1_MAN_Move(int playerNumber, int pieceId, int dice)
        {
            PlayerNumber = playerNumber;
            PieceId = pieceId;
            Dice = dice;
        }
    }
}
