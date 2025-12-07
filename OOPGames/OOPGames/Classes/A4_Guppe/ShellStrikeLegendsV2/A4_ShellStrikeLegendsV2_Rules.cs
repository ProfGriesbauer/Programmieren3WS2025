using System;

namespace OOPGames
{
    // Minimal rules to host the V2 GameField so the painter can be selected and used.
    public class A4_ShellStrikeLegendsV2_Rules : IGameRules2
    {
        private readonly A4_ShellStrikeLegendsV2_GameField _field = new();

        // -------------------- TURN-SYSTEM ------------------------
        private enum TurnPhase
        {
            Driving,
            Aiming
        }

        // Wer ist dran? 1 oder 2
        private int _activePlayer = 1;
        private TurnPhase _phase = TurnPhase.Driving;

        // Timer (Tick ~40ms => ca. 25 Ticks/Sek.)
        private const int TicksPerSecond = 25;
        private const int DrivingPhaseSeconds = 20;
        private const int AimingPhaseSeconds = 20;

        // Wie viele Ticks bleiben noch in der aktuellen Phase?
        private int _phaseTicksRemaining = DrivingPhaseSeconds * TicksPerSecond;

        private void ResetToDrivingPhase()
        {
            _phase = TurnPhase.Driving;
            _phaseTicksRemaining = DrivingPhaseSeconds * TicksPerSecond;
        }

        private void SwitchToAimingPhase()
        {
            _phase = TurnPhase.Aiming;
            _phaseTicksRemaining = AimingPhaseSeconds * TicksPerSecond;
        }

        private void SwitchActivePlayer()
        {
            _activePlayer = _activePlayer == 1 ? 2 : 1;
            ResetToDrivingPhase();
        }


        public string Name => "A4 ShellStrikeLegends V2 Rules (Terrain Demo)";
        public IGameField CurrentField => _field;
        public bool MovesPossible => true; // no blocking
        public void StartedGameCall()
        {
            // Beim Start immer bei Spieler 1 beginnen
            _activePlayer = 1;
            ResetToDrivingPhase();

            // Projektil sicher deaktivieren
            if (_field.Projectile != null)
            {
                _field.Projectile.IsActive = false;
            }
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
        // Hilfsmethode 2: Schusslogik (Position der Mündung, Projektil an diese Position setzen, Startgeschwindigkeit berechnen, Projektil aktivieren)
        // Einen Schuss von einem bestimmten Tank auslösen
        private void FireFromTank(A4_ShellStrikeLegendsV2_Tank t)
        {
            if (t == null) return;
            var p = _field.Projectile;
            if (p == null) return;
            if (p.IsActive) return; // es fliegt bereits ein Schuss

            double barrelLen = A4_ShellStrikeLegendsV2_Config.BarrelWidthPx *
                               A4_ShellStrikeLegendsV2_Config.TankScale;

            t.GetMuzzleWorldPosition(barrelLen, out double mx, out double my);

            double angle = t.RotationRad + t.BarrelAngleRad;

            p.X = mx;
            p.Y = my;
            p.VX = Math.Cos(angle) * A4_ShellStrikeLegendsV2_Config.ProjectileStartSpeedPx;
            p.VY = Math.Sin(angle) * A4_ShellStrikeLegendsV2_Config.ProjectileStartSpeedPx;
            p.IsActive = true;
        }

        //Hauptmethode die bei jedem Tick aufgerufen wird
        public void TickGameCall()
        {
            if (_field?.Terrain == null) return;

            // 1) Beide Tanks updaten (Fallphysik + Terrain-Ausrichtung)
            if (_field.Tank1 != null)
            {
                UpdateTankFallAndTerrain(_field.Tank1);
            }

            if (_field.Tank2 != null)
            {
                UpdateTankFallAndTerrain(_field.Tank2);
            }

            // 2) Phasen-Timer runterzählen
            if (_phaseTicksRemaining > 0)
            {
                _phaseTicksRemaining--;
            }

            // Phasenwechsel / Auto-Fire
            if (_phase == TurnPhase.Driving && _phaseTicksRemaining <= 0)
            {
                // Fahrzeit vorbei -> in Ziel-Phase wechseln
                SwitchToAimingPhase();
            }
            else if (_phase == TurnPhase.Aiming && _phaseTicksRemaining <= 0)
            {
                // Zielzeit vorbei -> automatisch feuern + Spieler wechseln
                var activeTank = _activePlayer == 1 ? _field.Tank1 : _field.Tank2;
                //FireFromTank(activeTank);
                SwitchActivePlayer();
            }

            // 3) Projektil-Physik
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
                        // TODO: Explosion / Schaden
                    }
                }
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (move is not A4_ShellStrikeLegendsV2_Move m)
                return;

            // Tank anhand unseres Turn-Systems auswählen, NICHT anhand des Moves
            A4_ShellStrikeLegendsV2_Tank t =
                _activePlayer == 1 ? _field.Tank1 : _field.Tank2;

            if (t == null || _field.Terrain == null)
                return;

            switch (_phase)
            {
                case TurnPhase.Driving:
                    // In der Fahr-Phase NUR links/rechts bewegen
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

                        default:
                            // BarrelUp/Down/Fire in dieser Phase ignorieren
                            break;
                    }
                    break;

                case TurnPhase.Aiming:
                    // In der Ziel-Phase NUR Rohr bewegen + Schuss
                    switch (m.Action)
                    {
                        case SSLV2Action.BarrelUp:
                            t.BarrelAngleRad -= A4_ShellStrikeLegendsV2_Config.BarrelAngleStepRad;
                            break;

                        case SSLV2Action.BarrelDown:
                            t.BarrelAngleRad += A4_ShellStrikeLegendsV2_Config.BarrelAngleStepRad;
                            break;

                        case SSLV2Action.Fire:
                            // manuelles Fire -> sofort schießen + Spielerwechsel
                            FireFromTank(t);
                            SwitchActivePlayer();
                            break;

                        default:
                            // MoveLeft/Right hier ignorieren
                            break;
                    }
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

            if (_field?.Tank2 != null)
            {
                _field.Tank2.X = A4_ShellStrikeLegendsV2_Config.TankSpawnX_Player2;
                _field.Tank2.Y = A4_ShellStrikeLegendsV2_Config.TankSpawnY;
                _field.Tank2.FallVelocity = 0;
            }

            if (_field?.Projectile != null)
            {
                _field.Projectile.IsActive = false;
            }

            _activePlayer = 1;
            ResetToDrivingPhase();
        }



        public int CheckIfPLayerWon()
        {
            return -1; // no win condition in terrain-only demo
        }
    }
}
