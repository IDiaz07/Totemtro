using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    public Image icon;
    public TMP_Text amountText;

    [HideInInspector] public int slotIndex;

    RunInventory inventory;

    ItemData currentItem;
    int currentAmount;

    public static int draggingFromIndex = -1;

    bool isPointerOver = false;

    float holdTimer = 0f;
    float repeatTimer = 0f;

    float initialDelay = 0.4f;
    float repeatDelay = 0.25f;
    float minRepeatDelay = 0.05f;
    float acceleration = 0.9f;

    void Awake()
    {
        inventory = FindFirstObjectByType<RunInventory>();
    }

    void Update()
    {
        if (!isPointerOver)
            return;

        if (currentItem == null)
            return;

        // TAP Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleDropInput();
            holdTimer = 0f;
            repeatTimer = 0f;
        }

        // HOLD Q
        if (Input.GetKey(KeyCode.Q))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer > initialDelay)
            {
                repeatTimer += Time.deltaTime;

                if (repeatTimer >= repeatDelay)
                {
                    inventory.DropOne(slotIndex);

                    repeatTimer = 0f;

                    repeatDelay = Mathf.Max(
                        minRepeatDelay,
                        repeatDelay * acceleration
                    );
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            holdTimer = 0f;
            repeatTimer = 0f;
            repeatDelay = 0.25f;
        }
    }

    void HandleDropInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inventory.DropStack(slotIndex);
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            inventory.DropHalf(slotIndex);
        }
        else
        {
            inventory.DropOne(slotIndex);
        }
    }

    public void Setup(ItemData item, int amount)
    {
        currentItem = item;
        currentAmount = amount;

        if (item == null)
        {
            icon.sprite = null;
            icon.color = new Color(1, 1, 1, 0);
            amountText.text = "";
            return;
        }

        icon.sprite = item.icon;
        icon.color = Color.white;
        amountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        draggingFromIndex = slotIndex;

        icon.color = new Color(1, 1, 1, 0);
        amountText.text = "";

        DragItemUI.Instance.Show(currentItem, currentAmount);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragItemUI.Instance.Hide();
        inventory.NotifyInventoryChanged();
        draggingFromIndex = -1;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggingFromIndex == -1)
            return;

        if (draggingFromIndex == slotIndex)
            return;

        inventory.MoveItem(draggingFromIndex, slotIndex);
        draggingFromIndex = -1;
    }
}