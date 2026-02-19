using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public MaterialInventory materialInventory;
    public ActiveItemInventory activeInventory;

    public bool Craft(CraftingRecipe recipe)
    {
        // Check materials
        foreach (var ing in recipe.ingredients)
        {
            if (!materialInventory.Has(ing.item, ing.amount))
                return false;
        }

        for (int i = 0; i < recipe.resultAmount; i++)
        {
            activeInventory.AddAbility(recipe.resultAbility);
        }

        // Remove materials
        foreach (var ing in recipe.ingredients)
        {
            materialInventory.Remove(ing.item, ing.amount);
        }

        // Add ability
        activeInventory.AddAbility(recipe.resultAbility);

        return true;
    }
}
