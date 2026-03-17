using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural/SpawnTable")]
public class SpawnTable : ScriptableObject
{
    public List<SpawnEntry> entries;

    public GameObject GetRandom()
    {
        float total = 0;

        foreach (var e in entries)
            total += e.weight;

        float roll = Random.Range(0, total);

        float cumulative = 0;

        foreach (var e in entries)
        {
            cumulative += e.weight;

            if (roll <= cumulative)
                return e.prefab;
        }

        return null;
    }
}