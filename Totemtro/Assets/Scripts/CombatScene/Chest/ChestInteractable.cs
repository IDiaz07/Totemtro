using UnityEngine;

public class ChestInteractable : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 2f;

    [Header("UI")]
    public GameObject interactUI;

    Transform player;
    ChestUI chestUI;

    bool playerInRange = false;
    bool isOpened = false;

    [HideInInspector]
    public bool hasGeneratedLoot = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        chestUI = FindFirstObjectByType<ChestUI>(FindObjectsInactive.Include);

        if (chestUI == null)
        {
            Debug.LogError("No se encontró ChestUI en la escena!");
        }

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= interactRange;

        // Mostrar tecla solo si está cerca y no abierto
        if (interactUI != null)
            interactUI.SetActive(playerInRange && !isOpened);

        // 🔥 Usa tu sistema de input
        if (playerInRange && !isOpened &&
            InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Interact))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (chestUI == null)
        {
            Debug.LogError("ChestUI no asignado en la escena");
            return;
        }

        chestUI.Open(this);
    }

    public void ResetChest()
    {
        isOpened = false;
    }
}