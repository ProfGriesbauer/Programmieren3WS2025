using System;

namespace OOPGames
{
    public class A4_TicTacToeHumanPlayer : X_BaseHumanTicTacToePlayer
    {
        int _PlayerNumber = 0;

        public override string Name { get { return "A4_HumanTicTacToePlayer"; } }

        public override int PlayerNumber { get { return _PlayerNumber; } }

        public override IGamePlayer Clone()
        {
            A4_TicTacToeHumanPlayer p = new A4_TicTacToeHumanPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public override IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            // selection contains click coordinates relative to the canvas
            // we try to read optional canvas size via reflection (if ClickSelection was extended);
            // otherwise we fall back to a sensible default (500x500) and compute cell indices.

            if (selection is IClickSelection sel)
            {
                double canvasW = 500.0;
                double canvasH = 500.0;

                // try reflection to get optional CanvasWidth/CanvasHeight properties
                try
                {
                    var sType = selection.GetType();
                    var pw = sType.GetProperty("CanvasWidth");
                    var ph = sType.GetProperty("CanvasHeight");
                    if (pw != null)
                    {
                        var v = pw.GetValue(selection);
                        if (v is int) canvasW = (int)v;
                        else if (v is double) canvasW = (double)v;
                    }
                    if (ph != null)
                    {
                        var v = ph.GetValue(selection);
                        if (v is int) canvasH = (int)v;
                        else if (v is double) canvasH = (double)v;
                    }
                }
                catch { }

                int N = A4_TicTacToeConfig.N;
                double cellW = canvasW / N;
                double cellH = canvasH / N;

                int col = (int)(sel.XClickPos / cellW);
                int row = (int)(sel.YClickPos / cellH);

                if (row < 0) row = 0;
                if (col < 0) col = 0;
                if (row >= N) row = N - 1;
                if (col >= N) col = N - 1;

                if (field[row, col] <= 0)
                {
                    return new A4_TicTacToeMove(row, col, _PlayerNumber);
                }
            }

            return null;
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }
    }
}
