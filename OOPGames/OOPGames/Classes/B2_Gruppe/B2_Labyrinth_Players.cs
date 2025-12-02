using System;
using System.Windows.Input;

namespace OOPGames
{
    /******************************************************************************
     * B2 Labyrinth Game - Player Components
     * 
     * Spieler-Implementierungen für Labyrinth-Navigation
     ******************************************************************************/

    #region Abstract Base Classes

    
    /// Abstrakte Basis für menschliche Labyrinth-Spieler
    
    public abstract class B2_AbstractMazeHumanPlayer : IHumanGamePlayer
    {
        public virtual string Name { get; protected set; }
        public virtual int PlayerNumber { get; protected set; }

        public virtual void SetPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

        public virtual bool CanBeRuledBy(IGameRules rules) => rules is B2_AbstractMazeRules;

        public abstract IGamePlayer Clone();

        public abstract IPlayMove GetMove(IMoveSelection selection, IGameField field);
    }

    #endregion

    #region Concrete Implementations

    
    /// Dual-Controller Player - Steuert BEIDE Spieler gleichzeitig
    /// WASD = Spieler 1 (Blau), Pfeiltasten = Spieler 2 (Rot)
    
    public class B2_MazeDualPlayer : B2_AbstractMazeHumanPlayer
    {
        public B2_MazeDualPlayer(string name = "B2 - Maze Dual Controller")
        {
            Name = name;
        }

        public override IGamePlayer Clone()
        {
            return new B2_MazeDualPlayer(Name);
        }

        public override IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection is IKeySelection keySelection)
            {
                B2_MazeDirection direction = null;
                int targetPlayer = 0;

                // WASD steuert Spieler 1
                switch (keySelection.Key)
                {
                    case Key.W:
                        direction = B2_MazeDirection.Up;
                        targetPlayer = 1;
                        break;
                    case Key.S:
                        direction = B2_MazeDirection.Down;
                        targetPlayer = 1;
                        break;
                    case Key.A:
                        direction = B2_MazeDirection.Left;
                        targetPlayer = 1;
                        break;
                    case Key.D:
                        direction = B2_MazeDirection.Right;
                        targetPlayer = 1;
                        break;

                    // Pfeiltasten steuern Spieler 2
                    case Key.Up:
                        direction = B2_MazeDirection.Up;
                        targetPlayer = 2;
                        break;
                    case Key.Down:
                        direction = B2_MazeDirection.Down;
                        targetPlayer = 2;
                        break;
                    case Key.Left:
                        direction = B2_MazeDirection.Left;
                        targetPlayer = 2;
                        break;
                    case Key.Right:
                        direction = B2_MazeDirection.Right;
                        targetPlayer = 2;
                        break;
                }

                if (direction != null && targetPlayer > 0)
                {
                    // Gebe Move für den korrekten Spieler zurück
                    return new B2_MazeMove(targetPlayer, direction);
                }
            }

            return null;
        }
    }

    #endregion
}
