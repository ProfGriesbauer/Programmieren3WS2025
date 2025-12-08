using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // ============= SHARED DATA MODEL =============
    
    /// <summary>
    /// Repräsentiert ein Schiff im Schiffe-Versenken-Spiel
    /// </summary>
    public class A3_LEA_Ship
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsHorizontal { get; set; }
        public bool[] HitCells { get; set; }
        public int Hits { get { return HitCells.Count(h => h); } }
        
        public A3_LEA_Ship(int id, int size) 
        { 
            Id = id; 
            Size = size; 
            HitCells = new bool[size];
        }
    }

    // ============= ABSTRACT BASE CLASSES =============
    
    // Abstrakte Basis-Rules
    public abstract class A3_LEA_BaseSchiffeRules : IA3_LEA_SchiffeRules
    {
        public abstract IA3_LEA_SchiffeField SchiffeField { get; }
        public abstract List<A3_LEA_Ship> Ships { get; }
        public abstract bool CanPlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal);
        public abstract void PlaceShip(A3_LEA_Ship ship, int x, int y, bool horizontal);
        public abstract bool ShootAt(int x, int y);
        public abstract IGameField CurrentField { get; }
        public abstract bool MovesPossible { get; }
        public abstract string Name { get; }
        public abstract int CheckIfPLayerWon();
        public abstract void ClearField();
        public abstract void DoMove(IPlayMove move);
        
        // IGameRules2 methods
        public abstract void StartedGameCall();
        public abstract void TickGameCall();
    }

    // Abstrakte Basis-Painter
    public abstract class A3_LEA_BaseSchiffePaint : IA3_LEA_SchiffePaint
    {
        public abstract string Name { get; }
        public abstract void PaintSchiffeField(System.Windows.Controls.Canvas canvas, IA3_LEA_SchiffeField field, List<A3_LEA_Ship> ships);
        public abstract void PaintGameField(System.Windows.Controls.Canvas canvas, IGameField currentField);
        
        // IPaintGame2 method
        public abstract void TickPaintGameField(System.Windows.Controls.Canvas canvas, IGameField currentField);
    }

    // Abstrakte Basis-Human Player
    public abstract class A3_LEA_BaseHumanSchiffePlayer : IA3_LEA_HumanSchiffePlayer
    {
        public abstract string Name { get; }
        public abstract int PlayerNumber { get; }
        public abstract void SetPlayerNumber(int playerNumber);
        public abstract IGamePlayer Clone();
        public abstract IA3_LEA_SchiffeMove GetMove(IMoveSelection selection, IA3_LEA_SchiffeField field);

        public bool CanBeRuledBy(IGameRules rules) => rules is IA3_LEA_SchiffeRules;
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is IA3_LEA_SchiffeField)
                return GetMove(selection, (IA3_LEA_SchiffeField)field);
            return null;
        }
        
        // IHumanGamePlayerWithMouse method
        public abstract void OnMouseMoved(System.Windows.Input.MouseEventArgs e);
    }

    // Abstrakte Basis-Computer Player
    public abstract class A3_LEA_BaseComputerSchiffePlayer : IA3_LEA_ComputerSchiffePlayer
    {
        public abstract string Name { get; }
        public abstract int PlayerNumber { get; }
        public abstract void SetPlayerNumber(int playerNumber);
        public abstract IGamePlayer Clone();
        public abstract IA3_LEA_SchiffeMove GetMove(IA3_LEA_SchiffeField field);

        public bool CanBeRuledBy(IGameRules rules) => rules is IA3_LEA_SchiffeRules;
        public IPlayMove GetMove(IGameField field)
        {
            if (field is IA3_LEA_SchiffeField)
                return GetMove((IA3_LEA_SchiffeField)field);
            return null;
        }
    }

    // Abstrakte Basis-Move
    public abstract class A3_LEA_BaseSchiffeMove : IA3_LEA_SchiffeMove
    {
        public abstract int X { get; }
        public abstract int Y { get; }
        public abstract int PlayerNumber { get; }
        public abstract MoveType MoveType { get; }

        // IRowMove Implementation
        public int Row => Y;

        // IColumnMove Implementation
        public int Column => X;
    }
}
