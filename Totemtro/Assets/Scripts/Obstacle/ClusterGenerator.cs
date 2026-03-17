using UnityEngine;

public static class ClusterGenerator
{
    public static void Generate(MapGenerator map)
    {
        int clusters = Random.Range(5, 8);

        for (int i = 0; i < clusters; i++)
        {
            Vector2 center = map.GetRandomPosition();

            int size = Random.Range(6, 12);

            for (int j = 0; j < size; j++)
            {
                Vector2 pos = center + Random.insideUnitCircle * 2.5f;

                if (map.IsInsideMazeArea(pos))
                    continue;

                if (map.IsBlocked(pos))
                    continue;

                GameObject prefab = map.obstacleTable.GetRandom();

                Object.Instantiate(
                    prefab,
                    pos,
                    Quaternion.identity,
                    map.obstacleParent
                );
            }
        }
    }
}