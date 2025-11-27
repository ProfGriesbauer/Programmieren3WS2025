using System.Windows.Input;

namespace OOPGames
{
    public class A2_ConquestHumanPlayer : IHumanGamePlayer
    {
        public string Name => "A2_Conquest_Human";
        public int PlayerNumber { get; private set; } = 1;

        public A2_ConquestHumanPlayer(int playerNumber = 1) => PlayerNumber = playerNumber;

        public void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;
        public bool CanBeRuledBy(IGameRules rules) => rules is A2_ConquestRules;
        public IGamePlayer Clone() => new A2_ConquestHumanPlayer(PlayerNumber);

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null) return null;
            if (field is not A2_ConquestGameField gf) return null;

            var game = gf.Game;
            int myId = PlayerNumber - 1;
            if (game.CurrentPlayerId != myId) return null;

            // Space = Pass (damit man Ressourcen sammeln kann)
            if (selection is IKeySelection ks && ks.Key == Key.Space)
                return new A2_ConquestPassMove(PlayerNumber);

            if (selection is not IClickSelection cs) return null;

            // Rechtsklick = Pass
            if (cs.ChangedButton == 1)
                return new A2_ConquestPassMove(PlayerNumber);

            int x = (cs.XClickPos - A2_ConquestPainter.BoardLeft) / A2_ConquestPainter.CellSize;
            int y = (cs.YClickPos - A2_ConquestPainter.BoardTop) / A2_ConquestPainter.CellSize;

            if (x < 0 || y < 0 || x >= game.Field.Width || y >= game.Field.Height)
                return null;

            // 1) Klick auf eigene Troop => auswählen/abwählen (kein Move -> null zurückgeben!)
            if (game.TryGetTroopAt(x, y, out var t) && t.OwnerId == myId)
            {
                int cur = game.SelectedTroopLocalIndex[myId];
                game.SelectedTroopLocalIndex[myId] = (cur == t.LocalIndex) ? -1 : t.LocalIndex;
                return null;
            }

            // 2) Klick auf Feld => wenn Troop ausgewählt, versuche King-Move
            int sel = game.SelectedTroopLocalIndex[myId];
            if (sel < 0) return null;

            if (!game.IsLegalTroopMove(myId, sel, x, y))
                return null;

            return new A2_ConquestTroopMove(PlayerNumber, sel, y, x);
        }
    }
}
