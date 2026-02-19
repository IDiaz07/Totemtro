using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image icon;
    public TMP_Text amountText;

    ItemData currentItem;
    int currentAmount;

    bool isHovered = false;
    Vector3 originalScale;

    void Awake()
    {
        if (icon != null)
            originalScale = icon.transform.localScale;
    }

    public void Setup(ItemData item, int amount)
    {
        currentItem = item;
        currentAmount = amount;

        if (item == null)
        {
            icon.sprite = null;
            icon.enabled = false;

            amountText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;

        amountText.text = amount > 1 ? amount.ToString() : "";
    }

    void Update()
    {
        if (!icon.enabled)
            return;

        // Animación suave
        Vector3 targetScale = isHovered
            ? originalScale * 1.1f
            : originalScale;

        icon.transform.localScale =
            Vector3.Lerp(
                icon.transform.localScale,
                targetScale,
                Time.unscaledDeltaTime * 12f
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        isHovered = true;

        icon.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        isHovered = false;
        icon.color = Color.white;
    }

    public void PlayStackAnimation()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(StackPunch());
    }


    IEnumerator StackPunch()
    {
        float duration = 0.15f;
        float timer = 0f;

        Vector3 originalScale = icon.transform.localScale;
        Vector3 punchScale = originalScale * 1.2f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;

            icon.transform.localScale =
                Vector3.Lerp(punchScale, originalScale, t);

            yield return null;
        }

        icon.transform.localScale = originalScale;
    }
}
