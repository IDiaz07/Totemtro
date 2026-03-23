using UnityEngine;

/// <summary>
/// Limpia el inventario de run (bag + actionbar) cuando termina la partida
/// </summary>
public class RunInventoryCleaner : MonoBehaviour
{
    public static RunInventoryCleaner Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Limpia bag y actionbar (se pierden al morir)
    /// </summary>
    public void ClearRunInventory()
    {
        ClearBag();
        ClearActionBar();

        Debug.Log("✅ Run inventory cleared (Bag + ActionBar)");
    }

    void ClearBag()
    {
        if (MetaInventory.Instance == null)
        {
            Debug.LogWarning("MetaInventory is NULL — can't clear bag");
            return;
        }

        var bagSlots = MetaInventory.Instance.bagSlots;

        if (bagSlots == null)
            return;

        for (int i = 0; i < bagSlots.Length; i++)
        {
            bagSlots[i].Clear();
        }

        MetaInventory.Instance.NotifyInventoryChanged();
    }

    void ClearActionBar()
    {
        if (ActionBarController.Instance == null)
        {
            Debug.LogWarning("ActionBarController is NULL — can't clear action bar");
            return;
        }

        var slots = ActionBarController.Instance.slots;

        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Clear();
        }

        if (MetaInventory.Instance != null)
            MetaInventory.Instance.NotifyInventoryChanged();
    }

    /// <summary>
    /// Limpia inventario cuando se cierra la aplicación o se pausa (móvil)
    /// </summary>
    void OnApplicationQuit()
    {
        ClearRunInventory();

        if (MetaInventory.Instance != null)
            MetaInventory.Instance.SaveMetaInventory();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ClearRunInventory();

            if (MetaInventory.Instance != null)
                MetaInventory.Instance.SaveMetaInventory();
        }
    }
}