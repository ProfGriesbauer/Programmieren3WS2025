using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    /// <summary>
    /// Special field types for advanced mode
    /// </summary>
    public enum SpecialFieldType
    {
        None,           // Normal field
        Skip,           // Aussetzen (miss next turn)
        RollAgain,      // Nochmal würfeln
        Forward2,       // 2 Felder vorwärts
        Backward2,      // 2 Felder rückwärts
        BackToStart     // Zurück zum Start (in die Basis)
    }

    /// <summary>
    /// Board model for "Mensch ärgere dich nicht".
    /// Manages piece positions, track, home slots, and game state.
    /// </summary>
    public class B1_MAN_Board : OOPGames.IGameField
    {
        #region Constants
        public const int TrackLength = 40;
        public const int HomeBaseStart = 100; // home slots start index
        #endregion

        #region Fields
        private Dictionary<int, B1_MAN_Piece> _trackMap = new Dictionary<int, B1_MAN_Piece>();
        private Dictionary<int, SpecialFieldType> _specialFields = new Dictionary<int, SpecialFieldType>();
        #endregion

        #region Properties
        public List<B1_MAN_Player> Players { get; } = new List<B1_MAN_Player>();
        
        public B1_MAN_Dice Dice { get; } = new B1_MAN_Dice();
        
        public int CurrentPlayer { get; set; } = 1;
        
        public B1_MAN_Piece SelectedPiece { get; set; } = null;
        
        public bool IsProcessing { get; set; } = false;
        
        public int RollAttemptsRemaining { get; set; } = 0;
        
        public bool AdvancedMode { get; set; } = false;
        
        public bool MustSkipNextTurn { get; set; } = false;
        
        public bool CanRollAgain { get; set; } = false;
        #endregion

        #region Constructor
        public B1_MAN_Board(int players = 4, bool advancedMode = false)
        {
            if (players < 2 || players > 4) throw new ArgumentException("players must be 2..4");
            AdvancedMode = advancedMode;
            for (int p = 1; p <= players; p++)
            {
                Players.Add(new B1_MAN_Player(p, $"Player{p}"));
            }
            
            if (AdvancedMode)
            {
                InitializeSpecialFields();
            }
        }
        #endregion

        #region Special Fields (Advanced Mode)
        private void InitializeSpecialFields()
        {
            _specialFields[5] = SpecialFieldType.RollAgain;      // Nochmal würfeln
            _specialFields[12] = SpecialFieldType.Forward2;      // 2 vor
            _specialFields[18] = SpecialFieldType.Skip;          // Aussetzen
            _specialFields[25] = SpecialFieldType.Backward2;     // 2 zurück
            _specialFields[32] = SpecialFieldType.BackToStart;   // Zurück zum Start
            _specialFields[37] = SpecialFieldType.RollAgain;     // Nochmal würfeln
        }
        
        public SpecialFieldType GetSpecialFieldType(int position)
        {
            if (!AdvancedMode || position < 0 || position >= TrackLength) 
                return SpecialFieldType.None;
            
            if (_specialFields.TryGetValue(position, out var fieldType))
                return fieldType;
            
            return SpecialFieldType.None;
        }

        private void ApplySpecialFieldEffect(B1_MAN_Piece piece, int position)
        {
            if (!AdvancedMode || !piece.IsOnTrack) return;
            
            var fieldType = GetSpecialFieldType(position);
            
            switch (fieldType)
            {
                case SpecialFieldType.Skip:
                    MustSkipNextTurn = true;
                    break;
                    
                case SpecialFieldType.RollAgain:
                    CanRollAgain = true;
                    break;
                    
                case SpecialFieldType.Forward2:
                    {
                        int forwardPos = (position + 2) % TrackLength;
                        if (_trackMap.TryGetValue(forwardPos, out var fwdOcc))
                        {
                            if (fwdOcc.Owner != piece.Owner)
                            {
                                SendToBase(fwdOcc);
                            }
                            else
                            {
                                break;
                            }
                        }
                        PlacePiece(piece, forwardPos);
                        break;
                    }
                    
                case SpecialFieldType.Backward2:
                    {
                        int backwardPos = (position - 2 + TrackLength) % TrackLength;
                        if (_trackMap.TryGetValue(backwardPos, out var bwdOcc))
                        {
                            if (bwdOcc.Owner != piece.Owner)
                            {
                                SendToBase(bwdOcc);
                            }
                            else
                            {
                                break;
                            }
                        }
                        PlacePiece(piece, backwardPos);
                        break;
                    }
                    
                case SpecialFieldType.BackToStart:
                    SendToBase(piece);
                    break;
                    
                case SpecialFieldType.None:
                default:
                    break;
            }
        }
        #endregion

        #region Piece Position Helpers
        public B1_MAN_Piece GetPieceAt(int trackIndex)
        {
            if (_trackMap.TryGetValue(trackIndex, out var piece)) return piece;
            return null;
        }

        public void PlacePiece(B1_MAN_Piece piece, int position)
        {
            var prevKeys = _trackMap.Where(kv => kv.Value == piece).Select(kv => kv.Key).ToList();
            foreach (var k in prevKeys) _trackMap.Remove(k);

            piece.SetPosition(position);
            if (position >= 0 && position < TrackLength)
            {
                _trackMap[position] = piece;
            }
        }

        public void SendToBase(B1_MAN_Piece piece)
        {
            PlacePiece(piece, -1);
        }

        public B1_MAN_Piece GetPlayerPiece(int playerNumber, int pieceId)
        {
            var pl = Players.FirstOrDefault(p => p.PlayerNumber == playerNumber);
            return pl?.Pieces.FirstOrDefault(pc => pc.Id == pieceId);
        }
        #endregion

        #region Player Entry and Home Base
        public int EntryIndexForPlayer(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1: return 30; // Rot
                case 2: return 20; // Schwarz
                case 3: return 10; // Gelb
                case 4: return 0;  // Grün
                default: return 30;
            }
        }

        public int HomeBaseForPlayer(int playerNumber)
        {
            return HomeBaseStart + (playerNumber - 1) * 4;
        }

        public bool IsTrackFree(int index)
        {
            return !_trackMap.ContainsKey(index);
        }
        #endregion

        #region Move Validation
        public bool CanMovePiece(B1_MAN_Piece piece, int diceValue)
        {
            if (piece == null) return false;
            
            if (piece.IsInBase)
            {
                if (diceValue != 6) return false;
                int entry = EntryIndexForPlayer(piece.Owner);
                if (_trackMap.TryGetValue(entry, out var occ))
                {
                    if (occ.Owner == piece.Owner) return false;
                }
                return true;
            }
            
            if (piece.IsOnTrack)
            {
                int owner = piece.Owner;
                int homeEntry = (EntryIndexForPlayer(owner) + TrackLength - 1) % TrackLength;
                int stepsToHomeEntry = (homeEntry - piece.Position + TrackLength) % TrackLength;
                
                if (diceValue <= stepsToHomeEntry)
                {
                    int newPos = (piece.Position + diceValue) % TrackLength;
                    if (_trackMap.TryGetValue(newPos, out var occ))
                    {
                        if (occ.Owner == owner) return false;
                    }
                    return true;
                }
                else
                {
                    int stepsIntoHome = diceValue - stepsToHomeEntry - 1;
                    if (stepsIntoHome < 0) return false;
                    
                    if (stepsIntoHome > 3)
                    {
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
            
            if (piece.IsInHome)
            {
                int owner = piece.Owner;
                int baseHome = HomeBaseForPlayer(owner);
                int curSlot = piece.Position - baseHome;
                int newSlot = curSlot + diceValue;
                if (newSlot < 0 || newSlot > 3) return false;
                
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
        #endregion

        #region Move Execution
        public (bool moved, bool captured, B1_MAN_Piece capturedPiece) MovePiece(B1_MAN_Piece piece, int steps)
        {
            if (piece.IsInBase)
            {
                return MovePieceFromBase(piece, steps);
            }

            if (piece.IsOnTrack)
            {
                return MovePieceOnTrack(piece, steps);
            }

            if (piece.IsInHome)
            {
                return MovePieceInHome(piece, steps);
            }

            return (false, false, null);
        }

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MovePieceFromBase(B1_MAN_Piece piece, int steps)
        {
            if (steps != 6) return (false, false, null);
            int entry = EntryIndexForPlayer(piece.Owner);
            if (_trackMap.TryGetValue(entry, out var occ))
            {
                if (occ.Owner == piece.Owner)
                {
                    return (false, false, null);
                }
                else
                {
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

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MovePieceOnTrack(B1_MAN_Piece piece, int steps)
        {
            int owner = piece.Owner;
            int homeEntry = (EntryIndexForPlayer(owner) + TrackLength - 1) % TrackLength;
            int stepsToHomeEntry = (homeEntry - piece.Position + TrackLength) % TrackLength;

            if (steps <= stepsToHomeEntry)
            {
                return MoveOnTrackStayOnTrack(piece, steps, owner);
            }
            else
            {
                return MoveOnTrackToHome(piece, steps, owner, stepsToHomeEntry);
            }
        }

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MoveOnTrackStayOnTrack(B1_MAN_Piece piece, int steps, int owner)
        {
            int newPos = (piece.Position + steps) % TrackLength;
            if (_trackMap.TryGetValue(newPos, out var occ))
            {
                if (occ.Owner == owner)
                {
                    return (false, false, null);
                }
                else
                {
                    SendToBase(occ);
                    PlacePiece(piece, newPos);
                    ApplySpecialFieldEffect(piece, newPos);
                    return (true, true, occ);
                }
            }
            else
            {
                PlacePiece(piece, newPos);
                ApplySpecialFieldEffect(piece, newPos);
                return (true, false, null);
            }
        }

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MoveOnTrackToHome(B1_MAN_Piece piece, int steps, int owner, int stepsToHomeEntry)
        {
            int stepsIntoHome = steps - stepsToHomeEntry - 1;
            if (stepsIntoHome < 0) return (false, false, null);
            
            if (stepsIntoHome > 3)
            {
                return MoveOnTrackPassHome(piece, steps, owner);
            }

            int targetHome = HomeBaseForPlayer(owner) + stepsIntoHome;
            var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
            if (pl == null) return (false, false, null);
            bool occupied = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == targetHome);
            if (occupied)
            {
                return MoveOnTrackPassHome(piece, steps, owner);
            }

            PlacePiece(piece, targetHome);
            return (true, false, null);
        }

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MoveOnTrackPassHome(B1_MAN_Piece piece, int steps, int owner)
        {
            int newPos = (piece.Position + steps) % TrackLength;
            if (_trackMap.TryGetValue(newPos, out var occ))
            {
                if (occ.Owner == owner)
                {
                    return (false, false, null);
                }
                else
                {
                    SendToBase(occ);
                    PlacePiece(piece, newPos);
                    ApplySpecialFieldEffect(piece, newPos);
                    return (true, true, occ);
                }
            }
            else
            {
                PlacePiece(piece, newPos);
                ApplySpecialFieldEffect(piece, newPos);
                return (true, false, null);
            }
        }

        private (bool moved, bool captured, B1_MAN_Piece capturedPiece) MovePieceInHome(B1_MAN_Piece piece, int steps)
        {
            int owner = piece.Owner;
            int baseHome = HomeBaseForPlayer(owner);
            int curSlot = piece.Position - baseHome;
            int newSlot = curSlot + steps;
            if (newSlot < 0 || newSlot > 3) return (false, false, null);
            
            int targetHome = baseHome + newSlot;
            var pl = Players.FirstOrDefault(p => p.PlayerNumber == owner);
            if (pl == null) return (false, false, null);
            
            for (int slot = curSlot + 1; slot <= newSlot; slot++)
            {
                int checkPos = baseHome + slot;
                bool blocked = pl.Pieces.Exists(pc => pc.IsInHome && pc.Position == checkPos);
                if (blocked) return (false, false, null);
            }
            
            PlacePiece(piece, targetHome);
            return (true, false, null);
        }
        #endregion

        #region Turn Management
        public bool TryMoveSelectedPiece()
        {
            if (SelectedPiece == null || !Dice.HasBeenRolled) return false;
            
            var result = MovePiece(SelectedPiece, Dice.CurrentValue);
            return result.moved;
        }
        
        public void EndTurn()
        {
            Dice.FullReset();
            SelectedPiece = null;
            
            if (CanRollAgain)
            {
                CanRollAgain = false;
                return;
            }
            
            CurrentPlayer = (CurrentPlayer % Players.Count) + 1;
            
            if (MustSkipNextTurn)
            {
                MustSkipNextTurn = false;
                CurrentPlayer = (CurrentPlayer % Players.Count) + 1;
            }
            
            var currentPlayerObj = Players.FirstOrDefault(p => p.PlayerNumber == CurrentPlayer);
            if (currentPlayerObj != null)
            {
                bool allInBase = currentPlayerObj.Pieces.All(pc => pc.IsInBase);
                
                if (allInBase)
                {
                    RollAttemptsRemaining = 3;
                }
                else
                {
                    bool anyOnTrack = currentPlayerObj.Pieces.Any(pc => pc.IsOnTrack);
                    bool anyInBase = currentPlayerObj.Pieces.Any(pc => pc.IsInBase);
                    
                    if (!anyOnTrack && anyInBase)
                    {
                        int baseHome = HomeBaseForPlayer(CurrentPlayer);
                        var homePieces = currentPlayerObj.Pieces.Where(pc => pc.IsInHome).ToList();
                        
                        bool allHomeOnFinalPositions = true;
                        
                        if (homePieces.Count > 0)
                        {
                            var sortedHome = homePieces.OrderByDescending(pc => pc.Position).ToList();
                            
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
        #endregion

        #region Utilities
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

        public bool CanBePaintedBy(OOPGames.IPaintGame painter)
        {
            if (painter == null) return false;
            try
            {
                if (!string.IsNullOrEmpty(painter.Name) && painter.Name.Contains("MAN")) return true;
            }
            catch { }
            return false;
        }
        #endregion
    }
}
