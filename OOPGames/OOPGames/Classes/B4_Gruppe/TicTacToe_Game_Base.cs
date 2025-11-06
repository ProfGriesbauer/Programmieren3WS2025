using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGamess
{
    public abstract class B4_BaseTicTacToePaint : IB4_PaintTicTacToe
    {
        public abstract string Name { get; }

        public abstract void PaintTicTacToeField(Canvas canvas, IB4_TicTacToeField currentField);

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is IB4_TicTacToeField)
            {
                PaintTicTacToeField(canvas, (IB4_TicTacToeField)currentField);
            }
        }
    }

    public abstract class B4_BaseTicTacToeField : IB4_TicTacToeField
    {
        public abstract int this[int r, int c] { get; set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IB4_PaintTicTacToe;
        }
    }

    public abstract class B4_BaseTicTacToeRules : IB4_TicTacToeRules
    {
        public abstract IB4_TicTacToeField TicTacToeField { get; }

        public abstract bool MovesPossible { get; }

        public abstract string Name { get; }

        public abstract int CheckIfPLayerWon();

        public abstract void ClearField();

        public abstract void DoTicTacToeMove(IB4_TicTacToeMove move);

        public IGameField CurrentField { get { return TicTacToeField; } }

        public void DoMove(IPlayMove move)
        {
            if (move is IB4_TicTacToeMove)
            {
                DoTicTacToeMove((IB4_TicTacToeMove)move);
            }
        }
    }

    public abstract class B4_BaseHumanTicTacToePlayer : IB4_HumanTicTacToePlayer
    {
        public abstract string Name { get; }

        public abstract int PlayerNumber { get; }

        public abstract IB4_TicTacToeMove GetMove(IMoveSelection selection, IB4_TicTacToeField field);

        public abstract void SetPlayerNumber(int playerNumber);

        public abstract IGamePlayer Clone();

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IB4_TicTacToeRules;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is IB4_TicTacToeField)
            {
                return GetMove(selection, (IB4_TicTacToeField)field);
            }
            else
            {
                return null;
            }
        }
    }

    public abstract class B4_BaseComputerTicTacToePlayer : IB4_ComputerTicTacToePlayer
    {
        public abstract string Name { get; }

        public abstract int PlayerNumber { get; }

        public abstract void SetPlayerNumber(int playerNumber);

        public abstract IB4_TicTacToeMove GetMove(IB4_TicTacToeField field);

        public abstract IGamePlayer Clone();

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IB4_TicTacToeRules;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is IB4_TicTacToeField)
            {
                return GetMove((IB4_TicTacToeField)field);
            }
            else
            {
                return null;
            }
        }
    }
}
