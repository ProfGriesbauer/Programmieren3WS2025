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
            double topLimitLeft = groundLeftY - A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetScaled;
            double topLimitRight = groundRightY - A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetScaled;

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

            // ----------------------------------------------------------
            // 5) Projektil-Physik
            // ----------------------------------------------------------
            var proj = _field.Projectile;
            if (proj != null && proj.IsActive)
            {
                // Gravitation anwenden
                proj.VY += A4_ShellStrikeLegendsV2_Config.ProjectileGravityPx;

                // Position updaten
                proj.X += proj.VX;
                proj.Y += proj.VY;

                // Bounds / Terrain prüfen
                // Bildschirmgrenzen grob: [0, CanvasWidth)
                if (proj.X < 0 || proj.X >= _field.Terrain.CanvasWidth ||
                    proj.Y >= _field.Terrain.CanvasHeight)
                {
                    proj.IsActive = false;
                }
                else
                {
                    int groundY = _field.Terrain.GroundYAt(proj.X);
                    if (proj.Y >= groundY)
                    {
                        // Kollision mit Terrain -> Projektile deaktivieren
                        proj.IsActive = false;
                        // TODO: später Explosion / Terrain-Deformation hier
                    }
                }
            }

        }


        // Methode um die Tasteneingaben aus HumanPlayer zu verarbeiten
        public void DoMove(IPlayMove move)
        {
            if (move is not A4_ShellStrikeLegendsV2_Move m)
                return;

            var t = _field.Tank1;
            if (t == null || _field.Terrain == null)
                return;

            switch (m.Action)
            {
                case SSLV2Action.MoveLeft:
                    t.MoveLeft();
                    t.UpdateFromTerrain(_field.Terrain);
                    break;

                case SSLV2Action.MoveRight:
                    t.MoveRight();
                    t.UpdateFromTerrain(_field.Terrain);
                    break;

                case SSLV2Action.BarrelUp:
                    t.BarrelAngleRad -= 0.02;   // ↑
                    break;

                case SSLV2Action.BarrelDown:
                    t.BarrelAngleRad += 0.02;   // ↓
                    break;

                case SSLV2Action.Fire:
                    {
                        var p = _field.Projectile;
                        if (p == null) break;

                        // Nur ein Projektil gleichzeitig
                        if (p.IsActive) break;

                        double barrelLen = A4_ShellStrikeLegendsV2_Config.BarrelWidthPx *
                                           A4_ShellStrikeLegendsV2_Config.TankScale;

                        t.GetMuzzleWorldPosition(barrelLen, out double mx, out double my);

                        double angle = t.RotationRad + t.BarrelAngleRad;

                        p.X = mx;
                        p.Y = my;
                        p.VX = Math.Cos(angle) * A4_ShellStrikeLegendsV2_Config.ProjectileStartSpeedPx;
                        p.VY = Math.Sin(angle) * A4_ShellStrikeLegendsV2_Config.ProjectileStartSpeedPx;
                        p.IsActive = true;
                        break;
                    }

                case SSLV2Action.None:
                default:
                    // nichts tun
                    break;
            }
        }




        public void ClearField()
        {
            if (_field?.Tank1 != null)
            {
                _field.Tank1.X = A4_ShellStrikeLegendsV2_Config.TankSpawnX;
                _field.Tank1.Y = A4_ShellStrikeLegendsV2_Config.TankSpawnY;
                _field.Tank1.FallVelocity = 0;
            }
        }


        public int CheckIfPLayerWon()
        {
            return -1; // no win condition in terrain-only demo
        }
    }
}
