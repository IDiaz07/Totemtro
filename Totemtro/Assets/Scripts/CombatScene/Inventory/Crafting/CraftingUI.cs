using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    public CraftingSystem craftingSystem;

    public CraftingRecipe nullGrenadeRecipe;
    public CraftingRecipe normalGrenadeRecipe;

    public void CraftNullGrenade()
    {
        craftingSystem.Craft(nullGrenadeRecipe);
    }

    public void CraftNormalGrenade()
    {
        craftingSystem.Craft(normalGrenadeRecipe);
    }
}
