using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    /// <summary>
    /// Repräsentiert ein bereits platziertes Teil in einem Level
    /// </summary>
    public class A3_LEA_PlacedPiece
    {
        public int PieceId { get; set; }      // 1-12
        public int X { get; set; }             // X-Position auf dem Feld
        public int Y { get; set; }             // Y-Position auf dem Feld
        public int Rotation { get; set; }      // 0, 1, 2, 3 (entspricht 0°, 90°, 180°, 270°)
        public bool IsFlipped { get; set; }    // Geflippt oder nicht

        public A3_LEA_PlacedPiece(int pieceId, int x, int y, int rotation = 0, bool isFlipped = false)
        {
            PieceId = pieceId;
            X = x;
            Y = y;
            Rotation = rotation;
            IsFlipped = isFlipped;
        }
    }

    /// <summary>
    /// Definiert ein vordefiniertes Level/Aufgabe für das IQ Puzzle Spiel
    /// </summary>
    public class A3_LEA_IQPuzzleLevel
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; }
        public List<A3_LEA_PlacedPiece> PrePlacedPieces { get; set; }
        public int[,] GridLayout { get; set; }  // Direkte Grid-Repräsentation

        public A3_LEA_IQPuzzleLevel(int levelNumber, string levelName)
        {
            LevelNumber = levelNumber;
            LevelName = levelName;
            PrePlacedPieces = new List<A3_LEA_PlacedPiece>();
            GridLayout = new int[11, 5]; // 11 columns, 5 rows
        }

        /// <summary>
        /// Setzt das Grid-Layout aus einem 2D-Array
        /// </summary>
        public void SetGridLayout(int[,] layout)
        {
            GridLayout = layout;
            AnalyzeGridToPieces();
        }

        /// <summary>
        /// Analysiert das Grid und extrahiert die platzierten Teile
        /// </summary>
        private void AnalyzeGridToPieces()
        {
            PrePlacedPieces.Clear();
            HashSet<int> processedPieces = new HashSet<int>();

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    int pieceId = GridLayout[x, y];
                    if (pieceId > 0 && !processedPieces.Contains(pieceId))
                    {
                        processedPieces.Add(pieceId);
                        // Finde die Position des ersten Vorkommens dieses Teils
                        PrePlacedPieces.Add(new A3_LEA_PlacedPiece(pieceId, x, y, 0, false));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sammlung aller vordefinierter Level für das IQ Puzzle
    /// </summary>
    public static class A3_LEA_IQPuzzleLevels
    {
        public static List<A3_LEA_IQPuzzleLevel> AllLevels { get; private set; }

        static A3_LEA_IQPuzzleLevels()
        {
            AllLevels = new List<A3_LEA_IQPuzzleLevel>();
            InitializeLevels();
        }

        private static void InitializeLevels()
        {
            // Level 1: Starter
            var level1 = new A3_LEA_IQPuzzleLevel(1, "Starter");
            level1.SetGridLayout(new int[,]
            {
                // Spalte 0-4 (Rows 0-4)
                { 9, 9, 12, 12, 11 },      // Column 0
                { 9, 8, 12, 12, 11 },      // Column 1
                { 2, 8, 11, 11, 11 },      // Column 2
                { 2, 8, 8, 10, 10 },       // Column 3
                { 2, 8, 1, 1, 10 },        // Column 4
                { 2, 0, 0, 1, 10 },        // Column 5
                { 0, 0, 0, 1, 10 },        // Column 6
                { 0, 0, 0, 0, 0 },         // Column 7
                { 0, 0, 0, 0, 0 },         // Column 8
                { 0, 0, 0, 0, 0 },         // Column 9
                { 0, 0, 0, 0, 0 }          // Column 10
            });
            AllLevels.Add(level1);

            // Weitere Level können hier hinzugefügt werden
            // Level 2, 3, 4, etc.

            // Level 51: Challenge (added similar to Level 1)
            var level51 = new A3_LEA_IQPuzzleLevel(51, "Challenge");
            level51.SetGridLayout(new int[,]
            {
                { 10, 10, 11, 12, 12 },   // Column 0
                { 10, 2, 11, 12, 12 },  // Column 1
                { 10, 2, 11, 11, 11 },    // Column 2
                { 10, 2, 4, 4, 4 },    // Column 3
                { 0, 2, 4, 0, 4 },     // Column 4
                { 0, 0, 0, 0, 0 },     // Column 5
                { 0, 0, 0, 0, 0 },     // Column 6
                { 0, 0, 0, 0, 0 },     // Column 7
                { 0, 0, 0, 0, 0 },     // Column 8
                { 0, 0, 0, 0, 0 },     // Column 9
                { 0, 0, 0, 0, 0 }      // Column 10
            });
            AllLevels.Add(level51);
        }

        /// <summary>
        /// Gibt ein spezifisches Level zurück
        /// </summary>
        public static A3_LEA_IQPuzzleLevel GetLevel(int levelNumber)
        {
            return AllLevels.FirstOrDefault(l => l.LevelNumber == levelNumber);
        }

        /// <summary>
        /// Gibt das erste Level zurück (Standard-Start-Level)
        /// </summary>
        public static A3_LEA_IQPuzzleLevel GetStartLevel()
        {
            return AllLevels.FirstOrDefault();
        }
    }
}
