using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance;

    private IStoreController controller;
    private IExtensionProvider extensions;

    // 🔹 PRODUCT IDS (DEBEN COINCIDIR CON DASHBOARD / GOOGLE / APPLE)
    public const string STARTER_PACK = "starter_pack";
    public const string GEM_PACK_SMALL = "gem_pack_small";
    public const string GEM_PACK_BIG = "gem_pack_big";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIAP();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    void InitializeIAP()
    {
        if (controller != null)
            return;

        var module = StandardPurchasingModule.Instance();
        module.useFakeStoreAlways = true; // 👈 IMPORTANTE

        var builder = ConfigurationBuilder.Instance(module);

        builder.AddProduct(STARTER_PACK, ProductType.Consumable);
        builder.AddProduct(GEM_PACK_SMALL, ProductType.Consumable);
        builder.AddProduct(GEM_PACK_BIG, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController ctrl, IExtensionProvider ext)
    {
        controller = ctrl;
        extensions = ext;
        Debug.Log("✅ IAP Initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("❌ IAP Init Failed: " + error);
    }

    // 👇 ESTA ES LA FIRMA NUEVA QUE TE FALTABA
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("❌ IAP Init Failed: " + error + " | " + message);
    }

    // --------------------------------------------------
    // PURCHASE
    // --------------------------------------------------

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("🛒 Purchase Success: " + args.purchasedProduct.definition.id);

        switch (args.purchasedProduct.definition.id)
        {
            case STARTER_PACK:
                GiveStarterPack();
                break;

            case GEM_PACK_SMALL:
                MetaCurrencySystem.Instance.AddGems(100);
                break;

            case GEM_PACK_BIG:
                MetaCurrencySystem.Instance.AddGems(500);
                break;
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogError("❌ Purchase Failed: " + reason);
    }

    // --------------------------------------------------
    // PUBLIC METHODS
    // --------------------------------------------------

    public void Buy(string productId)
    {
        if (controller == null)
        {
            Debug.LogError("IAP not initialized.");
            return;
        }

        controller.InitiatePurchase(productId);
    }

    public string GetLocalizedPrice(string productId)
    {
        if (controller == null)
            return null;

        Product product = controller.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            return product.metadata.localizedPriceString;
        }

        return null;
    }

    // --------------------------------------------------
    // REWARDS
    // --------------------------------------------------

    void GiveStarterPack()
    {
        MetaCurrencySystem.Instance.AddGems(200);
        MetaCurrencySystem.Instance.AddGold(5000);
    }
}