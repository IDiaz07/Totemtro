using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(80, 80);
    public float spawnClearRadius = 6f;

    [Header("Poisson Settings")]
    public float poissonRadius = 2.5f;
    public int rejectionSamples = 30;

    [Header("Obstacle Spacing")]
    public float obstacleMinDistance = 1.8f;
    public float densityCheckRadius = 4f;
    public int maxObstaclesNearby = 4;

    [Header("Navigation Corridors")]
    public float corridorScale = 0.03f;
    public float corridorThreshold = 0.35f;

    [Header("Maze Settings")]
    public float mazeSafeRadius = 20f;

    [Header("References")]
    public SpawnTable obstacleTable;
    public Transform obstacleParent;

    public GameObject mazeWallPrefab;
    public GameObject chestPrefab;
    public GameObject slotMachinePrefab;

    [Header("Maze Decoration")]
    public GameObject[] mazeDecorations;

    [Header("POI")]
    public GameObject ritualAltarPrefab;
    public GameObject ritualRockPrefab;
    public GameObject candlePrefab;

    [Header("Biome Noise")]
    public float biomeMacroScale = 0.04f;
    public float biomeMicroScale = 0.15f;

    [Header("Biome Prefabs")]
    public GameObject[] forestPrefabs;
    public GameObject[] clutterPrefabs;

    [Header("Ruins")]
    public GameObject[] ruinWalls;
    public GameObject ruinBenchPrefab;

    [HideInInspector]
    public int spawnedBenches = 0;

    BiomeSystem biomeSystem;

    List<Vector2> points;

    bool mazeActive = false;
    Vector2 mazeCenter;

    void Start()
    {
        biomeSystem = new BiomeSystem(
            biomeMacroScale,
            biomeMicroScale
        );

        GenerateMap();
    }

    GameObject GetBiomePrefab(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Forest:
                return forestPrefabs[
                    Random.Range(0, forestPrefabs.Length)
                ];
        }

        return clutterPrefabs[
            Random.Range(0, clutterPrefabs.Length)
        ];
    }

    void GenerateMap()
    {
        TryGenerateMaze();

        points = PoissonDiscSampling.GeneratePoints(
            poissonRadius,
            mapSize,
            rejectionSamples
        );

        foreach (var p in points)
        {
            Vector2 pos = p - mapSize / 2;

            if (Vector2.Distance(pos, Vector2.zero) < spawnClearRadius)
                continue;

            SpawnObstacle(pos);
        }

        ClusterGenerator.Generate(this);
        POIGenerator.Generate(this);
    }

    void SpawnObstacle(Vector2 pos)
    {
        if (IsInsideMazeArea(pos))
            return;

        if (mazeActive && Vector2.Distance(pos, mazeCenter) < mazeSafeRadius)
            return;

        BiomeType biome = biomeSystem.GetBiome(pos);
        float clutter = biomeSystem.GetClutter(pos);

        if (biome == BiomeType.Clearing)
            return;

        if (IsCorridor(pos))
            return;

        if (biome == BiomeType.Ruins)
        {
            if (Random.value < 0.15f)
                RuinGenerator.SpawnRuin(this, pos);

            return;
        }

        if (clutter < 0.35f)
            return;

        GameObject prefab = GetBiomePrefab(biome);

        if (prefab == null)
            return;

        if (IsBlocked(pos))
            return;

        if (IsTooDense(pos))
            return;

        GameObject obj = Instantiate(
            prefab,
            pos,
            Quaternion.identity,
            obstacleParent
        );

        float scale = Random.Range(0.9f, 1.15f);
        obj.transform.localScale *= scale;
    }

    bool IsCorridor(Vector2 pos)
    {
        float noise = Mathf.PerlinNoise(
            pos.x * corridorScale,
            pos.y * corridorScale
        );

        return noise < corridorThreshold;
    }

    public bool IsBlocked(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            pos,
            obstacleMinDistance,
            LayerMask.GetMask("Obstacle")
        );

        return hit != null;
    }

    bool IsTooDense(Vector2 pos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            pos,
            densityCheckRadius,
            LayerMask.GetMask("Obstacle")
        );

        return hits.Length > maxObstaclesNearby;
    }

    public Vector2 GetRandomPosition()
    {
        float x = Random.Range(-mapSize.x / 2, mapSize.x / 2);
        float y = Random.Range(-mapSize.y / 2, mapSize.y / 2);

        return new Vector2(x, y);
    }

    void TryGenerateMaze()
    {
        if (Random.Range(0, 16) == 0)
        {
            mazeActive = true;

            mazeCenter = GetMazePosition();

            MazeGenerator.Generate(this, mazeCenter);
        }
    }

    public Vector2 GetMazePosition()
    {
        float margin = mazeSafeRadius + 5f;

        float x = Random.Range(
            -mapSize.x / 2 + margin,
            mapSize.x / 2 - margin
        );

        float y = Random.Range(
            -mapSize.y / 2 + margin,
            mapSize.y / 2 - margin
        );

        Vector2 pos = new Vector2(x, y);

        if (Vector2.Distance(pos, Vector2.zero) < spawnClearRadius + 10)
            return GetMazePosition();

        return pos;
    }

    public bool IsInsideMazeArea(Vector2 pos)
    {
        if (!mazeActive)
            return false;

        return Vector2.Distance(pos, mazeCenter) < mazeSafeRadius;
    }

    public Vector2 GetMazeCenter()
    {
        return mazeCenter;
    }
}