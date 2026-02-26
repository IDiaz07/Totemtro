using UnityEngine;
using TMPro;

public class InventoryGoldUI : MonoBehaviour
{
    public TMP_Text goldText;

    GoldSystem gold;

    void Awake()
    {
        gold = FindFirstObjectByType<GoldSystem>();
    }

    void Update()
    {
        if (gold != null)
            goldText.text = gold.currentGold.ToString();
    }
}
