namespace OOPGames
{
    #region Tank Enums

    /// <summary>
    /// Visual identification color for each tank.
    /// Player 1 uses Red, Player 2 uses Blue.
    /// </summary>
    public enum B5_Shellshock_TankColor
    {
        Red,
        Blue
    }

    #endregion

    #region Terrain Enums

    /// <summary>
    /// Different terrain configurations that affect gameplay and strategy.
    /// Terrain type determines movement allowance and tactical options.
    /// </summary>
    public enum B5_Shellshock_TerrainType
    {
        /// <summary>Nearly horizontal battlefield with minimal height variation.</summary>
        Flat,
        /// <summary>Single hill with randomized position and slopes.</summary>
        Hill,
        /// <summary>V-shaped terrain with low center point.</summary>
        Valley,
        /// <summary>Rolling hills using layered sine waves.</summary>
        Curvy
    }

    #endregion

    #region Action Enums

    /// <summary>
    /// All possible player actions during a turn.
    /// Movement and aiming can be done multiple times; shooting ends the turn.
    /// </summary>
    public enum B5_Shellshock_ActionType
    {
        /// <summary>Move tank left (costs one movement point).</summary>
        MoveLeft,
        /// <summary>Move tank right (costs one movement point).</summary>
        MoveRight,
        /// <summary>Increase firing angle (unlimited).</summary>
        IncreaseAngle,
        /// <summary>Decrease firing angle (unlimited).</summary>
        DecreaseAngle,
        /// <summary>Increase shot power (unlimited).</summary>
        IncreasePower,
        /// <summary>Decrease shot power (unlimited).</summary>
        DecreasePower,
        /// <summary>Fire projectile (ends turn).</summary>
        Shoot,
        /// <summary>Start or restart the game.</summary>
        StartGame
    }

    #endregion

    #region Game State Enums

    /// <summary>
    /// Current phase of the game state machine.
    /// Controls what actions are available and what the UI displays.
    /// </summary>
    public enum B5_Shellshock_GamePhase
    {
        /// <summary>Initial state showing start screen. Press Space to begin.</summary>
        Setup,
        /// <summary>Active player can move, aim, and shoot.</summary>
        PlayerTurn,
        /// <summary>Projectile is flying; physics simulation active.</summary>
        ProjectileInFlight,
        /// <summary>One tank destroyed; shows winner and restart option.</summary>
        GameOver
    }

    #endregion
}
