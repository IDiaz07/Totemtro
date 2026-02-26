using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    public static EquipmentSystem Instance;

    public InventorySlot[] equipmentSlots = new InventorySlot[4];

    public System.Action onEquipmentChanged;

    void Awake()
    {
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < equipmentSlots.Length; i++)
            equipmentSlots[i] = new InventorySlot(null, 0);
    }

    int GetSlotIndex(EquipmentSlotType type)
    {
        switch (type)
        {
            case EquipmentSlotType.Helmet: return 0;
            case EquipmentSlotType.Chest: return 1;
            case EquipmentSlotType.Pants: return 2;
            case EquipmentSlotType.Boots: return 3;
            default: return -1;
        }
    }

    public bool EquipItem(ItemData item)
    {
        if (item.itemType != ItemType.Equipment)
            return false;

        int index = GetSlotIndex(item.equipmentSlotType);

        if (index == -1)
            return false;

        equipmentSlots[index].item = item;
        equipmentSlots[index].amount = 1;

        onEquipmentChanged?.Invoke();
        return true;
    }

    public void Unequip(int index)
    {
        if (index < 0 || index >= equipmentSlots.Length)
            return;

        equipmentSlots[index].Clear();
        onEquipmentChanged?.Invoke();
    }
}