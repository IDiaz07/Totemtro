using UnityEngine;

public enum ItemType
{
    Material,
    Consumable,
    Equipment,
    Totem
}

public enum EquipmentSlotType
{
    None = 0,
    Helmet = 1,
    Chest = 2,
    Pants = 3,
    Boots = 4
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [TextArea]
    public string description;

    [Header("Tooltip Stats")]

    public bool showDamage;
    public int damage;

    public bool showDPS;
    public int damagePerSecond;

    public bool showFinalDamage;
    public int finalDamage;

    public bool showHealing;
    public int healingAmount;

    public bool stackable = true;
    public int maxStack = 99;

    public bool usableInActionBar;
    public float cooldown;

    public GameObject worldPrefab;

    public ActiveAbilityBase ability;
    public bool isConsumableInLoadout = false;

    public EquipmentSlotType equipmentSlotType;

    [Header("Armor")]
    [Range(0f, 1f)]
    public float damageReduction;

    public virtual void Use(GameObject user)
    {
        Debug.Log("Use llamado");

        if (ability == null)
        {
            Debug.LogError("Ability es NULL en: " + itemName);
            return;
        }

        ability.TryActivate(user);
    }

#if UNITY_EDITOR
void OnValidate()
{
    if (string.IsNullOrEmpty(itemID))
    {
        itemID = name.Replace(" ", "_").ToLower();
    }
}
#endif
}
