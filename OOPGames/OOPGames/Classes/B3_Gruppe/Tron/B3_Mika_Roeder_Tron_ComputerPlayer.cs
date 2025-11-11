using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Simple Tron AI: tries to go straight, otherwise picks a safe random turn
    public class B3_Mika_Roeder_Tron_ComputerPlayer : IComputerGamePlayer
    {
        private int _playerNumber = 0;
        private Random _rand = new Random();

        public string Name { get { return "B3 Mika Röder Tron Computer"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B3_Mika_Roeder_Tron_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B3_Mika_Roeder_Tron_ComputerPlayer();
        }

        public IPlayMove GetMove(IGameField field)
        {
            // Improved AI:
            // - usually keep current direction (return null) to avoid jitter
            // - when deciding, look from the player's current head cell and pick the direction
            //   that offers the largest free straight-line distance (avoids walls/trails)

            if (field is B3_Mika_Roeder_Tron_Field bf)
            {
                // find a candidate head cell for this player: pick the last occurrence in scan order
                int headR = -1, headC = -1;
                for (int r = 0; r < bf.Height; r++)
                {
                    for (int c = 0; c < bf.Width; c++)
                    {
                        if (bf[r, c] == _playerNumber) { headR = r; headC = c; }
                    }
                }

                // if we couldn't find any cell, fallback to random occasional move
                if (headR == -1)
                {
                    if (_rand.NextDouble() > 0.5) return null;
                    var opts = new List<B3TronDirection>() { B3TronDirection.Up, B3TronDirection.Right, B3TronDirection.Down, B3TronDirection.Left };
                    var pick = opts[_rand.Next(opts.Count)];
                    return new B3_Mika_Roeder_Tron_Move(_playerNumber, pick);
                }

                // evaluate each direction by counting free cells ahead
                var scores = new Dictionary<B3TronDirection, int>();
                foreach (B3TronDirection d in new[] { B3TronDirection.Up, B3TronDirection.Right, B3TronDirection.Down, B3TronDirection.Left })
                {
                    int steps = 0;
                    int rr = headR, cc = headC;
                    while (true)
                    {
                        switch (d)
                        {
                            case B3TronDirection.Up: rr--; break;
                            case B3TronDirection.Down: rr++; break;
                            case B3TronDirection.Left: cc--; break;
                            default: cc++; break;
                        }
                        if (rr < 0 || rr >= bf.Height || cc < 0 || cc >= bf.Width) break;
                        if (bf[rr, cc] != 0) break;
                        steps++;
                        if (steps > 50) break; // cap lookahead
                    }
                    scores[d] = steps;
                }

                // choose the direction(s) with maximum score
                int best = -1;
                foreach (var v in scores.Values) if (v > best) best = v;
                var bestDirs = new List<B3TronDirection>();
                foreach (var kv in scores) if (kv.Value == best) bestDirs.Add(kv.Key);

                // bias to not change direction every tick: 60% chance to keep current (null)
                if (_rand.NextDouble() < 0.6) return null;

                // pick among best
                var chosen = bestDirs[_rand.Next(bestDirs.Count)];
                return new B3_Mika_Roeder_Tron_Move(_playerNumber, chosen);
            }

            // fallback
            if (_rand.NextDouble() > 0.5) return null;
            var opts2 = new List<B3TronDirection>() { B3TronDirection.Up, B3TronDirection.Right, B3TronDirection.Down, B3TronDirection.Left };
            var pick2 = opts2[_rand.Next(opts2.Count)];
            return new B3_Mika_Roeder_Tron_Move(_playerNumber, pick2);
        }
    }
}
