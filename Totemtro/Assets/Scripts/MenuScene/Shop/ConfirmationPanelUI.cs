using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ConfirmationPanelUI : MonoBehaviour
{
    public static ConfirmationPanelUI Instance;

    public GameObject root;
    public TMP_Text titleText;
    public TMP_Text priceText;
    public Button confirmButton;
    public Button cancelButton;

    ShopItemData currentItem;
    Vector3 purchaseWorldPosition;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Open(ShopItemData item, Vector3 worldPos)
    {
        currentItem = item;
        purchaseWorldPosition = worldPos;

        titleText.text = item.displayName;
        priceText.text =
            ShopSystem.Instance.GetFinalPrice(item).ToString();

        root.SetActive(true);
        StartCoroutine(UIAnimationUtility.FadeScaleIn(root.transform));

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(Confirm);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(Close);
    }

    public void Open(ShopItemData item)
    {
        Open(item, Vector3.zero);

        titleText.text = item.displayName;
        priceText.text =
            ShopSystem.Instance.GetFinalPrice(item).ToString();

        root.SetActive(true);
        StartCoroutine(UIAnimationUtility.FadeScaleIn(root.transform));

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(Confirm);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(Close);

    }

    void Confirm()
    {
        ShopSystem.Instance.TryPurchase(
            currentItem.itemId,
            purchaseWorldPosition
        );

        ShopPanelUI.Instance.RefreshHeader();
        Close();
    }

    void Close()
    {
        root.SetActive(false);
    }
}