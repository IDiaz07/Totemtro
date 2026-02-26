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

    public int CalculateFinalReward(bool extracted)
    {
        float minTime = 20f;

        if (!extracted && runTime < minTime)
            return 0;

        int goldCollected = GetCollectedGold();

        float timeValue = runTime * 0.75f;

        float baseReward = goldCollected + timeValue;

        if (!extracted)
            baseReward *= 0.25f;

        return Mathf.FloorToInt(baseReward);
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

        timeBonus = Mathf.FloorToInt(runTime * 0.75f);

        float baseReward = goldCollected + timeBonus;

        if (!extracted)
        {
            penalty = Mathf.FloorToInt(baseReward * 0.75f);
            baseReward *= 0.25f;
        }
        else
        {
            penalty = 0;
        }

        finalReward = Mathf.FloorToInt(baseReward);
    }
}