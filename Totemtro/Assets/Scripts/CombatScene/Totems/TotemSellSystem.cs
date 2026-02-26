using UnityEngine;

public class TotemSellSystem : MonoBehaviour
{
    TotemInventory inventory;
    GoldSystem gold;

    void Awake()
    {
        inventory = GetComponent<TotemInventory>();
        gold = GetComponent<GoldSystem>();
    }

    public void SellTotem(TotemData data)
    {
        if (inventory == null || gold == null || data == null)
            return;

        // El propio inventory se encarga de quitarlo
        int sellValue = inventory.SellTotem(data);

        if (sellValue > 0)
        {
            gold.AddGold(sellValue);
            Debug.Log("Sold " + data.totemName + " for " + sellValue);
        }
    }
}
