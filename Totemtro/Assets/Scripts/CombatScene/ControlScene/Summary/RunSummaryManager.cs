using UnityEngine;

public class RunSummaryManager : MonoBehaviour
{
    public static RunSummaryManager Instance;

    // Datos estáticos — sobreviven cambios de escena
    public static RunSummaryData LastRunData;

    public RunSummaryData LastRun
    {
        get { return LastRunData; }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CreateSummary(
        float time,
        int kills,
        int gold,
        int timeBonus,
        int penalty,
        int total,
        bool extracted,
        MetaInventory inventory)
    {
        Debug.Log("SUMMARY CREATED");

        LastRunData = new RunSummaryData();

        LastRunData.timeSurvived = time;
        LastRunData.enemiesKilled = kills;
        LastRunData.goldCollected = gold;
        LastRunData.timeBonus = timeBonus;
        LastRunData.deathPenalty = penalty;
        LastRunData.totalReward = total;
        LastRunData.extracted = extracted;

        // Combat stats — leer directamente de los campos estáticos
        LastRunData.damageDealt = CombatStatsTracker.TotalDamageDealt;
        LastRunData.healthHealed = CombatStatsTracker.TotalHealthHealed;

        // Hero info
        HeroType heroType = HeroType.Vex;

        if (GameSessionManager.Instance != null &&
            GameSessionManager.Instance.selectedHero != null)
        {
            heroType = GameSessionManager.Instance.selectedHero.heroType;
        }

        LastRunData.heroType = heroType;

        // Copas + Maestría
        if (HeroMasterySystem.Instance != null)
        {
            MasteryTier tierBefore =
                HeroMasterySystem.Instance.GetMasteryTier(heroType);

            int masteryXP =
                HeroMasterySystem.Instance.CalculateMasteryXP(
                    extracted, time, kills);

            HeroMasterySystem.Instance.AddMasteryXP(heroType, masteryXP);

            int trophyDelta =
                HeroMasterySystem.Instance.ApplyTrophyResult(
                    heroType, extracted, time, kills);

            LastRunData.trophyDelta = trophyDelta;
            LastRunData.trophiesAfter =
                HeroMasterySystem.Instance.GetTrophies(heroType);

            LastRunData.masteryXPGained = masteryXP;
            LastRunData.masteryXPTotal =
                    HeroMasterySystem.Instance.GetMasteryXP(heroType);

            LastRunData.masteryTierBefore = tierBefore;
            LastRunData.masteryTierAfter =
                HeroMasterySystem.Instance.GetMasteryTier(heroType);
        }

        // Items
        if (inventory != null && inventory.slots != null)
        {
            foreach (var slot in inventory.slots)
            {
                if (!slot.IsEmpty())
                {
                    InventorySlotData data = new InventorySlotData();
                    data.itemID = slot.item.itemID;
                    data.amount = slot.amount;

                    LastRunData.collectedItems.Add(data);
                }
            }
        }

        Debug.Log($"Summary — Damage: {LastRunData.damageDealt}, Healed: {LastRunData.healthHealed}, Kills: {LastRunData.enemiesKilled}");
    }

    public static void Clear()
    {
        LastRunData = null;
    }
}