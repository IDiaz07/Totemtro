using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TotemCardUI : MonoBehaviour
{
    [Header("Flip")]
    public TotemCardFlip flip;

    [Header("Background Sprites")]
    public Image backgroundImage;
    public Sprite commonBackground;
    public Sprite rareBackground;
    public Sprite legendaryBackground;

    [Header("Legendary Frame")]
    public GameObject legendaryFrame;

    [Header("Icon & Text (Front)")]
    public Image icon;
    public TMP_Text nameText;

    [Header("Buy Button")]
    public Button buyButton;
    public TMP_Text buttonPriceText;

    [Header("Back Info")]
    public TMP_Text descriptionText;
    public TMP_Text buffText;
    public TMP_Text comparisonText;

    TotemData currentData;
    TotemSelectionUI selectionUI;
    TotemInventory inventory;
    GoldSystem gold;

    Color normalPriceColor = Color.white;
    Color noMoneyColor = new Color(1f, 0.3f, 0.3f);

    void Awake()
    {
        gold = FindFirstObjectByType<GoldSystem>();
        inventory = FindFirstObjectByType<TotemInventory>();

        if (legendaryFrame != null)
            legendaryFrame.SetActive(false);
    }

    // =====================================================
    // SETUP
    // =====================================================

    public void Setup(TotemData data, TotemSelectionUI ui)
    {
        if (data == null)
        {
            Debug.LogWarning("TotemCardUI: Setup received NULL data.");
            return;
        }

        currentData = data;
        selectionUI = ui;

        // FRONT SAFE
        if (nameText != null)
            nameText.text = data.totemName;

        if (icon != null)
            icon.sprite = data.icon;

        if (buttonPriceText != null)
            buttonPriceText.text = data.price.ToString();

        // BACK SAFE
        if (descriptionText != null)
            descriptionText.text = data.description;

        if (buffText != null)
            buffText.text = GetBuffText(data);

        if (comparisonText != null)
            comparisonText.text = GetComparison(data);

        SetupRarityVisual(data.rarity);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyPressed);
        }

        CheckUpgradeVisual();
    }

    // =====================================================
    // UPGRADE CHECK
    // =====================================================

    void CheckUpgradeVisual()
    {
        if (inventory == null || currentData == null)
            return;

        TotemData owned = inventory.ownedTotems.Find(o =>
            o.totemType == currentData.totemType
        );

        if (owned != null && currentData.rarity > owned.rarity)
        {
            StartCoroutine(UpgradeGlow());
        }
    }

    // =====================================================
    // RARITY VISUAL
    // =====================================================

    void SetupRarityVisual(TotemRarity rarity)
    {
        if (backgroundImage == null)
            return;

        if (legendaryFrame != null)
            legendaryFrame.SetActive(false);

        switch (rarity)
        {
            case TotemRarity.Common:
                if (commonBackground != null)
                    backgroundImage.sprite = commonBackground;
                break;

            case TotemRarity.Rare:
                if (rareBackground != null)
                    backgroundImage.sprite = rareBackground;
                break;

            case TotemRarity.Legendary:
                if (legendaryBackground != null)
                    backgroundImage.sprite = legendaryBackground;

                if (legendaryFrame != null)
                    legendaryFrame.SetActive(true);
                break;
        }
    }

    // =====================================================
    // UPDATE PRICE COLOR SAFE
    // =====================================================

    void Update()
    {
        UpdatePriceColor();
    }

    void UpdatePriceColor()
    {
        if (gold == null || currentData == null || buttonPriceText == null)
            return;

        bool canAfford = gold.currentGold >= currentData.price;
        buttonPriceText.color = canAfford ? normalPriceColor : noMoneyColor;
    }

    // =====================================================
    // BUY
    // =====================================================

    void OnBuyPressed()
    {
        Debug.Log("Botón presionado");

        if (selectionUI == null || currentData == null || gold == null)
        {
            Debug.Log("Faltan referencias");
            return;
        }

        if (gold.currentGold < currentData.price)
        {
            Debug.Log("No tienes suficiente oro");
            return;
        }

        StartCoroutine(BuyAnimation());
    }


    IEnumerator BuyAnimation()
    {
        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 8f;
            float pulse = 1f + Mathf.Sin(t * 15f) * 0.08f;
            transform.localScale = startScale * pulse;
            yield return null;
        }

        transform.localScale = startScale;

        if (selectionUI != null)
            selectionUI.Choose(currentData);
    }

    // =====================================================
    // INFO BUTTON
    // =====================================================

    public void OnInfoPressed()
    {
        if (flip != null)
            flip.Flip();
    }

    // =====================================================
    // TEXT GENERATION
    // =====================================================

    string GetBuffText(TotemData data)
    {
        switch (data.totemType)
        {
            case TotemType.Power: return "+30% Damage";
            case TotemType.Swiftness: return "+20% Move Speed";
            case TotemType.RapidBullets: return "+30% Fire Rate";
            case TotemType.TripleShot: return "+2 Projectiles";
            case TotemType.DualFire: return "+1 Projectile";
            case TotemType.Piercing: return "+1 Pierce";
            case TotemType.Ricochet: return "+1 Ricochet";
            case TotemType.Recovery: return "+1.5 HP/sec";
            case TotemType.Shielding: return "+25 Shield";
            case TotemType.SecondWind: return "Revive once";
            case TotemType.Dash: return "Unlock Dash";
            case TotemType.BloodPrice: return "Damage scales with missing HP";
            case TotemType.Retaliation: return "Damage nearby enemies";
        }

        return "";
    }

    string GetComparison(TotemData data)
    {
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null)
            return "";

        switch (data.totemType)
        {
            case TotemType.Power:
                return $"Damage: {stats.Damage:F1} → {(stats.Damage * 1.3f):F1}";

            case TotemType.Piercing:
                return $"Pierce: {stats.Pierce} → {stats.Pierce + 1}";

            case TotemType.Ricochet:
                return $"Ricochet: {stats.Ricochet} → {stats.Ricochet + 1}";
        }

        return "";
    }

    // =====================================================
    // UPGRADE GLOW
    // =====================================================

    IEnumerator UpgradeGlow()
    {
        float t = 0f;
        Vector3 baseScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 6f;
            float pulse = 1f + Mathf.Sin(t * 10f) * 0.05f;
            transform.localScale = baseScale * pulse;
            yield return null;
        }

        transform.localScale = baseScale;
    }
}
