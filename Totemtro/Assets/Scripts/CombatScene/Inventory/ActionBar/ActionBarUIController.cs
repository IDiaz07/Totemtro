using UnityEngine;

public class ActionBarUIController : MonoBehaviour
{
    public ActionBarController actionBar;
    public GameObject slotPrefab;
    public Transform container;

    ActionSlotUI[] slotsUI;

    void Start()
    {
        slotsUI = new ActionSlotUI[actionBar.slots.Length];

        for (int i = 0; i < actionBar.slots.Length; i++)
        {
            GameObject obj = Instantiate(slotPrefab, container);
            ActionSlotUI ui = obj.GetComponent<ActionSlotUI>();
            ui.Setup(i);
            slotsUI[i] = ui;
        }
    }

    void Awake()
    {
        if (actionBar == null)
            actionBar = FindFirstObjectByType<ActionBarController>();
    }
}