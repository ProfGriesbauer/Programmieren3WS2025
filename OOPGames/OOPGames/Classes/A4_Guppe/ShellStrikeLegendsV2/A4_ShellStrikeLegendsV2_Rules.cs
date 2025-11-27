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
            // Terrain & Tank vorhanden?
            if (_field?.Terrain == null) return;
            if (_field.Tank1 == null) return;

            var t = _field.Tank1;

            // 1. Aktuelle X-Positionen der beiden Pivots
            double leftPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetPx;
            double rightPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetPx;

            // 2. Bodenhöhe unter den Pivots
            double groundLeftY = _field.Terrain.GroundYAt(leftPivotX);
            double groundRightY = _field.Terrain.GroundYAt(rightPivotX);

            // 3. Y-Top-Limits, bei denen die Ketten genau aufliegen würden
            double topLimitLeft = groundLeftY - A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetPx;
            double topLimitRight = groundRightY - A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetPx;

            // Strengstes Limit nehmen (damit keine Seite im Boden steckt)
            double topLimit = Math.Min(topLimitLeft, topLimitRight);

            // 4. Ist der Tank noch in der Luft? -> weiterfallen lassen
            double nextY = t.Y + A4_ShellStrikeLegendsV2_Config.TankFallSpeedPxPerTick;
            if (nextY < topLimit)
            {
                t.Y = nextY;
                return; // noch nicht gelandet, keine Drehung
            }

            // 5. Tank ist "aufgeschlagen": Y auf Boden klemmen und Terrain-Ausrichtung machen
            t.Y = topLimit;
            t.UpdateFromTerrain(_field.Terrain);
        }


        public void DoMove(IPlayMove move)
        {
            // ignore moves in terrain-only demo
        }

        public void ClearField()
        {
            // Reset tank to top so it can fall again
            if (_field?.Tank1 != null) _field.Tank1.Y = -100; // etwas über dem Bild
        }

        public int CheckIfPLayerWon()
        {
            return -1; // no win condition in terrain-only demo
        }
    }
}
