using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    //TicTacToe specific paint game
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_PaintTicTacToe : IPaintGame
    {
        //Paints the given game field on the given canvas
        //NOTE: Clearing the canvas, etc. has to be done within this function
        void PaintTicTacToeField(Canvas canvas, IX_TicTacToeField currentField);
    }

    //TicTacToe specific game field 3x3
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_TicTacToeField : IGameField
    {
        //Indexer: returns 0 for a unused tictactoefield, 1 for player 1, 2 for player 2, etc.
        //indexed by the row r and column c
        int this[int r, int c] { get; set; }
    }

    //TicTacToe specific game rules
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_TicTacToeRules : IGameRules
    {
        //Gets the current state of the tictactoe field; the class implementing
        //this interface should hold a game field corresponding to the rules
        //it implements
        IX_TicTacToeField TicTacToeField { get; }

        //Adds the given move to the current tictactoe field if possible
        void DoTicTacToeMove(IX_TicTacToeMove move);
    }

    //TicTacToeMove which is derived from row and column
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_TicTacToeMove : IRowMove, IColumnMove
    {

    }

    //TicTacToe specific human player
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_HumanTicTacToePlayer : IHumanGamePlayer
    {
        //Returns a valid move if possible for the given selection and 
        //the given state of the tic tac toe field.
        //IF THE GIVEN SELECTION IS NO VALID MOVE, NULL HAS TO BE RETURNED.
        IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field);
    }

    //TicTacToe specific human player
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IX_ComputerTicTacToePlayer : IComputerGamePlayer
    {
        //Returns a valid move and the given state of the tic tac toe field.
        IX_TicTacToeMove GetMove(IX_TicTacToeField field);
    }
}
