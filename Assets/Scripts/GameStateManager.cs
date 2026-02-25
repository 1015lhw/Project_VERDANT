public enum GameState
{
    Normal,
    Task,
    Inventory
}

public static class GameStateManager
{
    public static GameState CurrentState { get; private set; } = GameState.Normal;

    public static bool IsNormal => CurrentState == GameState.Normal;

    public static void SetState(GameState newState)
    {
        CurrentState = newState;
    }

    public static void ResetToNormal()
    {
        CurrentState = GameState.Normal;
    }
}
