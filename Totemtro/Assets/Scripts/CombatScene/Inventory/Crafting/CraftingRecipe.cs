using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public CraftingIngredient[] ingredients;

    public ItemData resultItem;
    public int resultAmount = 1;
}
