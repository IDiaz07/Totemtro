using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    public SlotMachineUI ui;
    public GameObject slotPanel;
    public GameObject rewardPanel;

    [Header("Probabilities")]
    [Range(0, 1)] public float loss = 0.35f;
    [Range(0, 1)] public float different = 0.10f; // probabilidad "no toca nada + iconos diferentes"
    [Range(0, 1)] public float common = 0.32f;
    [Range(0, 1)] public float rare = 0.20f;
    [Range(0, 1)] public float epic = 0.10f;
    [Range(0, 1)] public float legendary = 0.03f;

    [Header("Loot Tables")]
    public SlotLootTable commonTable;
    public SlotLootTable rareTable;
    public SlotLootTable epicTable;
    public SlotLootTable legendaryTable;

    [Header("Cost")]
    public int goldCost = 50;
    [Range(0f, 1f)] public float healthCostPercent = 0f;
    public ItemData costItem;
    public int costItemAmount = 0;

    [Header("Spin control")]
    public float spinCooldown = 1f;
    private bool isSpinning = false;
    private float lastSpinTime = -99f;

    [Header("Jackpot")]
    [Range(0f, 1f)] public float jackpotChance = 0.01f;
    public int jackpotExtraRewards = 2;

    [Header("Variant")]
    public SlotMachineVariant variant = SlotMachineVariant.Normal;

    public bool pauseOnOpen = false;

    private HeroController player;
    private PlayerStats playerStats;

    [Header("Lever")]
    public LeverAnimator lever;

    public enum SlotMachineVariant
    {
        Normal,
        Cursed,
        Blood,
        Golden,
        Chaos
    }

    void Awake()
    {
        ui = FindFirstObjectByType<SlotMachineUI>(FindObjectsInactive.Include);

        if (ui == null)
        {
            Debug.LogError("SlotMachine: UI NO ENCONTRADA");
            return;
        }

        slotPanel = ui.gameObject;
    }

    // =========================
    public void Open(HeroController p)
    {
        player = p;
        playerStats = p.GetComponent<PlayerStats>();
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (slotPanel != null) slotPanel.SetActive(true);
        ui.SetMachine(this);
        UILayerManager.Open(UILayerManager.Layer.SlotMachine); // ← añadir
        GameInputLock.Lock();
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (slotPanel != null) slotPanel.SetActive(false);
        UILayerManager.Close(UILayerManager.Layer.SlotMachine); // ← añadir
        GameInputLock.Reset();
        Time.timeScale = 1f;
        Debug.Log($"SlotMachine CLOSED — timeScale={Time.timeScale}, IsLocked={GameInputLock.IsLocked}");
    }

    // =========================
    public void StartSpin()
    {
        if (ui == null) return;

        if (isSpinning)
        {
            Debug.Log("SlotMachine: ya girando");
            return;
        }

        if (Time.unscaledTime - lastSpinTime < spinCooldown)
        {
            Debug.Log("SlotMachine: cooldown activo");
            return;
        }

        if (!PayCosts())
        {
            Debug.Log("SlotMachine: recursos insuficientes");
            return;
        }

        isSpinning = true;
        lastSpinTime = Time.unscaledTime;

        ui.StartSpin(OnSpinFinished);
    }

    void OnSpinFinished()
    {
        // Generar resultados — la función decide si es el caso "different" o un resultado uniforme
        bool isDifferentOutcome;
        SlotIconType[] results = GenerateResults(out isDifferentOutcome, ui != null ? ui.reels.Length : 3);

        if (ui != null)
            ui.SetFinalResults(results);

        ApplyResults(results, isDifferentOutcome);

        isSpinning = false;
    }

    // =========================
    // GENERACIÓN DE RESULTADOS
    // Si isDifferentOutcome == true → NO HAY RECOMPENSA (iconos serán diferentes entre sí)
    SlotIconType[] GenerateResults(out bool isDifferentOutcome, int reelsCount)
    {
        isDifferentOutcome = false;

        // calcular probabilidades globales
        float l = loss;
        float d = different;
        float c = HasTable(commonTable) ? common : 0f;
        float r = HasTable(rareTable) ? rare : 0f;
        float e = HasTable(epicTable) ? epic : 0f;
        float le = HasTable(legendaryTable) ? legendary : 0f;

        switch (variant)
        {
            case SlotMachineVariant.Cursed:
                l = Mathf.Min(0.9f, l + 0.05f);
                e = Mathf.Min(1f, e + 0.02f);
                break;

            case SlotMachineVariant.Golden:
                r = Mathf.Min(1f, r + 0.02f);
                e = Mathf.Min(1f, e + 0.03f);
                le = Mathf.Min(1f, le + 0.01f);
                break;

            case SlotMachineVariant.Blood:
                r = Mathf.Min(1f, r + 0.03f);
                break;

            case SlotMachineVariant.Chaos:
                float total = l + d + c + r + e + le;
                l = Mathf.Clamp01(Random.Range(0f, total));
                d = Mathf.Clamp01(Random.Range(0f, total - l));
                break;
        }

        float totalProb = l + d + c + r + e + le;

        if (totalProb <= 0f)
        {
            // fallback: todos Loss
            SlotIconType[] fallbackResults = new SlotIconType[reelsCount];
            for (int i = 0; i < reelsCount; i++) fallbackResults[i] = SlotIconType.Loss;
            return fallbackResults;
        }

        float roll = Random.value * totalProb;

        // decidir outcome global
        if (roll < l)
        {
            // Loss global -> todos calavera
            SlotIconType[] res = new SlotIconType[reelsCount];
            for (int i = 0; i < reelsCount; i++) res[i] = SlotIconType.Loss;
            return res;
        }
        roll -= l;

        if (roll < d)
        {
            // Different outcome: generar N resultados distintos entre sí (no se otorgan rewards)
            isDifferentOutcome = true;
            List<SlotIconType> chosen = new List<SlotIconType>();
            int attempts = 0;
            while (chosen.Count < reelsCount && attempts < 100)
            {
                attempts++;
                SlotIconType candidate = RollSingle();
                if (!chosen.Contains(candidate))
                    chosen.Add(candidate);
            }

            // si por algún motivo no alcanzamos distintivos, rellenar con Loss/Commons alternativos
            while (chosen.Count < reelsCount)
                chosen.Add(SlotIconType.Loss);

            return chosen.ToArray();
        }
        roll -= d;

        if (roll < c)
        {
            SlotIconType[] res = new SlotIconType[reelsCount];
            for (int i = 0; i < reelsCount; i++) res[i] = SlotIconType.Common;
            return res;
        }
        roll -= c;

        if (roll < r)
        {
            SlotIconType[] res = new SlotIconType[reelsCount];
            for (int i = 0; i < reelsCount; i++) res[i] = SlotIconType.Rare;
            return res;
        }
        roll -= r;

        if (roll < e)
        {
            SlotIconType[] res = new SlotIconType[reelsCount];
            for (int i = 0; i < reelsCount; i++) res[i] = SlotIconType.Epic;
            return res;
        }

        // Legendary
        SlotIconType[] finalRes = new SlotIconType[reelsCount];
        for (int i = 0; i < reelsCount; i++) finalRes[i] = SlotIconType.Legendary;
        return finalRes;
    }

    // RollSingle mantiene probabilidades locales (usada por GenerateResults)
    SlotIconType RollSingle()
    {
        float l = loss;
        float c = HasTable(commonTable) ? common : 0f;
        float r = HasTable(rareTable) ? rare : 0f;
        float e = HasTable(epicTable) ? epic : 0f;
        float le = HasTable(legendaryTable) ? legendary : 0f;

        float totalProb = l + c + r + e + le;
        if (totalProb <= 0f) return SlotIconType.Loss;

        float roll = Random.value * totalProb;

        if (roll < l) return SlotIconType.Loss;
        roll -= l;

        if (roll < c) return SlotIconType.Common;
        roll -= c;

        if (roll < r) return SlotIconType.Rare;
        roll -= r;

        if (roll < e) return SlotIconType.Epic;

        return SlotIconType.Legendary;
    }

    // =========================
    // ApplyResults ahora recibe flag isDifferentOutcome;
    // si isDifferentOutcome == true → NO otorgar rewards (solo mostrar visual)
    void ApplyResults(SlotIconType[] results, bool isDifferentOutcome)
    {
        if (results == null || results.Length == 0) return;

        bool allLoss = true;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] != SlotIconType.Loss) { allLoss = false; break; }
        }

        if (allLoss)
        {
            Debug.Log("💀 Triple calavera: penalización 25% de vida actual");
            ApplySkullPenalty();
            if (ui != null)
                ui.ShowLoss(); // ← muestra FX y llama Close() al terminar
            return;
        }

        if (isDifferentOutcome)
        {
            Debug.Log("No toca nada — iconos distintos, sin recompensa.");
            // Cerrar tras un pequeño delay para que el jugador vea los rodillos
            StartCoroutine(CloseAfterDelay(1.5f));
            return;
        }

        bool shownUI = false;
        for (int i = 0; i < results.Length; i++)
        {
            var res = results[i];
            switch (res)
            {
                case SlotIconType.Loss:
                    // Loss individual dentro de resultado mixto — no penalizar, solo ignorar
                    break;
                case SlotIconType.Common:
                    GiveFromTable(commonTable, res, !shownUI);
                    if (!shownUI) shownUI = true;
                    break;
                case SlotIconType.Rare:
                    GiveFromTable(rareTable, res, !shownUI);
                    if (!shownUI) shownUI = true;
                    break;
                case SlotIconType.Epic:
                    GiveFromTable(epicTable, res, !shownUI);
                    if (!shownUI) shownUI = true;
                    break;
                case SlotIconType.Legendary:
                    GiveFromTable(legendaryTable, res, !shownUI);
                    if (!shownUI) shownUI = true;
                    break;
            }
        }
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Close();
    }

    // helper: no es estrictamente necesario pero evita marcar shownUI incorrectamente
    bool HasOnlyLossesButThis(SlotIconType[] results, int index)
    {
        // returns true if this reel is the only non-loss -> used to decide showing UI. For simplicity return false.
        return false;
    }

    void ApplySkullPenalty()
    {
        if (player == null)
            player = FindFirstObjectByType<HeroController>();

        if (player == null) return;

        var health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        float damage = player.CurrentHealth * 0.25f; // 25% of current health
        health.TakeDamage(damage, Vector2.zero);
    }

    // =========================
    bool HasTable(SlotLootTable table)
    {
        return table != null && table.items != null && table.items.Count > 0;
    }

    void GiveFromTable(SlotLootTable table, SlotIconType result, bool showUI = true)
    {
        if (table == null)
        {
            Debug.LogError("Tabla NULL");
            return;
        }

        if (table.items == null || table.items.Count == 0)
        {
            Debug.LogWarning("Tabla sin items");
            return;
        }

        float total = 0f;

        foreach (var e in table.items)
        {
            if (e == null)
            {
                Debug.LogWarning("Entry NULL en tabla");
                continue;
            }

            if (e.item == null)
            {
                Debug.LogWarning("Item NULL en entry");
                continue;
            }

            total += e.chance;
        }

        if (total <= 0f)
        {
            Debug.LogWarning("Total weight = 0");
            return;
        }

        float roll = Random.value * total;

        foreach (var e in table.items)
        {
            if (e == null || e.item == null) continue;

            if (roll < e.chance)
            {
                int amount = Random.Range(e.minAmount, e.maxAmount + 1);

                if (MetaInventory.Instance == null)
                {
                    Debug.LogError("MetaInventory NULL");
                    return;
                }

                MetaInventory.Instance.AddItem(e.item, amount);

                if (ui != null && showUI)
                    ui.ShowReward(e.item, null, result);

                return;
            }

            roll -= e.chance;
        }

        Debug.LogWarning("No reward selected");
    }

    void GiveJackpot()
    {
        if (!HasTable(legendaryTable))
            return;

        ItemData last = null;

        for (int i = 0; i < 1 + jackpotExtraRewards; i++)
        {
            float total = 0f;
            foreach (var e in legendaryTable.items)
                total += e.chance;

            float roll = Random.value * total;

            foreach (var e in legendaryTable.items)
            {
                if (roll < e.chance)
                {
                    int amount = Random.Range(e.minAmount, e.maxAmount + 1);
                    MetaInventory.Instance.AddItem(e.item, amount);
                    last = e.item;
                    break;
                }

                roll -= e.chance;
            }
        }

        if (ui != null && last != null)
            ui.ShowReward(last, null, SlotIconType.Legendary);
    }

    // =========================
    bool PayCosts()
    {
        if (goldCost > 0)
        {
            var gs = FindObjectOfType<GoldSystem>();
            if (gs == null)
            {
                Debug.LogWarning("GoldSystem no encontrado");
                return false;
            }

            if (!gs.SpendGold(goldCost))
                return false;
        }

        if (healthCostPercent > 0f)
        {
            if (playerStats == null || player == null)
            {
                player = FindFirstObjectByType<HeroController>();
                playerStats = player != null ? player.GetComponent<PlayerStats>() : null;
            }

            var health = player != null ? player.GetComponent<PlayerHealth>() : null;
            if (health == null)
            {
                Debug.LogWarning("PlayerHealth no encontrado");
                return false;
            }

            float damage = (player.MaxHealth * healthCostPercent);
            health.TakeDamage(damage, Vector2.zero);
        }

        if (costItem != null && costItemAmount > 0)
        {
            bool removed = MetaInventory.Instance.RemoveItem(costItem, costItemAmount);
            if (!removed)
                return false;
        }

        return true;
    }

    public void SpinButton()
    {
        Debug.Log($"[SpinButton] lever={lever}");

        if (lever != null)
        {
            Debug.Log("[SpinButton] llamando PlayPull");
            lever.PlayPull(() => StartSpin());
        }
        else
        {
            Debug.LogError("[SpinButton] lever ES NULL — asígnalo en el Inspector de SlotMachine");
            StartSpin();
        }
    }
}