using System;

namespace OOPGames
{
    public class A2_ConquestRules : IGameRules
    {
        private static int RandomEven(int minEvenInclusive, int maxEvenInclusive)
        {
            int a = minEvenInclusive / 2; // 12 -> 6
            int b = maxEvenInclusive / 2; // 48 -> 24
            return (Random.Shared.Next(a, b + 1)) * 2; // inclusive!
        }
        

        private Game _game;
        private A2_ConquestGameField _field;

        public string Name => "A2_Conquest_Rules";
        public IGameField CurrentField => _field;

        // Wichtig: Wenn ein Computer-Spieler dran ist, darf MovesPossible nicht true sein,
        // wenn GetMove() null liefern würde. Deshalb: Pass ist immer möglich -> true bis Winner.
        public bool MovesPossible => CheckIfPLayerWon() < 0;

        public A2_ConquestRules()
        {
            Reset();
        }

        public void ClearField()
        {
            Reset();
        }

        private void Reset()
        {
            int w = RandomEven(6, 12);
            int h = RandomEven(6, 12);

            _game = new Game(w, h);
            _field = new A2_ConquestGameField(_game);

            _game.StartTurn();
        }


        public void DoMove(IPlayMove move)
        {
            if (move == null) return;
            if (CheckIfPLayerWon() > 0) return;

            // PASS: beendet den Zug -> Captures ticken weiter
            if (move is A2_ConquestPassMove)
            {
                _game.EndTurn();
                return;
            }

            // Capture-Start über Tile-Koordinate
            if (move is IRowMove rm && move is IColumnMove cm)
            {
                bool ok = _game.TryStartCapture(cm.Column, rm.Row);
                if (ok)
                {
                    _game.EndTurn();
                }
                // bei ungültigem Click: einfach nichts tun (kein Turn-End)
            }
        }

        // Framework erwartet 1/2 oder -1 (siehe IGameRules) :contentReference[oaicite:1]{index=1}
        public int CheckIfPLayerWon()
        {
            if (_game.CheckWin(out int winnerId))
                return winnerId + 1;

            return -1;
        }
    }
}
