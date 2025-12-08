using System;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    /// <summary>
    /// Human player for Mensch-ärgere-dich-nicht.
    /// Note: Game logic is managed by UI (B1_MAN_Paint), not by this class.
    /// </summary>
    public class B1_MAN_HumanPlayer : OOPGames.IHumanGamePlayer
    {
        #region Fields
        private int _PlayerNumber = 0;
        #endregion

        #region Properties
        public string Name => "B1_MAN_HumanPlayer";

        public int PlayerNumber => _PlayerNumber;
        #endregion

        #region Public Methods
        public void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(OOPGames.IGameRules rules)
        {
            return rules is B1_MAN_Rules || (rules?.CurrentField is B1_MAN_Board);
        }

        /// <summary>
        /// Returns null because game logic is managed by UI, not by framework.
        /// UI directly controls: Dice.Roll(), TryMoveSelectedPiece(), and EndTurn().
        /// </summary>
        public OOPGames.IPlayMove GetMove(OOPGames.IMoveSelection selection, OOPGames.IGameField field)
        {
            return null;
        }

        public OOPGames.IGamePlayer Clone()
        {
            var c = new B1_MAN_HumanPlayer();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }
        #endregion
    }
}
