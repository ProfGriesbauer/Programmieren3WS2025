using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    /**************************************************************************
     * BASE CLASSES FÜR IQ PUZZLER PRO
     **************************************************************************/

    /// <summary>
    /// Abstrakte Basisklasse für IQ Puzzle Painter
    /// </summary>
    public abstract class A3_LEA_BaseIQPuzzlePaint : IA3_LEA_PaintIQPuzzle
    {
        public abstract string Name { get; }

        public abstract void PaintIQPuzzleField(Canvas canvas, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces, IA3_LEA_IQPuzzlePiece selectedPiece);

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is IA3_LEA_IQPuzzleField)
            {
                var field = (IA3_LEA_IQPuzzleField)currentField;
                var availablePieces = new List<IA3_LEA_IQPuzzlePiece>();
                IA3_LEA_IQPuzzlePiece selectedPiece = null;

                // Hole verfügbare Teile und Auswahl von den aktiven Rules
                if (OOPGamesManager.Singleton.ActiveRules is IA3_LEA_IQPuzzleRules)
                {
                    var rules = (IA3_LEA_IQPuzzleRules)OOPGamesManager.Singleton.ActiveRules;
                    availablePieces = rules.AvailablePieces;
                    
                    // Hole ausgewähltes Teil (cast zu konkreter Klasse)
                    if (OOPGamesManager.Singleton.ActiveRules is A3_LEA_IQPuzzleRules)
                    {
                        selectedPiece = ((A3_LEA_IQPuzzleRules)OOPGamesManager.Singleton.ActiveRules).SelectedPieceForPainting;
                    }
                }

                PaintIQPuzzleField(canvas, field, availablePieces, selectedPiece);
            }
        }
    }

    /// <summary>
    /// Abstrakte Basisklasse für IQ Puzzle Field
    /// </summary>
    public abstract class A3_LEA_BaseIQPuzzleField : IA3_LEA_IQPuzzleField
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int this[int x, int y] { get; set; }

        public abstract bool IsValidPosition(int x, int y);
        public abstract bool IsFull();
        public abstract List<(int x, int y, int pieceId)> GetOccupiedCells();

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IA3_LEA_PaintIQPuzzle;
        }
    }

    /// <summary>
    /// Abstrakte Basisklasse für IQ Puzzle Rules
    /// </summary>
    public abstract class A3_LEA_BaseIQPuzzleRules : IA3_LEA_IQPuzzleRules
    {
        public abstract IA3_LEA_IQPuzzleField IQPuzzleField { get; }
        public abstract List<IA3_LEA_IQPuzzlePiece> AllPieces { get; }
        public abstract List<IA3_LEA_IQPuzzlePiece> PlacedPieces { get; }
        public abstract List<IA3_LEA_IQPuzzlePiece> AvailablePieces { get; }
        public abstract int DifficultyLevel { get; }

        public abstract bool MovesPossible { get; }
        public abstract string Name { get; }

        public abstract int CheckIfPLayerWon();
        public abstract void ClearField();

        public abstract bool CanPlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y);
        public abstract void PlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y);
        public abstract void RemovePiece(IA3_LEA_IQPuzzlePiece piece);
        public abstract void DoIQPuzzleMove(IA3_LEA_IQPuzzleMove move);
        public abstract IA3_LEA_IQPuzzleMove GetHint();
        public abstract void LoadChallenge(int challengeNumber);
        public abstract bool SolvePuzzle();

        public IGameField CurrentField { get { return IQPuzzleField; } }

        public void DoMove(IPlayMove move)
        {
            if (move is IA3_LEA_IQPuzzleMove)
            {
                DoIQPuzzleMove((IA3_LEA_IQPuzzleMove)move);
            }
        }
    }

    /// <summary>
    /// Abstrakte Basisklasse für menschlichen IQ Puzzle Spieler
    /// </summary>
    public abstract class A3_LEA_BaseHumanIQPuzzlePlayer : IA3_LEA_HumanIQPuzzlePlayer
    {
        public abstract string Name { get; }
        public abstract int PlayerNumber { get; }
        public abstract IA3_LEA_IQPuzzlePiece SelectedPiece { get; set; }

        public abstract void SetPlayerNumber(int playerNumber);
        public abstract IGamePlayer Clone();

        public abstract IA3_LEA_IQPuzzleMove GetMove(IMoveSelection selection, IA3_LEA_IQPuzzleField field,
            List<IA3_LEA_IQPuzzlePiece> availablePieces);

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IA3_LEA_IQPuzzleRules;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is IA3_LEA_IQPuzzleField && OOPGamesManager.Singleton.ActiveRules is IA3_LEA_IQPuzzleRules)
            {
                var rules = (IA3_LEA_IQPuzzleRules)OOPGamesManager.Singleton.ActiveRules;
                return GetMove(selection, (IA3_LEA_IQPuzzleField)field, rules.AvailablePieces);
            }
            return null;
        }
    }
}
