using System.Collections.Generic;

namespace OOPGames
{
    // Interface für das Schiffeversenken-Spielfeld
    public interface IA3_LEA_SchiffeField : IGameField
    {
        int Width { get; }
        int Height { get; }
        int this[int x, int y] { get; set; }
        bool IsValidPosition(int x, int y);
        List<(int x, int y, int shipId)> GetOccupiedCells();
    }

    // Interface für die Rules
    public interface IA3_LEA_SchiffeRules : IGameRules
    {
        IA3_LEA_SchiffeField SchiffeField { get; }
        List<A3_LEA_Ship> Ships { get; }
        bool CanPlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal);
        void PlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal);
        bool ShootAt(int x, int y);
    }

    // Interface für Painter
    public interface IA3_LEA_SchiffePaint : IPaintGame
    {
        void PaintSchiffeField(System.Windows.Controls.Canvas canvas, IA3_LEA_SchiffeField field, List<A3_LEA_Ship> ships);
    }

    // Interface für den Human Player
    public interface IA3_LEA_HumanSchiffePlayer : IHumanGamePlayer
    {
        IA3_LEA_SchiffeMove GetMove(IMoveSelection selection, IA3_LEA_SchiffeField field);
    }

    // Interface für den Computer Player
    public interface IA3_LEA_ComputerSchiffePlayer : IComputerGamePlayer
    {
        IA3_LEA_SchiffeMove GetMove(IA3_LEA_SchiffeField field);
    }

    // Interface für einen Zug
    public interface IA3_LEA_SchiffeMove : IPlayMove
    {
        int X { get; }
        int Y { get; }
    }
}
