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

            // ----------------------------------------------------------
            // 1) FALLPHYSIK: Geschwindigkeit erhöht sich durch Gravitation
            // ----------------------------------------------------------
            t.FallVelocity += t.FallAccelerationPx;       // z.B. 0.5 px pro Tick^2
            double nextY = t.Y + t.FallVelocity;

            // ----------------------------------------------------------
            // 2) Bodenhöhe unter beiden Pivot-Punkten bestimmen
            // ----------------------------------------------------------
            double leftPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetPx;
            double rightPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetPx;

            double groundLeftY = _field.Terrain.GroundYAt(leftPivotX);
            double groundRightY = _field.Terrain.GroundYAt(rightPivotX);

            // Oberkante des Tanks, bei der die Pivots genau auf dem Boden liegen würden
            double topLimitLeft = groundLeftY - A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetPx;
            double topLimitRight = groundRightY - A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetPx;

            // strengstes Limit wählen, damit keine Seite in den Boden clippt
            double topLimit = Math.Min(topLimitLeft, topLimitRight);

            // ----------------------------------------------------------
            // 3) TANK IST NOCH IN DER LUFT → weiterfallen lassen
            // ----------------------------------------------------------
            if (nextY < topLimit)
            {
                t.Y = nextY;
                return;   // keine Drehung während des Falls!
            }

            // ----------------------------------------------------------
            // 4) TANK LANDET: Auf Boden setzen + Rotation übernehmen
            // ----------------------------------------------------------
            t.Y = topLimit;
            t.FallVelocity = 0;                    // wichtig: Geschwindigkeit zurücksetzen
            t.UpdateFromTerrain(_field.Terrain);   // Pivots und Rotation berechnen
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
