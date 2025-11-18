using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Einfacher Computer-Player: wählt zufällig ein freies Feld
    public class B3_Mika_Roeder_ComputerPlayer : IComputerGamePlayer
    {
        private int _playerNumber = 0;
        private Random _rand = new Random();

        public string Name { get { return "B3 Mika Röder Computer (Random)"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B3_Mika_Roeder_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B3_Mika_Roeder_ComputerPlayer();
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field == null) return null;

            List<Tuple<int,int>> free = new List<Tuple<int,int>>();

            if (field is B3_Mika_Roeder_Field bf)
            {
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        if (bf[r, c] == 0) free.Add(Tuple.Create(r, c));
            }
            else
            {
                try
                {
                    var t = field.GetType();
                    var prop = t.GetProperty("Item", new Type[] { typeof(int), typeof(int) });
                    if (prop != null)
                    {
                        for (int r = 0; r < 3; r++)
                            for (int c = 0; c < 3; c++)
                            {
                                var v = prop.GetValue(field, new object[] { r, c });
                                if (v is int && (int)v == 0)
                                    free.Add(Tuple.Create(r, c));
                            }
                    }
                }
                catch
                {
                    // ignore and return null below
                }
            }

            if (free.Count == 0) return null;

            var pick = free[_rand.Next(free.Count)];
            return new B3_Mika_Roeder_Move(pick.Item1, pick.Item2, _playerNumber);
        }
    }
}
