using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TotemInventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public Image background;
    public GameObject legendaryFrame;

    [Header("Sell UI")]
    public TMP_Text sellPriceText;
    public Button sellButton;

    [Header("Rarity BG")]
    public Sprite commonBG;
    public Sprite rareBG;
    public Sprite legendaryBG;

    [Header("Legendary FX")]
    public GameObject legendaryParticles;

    [Header("Sell FX")]
    public Image flashImage;
    public float sellFlashDuration = 0.25f;

    [Header("Confirm UI")]
    public TotemSellConfirmUI confirmUI;

    TotemData currentData;
    TotemSellSystem sellSystem;


    bool isEmpty = false;

    void Awake()
    {
        sellSystem = FindFirstObjectByType<TotemSellSystem>();
        confirmUI = FindFirstObjectByType<TotemSellConfirmUI>();

        if (flashImage != null)
            flashImage.color = new Color(1, 1, 1, 0);
    }

    // =========================================
    // SETUP
    // =========================================

    public void Setup(TotemData data)
    {
        currentData = data;
        isEmpty = data == null;

        if (isEmpty)
        {
            SetupEmptySlot();
            return;
        }

        icon.gameObject.SetActive(true);
        sellButton.gameObject.SetActive(true);
        sellPriceText.gameObject.SetActive(true);

        icon.sprite = data.icon;

        int sellValue = Mathf.RoundToInt(data.price * 0.6f);
        sellPriceText.text = sellValue.ToString();

        SetupRarityVisual(data.rarity);

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(ConfirmSell);
    }

    void SetupEmptySlot()
    {
        icon.gameObject.SetActive(false);
        sellButton.gameObject.SetActive(false);
        sellPriceText.gameObject.SetActive(false);
        legendaryFrame.SetActive(false);

        if (legendaryParticles != null)
            legendaryParticles.SetActive(false);

        background.sprite = commonBG;
    }

    // =========================================
    // SELL CONFIRM
    // =========================================

    void ConfirmSell()
    {
        Debug.Log("CLICK SELL SLOT");

        if (currentData == null) return;

        confirmUI.Open(currentData, () =>
        {
            StartCoroutine(SellAnimation());
        });
    }

    // =========================================
    // RARITY VISUAL
    // =========================================

    void SetupRarityVisual(TotemRarity rarity)
    {
        legendaryFrame.SetActive(false);

        if (legendaryParticles != null)
            legendaryParticles.SetActive(false);

        switch (rarity)
        {
            case TotemRarity.Common:
                background.sprite = commonBG;
                break;

            case TotemRarity.Rare:
                background.sprite = rareBG;
                break;

            case TotemRarity.Legendary:
                background.sprite = legendaryBG;
                legendaryFrame.SetActive(true);

                if (legendaryParticles != null)
                    legendaryParticles.SetActive(true);
                break;
        }
    }

    // =========================================
    // SELL ANIMATION
    // =========================================

    IEnumerator SellAnimation()
    {
        sellButton.interactable = false;

        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / sellFlashDuration;

            float ease = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(startScale, startScale * 1.2f, ease);

            if (flashImage != null)
                flashImage.color = new Color(1f, 0.8f, 0.3f, 1f - ease);

            yield return null;
        }

        if (sellSystem != null)
            sellSystem.SellTotem(currentData);

        Setup(null);
    }
}
