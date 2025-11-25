using System;
using System.Collections.Generic;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet die Bewegungslogik der Schlange inkl. Grid-Alignment und Richtungswechsel
    /// </summary>
    public class SnakeMovementController
    {
        private readonly SnakeGameConfig _config;
        private PixelPosition _pendingDirection;
        private PixelPosition _targetPosition;
        private PixelPosition _queuedDirection; // Zweite Eingabe wird gepuffert
        private int _lastTurnGridX = -1;
        private int _lastTurnGridY = -1;

        public SnakeMovementController(SnakeGameConfig config)
        {
            _config = config;
        }

        public void QueueDirectionChange(PixelPosition newDirection, PixelPosition currentDirection, PixelPosition headPosition)
        {
            if (newDirection.IsOpposite(currentDirection))
                return;

            // Blockiere Richtungsänderung wenn bereits eine Drehung in diesem Kästchen stattgefunden hat
            int currentGridX = (int)(headPosition.X / _config.CellSize);
            int currentGridY = (int)(headPosition.Y / _config.CellSize);

            if (_lastTurnGridX == currentGridX && _lastTurnGridY == currentGridY)
            {
                // Puffere die zweite Eingabe für das nächste Kästchen
                if (_queuedDirection == null && !newDirection.IsOpposite(currentDirection))
                {
                    _queuedDirection = newDirection;
                }
                return;
            }

            _pendingDirection = newDirection;
            _targetPosition = CalculateTurnTarget(headPosition, currentDirection);
        }

        public bool ShouldExecuteTurn(PixelPosition headPosition, PixelPosition currentDirection, out PixelPosition newDirection, out PixelPosition targetPos)
        {
            newDirection = null;
            targetPos = null;

            if (_targetPosition == null || _pendingDirection == null)
            {
                // Prüfe ob gepufferte Richtung aktiviert werden kann
                if (_queuedDirection != null && !_queuedDirection.IsOpposite(currentDirection))
                {
                    int currentGridX = (int)(headPosition.X / _config.CellSize);
                    int currentGridY = (int)(headPosition.Y / _config.CellSize);
                    
                    // Aktiviere gepufferte Richtung wenn wir in ein neues Kästchen gewechselt haben
                    if (_lastTurnGridX != currentGridX || _lastTurnGridY != currentGridY)
                    {
                        _pendingDirection = _queuedDirection;
                        _targetPosition = CalculateTurnTarget(headPosition, currentDirection);
                        _queuedDirection = null;
                    }
                }
                
                if (_targetPosition == null || _pendingDirection == null)
                    return false;
            }

            bool reachedTarget = HasReachedTarget(headPosition, currentDirection, _targetPosition);

            if (reachedTarget)
            {
                newDirection = _pendingDirection;
                targetPos = _targetPosition;
                
                // Merke Position der Drehung
                _lastTurnGridX = (int)(targetPos.X / _config.CellSize);
                _lastTurnGridY = (int)(targetPos.Y / _config.CellSize);
                
                _pendingDirection = null;
                _targetPosition = null;
                return true;
            }

            return false;
        }

        private bool HasReachedTarget(PixelPosition current, PixelPosition direction, PixelPosition target)
        {
            if (direction.X > 0) return current.X >= target.X;
            if (direction.X < 0) return current.X <= target.X;
            if (direction.Y > 0) return current.Y >= target.Y;
            if (direction.Y < 0) return current.Y <= target.Y;
            return false;
        }

        private PixelPosition CalculateTurnTarget(PixelPosition head, PixelPosition direction)
        {
            int gridX = (int)(head.X / _config.CellSize);
            int gridY = (int)(head.Y / _config.CellSize);

            double centerX = gridX * _config.CellSize + _config.CellSize / 2.0 - 16;
            double centerY = gridY * _config.CellSize + _config.CellSize / 2.0 - 16;

            double threeQuarterX = gridX * _config.CellSize + (_config.CellSize * 3.0 / 4.0) - 16;
            double threeQuarterY = gridY * _config.CellSize + (_config.CellSize * 3.0 / 4.0) - 16;

            bool beforeThreeQuarterX = (direction.X > 0 && head.X < threeQuarterX) || (direction.X < 0 && head.X > threeQuarterX);
            bool beforeThreeQuarterY = (direction.Y > 0 && head.Y < threeQuarterY) || (direction.Y < 0 && head.Y > threeQuarterY);
            bool atThreeQuarterX = direction.X == 0 || Math.Abs(head.X - threeQuarterX) < 1;
            bool atThreeQuarterY = direction.Y == 0 || Math.Abs(head.Y - threeQuarterY) < 1;

            if ((beforeThreeQuarterX || atThreeQuarterX) && (beforeThreeQuarterY || atThreeQuarterY))
            {
                return new PixelPosition(centerX, centerY);
            }
            else
            {
                double nextCenterX = (gridX + direction.X) * _config.CellSize + _config.CellSize / 2.0 - 16;
                double nextCenterY = (gridY + direction.Y) * _config.CellSize + _config.CellSize / 2.0 - 16;
                return new PixelPosition(nextCenterX, nextCenterY);
            }
        }

        public void Reset()
        {
            _pendingDirection = null;
            _targetPosition = null;
            _queuedDirection = null;
            _lastTurnGridX = -1;
            _lastTurnGridY = -1;
        }
    }
}
