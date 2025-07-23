using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    //Any game painting on the canvas needs to implement this interface
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IPaintGame
    {
        //Name of the Game Painter: possibly use a unique name
        string Name { get; }

        //Paints the given game field on the given canvas
        //Called whenever a game player has finished a move.
        //NOTE: Clearing the canvas, etc. has to be done within this function
        void PaintGameField(Canvas canvas, IGameField currentField);
    }

    //Extension of painting interface to support ticked (every 40ms) painting
    //Implement this interface to do i.e. animations, etc.
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IPaintGame2 : IPaintGame
    {
        //Paints the given game field on the given canvas
        //Called every 40ms.
        //NOTE: Clearing the canvas, etc. has to be done within this function
        void TickPaintGameField(Canvas canvas, IGameField currentField);
    }

    //Type of available move inputs
    public enum MoveType { click, key};

    //Any game requesting a move selection gets the corresponding object
    //for clicks or keys (see below) via this interface
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IMoveSelection
    {
        MoveType MoveType { get; }
    }

    //Any game requesting mouse click positions from the canvas get the latter
    //via this interface
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IClickSelection : IMoveSelection 
    {
        //X position of the mouse click
        int XClickPos { get; }

        //Y Position of the mouse click
        int YClickPos { get; }

        //Pressed/Changed Mouse Button
        int ChangedButton { get; }  
    }

    //Any game requesting pressed keys from the canvas get the latter
    //via this interface
    //DIESES INTERFACE NICHT ÄNDERN!
    public interface IKeySelection : IMoveSelection
    {
        //Pressed Key on Keyboard
        Key Key { get; }
    }
}
