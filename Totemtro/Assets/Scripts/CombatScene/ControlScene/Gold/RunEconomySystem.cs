using UnityEngine;

public class RunEconomySystem : MonoBehaviour
{
    public static RunEconomySystem Instance;

    float runTime;
    GoldSystem goldSystem;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        goldSystem = FindFirstObjectByType<GoldSystem>();
        runTime = 0f;
    }

    void Update()
    {
        runTime += Time.deltaTime;
    }

    public int GetCollectedGold()
    {
        return goldSystem != null ? goldSystem.currentGold : 0;
    }

    public float GetRunTime()
    {
        return runTime;
    }

    public void ResetRun()
    {
        runTime = 0f;

        if (goldSystem != null)
            goldSystem.currentGold = 0;
    }

    public void GetRewardBreakdown(
        bool extracted,
        out int goldCollected,
        out int timeBonus,
        out int penalty,
        out int finalReward)
    {
        goldCollected = GetCollectedGold();

        // 1 moneda por cada 10 segundos (ganes o pierdas)
        timeBonus = Mathf.FloorToInt(runTime / 10f);

        // Victoria: +100 bonus
        int victoryBonus = extracted ? 100 : 0;

        penalty = 0;

        finalReward = timeBonus + victoryBonus;
    }
}