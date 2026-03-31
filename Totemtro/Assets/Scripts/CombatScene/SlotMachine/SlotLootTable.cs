using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SlotMachine/LootTable")]
public class SlotLootTable : ScriptableObject
{
    public List<SlotLootEntry> items;
}