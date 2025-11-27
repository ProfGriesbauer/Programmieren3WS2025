using System;

namespace OOPGames
{
    // Minimal rules to host the V2 GameField so the painter can be selected and used.
    public class A4_ShellStrikeLegendsV2_Rules : IGameRules2
    {
        private readonly A4_ShellStrikeLegendsV2_GameField _field = new();

        public string Name => "A4 ShellStrikeLegends V2 Rules (Terrain Demo)";
        public IGameField CurrentField => _field;
        public bool MovesPossible => true; // no blocking

        public void StartedGameCall()
        {
            // nothing dynamic yet; terrain generated on-demand by painter
        }

        public void TickGameCall()
        {
            // Constant-velocity fall for Tank1; stop when wheel pivots contact ground
            if (_field?.Terrain?.Heights == null || _field.Terrain.Heights.Length == 0) return;
            if (_field.Tank1 == null) return;

            var t = _field.Tank1;
            t.Y += A4_ShellStrikeLegendsV2_Config.TankFallSpeedPxPerTick;

            // Compute absolute X of both pivots
            double leftPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetPx;
            double rightPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetPx;

            // Terrain Y under each pivot
            int groundLeftY = _field.Terrain.GroundYAt(leftPivotX);
            int groundRightY = _field.Terrain.GroundYAt(rightPivotX);

            // Required top Y so that each pivot sits on ground: top = groundY - pivotYOffset
            double topLimitLeft = groundLeftY - A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetPx;
            double topLimitRight = groundRightY - A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetPx;

            // To avoid penetrating terrain at either pivot, honor the stricter (smaller) top limit
            double topLimit = Math.Min(topLimitLeft, topLimitRight);
            if (t.Y > topLimit) t.Y = topLimit;
        }

        public void DoMove(IPlayMove move)
        {
            // ignore moves in terrain-only demo
        }

        public void ClearField()
        {
            // Reset tank to top so it can fall again
            if (_field?.Tank1 != null) _field.Tank1.Y = 0;
        }

        public int CheckIfPLayerWon()
        {
            return -1; // no win condition in terrain-only demo
        }
    }
}
