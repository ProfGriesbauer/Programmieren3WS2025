using System;

namespace OOPGames
{
    // A4-specific click selection: includes canvas dimensions so A4 players can map clicks correctly
    public class A4_ClickSelection : IClickSelection
    {
        int _ClickX;
        int _ClickY;
        int _ChangedButton;
        int _CanvasWidth;
        int _CanvasHeight;

        public A4_ClickSelection(int clickX, int clickY, int ChangedButton, int canvasWidth, int canvasHeight)
        {
            _ClickX = clickX;
            _ClickY = clickY;
            _ChangedButton = ChangedButton;
            _CanvasWidth = canvasWidth;
            _CanvasHeight = canvasHeight;
        }

        public int XClickPos { get { return _ClickX; } }

        public int YClickPos { get { return _ClickY; } }

        public MoveType MoveType { get { return MoveType.click; } }

        public int ChangedButton { get { return _ChangedButton; } }

        public int CanvasWidth { get { return _CanvasWidth; } }

        public int CanvasHeight { get { return _CanvasHeight; } }
    }
}
