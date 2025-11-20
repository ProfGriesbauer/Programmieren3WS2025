using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    // Simplified board model for "Mensch ärgere dich nicht".
    // Track positions: 0..39 main ring; each player's home slots are 100 + (player-1)*4 .. 100+(player-1)*4+3
    public class B1_MAN_Board : OOPGames.IGameField
    {
        public const int TrackLength = 40;
        public const int HomeBaseStart = 100; // home slots start index

        public List<B1_MAN_Player> Players { get; } = new List<B1_MAN_Player>();

        // Helper map: track index -> piece (only one piece allowed per track square)
        private Dictionary<int, B1_MAN_Piece> _trackMap = new Dictionary<int, B1_MAN_Piece>();

        public B1_MAN_Board(int players = 4)
        {
            if (players < 2 || players > 4) throw new ArgumentException("players must be 2..4");
            for (int p = 1; p <= players; p++)
            {
                Players.Add(new B1_MAN_Player(p, $"Player{p}"));
            }
        }

        public B1_MAN_Piece GetPieceAt(int trackIndex)
        {
            if (_trackMap.TryGetValue(trackIndex, out var piece)) return piece;
            return null;
        }

        // Places a piece to a given position (handles track and home indices). Does not validate moves.
        public void PlacePiece(B1_MAN_Piece piece, int position)
        {
            // remove from previous track index if present
            var prevKeys = _trackMap.Where(kv => kv.Value == piece).Select(kv => kv.Key).ToList();
            foreach (var k in prevKeys) _trackMap.Remove(k);

            piece.SetPosition(position);
            if (position >= 0 && position < TrackLength)
            {
                _trackMap[position] = piece;
            }
        }

        // Send piece back to base
        public void SendToBase(B1_MAN_Piece piece)
        {
            PlacePiece(piece, -1);
        }

        // Find a player's piece by id
        public B1_MAN_Piece GetPlayerPiece(int playerNumber, int pieceId)
        {
            var pl = Players.FirstOrDefault(p => p.PlayerNumber == playerNumber);
            return pl?.Pieces.FirstOrDefault(pc => pc.Id == pieceId);
        }

        // Very small helper: get absolute start index on track for a player (where pieces enter the track)
        public int EntryIndexForPlayer(int playerNumber)
        {
            // Typical mapping: player1->0, player2->10, player3->20, player4->30
            return ((playerNumber - 1) * 10) % TrackLength;
        }

        // Determine whether a track cell is free (or occupier is opponent -> can be captured)
        public bool IsTrackFree(int index)
        {
            return !_trackMap.ContainsKey(index);
        }

        // Helper to compute home slot index base for player
        public int HomeBaseForPlayer(int playerNumber)
        {
            return HomeBaseStart + (playerNumber - 1) * 4;
        }

        // Simple move procedure (does not enforce dice or full rules) -- returns capture info
        // Returns (moved, captured, capturedPiece)
        public (bool moved, bool captured, B1_MAN_Piece capturedPiece) MovePiece(B1_MAN_Piece piece, int steps)
        {
            // In base: only enter on 6
            if (piece.IsInBase)
            {
                if (steps != 6) return (false, false, null);
                int entry = EntryIndexForPlayer(piece.Owner);
                if (_trackMap.TryGetValue(entry, out var occ))
                {
                    if (occ.Owner == piece.Owner)
                    {
                        // blocked by own piece
                        return (false, false, null);
                    }
                    else
                    {
                        // capture opponent
                        SendToBase(occ);
                        PlacePiece(piece, entry);
                        return (true, true, occ);
                    }
                }
                else
                {
                    PlacePiece(piece, entry);
                    return (true, false, null);
                }
            }

            // On track: may move along track or enter home
            if (piece.IsOnTrack)
            {
                int owner = piece.Owner;
                int homeEntry = (EntryIndexForPlayer(owner) + TrackLength - 1) % TrackLength; // square before entry
                int stepsToHomeEntry = (homeEntry - piece.Position + TrackLength) % TrackLength;

                if (steps <= stepsToHomeEntry)
                {
                    // stays on track
                    int newPos = (piece.Position + steps) % TrackLength;
                    if (_trackMap.TryGetValue(newPos, out var occ))
                    {
                        if (occ.Owner == owner)
                        {
                            // blocked by own piece
                            return (false, false, null);
                        }
                        else
                        {
                            SendToBase(occ);
                            PlacePiece(piece, newPos);
                            return (true, true, occ);
                        }
                    }
                    else
                    {
                        PlacePiece(piece, newPos);
                        return (true, false, null);
                    }
                }
                else
                {
                    // attempt to enter home
                    int stepsIntoHome = steps - stepsToHomeEntry - 1; // 0 = first home slot
                    if (stepsIntoHome < 0) return (false, false, null);
                    if (stepsIntoHome > 3) return (false, false, null); // cannot move beyond home

                    int targetHome = HomeBaseForPlayer(owner) + stepsIntoHome;
                    // check if target home slot free
                    var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                    if (pl == null) return (false, false, null);
                    bool occupied = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == targetHome);
                    if (occupied) return (false, false, null);

                    // moving into home: remove from track map and set position
                    PlacePiece(piece, targetHome);
                    return (true, false, null);
                }
            }

            // In home: move within home slots
            if (piece.IsInHome)
            {
                int owner = piece.Owner;
                int baseHome = HomeBaseForPlayer(owner);
                int curSlot = piece.Position - baseHome;
                int newSlot = curSlot + steps;
                if (newSlot < 0 || newSlot > 3) return (false, false, null);
                int targetHome = baseHome + newSlot;
                var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                if (pl == null) return (false, false, null);
                bool occupied = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == targetHome);
                if (occupied) return (false, false, null);
                PlacePiece(piece, targetHome);
                return (true, false, null);
            }

            return (false, false, null);
        }

        // Clears the board: send all pieces back to base and clear track map
        public void Clear()
        {
            _trackMap.Clear();
            foreach (var p in Players)
            {
                foreach (var pc in p.Pieces)
                {
                    pc.SetPosition(-1);
                }
            }
        }

        // IGameField implementation: simple painter acceptance check
        public bool CanBePaintedBy(OOPGames.IPaintGame painter)
        {
            if (painter == null) return false;
            // Accept painter that we provide (B1_MAN_Paint) or any painter that contains "MAN" in its name
            try
            {
                if (!string.IsNullOrEmpty(painter.Name) && painter.Name.Contains("MAN")) return true;
            }
            catch { }
            return false;
        }
    }
}
