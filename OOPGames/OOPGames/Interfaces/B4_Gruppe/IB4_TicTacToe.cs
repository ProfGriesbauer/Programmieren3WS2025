using System;
using System.Windows.Controls;

namespace OOPGames
{
    public interface IB4_TicTacToeField : IGameField
    {
        int this[int r, int c] { get; set; }
    }

    public interface IB4_TicTacToeMove : IPlayMove
    {
        int Row { get; }
        int Column { get; }
        new int PlayerNumber { get; }
    }

    public interface IB4_TicTacToeRules : IGameRules
    {
        IB4_TicTacToeField TicTacToeField { get; }
        void DoTicTacToeMove(IB4_TicTacToeMove move);
    }

    public interface IB4_PaintTicTacToe : IPaintGame
    {
        void PaintTicTacToeField(Canvas canvas, IB4_TicTacToeField field);
    }

    public interface IB4_HumanTicTacToePlayer : IHumanGamePlayer
    {
        IB4_TicTacToeMove GetMove(IMoveSelection selection, IB4_TicTacToeField field);
    }

    public interface IB4_ComputerTicTacToePlayer : IComputerGamePlayer
    {
        IB4_TicTacToeMove GetMove(IB4_TicTacToeField field);
    }
}