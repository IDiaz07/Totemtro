using UnityEngine;

public class SlotMachineWorld : MonoBehaviour
{
    public SlotMachine slotMachine;

    bool playerInRange = false;

    void Start()
    {
        Debug.Log("SlotMachineWorld START");
    }

    void Awake()
    {
        // 🔥 Busca incluso si está desactivado (CLAVE)
        if (slotMachine == null)
        {
            slotMachine = FindFirstObjectByType<SlotMachine>(
                FindObjectsInactive.Include
            );
        }

        if (slotMachine == null)
        {
            Debug.LogError("No se encontró SlotMachine en la escena");
        }
    }

    void Update()
    {
        Debug.Log("Update funcionando");

        if (!playerInRange) return;
        if (GameInputLock.IsLocked) return;

        if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Interact))
        {
            Debug.Log("PULSASTE F");
            Interact();
        }
    }

    void Interact()
    {
        if (slotMachine == null)
        {
            Debug.LogError("SlotMachine NULL al interactuar");
            return;
        }

        HeroController player = FindFirstObjectByType<HeroController>();

        if (player == null) return;

        GameInputLock.Lock();

        slotMachine.Open(player);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTER: " + other.name + " TAG: " + other.tag);

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player ENTRA en rango");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player SALE del rango");
        }
    }
}