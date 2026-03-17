using UnityEngine;

public class ArmorUI : MonoBehaviour
{
    // Referencias a los HubSlotUI para cada pieza de armadura,
    // así puedes arrastrar/soltar desde la UI del Hub.
    public HubSlotUI helmetSlot;
    public HubSlotUI chestSlot;
    public HubSlotUI pantsSlot;
    public HubSlotUI bootsSlot;

    void Awake()
    {
        // Asegurar que cada HubSlotUI está configurado como ranura de Armor
        if (helmetSlot != null)
        {
            helmetSlot.slotType = DragSource.Armor;
            helmetSlot.armorSlot = HubSlotUI.ArmorSlotType.Helmet;
        }

        if (chestSlot != null)
        {
            chestSlot.slotType = DragSource.Armor;
            chestSlot.armorSlot = HubSlotUI.ArmorSlotType.Chest;
        }

        if (pantsSlot != null)
        {
            pantsSlot.slotType = DragSource.Armor;
            pantsSlot.armorSlot = HubSlotUI.ArmorSlotType.Pants;
        }

        if (bootsSlot != null)
        {
            bootsSlot.slotType = DragSource.Armor;
            bootsSlot.armorSlot = HubSlotUI.ArmorSlotType.Boots;
        }
    }

    void Start()
    {
        // Forzar refresco visual al iniciar
        helmetSlot?.Refresh();
        chestSlot?.Refresh();
        pantsSlot?.Refresh();
        bootsSlot?.Refresh();
    }
}
