using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCurrencyTileUI : MonoBehaviour
{
    [Header("UI")]
    public Image backgroundImage;
    public Image iconImage;
    public TMP_Text amountText;
    public TMP_Text priceText;
    public Button buyButton;

    [Header("Sprites")]
    public Sprite goldBackground;
    public Sprite gemsBackground;

    [Header("Payment Icons")]
    public Image iconGold;
    public Image iconGem;

    [Header("Discount")]
    public GameObject bonusArea;
    public TMP_Text bonusText;

    ShopItemData currentItem;

    public void Setup(ShopItemData item)
    {
        currentItem = item;

        bool isGoldReward = item.goldAmount > 0;

        // =========================
        // REWARD VISUAL
        // =========================
        backgroundImage.sprite = isGoldReward ? goldBackground : gemsBackground;
        amountText.text = isGoldReward
            ? item.goldAmount.ToString("N0")
            : item.gemAmount.ToString("N0");

        if (iconImage && item.icon != null)
            iconImage.sprite = item.icon;

        float basePrice = item.priceAmount;
        float finalPrice = ShopSystem.Instance.GetFinalPrice(item);

        if (iconGold) iconGold.gameObject.SetActive(false);
        if (iconGem) iconGem.gameObject.SetActive(false);

        // =========================
        // PAYMENT TYPE
        // =========================
        switch (item.priceCurrency)
        {
            case ShopCurrencyType.Gold:
                priceText.text = finalPrice.ToString("N0");
                if (iconGold) iconGold.gameObject.SetActive(true);
                break;

            case ShopCurrencyType.Gems:
                priceText.text = finalPrice.ToString("N0");
                if (iconGem) iconGem.gameObject.SetActive(true);
                break;

            case ShopCurrencyType.RealMoney:
                priceText.text = "$" + finalPrice.ToString("0.##");
                break;
        }

        // =========================
        // DISCOUNT
        // =========================
        bool hasDiscount = basePrice > finalPrice && basePrice > 0f;

        if (bonusArea != null)
            bonusArea.SetActive(hasDiscount);

        if (hasDiscount && bonusText != null)
        {
            float discount = 1f - (finalPrice / basePrice);
            int percent = Mathf.RoundToInt(discount * 100f);
            bonusText.text = "-" + percent + "%";
        }

        // =========================
        // BUTTON
        // =========================
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            ConfirmationPanelUI.Instance?.Open(item, transform.position);
        });
    }
}