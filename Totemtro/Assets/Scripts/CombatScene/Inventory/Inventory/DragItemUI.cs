using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragItemUI : MonoBehaviour
{
    public static DragItemUI Instance;

    public Image icon;

    public bool IsDragging { get; private set; }
    public ItemData draggedItem;
    public int draggedAmount;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (IsDragging)
            transform.position = Input.mousePosition;
    }

    public void Show(ItemData item, int amount)
    {
        if (item == null) return;

        draggedItem = item;
        draggedAmount = amount;

        icon.sprite = item.icon;
        icon.color = Color.white;

        IsDragging = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        IsDragging = false;
        draggedItem = null;
        draggedAmount = 0;

        gameObject.SetActive(false);
    }
}
