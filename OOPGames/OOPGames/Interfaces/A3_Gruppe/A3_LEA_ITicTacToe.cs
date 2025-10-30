using System;
using System.Windows.Controls;

namespace OOPGames
{
    /**************************************************************************
     * A3 LEA TicTacToe Interfaces - Gruppenspezifische Interfaces
     * Diese Interfaces sind spezifisch für die A3 LEA Implementierung
     **************************************************************************/

    /// <summary>
    /// Painter-Interface für TicTacToe-Spielfelder (A3 LEA)
    /// </summary>
    public interface IA3_LEA_PaintTicTacToe : IPaintGame
    {
        void PaintTicTacToeField(Canvas canvas, IA3_LEA_TicTacToeField currentField);
    }

    /// <summary>
    /// 3x3 TicTacToe-Spielfeld mit Indexer-Zugriff (A3 LEA)
    /// </summary>
    public interface IA3_LEA_TicTacToeField : IGameField
    {
        int this[int r, int c] { get; set; }
    }

    /// <summary>
    /// Ein TicTacToe-Spielzug mit Zeile und Spalte (A3 LEA)
    /// </summary>
    public interface IA3_LEA_TicTacToeMove : IRowMove, IColumnMove
    {
        // Row und Column werden von IRowMove und IColumnMove geerbt
    }

    /// <summary>
    /// Spielregeln für TicTacToe (A3 LEA)
    /// </summary>
    public interface IA3_LEA_TicTacToeRules : IGameRules
    {
        IA3_LEA_TicTacToeField TicTacToeField { get; }
        void DoTicTacToeMove(IA3_LEA_TicTacToeMove move);
    }

    /// <summary>
    /// Menschlicher TicTacToe-Spieler (A3 LEA)
    /// </summary>
    public interface IA3_LEA_HumanTicTacToePlayer : IHumanGamePlayer
    {
        IA3_LEA_TicTacToeMove GetMove(IMoveSelection selection, IA3_LEA_TicTacToeField field);
    }

    /// <summary>
    /// Computer TicTacToe-Spieler (A3 LEA)
    /// </summary>
    public interface IA3_LEA_ComputerTicTacToePlayer : IComputerGamePlayer
    {
        IA3_LEA_TicTacToeMove GetMove(IA3_LEA_TicTacToeField field);
    }
}
