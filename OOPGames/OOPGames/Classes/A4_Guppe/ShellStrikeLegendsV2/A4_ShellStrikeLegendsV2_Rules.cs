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

        // Hilfsmethode: Tank-Fallphysik und Terrain-Update abrufbar in dieser Methode
        private void UpdateTankFallAndTerrain(A4_ShellStrikeLegendsV2_Tank t)
        {
            if (t == null || _field?.Terrain == null) return;

            // 1) FALLPHYSIK
            t.FallVelocity += t.FallAccelerationPx;
            double nextY = t.Y + t.FallVelocity;

            // 2) Bodenhöhe unter beiden Pivot-Punkten bestimmen
            double leftPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotLeftXOffsetScaled;
            double rightPivotX = t.X + A4_ShellStrikeLegendsV2_Config.TankPivotRightXOffsetScaled;

            double groundLeftY = _field.Terrain.GroundYAt(leftPivotX);
            double groundRightY = _field.Terrain.GroundYAt(rightPivotX);

            // Oberkante des Tanks, bei der die Pivots genau auf dem Boden liegen würden
            double topLimitLeft =
                groundLeftY - A4_ShellStrikeLegendsV2_Config.TankPivotLeftYOffsetScaled;
            double topLimitRight =
                groundRightY - A4_ShellStrikeLegendsV2_Config.TankPivotRightYOffsetScaled;

            double topLimit = Math.Min(topLimitLeft, topLimitRight);

            // 3) TANK IST NOCH IN DER LUFT → weiterfallen lassen
            if (nextY < topLimit)
            {
                t.Y = nextY;
                return;   // keine Drehung während des Falls!
            }

            // 4) TANK LANDET: Auf Boden setzen + Rotation übernehmen
            t.Y = topLimit;
            t.FallVelocity = 0;
            t.UpdateFromTerrain(_field.Terrain);
        }
        //Hauptmethode die bei jedem Tick aufgerufen wird
        public void TickGameCall()
        {
            if (_field?.Terrain == null) return;

            // ----------------------------------------------------------
            // 1) Beide Tanks updaten (Fallphysik + Terrain-Ausrichtung)
            // ----------------------------------------------------------
            if (_field.Tank1 != null)
            {
                UpdateTankFallAndTerrain(_field.Tank1);
            }

            if (_field.Tank2 != null)
            {
                UpdateTankFallAndTerrain(_field.Tank2);
            }

            // ----------------------------------------------------------
            // 2) Projektil-Physik
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
                        proj.IsActive = false;
                        // TODO: später Explosion / Terrain-Deformation hier
                    }
                }
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not A4_ShellStrikeLegendsV2_Move m)
                return;

            // Welcher Tank? Player 1 -> Tank1, Player 2 -> Tank2
            A4_ShellStrikeLegendsV2_Tank t = null;

            if (m.PlayerNumber == 1)
                t = _field.Tank1;
            else if (m.PlayerNumber == 2)
                t = _field.Tank2;

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
                    t.BarrelAngleRad -= 0.02;
                    break;

                case SSLV2Action.BarrelDown:
                    t.BarrelAngleRad += 0.02;
                    break;

                case SSLV2Action.Fire:
                    {
                        var p = _field.Projectile;
                        if (p == null) break;
                        if (p.IsActive) break;  // nur ein Schuss gleichzeitig

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
                    break;
            }
        }





        public void ClearField()
        {
            if (_field?.Tank1 != null)
            {
                _field.Tank1.X = A4_ShellStrikeLegendsV2_Config.TankSpawnX_Player1;
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
