using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    MetaInventory inventory;

    void Awake()
    {
        inventory = MetaInventory.Instance;
    }

    public bool Craft(CraftingRecipe recipe)
    {
        if (recipe == null || inventory == null)
            return false;

        // =========================
        // 1️⃣ Verificar ingredientes
        // =========================
        foreach (var ing in recipe.ingredients)
        {
            if (inventory.GetAmount(ing.item) < ing.amount)
                return false;
        }

        // =========================
        // 2️⃣ Remover ingredientes
        // =========================
        foreach (var ing in recipe.ingredients)
        {
            inventory.RemoveItem(ing.item, ing.amount);
        }

        // =========================
        // 3️⃣ Añadir resultado
        // =========================
        bool added = inventory.AddItem(
            recipe.resultItem,
            recipe.resultAmount
        );

        if (!added)
        {
            Debug.LogWarning("Inventory full. Craft failed.");
            return false;
        }

        inventory.NotifyInventoryChanged();
        inventory.SaveMetaInventory();
        Debug.Log("Crafted: " + recipe.resultItem.itemID);
        return true;
    }
}