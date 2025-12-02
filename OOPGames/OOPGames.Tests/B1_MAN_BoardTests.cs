using Microsoft.VisualStudio.TestTools.UnitTesting;
using OOPGames.B1_Gruppe.MenschAergereDichNicht;

namespace OOPGames.Tests
{
    [TestClass]
    public class B1_MAN_BoardTests
    {
        [TestMethod]
        public void BaseEntry_OnSix_EntersTrack()
        {
            var board = new B1_MAN_Board(2);
            var piece = board.GetPlayerPiece(1, 0);
            Assert.IsTrue(piece.IsInBase);

            var res = board.MovePiece(piece, 6);
            Assert.IsTrue(res.moved);
            int entry = board.EntryIndexForPlayer(1);
            Assert.IsTrue(piece.IsOnTrack);
            Assert.AreEqual(entry, piece.Position);
        }

        [TestMethod]
        public void BaseEntry_NotSix_NoEntry()
        {
            var board = new B1_MAN_Board(2);
            var piece = board.GetPlayerPiece(1, 0);
            var res = board.MovePiece(piece, 5);
            Assert.IsFalse(res.moved);
            Assert.IsTrue(piece.IsInBase);
        }

        [TestMethod]
        public void Entry_CapturesOpponent()
        {
            var board = new B1_MAN_Board(2);
            var entry = board.EntryIndexForPlayer(1);
            var oppPiece = board.GetPlayerPiece(2, 0);
            board.PlacePiece(oppPiece, entry);

            var piece = board.GetPlayerPiece(1, 0);
            var res = board.MovePiece(piece, 6);
            Assert.IsTrue(res.moved);
            Assert.IsTrue(res.captured);
            Assert.IsNotNull(res.capturedPiece);
            Assert.IsTrue(res.capturedPiece.IsInBase);
            Assert.AreEqual(entry, piece.Position);
        }

        [TestMethod]
        public void Entry_BlockedByOwn_NoEntry()
        {
            var board = new B1_MAN_Board(2);
            var entry = board.EntryIndexForPlayer(1);
            var own = board.GetPlayerPiece(1, 1);
            board.PlacePiece(own, entry);

            var piece = board.GetPlayerPiece(1, 0);
            var res = board.MovePiece(piece, 6);
            Assert.IsFalse(res.moved);
            Assert.IsTrue(piece.IsInBase);
        }

        [TestMethod]
        public void EnterHome_ExactSteps_Works()
        {
            var board = new B1_MAN_Board(2);
            var player = 1;
            int entry = board.EntryIndexForPlayer(player);
            int homeEntry = (entry + B1_MAN_Board.TrackLength - 1) % B1_MAN_Board.TrackLength;

            var piece = board.GetPlayerPiece(player, 0);
            // place piece on square before entry
            board.PlacePiece(piece, homeEntry);
            var res = board.MovePiece(piece, 1);
            Assert.IsTrue(res.moved);
            Assert.IsTrue(piece.IsInHome);
        }

        [TestMethod]
        public void Cannot_Overshoot_Home()
        {
            var board = new B1_MAN_Board(2);
            var player = 1;
            int baseHome = board.HomeBaseForPlayer(player);
            var piece = board.GetPlayerPiece(player, 0);
            // place directly into last home slot
            board.PlacePiece(piece, baseHome + 3);
            var res = board.MovePiece(piece, 1);
            Assert.IsFalse(res.moved);
            Assert.IsTrue(piece.IsInHome);
            Assert.AreEqual(baseHome + 3, piece.Position);
        }

        [TestMethod]
        public void Move_Within_Home_Works()
        {
            var board = new B1_MAN_Board(2);
            var player = 1;
            int baseHome = board.HomeBaseForPlayer(player);
            var piece = board.GetPlayerPiece(player, 0);
            board.PlacePiece(piece, baseHome + 0);
            var res = board.MovePiece(piece, 1);
            Assert.IsTrue(res.moved);
            Assert.IsTrue(piece.IsInHome);
            Assert.AreEqual(baseHome + 1, piece.Position);
        }

        [TestMethod]
        public void Rules_Detects_Winner()
        {
            var board = new B1_MAN_Board(2);
            var rules = new B1_MAN_Rules(board);
            int player = 1;
            int baseHome = board.HomeBaseForPlayer(player);
            // put all pieces in home
            for (int i = 0; i < 4; i++)
            {
                var pc = board.GetPlayerPiece(player, i);
                board.PlacePiece(pc, baseHome + i);
            }

            Assert.AreEqual(player, rules.CheckIfPLayerWon());
        }
    }
}
