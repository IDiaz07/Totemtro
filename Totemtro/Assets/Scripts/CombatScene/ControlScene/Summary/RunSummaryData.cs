using System.Collections.Generic;

[System.Serializable]
public class RunSummaryData
{
    public float timeSurvived;
    public int enemiesKilled;
    public int goldCollected;
    public int timeBonus;
    public int deathPenalty;
    public int totalReward;

    public List<InventorySlotData> collectedItems =
        new List<InventorySlotData>();
}

[System.Serializable]
public class InventorySlotData
{
    public string itemID;
    public int amount;
}