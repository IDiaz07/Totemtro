using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData item;
    [Range(0f, 1f)] public float dropChance = 0.5f;
    public int minAmount = 1;
    public int maxAmount = 3;
}

public class ChestLootTable : MonoBehaviour
{
    public LootEntry[] lootTable;

    public ItemStack[] GenerateLoot(int maxSlots)
    {
        ItemStack[] result = new ItemStack[maxSlots];

        int index = 0;

        foreach (var entry in lootTable)
        {
            if (index >= maxSlots) break;

            if (Random.value <= entry.dropChance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);

                result[index] = new ItemStack
                {
                    item = entry.item,
                    amount = amount
                };

                index++;
            }
        }

        return result;
    }
}