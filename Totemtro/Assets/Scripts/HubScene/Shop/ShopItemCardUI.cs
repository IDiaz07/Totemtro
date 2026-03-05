using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopItemCardUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    public Image heroIcon;
    public Image rewardIcon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text oldPriceText;

    [Header("Currency Icon")]
    public Image currencyIcon;
    public Sprite goldSprite;
    public Sprite gemSprite;

    [Header("Tags")]
    public GameObject hotTag;
    public GameObject limitedTag;
    public GameObject bonusTag;

    public Button buyButton;

    [Header("Hover Animation")]
    public float hoverScale = 1.08f;
    public float animationSpeed = 8f;

    ShopItemData currentItem;

    Vector3 defaultScale;
    Vector3 targetScale;

    void Awake()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );
    }

    public void Setup(ShopItemData item)
    {
        Debug.Log($"[CARD] Setting up {item.displayName}");
        currentItem = item;
        Debug.Log($"[CARD] {item.displayName} Active after setup: {gameObject.activeSelf}");
        SetupCountdown(item);
        SetupIcons(item);
        SetupText(item);
        SetupPrice(item);
        SetupTags(item);
        SetupButton(item);
    }

    void SetupCountdown(ShopItemData item)
    {
        OfferCountdownUI countdown =
            GetComponent<OfferCountdownUI>();

        if (countdown == null)
            return;

        if (LimitedHeroOfferSystem.Instance == null)
        {
            countdown.gameObject.SetActive(false);
            return;
        }

        if (item.itemType != ShopItemType.Hero ||
            item.heroReward == null)
        {
            countdown.gameObject.SetActive(false);
            return;
        }

        bool onOffer =
            LimitedHeroOfferSystem.Instance
            .IsHeroOnOffer(item.heroReward.heroType);

        countdown.gameObject.SetActive(onOffer);
    }

    void SetupIcons(ShopItemData item)
    {
        if (heroIcon) heroIcon.gameObject.SetActive(false);
        if (rewardIcon) rewardIcon.gameObject.SetActive(false);

        if (item.itemType == ShopItemType.Hero && item.heroReward != null)
        {
            heroIcon.gameObject.SetActive(true);
            heroIcon.sprite = item.heroReward.Icon;
        }
        else
        {
            rewardIcon.gameObject.SetActive(true);
            rewardIcon.sprite = item.icon;
        }
    }

    void SetupText(ShopItemData item)
    {
        if (nameText)
            nameText.text = item.displayName;
    }

    void SetupPrice(ShopItemData item)
    {
        bool isRealMoney =
            item.priceCurrency == ShopCurrencyType.RealMoney;

        // HERO dinámico
        if (item.itemType == ShopItemType.Hero &&
            item.heroReward != null)
        {
            HeroType type =
                item.heroReward.heroType;

            int normalPrice =
                HeroProgressSystem.Instance
                .GetGemUnlockPrice(type);

            int finalPrice = normalPrice;

            if (LimitedHeroOfferSystem.Instance != null)
            {
                finalPrice =
                    HeroProgressSystem.Instance
                    .GetLimitedOfferPrice(type);
            }

            priceText.text = finalPrice.ToString("N0");

            currencyIcon.gameObject.SetActive(true);
            currencyIcon.sprite = gemSprite;

            bool hasDiscount =
                normalPrice > finalPrice;

            oldPriceText.gameObject.SetActive(hasDiscount);

            if (hasDiscount)
                oldPriceText.text =
                    normalPrice.ToString("N0");

            return;
        }

        // RESTO
        float basePrice = item.priceAmount;
        float final =
            ShopSystem.Instance.GetFinalPrice(item);

        if (isRealMoney)
        {
            priceText.text =
                "$" + final.ToString("0.##");

            if (oldPriceText)
            {
                bool hasDiscount =
                    basePrice > final;

                oldPriceText.gameObject
                    .SetActive(hasDiscount);

                if (hasDiscount)
                    oldPriceText.text =
                        "$" + basePrice.ToString("0.##");
            }

            currencyIcon.gameObject.SetActive(false);
            return;
        }

        priceText.text = final.ToString("N0");

        currencyIcon.gameObject.SetActive(true);
        currencyIcon.sprite =
            item.priceCurrency == ShopCurrencyType.Gold
            ? goldSprite
            : gemSprite;

        bool discount =
            basePrice > final;

        oldPriceText.gameObject.SetActive(discount);

        if (discount)
            oldPriceText.text =
                basePrice.ToString("N0");
    }

    void SetupTags(ShopItemData item)
    {
        if (hotTag)
            hotTag.SetActive(item.isHot);

        if (bonusTag)
            bonusTag.SetActive(false);

        if (limitedTag &&
            item.itemType == ShopItemType.Hero &&
            item.heroReward != null &&
            LimitedHeroOfferSystem.Instance != null)
        {
            int normalPrice =
                HeroProgressSystem.Instance
                .GetGemUnlockPrice(
                    item.heroReward.heroType);

            int finalPrice =
                HeroProgressSystem.Instance
                .GetLimitedOfferPrice(
                    item.heroReward.heroType);

            limitedTag.SetActive(
                finalPrice < normalPrice);
        }
    }

    void SetupButton(ShopItemData item)
    {
        buyButton.onClick.RemoveAllListeners();

        buyButton.onClick.AddListener(() =>
        {
            ConfirmationPanelUI.Instance
                ?.Open(item, transform.position);
        });
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        targetScale =
            defaultScale * hoverScale;
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        targetScale = defaultScale;
    }
}