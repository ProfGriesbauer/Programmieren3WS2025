using System;
using System.Linq;
using System.Collections.Generic;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    public class B1_MAN_ComputerPlayer : OOPGames.IComputerGamePlayer
    {
        int _PlayerNumber = 0;
        Random _rnd = new Random();

        public string Name => "B1_MAN_ComputerPlayer";

        public int PlayerNumber => _PlayerNumber;

        public void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(OOPGames.IGameRules rules)
        {
            return rules is B1_MAN_Rules || (rules?.CurrentField is B1_MAN_Board);
        }

        public OOPGames.IPlayMove GetMove(OOPGames.IGameField field)
        {
            if (!(field is B1_MAN_Board board)) return null;

            // Wenn der Würfel bereits gerollt wurde (über UI), verwende UI-Steuerung
            // In diesem Fall gibt die UI die Kontrolle und wir sollten null zurückgeben
            // um nicht mit der UI zu kollidieren
            if (board.Dice.HasBeenRolled)
            {
                return null; // UI managed den Zug
            }

            // Ansonsten: Framework-Steuerung (normaler Computer-Player)
            var rules = new B1_MAN_Rules(board);
            int dice = rules.RollDice();
            var valid = rules.GetValidMoves(_PlayerNumber, dice);
            if (valid == null || valid.Count == 0) return null;

            // Simple Strategie: Zufällige Auswahl
            var pick = valid[_rnd.Next(valid.Count)];
            return new B1_MAN_Move(_PlayerNumber, pick.Id, dice);
        }

        public OOPGames.IGamePlayer Clone()
        {
            var c = new B1_MAN_ComputerPlayer();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }
    }
}
