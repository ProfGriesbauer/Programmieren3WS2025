using Microsoft.VisualStudio.TestTools.UnitTesting;
using OOPGames.B1_Gruppe;

namespace OOPGames.Tests
{
    [TestClass]
    public class B1BoardTests
    {
        [TestMethod]
        public void EmptyBoard_HasNoWinner()
        {
            var b = new B1_Board();
            Assert.IsFalse(b.IsFull());
            Assert.IsNull(b.CheckWinner());
        }

        [TestMethod]
        public void RowWinner_Detected()
        {
            var b = new B1_Board();
            b.MakeMove(1, 0, new B1_Cross());
            b.MakeMove(1, 1, new B1_Cross());
            b.MakeMove(1, 2, new B1_Cross());
            var winner = b.CheckWinner();
            Assert.IsNotNull(winner);
            Assert.IsInstanceOfType(winner, typeof(B1_Cross));
        }

        [TestMethod]
        public void DiagonalWinner_Detected()
        {
            var b = new B1_Board();
            b.MakeMove(0, 0, new B1_Circle());
            b.MakeMove(1, 1, new B1_Circle());
            b.MakeMove(2, 2, new B1_Circle());
            var winner = b.CheckWinner();
            Assert.IsNotNull(winner);
            Assert.IsInstanceOfType(winner, typeof(B1_Circle));
        }
    }
}
