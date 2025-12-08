using System;
using System.Collections.Generic;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    /// <summary>
    /// Implements game rules for Mensch-ärgere-dich-nicht.
    /// Handles player initialization, move validation, and dice rolls.
    /// </summary>
    public class B1_MAN_Rules : OOPGames.IGameRules
    {
        #region Fields
        private B1_MAN_Board _board;
        private readonly Random _rnd = new Random();
        private bool _playerCountSelected = false;
        #endregion

        #region Properties
        public string Name => "B1_MAN_Rules";

        public OOPGames.IGameField CurrentField => _board;

        public int LastDice { get; private set; } = 0;

        public bool LastMoveGivesExtraTurn { get; private set; } = false;

        public bool MovesPossible
        {
            get
            {
                foreach (var p in _board.Players)
                {
                    if (p.Pieces.Exists(pc => pc.IsOnTrack)) return true;
                    if (p.Pieces.Exists(pc => pc.IsInBase)) return true;
                }
                return false;
            }
        }
        #endregion

        #region Constructor
        public B1_MAN_Rules(B1_MAN_Board board)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }
        #endregion

        #region IGameRules Implementation
        public void ClearField()
        {
            if (!_playerCountSelected)
            {
                var config = ShowPlayerCountDialog();
                if (config.playerCount < 2 || config.playerCount > 4) config.playerCount = 4;
                _board = new B1_MAN_Board(config.playerCount, config.advancedMode);
                _playerCountSelected = true;
            }
            
            if (_board != null)
            {
                _board.Clear();
            }
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
        #endregion

        #region Dice and Move Logic
        public int RollDice()
        {
            return _rnd.Next(1, 7);
        }

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

        public (bool moved, bool captured, B1_MAN_Piece capturedPiece) DoMove(B1_MAN_Piece piece, int dice)
        {
            if (piece == null) return (false, false, null);
            return _board.MovePiece(piece, dice);
        }
        #endregion

        #region Private Methods
        private (int playerCount, bool advancedMode) ShowPlayerCountDialog()
        {
            try
            {
                var dialog = new PlayerCountDialog();
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialog.SelectedPlayerCount >= 2 && dialog.SelectedPlayerCount <= 4)
                {
                    return (dialog.SelectedPlayerCount, dialog.AdvancedMode);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Dialog-Fehler: {ex.Message}");
            }
            
            return (4, false);
        }
        #endregion
    }
}
