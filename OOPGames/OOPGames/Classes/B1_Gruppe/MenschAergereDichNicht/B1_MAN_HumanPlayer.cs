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
            if (!(field is B1_MAN_Board board)) return null;

            var rules = new B1_MAN_Rules(board);

            // Try to extract canvas dimensions from selection (A4_ClickSelection provides them)
            int canvasW = 400, canvasH = 400;
            try
            {
                var sType = selection.GetType();
                var pw = sType.GetProperty("CanvasWidth");
                var ph = sType.GetProperty("CanvasHeight");
                if (pw != null)
                {
                    var v = pw.GetValue(selection);
                    if (v is int) canvasW = (int)v;
                    else if (v is double) canvasW = (int)(double)v;
                }
                if (ph != null)
                {
                    var v = ph.GetValue(selection);
                    if (v is int) canvasH = (int)v;
                    else if (v is double) canvasH = (int)(double)v;
                }
            }
            catch { }

            // roll the dice now
            int dice = rules.RollDice();

            // map click to track index
            int clickX = 0, clickY = 0;
            if (selection is OOPGames.IClickSelection cs)
            {
                clickX = cs.XClickPos;
                clickY = cs.YClickPos;
            }

            // compute center and radius using same math as painter
            double cx = canvasW / 2.0;
            double cy = canvasH / 2.0;
            double r = Math.Min(canvasW, canvasH) * 0.38;

            double dx = clickX - cx;
            double dy = clickY - cy;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            int clickedIndex = -1;
            if (dist > r - 40 && dist < r + 40)
            {
                double ang = Math.Atan2(dy, dx);
                // painter used: ang = (i/TrackLength)*2pi - pi/2
                double t = (ang + Math.PI / 2.0) / (2.0 * Math.PI);
                while (t < 0) t += 1.0;
                while (t >= 1) t -= 1.0;
                clickedIndex = (int)(t * B1_MAN_Board.TrackLength);
                if (clickedIndex < 0) clickedIndex = 0;
                if (clickedIndex >= B1_MAN_Board.TrackLength) clickedIndex = B1_MAN_Board.TrackLength - 1;
            }

            var valid = rules.GetValidMoves(_PlayerNumber, dice);
            if (valid == null || valid.Count == 0) return null;

            // If the clicked index contains one of our movable pieces, pick that
            if (clickedIndex >= 0)
            {
                var occ = board.GetPieceAt(clickedIndex);
                if (occ != null && occ.Owner == _PlayerNumber && valid.Exists(p => p.Id == occ.Id))
                {
                    return new B1_MAN_Move(_PlayerNumber, occ.Id, dice);
                }
            }

            // fallback: pick first valid piece
            var pick = valid[0];
            return new B1_MAN_Move(_PlayerNumber, pick.Id, dice);
        }

        public OOPGames.IGamePlayer Clone()
        {
            var c = new B1_MAN_HumanPlayer();
            c.SetPlayerNumber(_PlayerNumber);
            return c;
        }
    }
}
