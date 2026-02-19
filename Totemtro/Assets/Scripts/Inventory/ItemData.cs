using UnityEngine;

public enum ItemType
{
    Material,
    Consumable,
    Special,
    Totem
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
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

    public virtual void Use(GameObject user)
    {
        Debug.Log("Using item: " + itemName);
    }


}
