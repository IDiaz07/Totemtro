using UnityEngine;

public class ActionBarUIController : MonoBehaviour
{
    public ActionBarController actionBar;
    public GameObject slotPrefab;
    public Transform container;

    ActionSlotUI[] slotsUI;

    void Start()
    {
        GenerateSlots();
    }

    void GenerateSlots()
    {
        slotsUI = new ActionSlotUI[actionBar.slots.Length];

        for (int i = 0; i < actionBar.slots.Length; i++)
        {
            GameObject slotObj =
                Instantiate(slotPrefab, container);

            ActionSlotUI slotUI =
                slotObj.GetComponent<ActionSlotUI>();

            slotUI.Setup(i);

            slotsUI[i] = slotUI;
        }
    }

    void Update()
    {
        for (int i = 0; i < slotsUI.Length; i++)
        {
            var slot = actionBar.slots[i];

            if (slot.item != null)
            {
                slotsUI[i].SetItem(slot.item);
                slotsUI[i].UpdateCooldown(
                    slot.cooldownRemaining,
                    slot.item.cooldown
                );

                int amount =
                    actionBar.inventory.GetAmount(slot.item);

                slotsUI[i].UpdateAmount(amount);
            }
            else
            {
                slotsUI[i].Clear();
            }
        }
    }
}
