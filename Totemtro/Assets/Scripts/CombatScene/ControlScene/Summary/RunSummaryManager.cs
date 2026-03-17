using UnityEngine;

public class RunSummaryManager : MonoBehaviour
{
    public static RunSummaryManager Instance;

    public RunSummaryData LastRun { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void CreateSummary(
        float time,
        int kills,
        int gold,
        int timeBonus,
        int penalty,
        int total,
        MetaInventory inventory)
    {
        Debug.Log("SUMMARY CREATED");

        LastRun = new RunSummaryData();

        LastRun.timeSurvived = time;
        LastRun.enemiesKilled = kills;
        LastRun.goldCollected = gold;
        LastRun.timeBonus = timeBonus;
        LastRun.deathPenalty = penalty;
        LastRun.totalReward = total;

        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                InventorySlotData data = new InventorySlotData();
                data.itemID = slot.item.itemID;
                data.amount = slot.amount;

                LastRun.collectedItems.Add(data);
            }
        }
    }
}