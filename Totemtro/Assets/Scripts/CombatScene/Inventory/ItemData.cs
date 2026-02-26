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

    public bool stackable = true;
    public int maxStack = 99;

    public bool usableInActionBar;
    public float cooldown;

    public GameObject worldPrefab;

    public ActiveAbilityBase ability;
    public bool isConsumableInLoadout = false;

    public EquipmentSlotType equipmentSlotType;

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
}
