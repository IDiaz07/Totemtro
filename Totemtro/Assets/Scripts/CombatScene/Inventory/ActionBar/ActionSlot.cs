[System.Serializable]
public class ActionSlot
{
    public ItemData item;
    public int amount;
    public float cooldownRemaining;

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
        cooldownRemaining = 0f;
    }
}