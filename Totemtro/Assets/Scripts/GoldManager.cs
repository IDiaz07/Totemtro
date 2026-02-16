using UnityEngine;
using System;

public class GoldManager : MonoBehaviour
{
    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public bool SpendGold(int amount)
    {
        if (CurrentGold < amount)
            return false;

        CurrentGold -= amount;
        OnGoldChanged?.Invoke(CurrentGold);
        return true;
    }
}
