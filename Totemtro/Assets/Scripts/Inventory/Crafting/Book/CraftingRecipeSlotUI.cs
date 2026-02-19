using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image resultIcon;
    public Transform ingredientsContainer;
    public GameObject ingredientPrefab;
    public Button craftButton;
    public TMP_Text resultAmountText;
    public GameObject plusPrefab;

    CraftingRecipe recipe;
    CraftingSystem craftingSystem;

    RectTransform rect;
    Vector3 originalPos;

    bool initialized = false;

    public void Setup(CraftingRecipe newRecipe, CraftingSystem system)
    {
        recipe = newRecipe;
        craftingSystem = system;

        rect = GetComponent<RectTransform>();
        originalPos = rect.localPosition;

        if (recipe == null || craftingSystem == null)
        {
            Debug.LogWarning("Recipe or CraftingSystem missing in slot.");
            return;
        }

        resultIcon.sprite = recipe.resultAbility.icon;
        resultAmountText.text = "x" + recipe.resultAmount;

        SetupIngredients();

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(TryCraft);

        initialized = true;
    }

    void SetupIngredients()
    {
        foreach (Transform child in ingredientsContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            var ing = recipe.ingredients[i];

            // Instanciar ingrediente
            GameObject obj = Instantiate(ingredientPrefab, ingredientsContainer);

            IngredientUI ui = obj.GetComponent<IngredientUI>();

            ui.icon.sprite = ing.item.icon;
            ui.amountText.text = "(" + ing.amount + ")";

            // Si no es el último ingrediente, añadir "+"
            if (i < recipe.ingredients.Length - 1)
            {
                Instantiate(plusPrefab, ingredientsContainer);
            }
        }
    }

    void Update()
    {
        if (!initialized) return;
        if (recipe == null) return;
        if (craftingSystem == null) return;
        if (craftingSystem.materialInventory == null) return;
        if (ingredientsContainer == null) return;

        int ingredientIndex = 0;

        for (int i = 0; i < ingredientsContainer.childCount; i++)
        {
            Transform child = ingredientsContainer.GetChild(i);

            IngredientUI ui = child.GetComponent<IngredientUI>();
            if (ui == null)
                continue;

            if (ingredientIndex >= recipe.ingredients.Length)
                break;

            var ing = recipe.ingredients[ingredientIndex];

            int current = 0;

            if (craftingSystem.materialInventory.Has(ing.item, 1))
            {
                current = craftingSystem.materialInventory.GetAll()[ing.item];
            }

            ui.amountText.text = current + " / " + ing.amount;
            ui.amountText.color = current < ing.amount ? Color.red : Color.white;

            ingredientIndex++;
        }
    }


    void TryCraft()
    {
        if (!initialized) return;

        bool success = craftingSystem.Craft(recipe);
        StartCoroutine(Shake());
    }

    System.Collections.IEnumerator Shake()
    {
        float duration = 0.2f;
        float strength = 8f;

        RectTransform rt = rect;
        Vector2 originalPos = rt.anchoredPosition;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;

            rt.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);

            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }
}
