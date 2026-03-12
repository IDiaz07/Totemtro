using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TotemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TotemCardUI[] cards;

    [Header("Totems Pool")]
    public List<TotemData> allTotems;

    [Header("Systems")]
    public GoldSystem gold;
    public TotemInventory inventory;
    public TotemSynergySystem synergySystem;

    float legendaryBonusChance = 0f;
    bool isOpen = false;

    void Awake()
    {
        if (gold == null)
            gold = FindFirstObjectByType<GoldSystem>();

        if (inventory == null)
            inventory = FindFirstObjectByType<TotemInventory>();

        if (synergySystem == null)
            synergySystem = FindFirstObjectByType<TotemSynergySystem>();

        panel?.SetActive(false);
    }

    // =====================================================
    // OPEN
    // =====================================================

    public void Open()
    {
        if (isOpen)
            return;

        if (panel == null || cards == null || allTotems == null)
            return;

        panel.SetActive(true);
        GamePause.Pause();
        isOpen = true;

        List<TotemData> used = new List<TotemData>();
        bool hasLegendary = false;

        for (int i = 0; i < cards.Length; i++)
        {
            TotemData random = GetRandomTotem(used);

            if (random != null)
            {
                used.Add(random);
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(random, this);

                if (random.rarity == TotemRarity.Legendary)
                    hasLegendary = true;
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        if (!hasLegendary)
            legendaryBonusChance += 0.02f;
        else
            legendaryBonusChance = 0f;
    }

    // =====================================================
    // CHOOSE
    // =====================================================

    public void Choose(TotemData data)
    {
        if (!isOpen)
            return;

        if (data == null || inventory == null || gold == null)
        {
            ForceClose();
            return;
        }

        TotemData owned = inventory.ownedTotems.Find(o =>
            o.totemType == data.totemType
        );

        int priceToPay = data.price;

        // Upgrade → pagar diferencia
        if (owned != null)
            priceToPay = Mathf.Max(0, data.price - owned.price);

        // Inventario lleno y no es upgrade
        if (inventory.IsFull() && owned == null)
        {
            Debug.Log("Inventory full → need to sell first");
            return; // aquí NO cerramos, porque debe vender antes
        }

        if (!gold.SpendGold(priceToPay))
            return;

        if (!inventory.AddOrUpgradeTotem(data))
        {
            gold.AddGold(priceToPay); // rollback
            ForceClose();
            return;
        }

        synergySystem?.CheckSynergies();
        Close();
    }

    // =====================================================
    // CLOSE
    // =====================================================

    public void Close()
    {
        panel?.SetActive(false);
        isOpen = false;

        GamePause.Reset();
    }

    // =====================================================
    // HARD SAFETY CLOSE
    // =====================================================

    public void ForceClose()
    {
        panel?.SetActive(false);
        isOpen = false;

        GamePause.Reset(); // ← fuerza TimeScale = 1
    }


    // =====================================================
    // RANDOM SELECTION
    // =====================================================

    TotemData GetRandomTotem(List<TotemData> excluded)
    {
        if (allTotems == null || allTotems.Count == 0)
            return null;

        HeroController hero = FindFirstObjectByType<HeroController>();
        if (hero == null || hero.currentHero == null)
            return null;

        HeroType heroType = hero.currentHero.heroType;
        TotemTargetType heroFlag = (TotemTargetType)(1 << (int)heroType);

        float roll = Random.value;
        float legendaryThreshold = 0.9f - legendaryBonusChance;

        TotemRarity rarity =
            roll < 0.65f ? TotemRarity.Common :
            roll < legendaryThreshold ? TotemRarity.Rare :
            TotemRarity.Legendary;

        List<TotemData> candidates = allTotems.FindAll(t =>
        {
            if (t == null) return false;
            if (excluded.Contains(t)) return false;
            if ((t.targetHeroes & heroFlag) == 0) return false;
            if (t.rarity != rarity) return false;

            TotemData owned = inventory?.ownedTotems.Find(o =>
                o.totemType == t.totemType
            );

            if (owned == null)
                return true;

            if (owned.rarity == TotemRarity.Legendary)
                return false;

            return t.rarity > owned.rarity;
        });

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }
}
