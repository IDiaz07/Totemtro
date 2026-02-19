using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public CraftingIngredient[] ingredients;
    public int resultAmount = 1;

    public ActiveAbilityBase resultAbility;
}
