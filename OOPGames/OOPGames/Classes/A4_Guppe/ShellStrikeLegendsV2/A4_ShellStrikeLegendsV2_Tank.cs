using System;

namespace OOPGames
{
    // Minimal V2 tank body only (no barrel/projectiles)
    public class A4_ShellStrikeLegendsV2_Tank
    {
        // World position (x in canvas pixels); y is taken from terrain at X
        public double X { get; set; }
        public double Y { get; set; }

        // Asset path for the hull sprite
        public string HullSpritePath { get; set; } = "Assets/ShellStrikeLegends/camo-tank-4 smaller.png";

        // Computed rotation (radians) aligning track pivots to terrain segment
        public double RotationRad { get; private set; }

        // World-space pivots for left/right track wheels
        public double LeftPivotX { get; private set; }
        public double LeftPivotY { get; private set; }
        public double RightPivotX { get; private set; }
        public double RightPivotY { get; private set; }

        // Align tank orientation and center to current terrain under both pivots
        // Align tank orientation and center to current terrain under both pivots
        public void UpdateFromTerrain(A4_ShellStrikeLegendsV2_Terrain terrain)
        {
            // 1. Terrain vorhanden?
            if (terrain == null)
                return;

            // 2. X-Positionen der beiden Pivots relativ zur Tank-Mitte
            double xL = X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetPx;
            double xR = X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetPx;

            // 3. Bodenhöhe unter jedem Pivot
            double yL = terrain.GroundYAt(xL);
            double yR = terrain.GroundYAt(xR);

            // 4. Pivots (Weltkoordinaten) speichern
            LeftPivotX = xL;
            LeftPivotY = yL;
            RightPivotX = xR;
            RightPivotY = yR;

            // 5. Rotationswinkel aus der Verbindungslinie der beiden Pivots
            double dx = xR - xL;
            double dy = yR - yL;
            RotationRad = Math.Atan2(dy, dx);


            // 6. Tank-Mittelpunkt als Mittelpunkt der beiden Pivots
            double midX = (xL + xR) * 0.5;
            double midPivotY = (yL + yR) * 0.5;

            // 7. Tank so hoch schieben, dass die Ketten aufliegen
            double avgPivotYOffset =
                (A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetPx +
                 A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetPx) * 0.5;

            X = midX;
            Y = midPivotY - avgPivotYOffset;   // Tank-Top-Y = mittlere Pivot-Höhe minus Offsets
        }
    }
}