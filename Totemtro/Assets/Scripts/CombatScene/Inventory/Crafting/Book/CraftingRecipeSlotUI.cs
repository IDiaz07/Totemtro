using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    Vector2 originalPos;

    bool initialized = false;

    public void Setup(CraftingRecipe newRecipe, CraftingSystem system)
    {
        recipe = newRecipe;
        craftingSystem = system;

        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;

        if (recipe == null || craftingSystem == null)
        {
            Debug.LogWarning("Recipe or CraftingSystem missing in slot.");
            return;
        }

        // 🔐 PROTECCIÓN
        if (recipe.resultItem == null)
        {
            Debug.LogError("Recipe has no resultItem assigned: " + recipe.name);
            return;
        }

        resultIcon.sprite = recipe.resultItem.icon;
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

    /*void Update()
    {
        if (!initialized) return;
        if (recipe == null) return;
        if (craftingSystem == null) return;
        if (craftingSystem.playerInventory == null) return;
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

            int current = craftingSystem.playerInventory.GetAmount(ing.item);

            ui.amountText.text = current + " / " + ing.amount;
            ui.amountText.color = current < ing.amount ? Color.red : Color.white;

            ingredientIndex++;
        }
    }*/


    void TryCraft()
    {
        if (!initialized) return;

        bool success = craftingSystem.Craft(recipe);

        if (success)
        {
            StartCoroutine(SuccessEffect());
        }
        else
        {
            StartCoroutine(FailEffect());
        }
    }

    IEnumerator FailEffect()
    {
        float duration = 0.2f;
        float strength = 0.05f;

        Vector3 originalScale = rect.localScale;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float shake = Mathf.Sin(timer * 80f) * strength;
            rect.localScale = originalScale + new Vector3(shake, 0, 0);

            yield return null;
        }

        rect.localScale = originalScale;
    }

    IEnumerator SuccessEffect()
    {
        float duration = 0.15f;

        Vector3 originalScale = rect.localScale;
        Vector3 targetScale = originalScale * 1.05f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;

            rect.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        rect.localScale = originalScale;
    }

}
