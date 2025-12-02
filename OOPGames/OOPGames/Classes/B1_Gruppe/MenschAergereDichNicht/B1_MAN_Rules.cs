using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    // Basic rules for the game implementing IGameRules: roll dice, determine valid moves, perform moves.
    public class B1_MAN_Rules : OOPGames.IGameRules
    {
        private B1_MAN_Board _board;
        private readonly Random _rnd = new Random();
        // last dice rolled by the rules (for convenience)
        public int LastDice { get; private set; } = 0;

        // indicates whether the last move grants an extra turn (dice == 6)
        public bool LastMoveGivesExtraTurn { get; private set; } = false;
        
        // Track if player count has been selected
        private bool _playerCountSelected = false;

        public B1_MAN_Rules(B1_MAN_Board board)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }

        // IGameRules implementation
        public string Name => "B1_MAN_Rules";

        public OOPGames.IGameField CurrentField => _board;

        public bool MovesPossible
        {
            get
            {
                // A move is possible if any player has a piece on track or a piece in base
                foreach (var p in _board.Players)
                {
                    if (p.Pieces.Exists(pc => pc.IsOnTrack)) return true;
                    if (p.Pieces.Exists(pc => pc.IsInBase)) return true;
                }
                return false;
            }
        }

        public void ClearField()
        {
            // Zeige Spieleranzahl-Dialog nur beim ersten Mal
            if (!_playerCountSelected)
            {
                int playerCount = ShowPlayerCountDialog();
                if (playerCount < 2 || playerCount > 4) playerCount = 4;
                _board = new B1_MAN_Board(playerCount);
                _playerCountSelected = true;
            }
            
            if (_board != null)
            {
                _board.Clear();
            }
        }
        
        // Dialog zur Auswahl der Spieleranzahl (2-4)
        private int ShowPlayerCountDialog()
        {
            try
            {
                var dialog = new PlayerCountDialog();
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialog.SelectedPlayerCount >= 2 && dialog.SelectedPlayerCount <= 4)
                {
                    return dialog.SelectedPlayerCount;
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Dialog-Fehler: {ex.Message}");
            }
            
            // Standard: 4 Spieler
            return 4;
        }

        public int CheckIfPLayerWon()
        {
            foreach (var p in _board.Players)
            {
                bool allHome = true;
                foreach (var pc in p.Pieces)
                {
                    if (!pc.IsInHome) { allHome = false; break; }
                }
                if (allHome) return p.PlayerNumber;
            }
            return -1;
        }

        // Dice roll helper (1..6)
        public int RollDice()
        {
            return _rnd.Next(1, 7);
        }

        // Returns a list of pieces that can be moved given the dice result (simplified)
        public List<B1_MAN_Piece> GetValidMoves(int playerNumber, int dice)
        {
            var result = new List<B1_MAN_Piece>();
            var player = _board.Players.Find(p => p.PlayerNumber == playerNumber);
            if (player == null) return result;

            foreach (var piece in player.Pieces)
            {
                if (piece.IsInBase && dice == 6) result.Add(piece);
                else if (piece.IsOnTrack) result.Add(piece);
            }

            return result;
        }

        // Perform a move expressed as a high-level IPlayMove
        public void DoMove(OOPGames.IPlayMove move)
        {
            if (move == null) return;
            if (!(move is B1_MAN_Move m)) return;

            LastDice = m.Dice;
            LastMoveGivesExtraTurn = (m.Dice == 6);

            var piece = _board.GetPlayerPiece(m.PlayerNumber, m.PieceId);
            if (piece == null) return;

            _board.MovePiece(piece, m.Dice);
        }

        // Convenience method used by computer/human players
        public (bool moved, bool captured, B1_MAN_Piece capturedPiece) DoMove(B1_MAN_Piece piece, int dice)
        {
            if (piece == null) return (false, false, null);
            return _board.MovePiece(piece, dice);
        }
    }
}
