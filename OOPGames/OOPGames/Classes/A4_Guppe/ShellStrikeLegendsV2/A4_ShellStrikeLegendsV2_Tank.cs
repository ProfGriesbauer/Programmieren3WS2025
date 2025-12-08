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

        //Sprite Fürs Barrel Initialisieren
        public string BarrelSpritePath { get; set; } = "Assets/ShellStrikeLegends/camo-tank-barrel.png";
        // Winkel des Kanonenrohrs in Radiant (relativ zum Tank!)
        //Berechnung der Barrel Rotation relativ zum Tank nicht mehr um oberkante!!!
        public double BarrelPivotX
        {
            get
            {
                // 1. Das Rotationszentrum des Tanks (Mitte des Bildes!)
                double cx = X;
                double cy = Y + (A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx *
                                 A4_ShellStrikeLegendsV2_Config.TankScale) / 2.0;

                // 2. Barrel-Offets relativ zur Tank-Mitte
                double lx = A4_ShellStrikeLegendsV2_Config.BarrelPivotOffsetXScaled;
                double ly = A4_ShellStrikeLegendsV2_Config.BarrelPivotOffsetYScaled
                            - (A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx *
                               A4_ShellStrikeLegendsV2_Config.TankScale) / 2.0;
                // Oberkante → zur Mitte verschoben

                double cos = Math.Cos(RotationRad);
                double sin = Math.Sin(RotationRad);

                // 3. Rotation um Tank-Mitte
                return cx + cos * lx - sin * ly;
            }
        }

        public double BarrelPivotY
        {
            get
            {
                double cx = X;
                double cy = Y + (A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx *
                                 A4_ShellStrikeLegendsV2_Config.TankScale) / 2.0;

                double lx = A4_ShellStrikeLegendsV2_Config.BarrelPivotOffsetXScaled;
                double ly = A4_ShellStrikeLegendsV2_Config.BarrelPivotOffsetYScaled
                            - (A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx *
                               A4_ShellStrikeLegendsV2_Config.TankScale) / 2.0;

                double cos = Math.Cos(RotationRad);
                double sin = Math.Sin(RotationRad);

                return cy + sin * lx + cos * ly;
            }
        }
        public void GetMuzzleWorldPosition(double barrelLength, out double mx, out double my) ///Berechnung der Mündung Position mit Hilfsmethode
        {
            double angle = RotationRad + BarrelAngleRad;

            // Startpunkt = Barrel-Pivot
            double px = BarrelPivotX;
            double py = BarrelPivotY;

            mx = px + Math.Cos(angle) * barrelLength;
            my = py + Math.Sin(angle) * barrelLength;
        }


        public double BarrelAngleRad { get; set; } = 0;   // 0 = waagrecht rechts

        // Computed rotation (radians) aligning track pivots to terrain segment
        public double RotationRad { get; private set; }

        // World-space pivots for left/right track wheels
        public double LeftPivotX { get; private set; }
        public double LeftPivotY { get; private set; }
        public double RightPivotX { get; private set; }
        public double RightPivotY { get; private set; }

        // Konstanten für Fallgeschwindigkeit und Beschleunigung
        public double FallVelocity = 0.0;
        public double FallAccelerationPx = 0.4;  // kannst du tunen (entspricht g)
        
        // Lebenspunkte des Tanks
        public int Health { get; set; } = A4_ShellStrikeLegendsV2_Config.TankMaxHealth;
        public bool IsDestroyed => Health <= 0;

        public void UpdateFromTerrain(A4_ShellStrikeLegendsV2_Terrain terrain)
        {
            // 1. Terrain vorhanden?
            if (terrain == null)
                return;

            // 2. X-Positionen der beiden Pivots relativ zur Tank-Mitte
            double xL = X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetScaled;
            double xR = X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetScaled;

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
                (A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetScaled +
                A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetScaled) * 0.5;

            X = midX;
            Y = midPivotY - avgPivotYOffset;   // Tank-Top-Y = mittlere Pivot-Höhe minus Offsets
        }

        //Variablen für die Tasteneingaben
        public void MoveLeft()
        {
            X -= A4_ShellStrikeLegendsV2_Config.TankDriveSpeedPx;
        }

        public void MoveRight()
        {
            X += A4_ShellStrikeLegendsV2_Config.TankDriveSpeedPx;
        }

    }
}