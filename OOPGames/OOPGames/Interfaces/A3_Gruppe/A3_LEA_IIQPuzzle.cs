using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPGames
{
    /**************************************************************************
     * INTERFACES FÜR IQ PUZZLER PRO
     **************************************************************************/

    /// <summary>
    /// Interface für ein IQ Puzzle Stück (Polyomino)
    /// </summary>
    public interface IA3_LEA_IQPuzzlePiece
    {
        /// <summary>
        /// Eindeutige ID des Puzzle-Stücks (1-12)
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Name des Stücks
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Farbe des Stücks
        /// </summary>
        System.Windows.Media.Color Color { get; }

        /// <summary>
        /// Form als 2D-Array (1 = belegt, 0 = leer)
        /// </summary>
        int[,] Shape { get; }

        /// <summary>
        /// Breite der aktuellen Form
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Höhe der aktuellen Form
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Rotiert das Stück um 90° im Uhrzeigersinn
        /// </summary>
        IA3_LEA_IQPuzzlePiece Rotate();

        /// <summary>
        /// Spiegelt das Stück horizontal
        /// </summary>
        IA3_LEA_IQPuzzlePiece Flip();

        /// <summary>
        /// Erstellt eine Kopie des Stücks
        /// </summary>
        IA3_LEA_IQPuzzlePiece Clone();
    }

    /// <summary>
    /// Interface für das IQ Puzzle Spielfeld
    /// </summary>
    public interface IA3_LEA_IQPuzzleField : IGameField
    {
        /// <summary>
        /// Breite des Spielfelds (normalerweise 11)
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Höhe des Spielfelds (normalerweise 5)
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Zugriff auf Feld-Zelle (0 = leer, 1-12 = Piece-ID)
        /// </summary>
        int this[int x, int y] { get; set; }

        /// <summary>
        /// Prüft ob eine Position gültig ist
        /// </summary>
        bool IsValidPosition(int x, int y);

        /// <summary>
        /// Prüft ob das Spielfeld komplett gefüllt ist
        /// </summary>
        bool IsFull();

        /// <summary>
        /// Gibt alle belegten Positionen zurück
        /// </summary>
        List<(int x, int y, int pieceId)> GetOccupiedCells();
    }

    /// <summary>
    /// Interface für einen IQ Puzzle Zug
    /// </summary>
    public interface IA3_LEA_IQPuzzleMove : IPlayMove
    {
        /// <summary>
        /// Das zu platzierende Stück
        /// </summary>
        IA3_LEA_IQPuzzlePiece Piece { get; }

        /// <summary>
        /// X-Position auf dem Spielfeld
        /// </summary>
        int X { get; }

        /// <summary>
        /// Y-Position auf dem Spielfeld
        /// </summary>
        int Y { get; }

        /// <summary>
        /// Gibt an, ob das Stück entfernt wird (statt platziert)
        /// </summary>
        bool IsRemove { get; }
    }

    /// <summary>
    /// Interface für IQ Puzzle Spielregeln
    /// </summary>
    public interface IA3_LEA_IQPuzzleRules : IGameRules
    {
        /// <summary>
        /// Das aktuelle Spielfeld
        /// </summary>
        IA3_LEA_IQPuzzleField IQPuzzleField { get; }

        /// <summary>
        /// Alle verfügbaren Puzzle-Stücke
        /// </summary>
        List<IA3_LEA_IQPuzzlePiece> AllPieces { get; }

        /// <summary>
        /// Bereits platzierte Stücke
        /// </summary>
        List<IA3_LEA_IQPuzzlePiece> PlacedPieces { get; }

        /// <summary>
        /// Noch verfügbare Stücke
        /// </summary>
        List<IA3_LEA_IQPuzzlePiece> AvailablePieces { get; }

        /// <summary>
        /// Aktueller Schwierigkeitsgrad
        /// </summary>
        int DifficultyLevel { get; }

        /// <summary>
        /// Prüft ob ein Stück an einer Position platziert werden kann
        /// </summary>
        bool CanPlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y);

        /// <summary>
        /// Platziert ein Stück auf dem Spielfeld
        /// </summary>
        void PlacePiece(IA3_LEA_IQPuzzlePiece piece, int x, int y);

        /// <summary>
        /// Entfernt ein Stück vom Spielfeld
        /// </summary>
        void RemovePiece(IA3_LEA_IQPuzzlePiece piece);

        /// <summary>
        /// Führt einen IQ Puzzle Zug aus
        /// </summary>
        void DoIQPuzzleMove(IA3_LEA_IQPuzzleMove move);

        /// <summary>
        /// Gibt einen Hinweis für den nächsten Zug
        /// </summary>
        IA3_LEA_IQPuzzleMove GetHint();

        /// <summary>
        /// Lädt eine vordefinierte Challenge
        /// </summary>
        void LoadChallenge(int challengeNumber);
    }

    /// <summary>
    /// Interface für IQ Puzzle Zeichnung
    /// </summary>
    public interface IA3_LEA_PaintIQPuzzle : IPaintGame
    {
        /// <summary>
        /// Zeichnet das IQ Puzzle Spielfeld
        /// </summary>
        void PaintIQPuzzleField(System.Windows.Controls.Canvas canvas, IA3_LEA_IQPuzzleField field, 
            List<IA3_LEA_IQPuzzlePiece> availablePieces, IA3_LEA_IQPuzzlePiece selectedPiece);
    }

    /// <summary>
    /// Interface für menschlichen IQ Puzzle Spieler
    /// </summary>
    public interface IA3_LEA_HumanIQPuzzlePlayer : IHumanGamePlayer
    {
        /// <summary>
        /// Aktuell ausgewähltes Stück
        /// </summary>
        IA3_LEA_IQPuzzlePiece SelectedPiece { get; set; }

        /// <summary>
        /// Ermittelt den Zug basierend auf Mauseingabe
        /// </summary>
        IA3_LEA_IQPuzzleMove GetMove(IMoveSelection selection, IA3_LEA_IQPuzzleField field, 
            List<IA3_LEA_IQPuzzlePiece> availablePieces);
    }

    /// <summary>
    /// Interface für Click Selection im IQ Puzzle
    /// </summary>
    public interface IA3_LEA_ClickSelection : IClickSelection
    {
        // Erbt alle Members von IClickSelection
    }

    /// <summary>
    /// Schwierigkeitsgrade für IQ Puzzle
    /// </summary>
    public enum IQPuzzleDifficulty
    {
        Starter = 1,
        Junior = 2,
        Expert = 3,
        Master = 4,
        Wizard = 5
    }
}
