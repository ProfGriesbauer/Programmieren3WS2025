using System;

namespace OOPGames
{
    // Menschlicher Spieler für B3 TicTacToe
    public class B3_Mika_Roeder_HumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber = 0;

        public string Name { get { return "B3 Mika Röder Human"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is B3_Mika_Roeder_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B3_Mika_Roeder_HumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || field == null) return null;

            if (!(field is B3_Mika_Roeder_Field)) return null;

            var bf = (B3_Mika_Roeder_Field)field;

            if (selection is IClickSelection click)
            {
                // Verwende die gleichen Werte wie im Painter
                double cellSize = 120.0;
                double offset = 20.0;

                int px = click.XClickPos;
                int py = click.YClickPos;

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        double xMin = offset + (col * cellSize);
                        double xMax = offset + ((col + 1) * cellSize);
                        double yMin = offset + (row * cellSize);
                        double yMax = offset + ((row + 1) * cellSize);

                        if (px >= xMin && px <= xMax && py >= yMin && py <= yMax && bf[row, col] == 0)
                        {
                            return new B3_Mika_Roeder_Move(row, col, _playerNumber);
                        }
                    }
                }
            }

            return null;
        }
    }
}
