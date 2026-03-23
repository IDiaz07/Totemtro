using System.Collections.Generic;
using UnityEngine;

public enum HeroType
{
    Vex,
    Murray,
    Kael,
    Grim,
    Orin,
    Nyra,
    Tro,
    Selene
}

public enum HeroRarity
{
    Rare,
    Epic,
    Legendary
}

public enum HeroRole
{
    Melee,
    Ranged,
    Support,
    Tank,
    Assassin,
    Mage
}

// =========================================
// MASTERY SYSTEM
// =========================================

public enum MasteryTier
{
    Unranked,   // Vacío — punto de partida
    Wood_III,
    Wood_II,
    Wood_I,
    Bronze_III,
    Bronze_II,
    Bronze_I,
    Silver_III,
    Silver_II,
    Silver_I,
    Gold_III,
    Gold_II,
    Gold_I,
    Diamond_III,
    Diamond_II,
    Diamond_I,
    Champion_V,
    Champion_IV,
    Champion_III,
    Champion_II,
    Champion_I,
    Master
}

[System.Serializable]
public class HeroDirectionalSprites
{
    public Sprite FrontView;
    public Sprite BackView;
    public Sprite LeftView;
    public Sprite RightView;

    public Sprite FrontLeftView;
    public Sprite FrontRightView;
    public Sprite BackLeftView;
    public Sprite BackRightView;
}

[CreateAssetMenu(fileName = "NewHero", menuName = "Game/Hero")]
public class HeroData : ScriptableObject
{
    [Header("Identity")]
    public HeroType heroType;

    [Header("Info")]
    public string heroName;

    [TextArea]
    public string description;

    [Header("UI Icons")]
    public Sprite Icon;
    public Sprite ChampsHeadIcon;

    [Header("Role")]
    public HeroRole role;

    [Header("Rarity")]
    public HeroRarity rarity;

    [Header("Directional Sprites")]
    public HeroDirectionalSprites directionalSprites;

    [Header("Base Stats")]
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float fireRate;

    [Header("Weapon")]
    public List<WeaponData> weapons;

    [Header("Unlock")]
    public bool unlockedByDefault = true;
    public int gemCost = 0;

    [Header("Level Scaling (0–9)")]
    public AnimationCurve healthScaling = AnimationCurve.Linear(1, 1, 9, 1.5f);
    public AnimationCurve damageScaling = AnimationCurve.Linear(1, 1, 9, 1.5f);
    public AnimationCurve speedScaling = AnimationCurve.Linear(1, 1, 9, 1.2f);
    public AnimationCurve fireRateScaling = AnimationCurve.Linear(1, 1, 9, 1.3f);

    [Header("Limited Offer")]
    public bool hasLimitedOffer;
    public int limitedDiscountPercent;
    public int limitedDurationHours;

    // =========================================
    // MASTERY — persistente por héroe
    // =========================================

    /// <summary>
    /// XP total necesaria para cada tier.
    /// Exponencial: empieza fácil, se vuelve brutal.
    /// </summary>
    public static int GetXPForTier(MasteryTier tier)
    {
        switch (tier)
        {
            case MasteryTier.Unranked:    return 0;
            case MasteryTier.Wood_III:    return 50;
            case MasteryTier.Wood_II:     return 120;
            case MasteryTier.Wood_I:      return 220;
            case MasteryTier.Bronze_III:   return 380;
            case MasteryTier.Bronze_II:    return 600;
            case MasteryTier.Bronze_I:     return 900;
            case MasteryTier.Silver_III:   return 1350;
            case MasteryTier.Silver_II:    return 1950;
            case MasteryTier.Silver_I:     return 2750;
            case MasteryTier.Gold_III:     return 3900;
            case MasteryTier.Gold_II:      return 5500;
            case MasteryTier.Gold_I:       return 7800;
            case MasteryTier.Diamond_III:  return 11000;
            case MasteryTier.Diamond_II:   return 15500;
            case MasteryTier.Diamond_I:    return 22000;
            case MasteryTier.Champion_V:   return 31000;
            case MasteryTier.Champion_IV:  return 42000;
            case MasteryTier.Champion_III: return 56000;
            case MasteryTier.Champion_II:  return 75000;
            case MasteryTier.Champion_I:   return 100000;
            case MasteryTier.Master:       return 140000;
            default:                       return 0;
        }
    }

    public static string GetTierDisplayName(MasteryTier tier)
    {
        switch (tier)
        {
            case MasteryTier.Unranked:     return "UNRANKED";
            case MasteryTier.Wood_III:     return "WOOD III";
            case MasteryTier.Wood_II:      return "WOOD II";
            case MasteryTier.Wood_I:       return "WOOD I";
            case MasteryTier.Bronze_III:   return "BRONZE III";
            case MasteryTier.Bronze_II:    return "BRONZE II";
            case MasteryTier.Bronze_I:     return "BRONZE I";
            case MasteryTier.Silver_III:   return "SILVER III";
            case MasteryTier.Silver_II:    return "SILVER II";
            case MasteryTier.Silver_I:     return "SILVER I";
            case MasteryTier.Gold_III:     return "GOLD III";
            case MasteryTier.Gold_II:      return "GOLD II";
            case MasteryTier.Gold_I:       return "GOLD I";
            case MasteryTier.Diamond_III:  return "DIAMOND III";
            case MasteryTier.Diamond_II:   return "DIAMOND II";
            case MasteryTier.Diamond_I:    return "DIAMOND I";
            case MasteryTier.Champion_V:   return "CHAMPION V";
            case MasteryTier.Champion_IV:  return "CHAMPION IV";
            case MasteryTier.Champion_III: return "CHAMPION III";
            case MasteryTier.Champion_II:  return "CHAMPION II";
            case MasteryTier.Champion_I:   return "CHAMPION I";
            case MasteryTier.Master:       return "MASTER";
            default:                       return "???";
        }
    }

    /// <summary>
    /// Dado un XP total, devuelve el tier actual.
    /// </summary>
    public static MasteryTier GetTierFromXP(int totalXP)
    {
        MasteryTier result = MasteryTier.Unranked;

        foreach (MasteryTier tier in System.Enum.GetValues(typeof(MasteryTier)))
        {
            if (totalXP >= GetXPForTier(tier))
                result = tier;
            else
                break;
        }

        return result;
    }

    /// <summary>
    /// Progreso 0–1 dentro del tier actual hacia el siguiente.
    /// </summary>
    public static float GetTierProgress(int totalXP)
    {
        MasteryTier current = GetTierFromXP(totalXP);

        if (current == MasteryTier.Master)
            return 1f;

        int next = (int)current + 1;

        if (next > (int)MasteryTier.Master)
            return 1f;

        MasteryTier nextTier = (MasteryTier)next;

        int currentXP = GetXPForTier(current);
        int nextXP = GetXPForTier(nextTier);

        int range = nextXP - currentXP;

        if (range <= 0)
            return 1f;

        return Mathf.Clamp01((float)(totalXP - currentXP) / range);
    }
}