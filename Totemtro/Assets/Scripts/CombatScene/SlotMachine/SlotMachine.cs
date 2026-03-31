using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachine : MonoBehaviour
{
    public SlotMachineUI ui;

    [Header("Probabilities")]
    [Range(0, 1)] public float loss = 0.35f;
    [Range(0, 1)] public float common = 0.32f;
    [Range(0, 1)] public float rare = 0.20f;
    [Range(0, 1)] public float epic = 0.10f;
    [Range(0, 1)] public float legendary = 0.03f;

    [Header("Loot Tables (ALL SAME TYPE)")]
    public SlotLootTable commonTable;
    public SlotLootTable rareTable;
    public SlotLootTable epicTable;
    public SlotLootTable legendaryTable;

    private HeroController player;
    private PlayerStats playerStats;

    // =========================
    public void Interact(HeroController p)
    {
        if (p == null) return;

        player = p;
        playerStats = p.GetComponent<PlayerStats>();

        StartSpin();
    }

    public void StartSpin()
    {
        if (ui == null) return;

        ui.StartSpin(OnSpinFinished);
    }

    void OnSpinFinished()
    {
        SlotIconType result = RollResult();

        ui.SetFinalResult(result);
        ApplyResult(result);
    }

    // =========================
    SlotIconType RollResult()
    {
        float l = loss;

        float c = HasTable(commonTable) ? common : 0f;
        float r = HasTable(rareTable) ? rare : 0f;
        float e = HasTable(epicTable) ? epic : 0f;
        float le = HasTable(legendaryTable) ? legendary : 0f;

        float total = l + c + r + e + le;

        if (total <= 0f)
            return SlotIconType.Loss;

        float roll = Random.value * total;

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
    void ApplyResult(SlotIconType result)
    {
        switch (result)
        {
            case SlotIconType.Loss:
                Debug.Log("💀 Has perdido");
                break;

            case SlotIconType.Common:
                GiveFromTable(commonTable);
                break;

            case SlotIconType.Rare:
                GiveFromTable(rareTable);
                break;

            case SlotIconType.Epic:
                GiveFromTable(epicTable);
                break;

            case SlotIconType.Legendary:
                GiveFromTable(legendaryTable);
                break;
        }
    }

    // =========================
    bool HasTable(SlotLootTable table)
    {
        return table != null && table.items != null && table.items.Count > 0;
    }

    // =========================
    void GiveFromTable(SlotLootTable table)
    {
        if (!HasTable(table))
        {
            Debug.Log("Tabla vacía");
            return;
        }

        float total = 0f;

        foreach (var e in table.items)
            total += e.chance;

        float roll = Random.value * total;

        foreach (var e in table.items)
        {
            if (roll < e.chance)
            {
                int amount = Random.Range(e.minAmount, e.maxAmount + 1);

                MetaInventory.Instance.AddItem(e.item, amount);

                if (ui != null)
                    ui.ShowReward(e.item.icon);

                return;
            }

            roll -= e.chance;
        }
    }
}