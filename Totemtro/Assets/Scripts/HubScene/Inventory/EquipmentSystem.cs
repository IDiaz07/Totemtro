using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    public static EquipmentSystem Instance;

    public InventorySlot[] equipmentSlots = new InventorySlot[4];

    public System.Action onEquipmentChanged;

    PlayerStats playerStats;

    void Awake()
    {
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < equipmentSlots.Length; i++)
            equipmentSlots[i] = new InventorySlot(null, 0);

        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    // =====================================================
    // ENUM → INDEX
    // =====================================================

    public int GetIndex(EquipmentSlotType type)
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

    // =====================================================
    // EQUIP
    // =====================================================

    public bool EquipItem(ItemData item, int slotIndex)
    {
        if (item == null)
            return false;

        if (item.itemType != ItemType.Equipment)
            return false;

        if (slotIndex < 0 || slotIndex >= equipmentSlots.Length)
            return false;

        // 🔥 VALIDACIÓN CORRECTA (FIX CLAVE)
        int correctIndex = GetIndex(item.equipmentSlotType);

        if (correctIndex != slotIndex)
        {
            Debug.LogWarning("Intentando equipar item en slot incorrecto");
            return false;
        }

        InventorySlot current = equipmentSlots[slotIndex];

        // 🔥 SWAP AUTOMÁTICO
        if (current.item != null)
        {
            var bag = MetaInventory.Instance?.bagSlots;

            if (bag != null)
            {
                foreach (var b in bag)
                {
                    if (b.IsEmpty())
                    {
                        b.item = current.item;
                        b.amount = 1;
                        break;
                    }
                }
            }
        }

        // 🔥 EQUIPAR
        current.item = item;
        current.amount = 1;

        onEquipmentChanged?.Invoke();

        // 🔥 RECALCULAR STATS
        if (playerStats != null)
            playerStats.Recalculate();

        // 🔥 REFRESH UI GLOBAL
        MetaInventory.Instance?.NotifyInventoryChanged();

        return true;
    }

    // =====================================================
    // UNEQUIP
    // =====================================================

    public void Unequip(int index)
    {
        if (index < 0 || index >= equipmentSlots.Length)
            return;

        equipmentSlots[index].Clear();

        onEquipmentChanged?.Invoke();

        MetaInventory.Instance?.NotifyInventoryChanged();
    }
}