namespace OOPGames
{
    public enum SSLV2Action
    {
        None,
        MoveLeft,
        MoveRight
    }

    public class A4_ShellStrikeLegendsV2_Move : IPlayMove
    {
        public SSLV2Action Action { get; private set; }

        // Vom IPlayMove-Interface gefordert
        public int PlayerNumber { get; private set; }

        public A4_ShellStrikeLegendsV2_Move(SSLV2Action action, int playerNumber)
        {
            Action = action;
            PlayerNumber = playerNumber;
        }

        public override string ToString() => $"{PlayerNumber}: {Action}";
    }
}
