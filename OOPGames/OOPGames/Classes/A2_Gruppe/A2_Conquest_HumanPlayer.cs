using System.Windows.Input;

namespace OOPGames
{
    public class A2_ConquestHumanPlayer : IHumanGamePlayer
    {
        public string Name => "A2_Conquest_Human";
        public int PlayerNumber { get; private set; }

        public A2_ConquestHumanPlayer(int playerNumber = 1) => PlayerNumber = playerNumber;

        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public bool CanBeRuledBy(IGameRules rules) => rules is A2_ConquestRules;

        public IGamePlayer Clone() => new A2_ConquestHumanPlayer(PlayerNumber);

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null) return null;

            // SPACE = Pass
            if (selection is IKeySelection ks && ks.Key == Key.Space)
                return new A2_ConquestPassMove(PlayerNumber);

            if (selection is not IClickSelection cs) return null;
            if (field is not A2_ConquestGameField gf) return null;

            int x = cs.XClickPos;
            int y = cs.YClickPos;

            int col = (x - A2_ConquestPainter.BoardLeft) / A2_ConquestPainter.CellSize;
            int row = (y - A2_ConquestPainter.BoardTop) / A2_ConquestPainter.CellSize;

            // bounds
            if (col < 0 || row < 0 || col >= gf.Game.Field.Width || row >= gf.Game.Field.Height)
                return null;

            // Optional: nur gültige Captures zulassen (sonst null)
            var tile = gf.Game.Field.GetTile(col, row);
            var current = gf.Game.CurrentPlayer;
            if (!tile.CanBeCapturedBy(current, gf.Game.Field)) return null;
            if (tile.IsBeingContested) return null;

            return new A2_ConquestMove(PlayerNumber, row, col);
        }
    }
}
