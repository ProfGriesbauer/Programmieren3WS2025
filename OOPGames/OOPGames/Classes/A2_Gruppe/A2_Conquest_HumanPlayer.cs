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

            // Pass per Space
            if (selection is IKeySelection ks && ks.Key == Key.Space)
                return new A2_ConquestPassMove(PlayerNumber);

            if (selection is not IClickSelection cs) return null;

            // Rechtsklick = Pass (optional)
            if (cs.ChangedButton == 1)
                return new A2_ConquestPassMove(PlayerNumber);

            // Spielfeld-Coordinates bestimmen
            int x = cs.XClickPos;
            int y = cs.YClickPos;

            int col = (x - A2_ConquestPainter.BoardLeft) / A2_ConquestPainter.CellSize;
            int row = (y - A2_ConquestPainter.BoardTop) / A2_ConquestPainter.CellSize;

            if (field is not A2_ConquestGameField gf) return null;

            if (col < 0 || row < 0 || col >= gf.Game.Field.Width || row >= gf.Game.Field.Height)
                return null;

            // **Keine** Capture-Logik mehr hier -> nur Move mit Koordinaten
            return new A2_ConquestMove(PlayerNumber, row, col);
        }

    }
}
