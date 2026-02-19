using UnityEngine;

public class ActionBarController : MonoBehaviour
{
    public PlayerInventory inventory;

    public ActionSlot[] slots = new ActionSlot[8];

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ActionSlot();
    }

    void Update()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                TryUseSlot(i);
            }

            if (slots[i].cooldownRemaining > 0f)
                slots[i].cooldownRemaining -= Time.deltaTime;
        }
    }

    public void AssignToSlot(ItemData item, int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        if (!item.usableInActionBar)
            return;

        slots[index].item = item;
    }

    void TryUseSlot(int index)
    {
        ActionSlot slot = slots[index];

        if (slot.item == null)
            return;

        if (slot.cooldownRemaining > 0f)
            return;

        if (!inventory.HasItem(slot.item, 1))
            return;

        slot.item.Use(gameObject);

        inventory.RemoveItem(slot.item, 1);

        slot.cooldownRemaining = slot.item.cooldown;
    }

    public void AssignToFirstFreeSlot(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                AssignToSlot(item, i);
                break;
            }
        }
    }

}
