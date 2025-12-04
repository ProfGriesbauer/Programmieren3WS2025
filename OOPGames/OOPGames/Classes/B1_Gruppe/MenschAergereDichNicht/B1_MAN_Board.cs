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
        
        // Dice for the game
        public B1_MAN_Dice Dice { get; } = new B1_MAN_Dice();
        
        // Current player (1-4)
        public int CurrentPlayer { get; set; } = 1;
        
        // Selected piece for movement (null if none selected)
        public B1_MAN_Piece SelectedPiece { get; set; } = null;
        
        // Flag to prevent double-processing of events
        public bool IsProcessing { get; set; } = false;
        
        // Track number of rolls when player has all pieces in base (max 3)
        public int RollAttemptsRemaining { get; set; } = 0;

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
            // Mapping based on actual track layout (im Uhrzeigersinn):
            // WICHTIG: Unabhängig von Spielerzahl bleiben die Farben an ihren Positionen
            // Player 1: Immer Rot bei Index 30 -> Feld (4,10)
            // Player 2: Immer Schwarz bei Index 20 -> Feld (10,6) (gegenüber von Rot)
            // Player 3: Immer Gelb bei Index 10 -> Feld (6,0) (gegenüber von Grün)
            // Player 4: Immer Grün bei Index 0 -> Feld (0,4) (gegenüber von Gelb)
            
            // Bei 2 Spielern: Spieler 1+2 (Rot + Schwarz) = gegenüberliegend
            // Bei 3 Spielern: Spieler 1+2+3 (Rot + Schwarz + Gelb)
            // Bei 4 Spielern: Alle
            
            switch (playerNumber)
            {
                case 1: return 30; // Rot starts at (4,10)
                case 2: return 20; // Schwarz starts at (10,6)
                case 3: return 10; // Gelb starts at (6,0)
                case 4: return 0;  // Grün starts at (0,4)
                default: return 30; // Fallback
            }
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

        // Check if a piece can be moved with the given dice value
        public bool CanMovePiece(B1_MAN_Piece piece, int diceValue)
        {
            if (piece == null) return false;
            
            // In base: only move on 6
            if (piece.IsInBase)
            {
                if (diceValue != 6) return false;
                int entry = EntryIndexForPlayer(piece.Owner);
                // Check if entry is blocked by own piece
                if (_trackMap.TryGetValue(entry, out var occ))
                {
                    if (occ.Owner == piece.Owner) return false;
                }
                return true;
            }
            
            // On track: check if can move
            if (piece.IsOnTrack)
            {
                int owner = piece.Owner;
                int homeEntry = (EntryIndexForPlayer(owner) + TrackLength - 1) % TrackLength;
                int stepsToHomeEntry = (homeEntry - piece.Position + TrackLength) % TrackLength;
                
                if (diceValue <= stepsToHomeEntry)
                {
                    // stays on track
                    int newPos = (piece.Position + diceValue) % TrackLength;
                    if (_trackMap.TryGetValue(newPos, out var occ))
                    {
                        if (occ.Owner == owner) return false; // blocked by own
                    }
                    return true;
                }
                else
                {
                    // would enter home
                    int stepsIntoHome = diceValue - stepsToHomeEntry - 1;
                    if (stepsIntoHome < 0) return false;
                    
                    if (stepsIntoHome > 3)
                    {
                        // too many steps -> continues on track
                        int newPos = (piece.Position + diceValue) % TrackLength;
                        if (_trackMap.TryGetValue(newPos, out var occ))
                        {
                            if (occ.Owner == owner) return false;
                        }
                        return true;
                    }
                    
                    int targetHome = HomeBaseForPlayer(owner) + stepsIntoHome;
                    var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                    if (pl == null) return false;
                    bool occupied = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == targetHome);
                    if (occupied)
                    {
                        // home occupied -> continues on track
                        int newPos = (piece.Position + diceValue) % TrackLength;
                        if (_trackMap.TryGetValue(newPos, out var occ))
                        {
                            if (occ.Owner == owner) return false;
                        }
                        return true;
                    }
                    return true;
                }
            }
            
            // In home: check if can move within home
            if (piece.IsInHome)
            {
                int owner = piece.Owner;
                int baseHome = HomeBaseForPlayer(owner);
                int curSlot = piece.Position - baseHome;
                int newSlot = curSlot + diceValue;
                if (newSlot < 0 || newSlot > 3) return false;
                
                // Check if any piece blocks the path
                var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                if (pl == null) return false;
                
                for (int slot = curSlot + 1; slot <= newSlot; slot++)
                {
                    int checkPos = baseHome + slot;
                    bool blocked = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == checkPos);
                    if (blocked) return false;
                }
                
                return true;
            }
            
            return false;
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
                    
                    if (stepsIntoHome > 3)
                    {
                        // Zu viele Schritte um ins Haus zu gehen -> laufe am Haus vorbei weiter auf der Strecke
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

                    int targetHome = HomeBaseForPlayer(owner) + stepsIntoHome;
                    // check if target home slot free
                    var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                    if (pl == null) return (false, false, null);
                    bool occupied = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == targetHome);
                    if (occupied)
                    {
                        // Zielfeld im Haus besetzt -> laufe am Haus vorbei weiter auf der Strecke
                        int newPos = (piece.Position + steps) % TrackLength;
                        if (_trackMap.TryGetValue(newPos, out var occ2))
                        {
                            if (occ2.Owner == owner)
                            {
                                // blocked by own piece
                                return (false, false, null);
                            }
                            else
                            {
                                SendToBase(occ2);
                                PlacePiece(piece, newPos);
                                return (true, true, occ2);
                            }
                        }
                        else
                        {
                            PlacePiece(piece, newPos);
                            return (true, false, null);
                        }
                    }

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
                
                // Prüfe dass keine Figur übersprungen wird
                int targetHome = baseHome + newSlot;
                var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
                if (pl == null) return (false, false, null);
                
                // Prüfe alle Positionen zwischen curSlot und newSlot
                for (int slot = curSlot + 1; slot <= newSlot; slot++)
                {
                    int checkPos = baseHome + slot;
                    bool blocked = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == checkPos);
                    if (blocked) return (false, false, null); // Kann nicht überspringen
                }
                
                PlacePiece(piece, targetHome);
                return (true, false, null);
            }

            return (false, false, null);
        }
        
        // Try to move selected piece with the current dice value
        public bool TryMoveSelectedPiece()
        {
            if (SelectedPiece == null || !Dice.HasBeenRolled) return false;
            
            var result = MovePiece(SelectedPiece, Dice.CurrentValue);
            // SelectedPiece wird in EndTurn() zurückgesetzt
            return result.moved;
        }
        
        // End current turn and switch to next player
        public void EndTurn()
        {
            Dice.FullReset();
            SelectedPiece = null;
            CurrentPlayer = (CurrentPlayer % Players.Count) + 1;
            
            // Prüfe ob der neue Spieler 3 Würfelversuche braucht
            var currentPlayerObj = Players.FirstOrDefault(p => p.PlayerNumber == CurrentPlayer);
            if (currentPlayerObj != null)
            {
                // Alle Figuren in der Basis -> 3 Versuche
                bool allInBase = currentPlayerObj.Pieces.All(pc => pc.IsInBase);
                
                if (allInBase)
                {
                    RollAttemptsRemaining = 3;
                }
                else
                {
                    // Prüfe ob nur Figuren auf Endpositionen im Haus sind und mindestens eine in der Basis
                    bool anyOnTrack = currentPlayerObj.Pieces.Any(pc => pc.IsOnTrack);
                    bool anyInBase = currentPlayerObj.Pieces.Any(pc => pc.IsInBase);
                    
                    if (!anyOnTrack && anyInBase)
                    {
                        // Keine auf Track, mindestens eine in Basis
                        // Prüfe ob alle Haus-Figuren auf Endpositionen sind
                        int baseHome = HomeBaseForPlayer(CurrentPlayer);
                        var homePieces = currentPlayerObj.Pieces.Where(pc => pc.IsInHome).ToList();
                        
                        bool allHomeOnFinalPositions = true;
                        
                        // Prüfe ob Figuren lückenlos von hinten (Slot 3) nach vorne besetzt sind
                        if (homePieces.Count > 0)
                        {
                            // Sortiere Figuren nach Position (höchste zuerst = hinten im Haus)
                            var sortedHome = homePieces.OrderByDescending(pc => pc.Position).ToList();
                            
                            // Die hinterste Figur muss auf Slot 3 sein
                            if (sortedHome[0].Position != baseHome + 3)
                            {
                                allHomeOnFinalPositions = false;
                            }
                            else if (sortedHome.Count >= 2 && sortedHome[1].Position != baseHome + 2)
                            {
                                allHomeOnFinalPositions = false;
                            }
                            else if (sortedHome.Count >= 3 && sortedHome[2].Position != baseHome + 1)
                            {
                                allHomeOnFinalPositions = false;
                            }
                        }
                        
                        if (allHomeOnFinalPositions)
                        {
                            RollAttemptsRemaining = 3;
                        }
                        else
                        {
                            RollAttemptsRemaining = 0;
                        }
                    }
                    else
                    {
                        RollAttemptsRemaining = 0;
                    }
                }
            }
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
            Dice.Reset();
            CurrentPlayer = 1;
            SelectedPiece = null;
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
