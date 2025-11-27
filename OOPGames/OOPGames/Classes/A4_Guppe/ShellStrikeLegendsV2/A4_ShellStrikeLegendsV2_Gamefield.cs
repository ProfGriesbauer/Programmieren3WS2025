namespace OOPGames
{
	// Holds the terrain for V2; no gameplay logic here.
	public class A4_ShellStrikeLegendsV2_GameField : IGameField
	{
		public A4_ShellStrikeLegendsV2_Terrain Terrain { get; private set; } = new();
		public A4_ShellStrikeLegendsV2_Tank Tank1 { get; private set; }

		// Ensure terrain exists for the current canvas size (fixed map shape)
		public void EnsureSetup(int canvasWidth, int canvasHeight)
		{
			if (Terrain.Heights == null || Terrain.Heights.Length != canvasWidth || Terrain.CanvasHeight != canvasHeight)
			{
				Terrain.BuildFixedSine(canvasWidth, canvasHeight);
			}
			if (Tank1 == null)
			{
				Tank1 = new A4_ShellStrikeLegendsV2_Tank
				{
					X = canvasWidth * 0.5,
					Y = 0 // start at top edge
				};
			}
		}

		public bool CanBePaintedBy(IPaintGame painter) => painter is A4_ShellStrikeLegendsV2_Painter;
	}
}

