using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventoryContainer inventory;
    public InventoryContainer bag;
    public InventoryContainer armor;
    public InventoryContainer loadout;

    void Awake()
    {
        Instance = this;

        inventory = new InventoryContainer(49);
        bag = new InventoryContainer(15);
        armor = new InventoryContainer(4);
        loadout = new InventoryContainer(6);
    }
}