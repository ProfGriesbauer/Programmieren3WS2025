using System;

namespace OOPGames
{
    // Very small, readable rules for the TestGame.
    // Purpose: keep two simple players and switch whose turn it is every 3 seconds.
    // This class intentionally stays simple so it's easy to read and modify.
    public class A4_Testgame_Rules : IGameRules2
    {
        // The game field shared with the painter
        private A4_Testgame_Field _field = new A4_Testgame_Field();

        // Public name shown in UI lists
        public string Name { get { return "A4_Testgame_Rules_Simple"; } }

        // The painter reads this field to draw the scene
        public IGameField CurrentField { get { return _field; } }

        // Keep game running (otherwise MainWindow might auto-restart)
        public bool MovesPossible { get { return true; } }

        // No moves in this simple demo
        public void DoMove(IPlayMove move) { /* intentionally empty */ }

        // Simple turn-timer state
        private DateTime _lastSwitch = DateTime.MinValue;
        private readonly TimeSpan _turnLength = TimeSpan.FromSeconds(3);

        // Initialize or reset the field: two players at 20% and 80% of the width
        public void ClearField()
        {
            _field.Players.Clear();
            _field.Players.Add(new A4_Testgame_Field.PlayerState { PlayerNumber = 1, XFrac = 0.2 });
            _field.Players.Add(new A4_Testgame_Field.PlayerState { PlayerNumber = 2, XFrac = 0.8 });
            _field.ActivePlayerNumber = 1; // start with player 1
            _lastSwitch = DateTime.Now;
        }

        // Called when MainWindow starts a game
        public void StartedGameCall()
        {
            ClearField();
        }

        // Called regularly by MainWindow's timer.
        // We simply check the clock and flip the active player every _turnLength.
        public void TickGameCall()
        {
            if (_lastSwitch == DateTime.MinValue) _lastSwitch = DateTime.Now;
            var now = DateTime.Now;
            if ((now - _lastSwitch) >= _turnLength)
            {
                // Toggle between player 1 and 2 if both exist
                if (_field.Players != null && _field.Players.Count >= 2)
                {
                    _field.ActivePlayerNumber = (_field.ActivePlayerNumber == 1) ? 2 : 1;
                }
                _lastSwitch = now;
            }
        }

        // No win logic here for the demo
        public int CheckIfPLayerWon() { return -1; }
    }

    // Very small, readable field class used by painter and rules.
    // It contains a list of players and a simple ActivePlayerNumber.
    public class A4_Testgame_Field : IGameField
    {
        // Simple player state: number and normalized X position (0..1)
        public class PlayerState
        {
            public int PlayerNumber { get; set; }
            public double XFrac { get; set; }
        }

        // Player list (always small for the demo)
        public System.Collections.Generic.List<PlayerState> Players { get; } = new System.Collections.Generic.List<PlayerState>();

        // Which player is active (1-based). Painter reads this to highlight the active player.
        public int ActivePlayerNumber { get; set; } = 1;

        // Accept any painter for this demo
        public bool CanBePaintedBy(IPaintGame painter) { return true; }
    }
}
