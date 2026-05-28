namespace NeonSerpent.Core
{
    /// <summary>
    /// Global game states. State transitions are driven by GameStateManager.
    /// </summary>
    public enum GameState
    {
        None,
        MainMenu,
        Playing,
        Paused,
        Dead,
        DeathReplay,
        Respawning,
        LevelComplete,
        GameOver
    }

    /// <summary>
    /// Game mode variants. Determines rule set and win/lose conditions.
    /// </summary>
    public enum GameMode
    {
        Campaign,
        Endless,
        Challenge
    }

    /// <summary>
    /// Rank tiers for level scoring.
    /// </summary>
    public enum Rank
    {
        None,
        C,
        B,
        A,
        S
    }
}
