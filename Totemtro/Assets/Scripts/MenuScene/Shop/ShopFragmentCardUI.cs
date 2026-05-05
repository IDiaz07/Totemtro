using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopFragmentCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image heroIcon;
    public TMP_Text fragmentAmountText;
    public TMP_Text priceText;
    public Button buyButton;

    ShopItemData currentItem;

    public void Setup(ShopItemData item)
    {
        currentItem = item;

        if (item.fragmentHero != null)
            heroIcon.sprite = item.fragmentHero.ChampsHeadIcon;

        fragmentAmountText.text = "+" + item.fragmentAmount;

        float finalPrice = ShopSystem.Instance.GetFinalPrice(item);
        priceText.text = finalPrice.ToString("N0");

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            ConfirmationPanelUI.Instance?.Open(item, transform.position);
        });
    }
}