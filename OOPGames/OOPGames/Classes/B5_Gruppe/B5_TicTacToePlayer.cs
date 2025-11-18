using System;
using System.Windows.Input;

namespace OOPGames
{
    // Einfacher Human-Player für B5 TicTacToe.
    // Wenn ein Feld angeklickt wird, liefert GetMove ein B5_TicTacToeMove zurück.
    public class B5_TicTacToePlayer : IHumanGamePlayer, IHumanGamePlayerWithMouse
    {
        public string Name => "B5 TicTacToe Player";

        public int PlayerNumber { get; private set; }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B5_GameRules;
        }

        public IGamePlayer Clone()
        {
            B5_TicTacToePlayer clone = new B5_TicTacToePlayer();
            clone.SetPlayerNumber(PlayerNumber);
            return clone;
        }

        // Wandelt eine Click-Selection in eine Board-Position um und gibt den Zug zurück.
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(selection is IClickSelection click)) return null;

            // Verwende immer die aktuelle Canvas-Größe aus dem MainWindow PaintCanvas.
            double canvasW = 300.0, canvasH = 300.0;
            try
            {
                var app = System.Windows.Application.Current;
                var main = app?.MainWindow;
                var canvas = main?.FindName("PaintCanvas") as System.Windows.Controls.Canvas;
                if (canvas != null)
                {
                    canvasW = canvas.ActualWidth > 0 ? canvas.ActualWidth : (canvas.Width > 0 ? canvas.Width : canvasW);
                    canvasH = canvas.ActualHeight > 0 ? canvas.ActualHeight : (canvas.Height > 0 ? canvas.Height : canvasH);
                }
            }
            catch { }

            // Falls Canvas nicht verfügbar, benutze Fallback-Größen
            if (canvasW <= 0) canvasW = 300.0;
            if (canvasH <= 0) canvasH = 300.0;

            // Zellengröße berechnen (3x3)
            double cellW = canvasW / 3.0;
            double cellH = canvasH / 3.0;

            // Row/Col berechnen (Math.Floor sorgt für konsistente Zuordnung)
            int col = (int)Math.Floor(click.XClickPos / cellW);
            int row = (int)Math.Floor(click.YClickPos / cellH);

            // Wenn der Klick außerhalb des Canvas liegt, akzeptiere den Move nicht.
            if (click.XClickPos < 0 || click.XClickPos > canvasW || click.YClickPos < 0 || click.YClickPos > canvasH)
            {
                return null;
            }

            // Prüfe, ob die berechneten Indices gültig sind
            if (row < 0 || row > 2 || col < 0 || col > 2) return null;

            // Prüfe, ob das Feld frei ist und gebe den Move zurück
            if (field is IB5_TicTacToeField ticField && ticField.GetFieldValue(row, col) == 0)
            {
                return new B5_TicTacToeMove(PlayerNumber, row, col);
            }

            return null;
        }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        // Optionale Unterstützung für Mausbewegungen (hier: keine Vorschau, leer)
        public void OnMouseMoved(MouseEventArgs e)
        {
            // Intentionally left blank: no hover preview required for this simple player.
        }
    }
}
