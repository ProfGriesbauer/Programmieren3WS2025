using System;

namespace OOPGames
{
    public class A2_ConquestRules : IGameRules
    {
        private Game _game;
        private A2_ConquestGameField _field;

        public string Name => "A2_Conquest_Rules";
        public IGameField CurrentField => _field;

        public bool MovesPossible => CheckIfPLayerWon() < 0;

        public A2_ConquestRules() => Reset();
        public void ClearField() => Reset();

        private static int RandomEven(int minEvenInclusive, int maxEvenInclusive)
        {
            int a = minEvenInclusive / 2;
            int b = maxEvenInclusive / 2;
            return Random.Shared.Next(a, b + 1) * 2;
        }

        private void Reset()
        {
            int w = RandomEven(6, 12);
            int h = RandomEven(6, 12);

            _game = new Game(w, h);
            _field = new A2_ConquestGameField(_game);
        }

        public void DoMove(IPlayMove move)
        {
            if (move == null) return;
            if (CheckIfPLayerWon() > 0) return;

            int pid = move.PlayerNumber - 1;
            if (pid != _game.CurrentPlayerId) return;

            if (move is A2_ConquestPassMove)
            {
                _game.EndTurn();
                return;
            }

            if (move is A2_ConquestTroopMove tm)
            {
                bool ok = _game.TryMoveTroop(pid, tm.TroopLocalIndex, tm.Column, tm.Row);
                if (!ok) return;

                _game.EndTurn();
            }
        }

        public int CheckIfPLayerWon()
        {
            if (_game.CheckWin(out int winnerId))
                return winnerId + 1; // Framework: 1/2

            return -1;
        }
    }
}
