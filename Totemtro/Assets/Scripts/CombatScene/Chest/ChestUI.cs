using UnityEngine;

public class ChestUI : MonoBehaviour
{
    public GameObject panel;
    public Transform chestGridParent;
    public GameObject slotPrefab;

    public int width = 5;
    public int height = 3;

    public static ChestInventory CurrentChest;
    public bool IsOpen => panel != null && panel.activeSelf;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf)
            return;

        // ESC o tecla de interact
        if (Input.GetKeyDown(KeyCode.Escape) ||
            InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Interact))
        {
            Close();
        }
    }

    public void Open(ChestInteractable chest)
    {
        panel.SetActive(true);

        CurrentChest = chest.GetComponent<ChestInventory>();

        if (CurrentChest == null)
        {
            Debug.LogError("ChestInventory missing!");
            return;
        }

        // 🔥 Inicializar si no existe
        if (CurrentChest.slots == null || CurrentChest.slots.Length == 0)
        {
            CurrentChest.Initialize(width * height);
        }

        // 🔥 Generar loot UNA sola vez
        if (!chest.hasGeneratedLoot)
        {
            GenerateLoot(chest);
            chest.hasGeneratedLoot = true;
        }

        // 🔥 Crear UI
        GenerateUI();

        Time.timeScale = 0f;
    }

    void GenerateUI()
    {
        foreach (Transform child in chestGridParent)
            Destroy(child.gameObject);

        for (int i = 0; i < CurrentChest.slots.Length; i++)
        {
            GameObject obj = Instantiate(slotPrefab, chestGridParent);
            HubSlotUI slot = obj.GetComponent<HubSlotUI>();

            slot.slotType = DragSource.Chest; // 🔥 IMPORTANTE
            slot.slotIndex = i;
        }
    }

    void GenerateLoot(ChestInteractable chest)
    {
        var chestInventory = chest.GetComponent<ChestInventory>();
        var loot = chest.GetComponent<ChestLootTable>();

        if (loot == null)
        {
            Debug.LogError("ChestLootTable missing!");
            return;
        }

        var items = loot.GenerateLoot(chestInventory.slots.Length);

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                chestInventory.slots[i].item = items[i].item;
                chestInventory.slots[i].amount = items[i].amount;
            }
        }

        chestInventory.onChestChanged?.Invoke();
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        if (CurrentChest != null)
        {
            var interactable = CurrentChest.GetComponent<ChestInteractable>();

            if (interactable != null)
                interactable.ResetChest();
        }

        CurrentChest = null;

        Time.timeScale = 1f;
    }
}