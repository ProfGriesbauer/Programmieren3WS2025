using System;

namespace OOPGames
{
    // Visual identification for each tank
    public enum B5_Shellshock_TankColor
    {
        Red,
        Blue
    }

    // Different terrain configurations affects tank placement and gameplay
    public enum B5_Shellshock_TerrainType
    {
        Flat,
        Hill,
        Valley
    }

    // Different actions a player can perform
    public enum B5_Shellshock_ActionType
    {
        MoveLeft,
        MoveRight,
        IncreaseAngle,
        DecreaseAngle,
        IncreasePower,
        DecreasePower,
        Shoot
    }

    // Current state of the game
    public enum B5_Shellshock_GamePhase
    {
        Setup,
        PlayerTurn,
        ProjectileInFlight,
        GameOver
    }

    // Affects AI accuracy and calculation
    public enum B5_Shellshock_AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
