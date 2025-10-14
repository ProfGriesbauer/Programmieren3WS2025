using System;

// Simple self-contained console TicTacToe
// Save as RunTicTacToe.cs and compile with csc or copy into a console project.

class RunTicTacToe
{
    static int[,] board = new int[3,3];
    static int current = 1; // 1=X, 2=O

    static void Main(string[] args)
    {
        Console.Title = "Sebastian's TicTacToe (Console)";
        Reset();
        while (true)
        {
            Console.Clear();
            DrawBoard();
            if (CheckWinner(out int winner))
            {
                Console.WriteLine(winner == 1 ? "Player X wins!" : "Player O wins!");
                if (!AskRestart()) break;
                continue;
            }
            if (IsDraw())
            {
                Console.WriteLine("Draw!");
                if (!AskRestart()) break;
                continue;
            }

            Console.WriteLine($"Player {(current==1? 'X':'O')}'s turn. Enter row and column (e.g. 1 2):");
            Console.Write(" > ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(new[]{' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 && parts[0].ToLower() == "q") break;
            if (parts.Length < 2) { Console.WriteLine("Please enter two numbers."); ContinuePrompt(); continue; }
            if (int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
            {
                r--; c--; // user inputs 1-based
                if (r < 0 || r > 2 || c < 0 || c > 2) { Console.WriteLine("Coordinates out of range (1-3)."); ContinuePrompt(); continue; }
                if (board[r,c] != 0) { Console.WriteLine("Cell already occupied."); ContinuePrompt(); continue; }
                board[r,c] = current;
                current = 3 - current;
            }
            else
            {
                Console.WriteLine("Invalid input."); ContinuePrompt();
            }
        }
    }

    static void Reset()
    {
        board = new int[3,3];
        current = 1;
    }

    static void DrawBoard()
    {
        Console.WriteLine("   1   2   3");
        for (int r = 0; r < 3; r++)
        {
            Console.Write($"{r+1} ");
            for (int c = 0; c < 3; c++)
            {
                char ch = board[r,c] == 1 ? 'X' : board[r,c] == 2 ? 'O' : ' ';
                Console.Write($" {ch} ");
                if (c < 2) Console.Write("|");
            }
            Console.WriteLine();
            if (r < 2) Console.WriteLine("  ---+---+---");
        }
    }

    static bool CheckWinner(out int winner)
    {
        // rows and cols
        for (int i = 0; i < 3; i++)
        {
            if (board[i,0] != 0 && board[i,0] == board[i,1] && board[i,1] == board[i,2]) { winner = board[i,0]; return true; }
            if (board[0,i] != 0 && board[0,i] == board[1,i] && board[1,i] == board[2,i]) { winner = board[0,i]; return true; }
        }
        if (board[0,0] != 0 && board[0,0] == board[1,1] && board[1,1] == board[2,2]) { winner = board[0,0]; return true; }
        if (board[0,2] != 0 && board[0,2] == board[1,1] && board[1,1] == board[2,0]) { winner = board[0,2]; return true; }
        winner = 0; return false;
    }

    static bool IsDraw()
    {
        for (int r = 0; r < 3; r++) for (int c = 0; c < 3; c++) if (board[r,c] == 0) return false;
        return true;
    }

    static bool AskRestart()
    {
        Console.Write("Play again? (y/n): ");
        var k = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(k) && (k[0]=='y' || k[0]=='Y')) { Reset(); return true; }
        return false;
    }

    static void ContinuePrompt() { Console.WriteLine("Press Enter to continue..."); Console.ReadLine(); }
}
