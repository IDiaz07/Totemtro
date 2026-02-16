using UnityEngine;
using System.Collections.Generic;

public class TotemInventory : MonoBehaviour
{
    public List<TotemData> ownedTotems = new List<TotemData>();
    public int maxTotems = 6;

    // =========================================
    // ➕ ADD OR UPGRADE
    // =========================================

    public bool AddOrUpgradeTotem(TotemData data)
    {
        if (data == null)
            return false;

        TotemData existing = ownedTotems.Find(t =>
            t.totemType == data.totemType
        );

        // 🆕 NO EXISTE
        if (existing == null)
        {
            if (ownedTotems.Count >= maxTotems)
                return false;

            ownedTotems.Add(data);
        }
        else
        {
            // 🔼 SOLO UPGRADE SI ES SUPERIOR
            if (data.rarity <= existing.rarity)
                return false;

            ownedTotems.Remove(existing);
            ownedTotems.Add(data);
        }

        GetComponent<PlayerStats>()?.Recalculate();
        GetComponent<TotemSynergySystem>()?.CheckSynergies();

        return true;
    }

    // =========================================
    // 💰 SELL
    // =========================================

    public int SellTotem(TotemData data)
    {
        if (data == null) return 0;
        if (!ownedTotems.Contains(data)) return 0;

        ownedTotems.Remove(data);

        GetComponent<PlayerStats>()?.Recalculate();
        GetComponent<TotemSynergySystem>()?.CheckSynergies();

        // 60% del valor
        return Mathf.RoundToInt(data.price * 0.6f);
    }

    public bool IsFull()
    {
        return ownedTotems.Count >= maxTotems;
    }

    public bool HasTotem(TotemType type)
    {
        return ownedTotems.Exists(t => t.totemType == type);
    }
}
