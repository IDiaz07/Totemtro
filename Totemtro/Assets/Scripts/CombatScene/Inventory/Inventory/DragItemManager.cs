using UnityEngine;

public class DragItemManager : MonoBehaviour
{
    public static DragItemManager Instance;

    public ItemData item;
    public int amount;

    public DragSourceType sourceType;
    public int sourceIndex;

    public bool IsDragging => item != null;

    void Awake()
    {
        Instance = this;
    }

    public void StartDrag(ItemData item, int amount, DragSourceType type, int index)
    {
        this.item = item;
        this.amount = amount;
        sourceType = type;
        sourceIndex = index;

        DragItemUI.Instance.Show(item, amount);
    }

    public void ClearDrag()
    {
        item = null;
        amount = 0;
        sourceIndex = -1;

        DragItemUI.Instance.Hide();
    }
}

public enum DragSourceType
{
    Inventory,
    ActionBar
}