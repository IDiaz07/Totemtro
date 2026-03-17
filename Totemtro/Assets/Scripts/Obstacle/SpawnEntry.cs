using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;

    [Range(0, 100)]
    public float weight = 10;

    public float minDistance = 1.5f;
}