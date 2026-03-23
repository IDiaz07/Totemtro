using UnityEngine;

[CreateAssetMenu(fileName = "MasteryTierIcons", menuName = "Game/Mastery Tier Icons")]
public class MasteryTierIcons : ScriptableObject
{
    [System.Serializable]
    public class TierIcon
    {
        public MasteryTier tier;
        public Sprite icon;
    }

    public TierIcon[] icons;

    public Sprite GetIcon(MasteryTier tier)
    {
        // Buscar icono exacto
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i].tier == tier)
                return icons[i].icon;
        }

        // Fallback: buscar por categoría base (ej: Wood_III → Wood_I)
        MasteryTier baseCategory = GetBaseCategory(tier);

        for (int i = 0; i < icons.Length; i++)
        {
            if (GetBaseCategory(icons[i].tier) == baseCategory)
                return icons[i].icon;
        }

        return null;
    }

    /// <summary>
    /// Agrupa tiers en su categoría base.
    /// Así puedes tener 1 icono por categoría (Wood, Bronze, etc.)
    /// o 1 por cada sub-tier si quieres.
    /// </summary>
    static MasteryTier GetBaseCategory(MasteryTier tier)
    {
        switch (tier)
        {
            case MasteryTier.Wood_III:
            case MasteryTier.Wood_II:
            case MasteryTier.Wood_I:
                return MasteryTier.Wood_I;

            case MasteryTier.Bronze_III:
            case MasteryTier.Bronze_II:
            case MasteryTier.Bronze_I:
                return MasteryTier.Bronze_I;

            case MasteryTier.Silver_III:
            case MasteryTier.Silver_II:
            case MasteryTier.Silver_I:
                return MasteryTier.Silver_I;

            case MasteryTier.Gold_III:
            case MasteryTier.Gold_II:
            case MasteryTier.Gold_I:
                return MasteryTier.Gold_I;

            case MasteryTier.Diamond_III:
            case MasteryTier.Diamond_II:
            case MasteryTier.Diamond_I:
                return MasteryTier.Diamond_I;

            case MasteryTier.Champion_V:
            case MasteryTier.Champion_IV:
            case MasteryTier.Champion_III:
            case MasteryTier.Champion_II:
            case MasteryTier.Champion_I:
                return MasteryTier.Champion_I;

            case MasteryTier.Master:
                return MasteryTier.Master;

            default:
                return MasteryTier.Unranked;
        }
    }
}