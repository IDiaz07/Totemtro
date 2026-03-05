using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public static ShopPanelUI Instance;

    [Header("Sections")]
    public Transform sectionOffers;
    public Transform sectionHeroes;
    public Transform sectionFragments;
    public Transform sectionCurrency;
    public Transform sectionBundles;

    [Header("Prefabs")]
    public GameObject offerCardPrefab;
    public GameObject currencyTilePrefab;
    public GameObject fragmentCardPrefab;

    [Header("Header UI")]
    public TMP_Text goldText;
    public TMP_Text gemsText;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            ShopSystem.Instance != null &&
            HeroProgressSystem.Instance != null &&
            PlayerShopProfileSystem.Instance != null);

        BuildShop();

        // Fuerza rebuild visual
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            sectionOffers.GetComponent<RectTransform>());

        RefreshHeader();
    }

    public void BuildShop()
    {
        SafeClear(sectionOffers);
        SafeClear(sectionHeroes);
        SafeClear(sectionFragments);
        SafeClear(sectionCurrency);
        SafeClear(sectionBundles);

        BuildOffers();
        BuildHeroes();
        BuildFragments();
        BuildCurrency();
        BuildBundles();
    }

    void BuildOffers()
    {
        if (sectionOffers == null || offerCardPrefab == null)
            return;

        var items = ShopSystem.Instance.GetItemsByCategory(ShopItemCategory.Featured);

        foreach (var item in items)
        {
            var obj = Instantiate(offerCardPrefab, sectionOffers);

            var cardUI = obj.GetComponent<ShopItemCardUI>();
            if (cardUI != null)
                cardUI.Setup(item);
        }
    }

    void BuildHeroes()
    {
        BuildCategory(
            ShopItemCategory.Heroes,
            sectionHeroes,
            offerCardPrefab
        );
    }

    void BuildFragments()
    {
        if (sectionFragments == null || fragmentCardPrefab == null)
        {
            Debug.LogError("Fragments section or prefab not assigned.");
            return;
        }

        List<ShopItemData> items = ShopSystem.Instance.GetDailyFragments();

        if (items == null || items.Count == 0)
            return;

        foreach (var item in items)
        {
            if (item == null) continue;

            var card = Instantiate(fragmentCardPrefab, sectionFragments);

            var ui = card.GetComponent<ShopFragmentCardUI>();

            if (ui != null)
                ui.Setup(item);
            else
                Debug.LogError("Fragment prefab missing ShopFragmentCardUI.");
        }
    }

    void BuildCurrency()
    {
        if (sectionCurrency == null || currencyTilePrefab == null)
        {
            Debug.LogError("Currency section or prefab not assigned.");
            return;
        }

        BuildCategory(ShopItemCategory.Currency, sectionCurrency, currencyTilePrefab);
    }

    void BuildBundles()
    {
        BuildCategory(ShopItemCategory.Bundles, sectionBundles, offerCardPrefab);
    }

    void BuildCategory(ShopItemCategory category, Transform parent, GameObject prefab)
    {
        Debug.Log($"[SHOP] Building category: {category}");

        if (parent == null || prefab == null)
        {
            Debug.LogError($"[SHOP] Parent or Prefab missing for {category}");
            return;
        }

        var items = ShopSystem.Instance.GetItemsByCategory(category);

        // 🔥 FILTRAR SOLO 3 HÉROES EN OFERTA
        if (category == ShopItemCategory.Heroes &&
            LimitedHeroOfferSystem.Instance != null)
        {
            items = items.FindAll(item =>
                item.heroReward != null &&
                LimitedHeroOfferSystem.Instance
                .IsHeroOnOffer(item.heroReward.heroType));
        }

        if (items == null)
        {
            Debug.LogError($"[SHOP] Items NULL for {category}");
            return;
        }

        Debug.Log($"[SHOP] {category} returned {items.Count} items");

        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("[SHOP] Null item skipped");
                continue;
            }

            var obj = Instantiate(prefab, parent);
            obj.SetActive(true);

            Debug.Log($"[SHOP] Instantiated {item.displayName} - Active: {obj.activeSelf}");

            var cardUI = obj.GetComponent<ShopItemCardUI>();
            var currencyUI = obj.GetComponent<ShopCurrencyTileUI>();

            if (cardUI != null)
                cardUI.Setup(item);
            else if (currencyUI != null)
                currencyUI.Setup(item);
            else
                Debug.LogError("[SHOP] Prefab missing UI script.");
        }
    }

    void SafeClear(Transform section)
    {
        if (section == null) return;

        foreach (Transform child in section)
            Destroy(child.gameObject);
    }

    public void RefreshHeader()
    {
        if (MetaCurrencySystem.Instance == null) return;

        goldText.text = MetaCurrencySystem.Instance.Gold.ToString("N0");
        gemsText.text = MetaCurrencySystem.Instance.Gems.ToString("N0");
    }

    void OnEnable()
    {
        MetaCurrencySystem.OnCurrencyChanged += RefreshHeader;
    }

    void OnDisable()
    {
        MetaCurrencySystem.OnCurrencyChanged -= RefreshHeader;
    }
}