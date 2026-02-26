using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public RunInventory playerInventory;

    void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<RunInventory>();
    }

    public bool Craft(CraftingRecipe recipe)
    {
        if (recipe == null || playerInventory == null)
            return false;

        // 1️⃣ Verificar materiales
        foreach (var ing in recipe.ingredients)
        {
            if (playerInventory.GetAmount(ing.item) < ing.amount)
                return false;
        }

        // 2️⃣ Verificar espacio antes de remover
        if (!playerInventory.AddItem(recipe.resultItem, recipe.resultAmount))
            return false;

        // 3️⃣ Ahora sí remover ingredientes
        foreach (var ing in recipe.ingredients)
        {
            RemoveIngredient(ing.item, ing.amount);
        }

        return true;
    }


    void RemoveIngredient(ItemData item, int amount)
    {
        int remaining = amount;

        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
            var slot = playerInventory.slots[i];

            if (slot.item == item)
            {
                int toRemove = Mathf.Min(slot.amount, remaining);

                playerInventory.RemoveItem(i, toRemove);

                remaining -= toRemove;

                if (remaining <= 0)
                    break;
            }
        }
    }

}
