using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [SerializeField] private MetaInventory inventory;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartRun();
    }

    // =========================
    // START RUN
    // =========================

    public void StartRun()
    {
        // Resetear estados estáticos de la partida anterior
        EnemyStatsTracker.Reset();
        CombatStatsTracker.Reset();
        GameInputLock.Reset();
        GamePause.Reset();
        HeroController.OnPlayerDeath = null;

        if (inventory == null)
            return;

        if (RunLoadoutSystem.Instance == null)
            return;

        CopyLoadoutToRunInventory();
    }

    // =========================
    // END RUN - DEATH
    // =========================

    public void EndRunByDeath()
    {
        Debug.Log("END RUN BY DEATH EXECUTED");

        if (RunEconomySystem.Instance == null)
        {
            Debug.LogError("RunEconomySystem is NULL");
            return;
        }

        int gold, timeBonus, penalty, total;

        RunEconomySystem.Instance.GetRewardBreakdown(
            false,
            out gold,
            out timeBonus,
            out penalty,
            out total);

        if (MetaCurrencySystem.Instance != null)
            MetaCurrencySystem.Instance.AddMetaGold(total);
        else
            Debug.LogError("MetaCurrencySystem is NULL");

        if (RunSummaryManager.Instance != null)
        {
            RunSummaryManager.Instance.CreateSummary(
                RunEconomySystem.Instance.GetRunTime(),
                EnemyStatsTracker.Kills,
                gold,
                timeBonus,
                penalty,
                total,
                false,
                inventory);
        }
        else
        {
            Debug.LogError("RunSummaryManager is NULL");
        }

        if (RunHistoryManager.Instance != null)
        {
            RunHistoryManager.Instance.AddRun(
                RunEconomySystem.Instance.GetRunTime(),
                EnemyStatsTracker.Kills,
                total,
                false);
        }
        else
        {
            Debug.LogWarning("RunHistoryManager is NULL");
        }

        // Limpieza de BAG + ACTIONBAR
        if (RunInventoryCleaner.Instance != null)
        {
            RunInventoryCleaner.Instance.ClearRunInventory();
        }
        else
        {
            Debug.LogError("RunInventoryCleaner is NULL");
        }

        if (inventory != null)
        {
            inventory.SaveMetaInventory();
        }
    }

    // =========================
    // END RUN - EXTRACTION
    // =========================

    public void EndRunByExtraction()
    {
        Debug.Log("END RUN BY EXTRACTION EXECUTED");

        if (RunEconomySystem.Instance == null)
        {
            Debug.LogError("RunEconomySystem is NULL");
            return;
        }

        int gold, timeBonus, penalty, total;

        RunEconomySystem.Instance.GetRewardBreakdown(
            true,
            out gold,
            out timeBonus,
            out penalty,
            out total);

        if (MetaCurrencySystem.Instance != null)
            MetaCurrencySystem.Instance.AddMetaGold(total);

        if (RunSummaryManager.Instance != null)
        {
            RunSummaryManager.Instance.CreateSummary(
                RunEconomySystem.Instance.GetRunTime(),
                EnemyStatsTracker.Kills,
                gold,
                timeBonus,
                penalty,
                total,
                true,
                inventory);
        }

        if (RunHistoryManager.Instance != null)
        {
            RunHistoryManager.Instance.AddRun(
                RunEconomySystem.Instance.GetRunTime(),
                EnemyStatsTracker.Kills,
                total,
                true);
        }

        TransferToMeta();

        if (inventory != null)
            ClearRunInventory();
    }

    // =========================
    // INTERNAL
    // =========================

    void TransferToMeta()
    {
        if (inventory == null)
        {
            Debug.LogError("RunInventory is NULL");
            return;
        }

        if (MetaInventory.Instance == null)
        {
            Debug.LogError("MetaInventory is NULL");
            return;
        }

        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                MetaInventory.Instance.AddItem(slot.item, slot.amount);
            }
        }

        MetaInventory.Instance.SaveMetaInventory();
    }

    void ClearRunInventory()
    {
        foreach (var slot in inventory.slots)
        {
            slot.Clear();
        }

        inventory.NotifyInventoryChanged();
    }

    void CopyLoadoutToRunInventory()
    {
        var loadout = RunLoadoutSystem.Instance.loadoutSlots;

        for (int i = 0; i < inventory.slots.Length; i++)
        {
            inventory.slots[i].Clear();

            if (i >= loadout.Length)
                continue;

            if (!loadout[i].IsEmpty())
            {
                inventory.slots[i].item = loadout[i].item;
                inventory.slots[i].amount = loadout[i].amount;
            }
        }

        inventory.NotifyInventoryChanged();
    }
}