using System;
using System.Collections.Generic;

namespace OOPGames
{

    /// Verwaltet die Schlange und ihre Segmente

    public class SnakeEntity
    {
        private readonly List<PixelPosition> _segments;
        private readonly List<PixelPosition> _movementHistory;
        private readonly SnakeGameConfig _config;

        public IReadOnlyList<PixelPosition> Segments => _segments;
        public PixelPosition Head => _segments.Count > 0 ? _segments[0] : null;
        public PixelPosition Direction { get; private set; }
        public int PlayerNumber { get; private set; }
        public bool IsAlive { get; private set; }

        public SnakeEntity(SnakeGameConfig config, int playerNumber = 1)
        {
            _config = config;
            _segments = new List<PixelPosition>();
            _movementHistory = new List<PixelPosition>();
            PlayerNumber = playerNumber;
            IsAlive = true;
        }

        public void Initialize(double startX, double startY)
        {
            _segments.Clear();
            _movementHistory.Clear();
            IsAlive = true;

            var startPos = new PixelPosition(startX, startY);
            startPos.DirX = 1;
            startPos.DirY = 0;
            _segments.Add(startPos);

            // Start mit drei Segmenten: Kopf, Körper, Rassel
            var bodyPos = new PixelPosition(startX - _config.CellSize, startY);
            bodyPos.DirX = 1;
            bodyPos.DirY = 0;
            _segments.Add(bodyPos);

            var rattlePos = new PixelPosition(startX - 2 * _config.CellSize, startY);
            rattlePos.DirX = 1;
            rattlePos.DirY = 0;
            _segments.Add(rattlePos);

            Direction = new PixelPosition(1, 0);

            // Bewegungshistorie vorbefüllen, damit Schwanz/Rassel sofort mitlaufen
            int neededHistory = (_segments.Count - 1) * _config.StepGap;
            for (int k = neededHistory; k >= 1; k--)
            {
                var prev = new PixelPosition(startX - k * _config.Speed, startY);
                prev.DirX = 1;
                prev.DirY = 0;
                _movementHistory.Add(prev);
            }

            var histStart = new PixelPosition(startX, startY);
            histStart.DirX = 1;
            histStart.DirY = 0;
            _movementHistory.Add(histStart);
        }

        public void SetDirection(PixelPosition newDirection)
        {
            Direction = newDirection;
        }

        public void MoveHead(double deltaX, double deltaY)
        {
            var newHead = new PixelPosition(
                Head.X + deltaX,
                Head.Y + deltaY
            );
            newHead.DirX = Direction.X;
            newHead.DirY = Direction.Y;

            _segments[0] = newHead;

            var historyPos = new PixelPosition(newHead.X, newHead.Y);
            historyPos.DirX = Direction.X;
            historyPos.DirY = Direction.Y;
            _movementHistory.Add(historyPos);
        }

        public void SetHeadPosition(double x, double y)
        {
            _segments[0].X = x;
            _segments[0].Y = y;
        }

        public void AddHistoryPoint(PixelPosition position)
        {
            _movementHistory.Add(position);
        }

        public void UpdateTailPositions()
        {
            for (int i = 1; i < _segments.Count; i++)
            {
                int idx = _movementHistory.Count - 1 - (i * _config.StepGap);
                if (idx >= 0)
                {
                    var p = _movementHistory[idx];
                    _segments[i].X = p.X;
                    _segments[i].Y = p.Y;
                    _segments[i].DirX = p.DirX;
                    _segments[i].DirY = p.DirY;
                }
            }
        }

        public void TrimHistory()
        {
            int maxHistory = (_segments.Count + 5) * _config.StepGap;
            int toTrim = _movementHistory.Count - maxHistory;
            if (toTrim > 0)
            {
                _movementHistory.RemoveRange(0, toTrim);
            }
        }

        public void Grow()
        {
            var last = _segments[_segments.Count - 1];
            _segments.Add(new PixelPosition(last.X, last.Y));
        }

        public bool CheckSelfCollision()
        {
            for (int i = 3; i < _segments.Count; i++)
            {
                double dx = Math.Abs(Head.X - _segments[i].X);
                double dy = Math.Abs(Head.Y - _segments[i].Y);
                if (dx < _config.CellSize / 2 && dy < _config.CellSize / 2)
                {
                    return true;
                }
            }
            return false;
        }

        public void Kill()
        {
            IsAlive = false;
        }
    }
}
