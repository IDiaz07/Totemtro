using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionSlotUI : MonoBehaviour
{
    public Image icon;
    public Image cooldownOverlay;
    public TMP_Text amountText;
    public TMP_Text keyText;

    ItemData currentItem;

    public void Setup(int keyIndex)
    {
        keyText.text = (keyIndex + 1).ToString();
        Clear();
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (item == null)
        {
            Clear();
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;
    }

    public void UpdateCooldown(float remaining, float total)
    {
        if (total <= 0f)
        {
            cooldownOverlay.fillAmount = 0f;
            return;
        }

        cooldownOverlay.fillAmount = remaining / total;
    }

    public void UpdateAmount(int amount)
    {
        amountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void Clear()
    {
        currentItem = null;
        icon.enabled = false;
        cooldownOverlay.fillAmount = 0f;
        amountText.text = "";
    }
}
