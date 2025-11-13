using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Rules for Tron-like game. Tick-driven: moves are applied during TickGameCall.
    public class B3_Mika_Roeder_Tron_Rules : IGameRules2
    {
        private B3_Mika_Roeder_Tron_Field _field;

        // positions and directions per player
        private Dictionary<int, (int r, int c)> _pos = new Dictionary<int, (int r, int c)>();
        private Dictionary<int, B3TronDirection> _dir = new Dictionary<int, B3TronDirection>();
        private Dictionary<int, B3TronDirection> _nextDir = new Dictionary<int, B3TronDirection>();
        private Dictionary<int, bool> _alive = new Dictionary<int, bool>();

        // width/height: size of grid. countdownSeconds: seconds before movement starts.
        // initialMoveIntervalTicks: number of paint ticks (40ms each) between moves at start
        public B3_Mika_Roeder_Tron_Rules(int width = 60, int height = 40, int countdownSeconds = 3, int initialMoveIntervalTicks = 8)
        {
            _field = new B3_Mika_Roeder_Tron_Field(width, height);
            _countdownSeconds = countdownSeconds;
            _initialMoveIntervalTicks = Math.Max(1, initialMoveIntervalTicks);
            ClearField();
        }

        // --- speed & countdown configuration ---
        private int _countdownSeconds = 3;
        private int _initialMoveIntervalTicks = 8; // movement every N ticks initially
    private int _minMoveIntervalTicks = 1; // fastest
    // accelerate faster by default: every 2 seconds
    private int _accelerationEveryTicks = 25 * 2; // every 2 seconds (25 ticks/sec)

    // when a crash happens, we enter a short animation period where
    // movement is stopped and the crash cell flashes. _gameOver controls that state.
    private bool _gameOver = false;

        public string Name { get { return "B3 Mika Röder Tron Rules"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                // game runs while at least one player alive
                foreach (var a in _alive.Values) if (a) return true;
                return false;
            }
        }

        public void ClearField()
        {
            _field.Clear();
            _pos.Clear(); _dir.Clear(); _nextDir.Clear(); _alive.Clear();
            _ticksSinceLastMove = 0;
            _ticksSinceStart = 0;
            _ticksSinceAcceleration = 0;
            _currentMoveIntervalTicks = _initialMoveIntervalTicks;

            _gameOver = false;

            // Initialize two players in opposite corners (roughly)
            int h = _field.Height; int w = _field.Width;
            _pos[1] = (h / 2, Math.Max(2, w / 4));
            _pos[2] = (h / 2, Math.Min(w - 3, 3 * w / 4));

            _dir[1] = B3TronDirection.Right; _nextDir[1] = B3TronDirection.Right; _alive[1] = true;
            _dir[2] = B3TronDirection.Left; _nextDir[2] = B3TronDirection.Left; _alive[2] = true;

            // mark initial positions as trails
            _field[_pos[1].r, _pos[1].c] = 1;
            _field[_pos[2].r, _pos[2].c] = 2;

            // set countdown on field
            _field.CountdownRemainingSeconds = _countdownSeconds;
        }

        // ticking bookkeeping
        private int _ticksSinceLastMove = 0;
        private int _ticksSinceStart = 0;
        private int _ticksSinceAcceleration = 0;
        private int _currentMoveIntervalTicks = 8;

        public void DoMove(IPlayMove move)
        {
            if (move is B3_Mika_Roeder_Tron_Move tm)
            {
                if (!_alive.ContainsKey(tm.PlayerNumber) || !_alive[tm.PlayerNumber]) return;
                // prevent 180° turns
                var cur = _dir[tm.PlayerNumber];
                if (IsReverse(cur, tm.Direction)) return;
                _nextDir[tm.PlayerNumber] = tm.Direction;
            }
        }

        private bool IsReverse(B3TronDirection a, B3TronDirection b)
        {
            return (a == B3TronDirection.Up && b == B3TronDirection.Down) ||
                   (a == B3TronDirection.Down && b == B3TronDirection.Up) ||
                   (a == B3TronDirection.Left && b == B3TronDirection.Right) ||
                   (a == B3TronDirection.Right && b == B3TronDirection.Left);
        }

        public int CheckIfPLayerWon()
        {
            int aliveCount = 0; int lastAlive = 0;
            foreach (var kv in _alive)
            {
                if (kv.Value) { aliveCount++; lastAlive = kv.Key; }
            }
            if (aliveCount == 1) return lastAlive;
            if (aliveCount == 0) return -1; // nobody won
            return 0; // game still running
        }

        // TickGameCall is called every 40ms from MainWindow — move players here
        public void TickGameCall()
        {
            // increment global ticks
            _ticksSinceStart++;

            // update countdown if running
            int ticksPerSecond = 25; // ~ (1000ms / 40ms)
            if (_ticksSinceStart <= _countdownSeconds * ticksPerSecond)
            {
                int remainingTicks = _countdownSeconds * ticksPerSecond - _ticksSinceStart;
                int secs = (int)Math.Ceiling(remainingTicks / (double)ticksPerSecond);
                _field.CountdownRemainingSeconds = Math.Max(0, secs);
                return; // don't move players yet
            }
            else
            {
                _field.CountdownRemainingSeconds = 0;
            }

            // if a crash animation is active, drive the crash animation and prevent movement
            if (_gameOver && _field.CrashActive)
            {
                if (_field.CrashRemainingTicks > 0)
                {
                    _field.CrashRemainingTicks--;
                    // create a flashing effect: toggle every 4 ticks
                    _field.CrashFlashOn = ((_field.CrashRemainingTicks / 4) % 2) == 0;
                    return;
                }
                else
                {
                    // animation finished -> end game
                    _field.CrashActive = false;
                    foreach (var k in new List<int>(_alive.Keys)) _alive[k] = false;
                    return;
                }
            }

            // acceleration handling
            _ticksSinceAcceleration++;
            if (_ticksSinceAcceleration >= _accelerationEveryTicks)
            {
                _ticksSinceAcceleration = 0;
                if (_currentMoveIntervalTicks > _minMoveIntervalTicks)
                    _currentMoveIntervalTicks = Math.Max(_minMoveIntervalTicks, _currentMoveIntervalTicks - 1);
            }

            // Only move when enough ticks have passed
            _ticksSinceLastMove++;
            if (_ticksSinceLastMove < _currentMoveIntervalTicks) return;
            _ticksSinceLastMove = 0;

            // prepare move order: 1 then 2
            var players = new List<int>(_pos.Keys);

            foreach (var p in players)
            {
                if (!_alive[p]) continue;
                // apply next dir
                _dir[p] = _nextDir[p];
                var pos = _pos[p];
                var newPos = MoveOne(pos, _dir[p]);

                // check collisions with walls
                if (newPos.r < 0 || newPos.r >= _field.Height || newPos.c < 0 || newPos.c >= _field.Width)
                {
                    // start crash animation at the position beyond the wall (clamped)
                    int cr = Math.Max(0, Math.Min(_field.Height - 1, newPos.r));
                    int cc = Math.Max(0, Math.Min(_field.Width - 1, newPos.c));
                    _alive[p] = false;
                    StartCrashAnimation(p, cr, cc);
                    // stop other players from moving (but keep them alive for display)
                    _gameOver = true;
                    continue;
                }

                var cell = _field[newPos.r, newPos.c];
                // if cell occupied => collision
                if (cell != 0)
                {
                    _alive[p] = false;
                    StartCrashAnimation(p, newPos.r, newPos.c);
                    _gameOver = true;
                    continue;
                }

                // mark new cell with player number (trail)
                _field[newPos.r, newPos.c] = p;
                _pos[p] = newPos;
            }
        }

        // Called when a game is started. We reset the field to initial state here.
        public void StartedGameCall()
        {
            ClearField();
        }

        private void StartCrashAnimation(int player, int r, int c)
        {
            // mark crash information on the field and request animation period (~2s)
            if (_field == null) return;
            _field.CrashActive = true;
            _field.CrashPlayerNumber = player;
            _field.CrashRow = r; _field.CrashCol = c;
            _field.CrashRemainingTicks = 25 * 2; // ~2 seconds
            _field.CrashFlashOn = true;
            _gameOver = true;
        }

        private (int r, int c) MoveOne((int r, int c) pos, B3TronDirection dir)
        {
            switch (dir)
            {
                case B3TronDirection.Up: return (pos.r - 1, pos.c);
                case B3TronDirection.Down: return (pos.r + 1, pos.c);
                case B3TronDirection.Left: return (pos.r, pos.c - 1);
                default: return (pos.r, pos.c + 1);
            }
        }
    }
}
