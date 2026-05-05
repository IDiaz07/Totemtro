using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachineSystem : MonoBehaviour
{
    public static SlotMachineSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Spin()
    {
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        yield return new WaitForSeconds(2f);

        List<ShopItemData> featured =
            ShopSystem.Instance.GetFeaturedItems();

        if (featured == null || featured.Count == 0)
            yield break;

        var reward = featured[0];

        // Pasamos Vector3.zero porque no viene de UI
        ShopPurchaseHandler.Execute(reward, Vector3.zero);
    }
}