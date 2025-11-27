namespace OOPGames
{
	// Minimal human player that performs no moves; rules handle spawning/falling.
	public class A4_ShellStrikeLegendsV2_HumanPlayer : IHumanGamePlayer
	{
		private int _playerNumber = 1;

		public string Name => "A4 ShellStrikeLegends V2 Human";

		public void SetPlayerNumber(int playerNumber) => _playerNumber = playerNumber;
		public int PlayerNumber => _playerNumber;

		public bool CanBeRuledBy(IGameRules rules) => rules is A4_ShellStrikeLegendsV2_Rules;

		public IGamePlayer Clone()
		{
			var p = new A4_ShellStrikeLegendsV2_HumanPlayer();
			p.SetPlayerNumber(_playerNumber);
			return p;
		}

		// No active input yet; returning null makes the engine not apply a move.
		public IPlayMove GetMove(IMoveSelection selection, IGameField field) => null;
	}
}

