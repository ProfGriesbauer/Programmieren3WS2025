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

    /// <summary>
    /// Abstrakte Basis für menschliche Labyrinth-Spieler
    /// </summary>
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

    /// <summary>
    /// Menschlicher Spieler - Steuerung mit Pfeiltasten
    /// </summary>
    public class B2_MazeHumanPlayer : B2_AbstractMazeHumanPlayer
    {
        public B2_MazeHumanPlayer(string name = "B2 - Maze Player")
        {
            Name = name;
        }

        public override IGamePlayer Clone()
        {
            return new B2_MazeHumanPlayer(Name);
        }

        public override IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            // Prüfe ob es eine Tastatur-Eingabe ist
            if (selection is IKeySelection keySelection)
            {
                B2_MazeDirection? direction = null;

                // Map Tasten zu Richtungen
                switch (keySelection.Key)
                {
                    case Key.Up:
                    case Key.W:
                        direction = B2_MazeDirection.Up;
                        break;
                    case Key.Down:
                    case Key.S:
                        direction = B2_MazeDirection.Down;
                        break;
                    case Key.Left:
                    case Key.A:
                        direction = B2_MazeDirection.Left;
                        break;
                    case Key.Right:
                    case Key.D:
                        direction = B2_MazeDirection.Right;
                        break;
                }

                if (direction.HasValue)
                {
                    return new B2_MazeMove(PlayerNumber, direction.Value);
                }
            }

            return null;
        }
    }

    #endregion
}
