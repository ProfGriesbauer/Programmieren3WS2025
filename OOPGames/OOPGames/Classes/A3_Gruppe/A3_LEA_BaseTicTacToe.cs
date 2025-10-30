using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    /**************************************************************************
     * BASE PAINTER - Abstrakte Basisklasse für TicTacToe Zeichnung
     **************************************************************************/
    public abstract class A3_LEA_BaseTicTacToePaint : IA3_LEA_PaintTicTacToe
    {
        public abstract string Name { get; }

        public abstract void PaintTicTacToeField(Canvas canvas, IA3_LEA_TicTacToeField currentField);

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is IA3_LEA_TicTacToeField)
            {
                PaintTicTacToeField(canvas, (IA3_LEA_TicTacToeField)currentField);
            }
        }
    }

    /**************************************************************************
     * BASE FIELD - Abstrakte Basisklasse für TicTacToe Spielfeld
     **************************************************************************/
    public abstract class A3_LEA_BaseTicTacToeField : IA3_LEA_TicTacToeField
    {
        public abstract int this[int r, int c] { get; set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IA3_LEA_PaintTicTacToe;
        }
    }

    /**************************************************************************
     * BASE RULES - Abstrakte Basisklasse für TicTacToe Spielregeln
     **************************************************************************/
    public abstract class A3_LEA_BaseTicTacToeRules : IA3_LEA_TicTacToeRules
    {
        public abstract IA3_LEA_TicTacToeField TicTacToeField { get; }

        public abstract bool MovesPossible { get; }

        public abstract string Name { get; }

        public abstract int CheckIfPLayerWon();

        public abstract void ClearField();

        public abstract void DoTicTacToeMove(IA3_LEA_TicTacToeMove move);

        public IGameField CurrentField { get { return TicTacToeField; } }

        public void DoMove(IPlayMove move)
        {
            if (move is IA3_LEA_TicTacToeMove)
            {
                DoTicTacToeMove((IA3_LEA_TicTacToeMove)move);
            }
        }
    }

    /**************************************************************************
     * BASE HUMAN PLAYER - Abstrakte Basisklasse für menschlichen Spieler
     **************************************************************************/
    public abstract class A3_LEA_BaseHumanTicTacToePlayer : IA3_LEA_HumanTicTacToePlayer
    {
        public abstract string Name { get; }

        public abstract int PlayerNumber { get; }

        public abstract IA3_LEA_TicTacToeMove GetMove(IMoveSelection selection, IA3_LEA_TicTacToeField field);

        public abstract void SetPlayerNumber(int playerNumber);

        public abstract IGamePlayer Clone();

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IA3_LEA_TicTacToeRules;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is IA3_LEA_TicTacToeField)
            {
                return GetMove(selection, (IA3_LEA_TicTacToeField)field);
            }
            else
            {
                return null;
            }
        }
    }

    /**************************************************************************
     * BASE COMPUTER PLAYER - Abstrakte Basisklasse für Computer-Spieler
     **************************************************************************/
    public abstract class A3_LEA_BaseComputerTicTacToePlayer : IA3_LEA_ComputerTicTacToePlayer
    {
        public abstract string Name { get; }

        public abstract int PlayerNumber { get; }

        public abstract void SetPlayerNumber(int playerNumber);

        public abstract IA3_LEA_TicTacToeMove GetMove(IA3_LEA_TicTacToeField field);

        public abstract IGamePlayer Clone();

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IA3_LEA_TicTacToeRules;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (field is IA3_LEA_TicTacToeField)
            {
                return GetMove((IA3_LEA_TicTacToeField)field);
            }
            else
            {
                return null;
            }
        }
    }
}
