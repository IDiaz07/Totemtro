using UnityEngine;
using System.Collections.Generic;

public class BundleProgressionSystem : MonoBehaviour
{
    public static BundleProgressionSystem Instance;

    Dictionary<string, int> progressionSteps = new();

    void Awake()
    {
        Instance = this;
    }

    public bool CanShowBundle(ShopItemData item)
    {
        if (string.IsNullOrEmpty(item.progressionGroupId))
            return true;

        int currentStep = progressionSteps.ContainsKey(item.progressionGroupId)
            ? progressionSteps[item.progressionGroupId]
            : 0;

        return item.progressionStep == currentStep + 1;
    }

    public void RegisterBundlePurchase(ShopItemData item)
    {
        if (string.IsNullOrEmpty(item.progressionGroupId))
            return;

        progressionSteps[item.progressionGroupId] = item.progressionStep;
    }
}