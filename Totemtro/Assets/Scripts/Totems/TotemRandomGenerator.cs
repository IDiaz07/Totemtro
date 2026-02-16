using UnityEngine;
using System.Collections.Generic;

public class TotemRandomGenerator : MonoBehaviour
{
    public List<TotemData> allTotems;

    public TotemData GetRandomTotem()
    {
        float roll = Random.value;

        TotemRarity rarity;

        if (roll < 0.65f)
            rarity = TotemRarity.Common;
        else if (roll < 0.9f)
            rarity = TotemRarity.Rare;
        else
            rarity = TotemRarity.Legendary;

        var candidates = allTotems.FindAll(t => t.rarity == rarity);

        return candidates[Random.Range(0, candidates.Count)];
    }
}
