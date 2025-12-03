using System;
using System.Collections.Generic;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    public class B1_MAN_HumanPlayer : OOPGames.IHumanGamePlayer
    {
        int _PlayerNumber = 0;

        public string Name => "B1_MAN_HumanPlayer";

        public int PlayerNumber => _PlayerNumber;

        public void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(OOPGames.IGameRules rules)
        {
            return rules is B1_MAN_Rules || (rules?.CurrentField is B1_MAN_Board);
        }

        // Simplified human interaction: click triggers a roll and the first valid piece is moved
        public OOPGames.IPlayMove GetMove(OOPGames.IMoveSelection selection, OOPGames.IGameField field)
        {
            // WICHTIG: Diese Methode sollte NICHT verwendet werden, da die UI (B1_MAN_Paint)
            // das komplette Spiel steuert (Würfel + Figurenauswahl).
            // Wenn diese Methode aufgerufen wird, bedeutet das, dass das Framework versucht,
            // einen Zug zu machen, obwohl die UI bereits alles managed.
            // Um Doppel-Züge zu verhindern, geben wir null zurück.
            
            // Das Framework sollte diese Methode nicht aufrufen, da:
            // 1. Die UI direkt board.Dice.Roll() aufruft
            // 2. Die UI direkt board.TryMoveSelectedPiece() aufruft
            // 3. Die UI direkt board.EndTurn() aufruft
            
            return null;
        }

        public OOPGames.IGamePlayer Clone()
        {
            var c = new B1_MAN_HumanPlayer();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }
    }
}
