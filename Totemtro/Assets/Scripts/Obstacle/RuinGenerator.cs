using UnityEngine;

public static class RuinGenerator
{
    public static void SpawnRuin(MapGenerator map, Vector2 center)
    {
        int width = Random.Range(3, 6);
        int height = Random.Range(2, 4);

        float tileSize = 2f;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (Random.value < 0.2f)
                    continue;

                Vector2 pos = center + new Vector2(x * tileSize, y * tileSize);

                if (map.IsBlocked(pos))
                    continue;

                GameObject prefab;

                // 10% banco pero máximo 3
                if (Random.value < 0.1f && map.spawnedBenches < 3)
                {
                    prefab = map.ruinBenchPrefab;
                    map.spawnedBenches++;
                }
                else
                {
                    prefab = map.ruinWalls[
                        Random.Range(0, map.ruinWalls.Length)
                    ];
                }

                Object.Instantiate(prefab, pos, Quaternion.identity, map.obstacleParent);
            }
    }
}