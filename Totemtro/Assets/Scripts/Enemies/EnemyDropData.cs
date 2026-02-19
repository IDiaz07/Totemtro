using UnityEngine;

[System.Serializable]
public class EnemyDropData
{
    public ItemData item;
    public float dropChance = 1f;
    public int minAmount = 1;
    public int maxAmount = 1;
}
