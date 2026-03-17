using UnityEngine;

public static class POIGenerator
{
    public static void Generate(MapGenerator map)
    {
        int count = Random.Range(2, 4);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = map.GetRandomPosition();

            if (map.IsInsideMazeArea(pos))
                continue;

            SpawnRitual(map, pos);
        }
    }

    static void SpawnRitual(MapGenerator map, Vector2 center)
    {
        Object.Instantiate(
            map.ritualAltarPrefab,
            center,
            Quaternion.identity
        );

        Object.Instantiate(
            map.ritualRockPrefab,
            center + new Vector2(2, 0),
            Quaternion.identity
        );

        Object.Instantiate(
            map.ritualRockPrefab,
            center + new Vector2(-2, 0),
            Quaternion.identity
        );

        Object.Instantiate(
            map.ritualRockPrefab,
            center + new Vector2(0, 2),
            Quaternion.identity
        );

        Object.Instantiate(
            map.candlePrefab,
            center + new Vector2(0, -2),
            Quaternion.identity
        );
    }
}