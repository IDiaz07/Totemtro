using UnityEngine;

public enum GameState
{
    Gameplay,
    BossIntro,
    BossFight,
    Inventory,
    Cinematic
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
    }

    public bool CanOpenInventory()
    {
        return CurrentState == GameState.Gameplay;
    }
}
