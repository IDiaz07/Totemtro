using UnityEngine;

public class GoldSystem : MonoBehaviour
{
    public int currentGold = 0;

    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (currentGold < amount)
            return false;

        currentGold -= amount;
        return true;
    }
}
