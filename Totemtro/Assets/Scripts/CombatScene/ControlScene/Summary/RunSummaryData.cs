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
    public bool extracted;

    // Stats de combate
    public float damageDealt;
    public float healthHealed;

    // Copas
    public int trophyDelta;
    public int trophiesAfter;

    // Maestría
    public int masteryXPGained;
    public int masteryXPTotal;
    public MasteryTier masteryTierBefore;
    public MasteryTier masteryTierAfter;

    // Hero
    public HeroType heroType;

    public List<InventorySlotData> collectedItems =
        new List<InventorySlotData>();
}

[System.Serializable]
public class InventorySlotData
{
    public string itemID;
    public int amount;
}