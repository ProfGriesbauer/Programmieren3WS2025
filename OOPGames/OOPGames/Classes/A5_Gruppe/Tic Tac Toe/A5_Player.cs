using System;

namespace OOPGames
{
    /// <summary>
    /// Die Spieler-Klasse: Repräsentiert einen menschlichen Spieler
    /// </summary>
    public class A5_Player : IX_HumanTicTacToePlayer
    {
        private int _playerNumber;
        
        public string Name { get { return "A5 Player TicTacToe"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeControlledBy(IGamePlayer player)
        {
            return true;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IX_TicTacToeRules;
        }

        public IGamePlayer Clone()
        {
            A5_Player clone = new A5_Player();
            clone.SetPlayerNumber(PlayerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (field is IX_TicTacToeField)
            {
                return GetMove(selection, (IX_TicTacToeField)field);
            }

            return null;
        }

        public IX_TicTacToeMove GetMove(IMoveSelection selection, IX_TicTacToeField field)
        {
            if (selection is IClickSelection)
            {
                IClickSelection click = (IClickSelection)selection;
                int xPos = click.XClickPos;
                int yPos = click.YClickPos;

                // Koordinaten entsprechend dem Spielfeld-Layout
                if (xPos >= 20 && xPos <= 320 && yPos >= 20 && yPos <= 320)
                {
                    int row = -1, col = -1;

                    // Spaltenberechnung
                    if (xPos < 120) col = 0;
                    else if (xPos < 220) col = 1;
                    else col = 2;

                    // Zeilenberechnung
                    if (yPos < 120) row = 0;
                    else if (yPos < 220) row = 1;
                    else row = 2;

                    // Überprüfe, ob das Feld frei ist
                    if (field[row, col] == 0)
                    {
                        return new A5_Move { Row = row, Column = col, PlayerNumber = PlayerNumber };
                    }
                }
            }

            return null;
        }
    }
}