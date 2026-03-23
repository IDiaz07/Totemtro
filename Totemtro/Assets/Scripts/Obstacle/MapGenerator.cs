using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(320, 320);
    public float spawnClearRadius = 6f;

    [Header("Map Shape")]
    public float islandRadius = 35f;
    public float edgeBlend = 6f;
    public float shapeNoiseScale = 0.05f;
    public float shapeNoiseStrength = 6f;

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

    [Header("Ground")]
    public Tilemap groundTilemap;
    public TileBase[] grassTiles;
    public TileBase sandTile;

    [Header("Island Material")]
    public Material islandEdgeMaterial; // Nuevo: material para bordes redondeados

    [Header("Edge Blending")]
    public Tilemap edgeTilemap;
    public TileBase[] edgeTransitionTiles;
    public Sprite edgeFadeSprite;
    public Transform edgeParent;

    [Header("Terrain System")]
    public bool useNewTerrainSystem = false;
    public TerrainDataMap terrainDataMap;
    public TerrainRenderer terrainRenderer;

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

    float seedX;
    float seedY;

    void Start()
    {
        seedX = Random.Range(0f, 9999f);
        seedY = Random.Range(0f, 9999f);

        biomeSystem = new BiomeSystem(
            biomeMacroScale,
            biomeMicroScale
        );

        GenerateMap();
    }

    // =========================
    // 🌍 GENERACIÓN GENERAL
    // =========================

    void GenerateMap()
    {
        GenerateGround();

        // Configurar material en lugar de máscara
        ApplyIslandMaterial();

        TryGenerateMaze();

        points = PoissonDiscSampling.GeneratePoints(
            poissonRadius,
            mapSize,
            rejectionSamples
        );

        foreach (var p in points)
        {
            Vector2 pos = new Vector2(
                p.x - mapSize.x / 2f,
                p.y - mapSize.y / 2f
            );

            if (Vector2.Distance(pos, Vector2.zero) < spawnClearRadius)
                continue;

            SpawnObstacle(pos);
        }

        ClusterGenerator.Generate(this);
        POIGenerator.Generate(this);
    }

    // =========================
    // 🎨 APLICAR MATERIAL
    // =========================

    void ApplyIslandMaterial()
    {
        if (islandEdgeMaterial != null && groundTilemap != null)
        {
            // Aplicar el material al Tilemap
            TilemapRenderer renderer = groundTilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                renderer.material = islandEdgeMaterial;
                
                // Pasar parámetros al shader
                islandEdgeMaterial.SetVector("_MapCenter", Vector2.zero);
                islandEdgeMaterial.SetFloat("_IslandRadius", islandRadius);
                islandEdgeMaterial.SetFloat("_EdgeBlend", edgeBlend);
                islandEdgeMaterial.SetFloat("_ShapeNoiseScale", shapeNoiseScale);
                islandEdgeMaterial.SetFloat("_ShapeNoiseStrength", shapeNoiseStrength);
                islandEdgeMaterial.SetFloat("_SeedX", seedX);
                islandEdgeMaterial.SetFloat("_SeedY", seedY);
            }
        }
    }

    // =========================
    // 🏝 FORMA DEL MAPA
    // =========================

    float GetIslandValue(Vector2 pos)
    {
        float noise = Mathf.PerlinNoise(
            (pos.x + seedX) * shapeNoiseScale,
            (pos.y + seedY) * shapeNoiseScale
        );

        float maxDist = Mathf.Min(mapSize.x, mapSize.y) * 0.5f;
        float dist = pos.magnitude;

        float island = maxDist * (0.7f + noise * 0.4f);

        return dist - island;
    }

    // Sub-pixel sampling: muestrea en sub-posiciones para suavizar
    float GetIslandValueSmooth(float x, float y)
    {
        float sum = 0f;

        // 12 muestras en un patrón circular alrededor del centro del tile
        // Esto "redondea" el borde en vez de dejarlo cuadrado
        sum += GetIslandValue(new Vector2(x, y));
        sum += GetIslandValue(new Vector2(x + 0.4f, y));
        sum += GetIslandValue(new Vector2(x - 0.4f, y));
        sum += GetIslandValue(new Vector2(x, y + 0.4f));
        sum += GetIslandValue(new Vector2(x, y - 0.4f));
        sum += GetIslandValue(new Vector2(x + 0.3f, y + 0.3f));
        sum += GetIslandValue(new Vector2(x - 0.3f, y + 0.3f));
        sum += GetIslandValue(new Vector2(x + 0.3f, y - 0.3f));
        sum += GetIslandValue(new Vector2(x - 0.3f, y - 0.3f));
        sum += GetIslandValue(new Vector2(x + 0.45f, y + 0.15f));
        sum += GetIslandValue(new Vector2(x - 0.15f, y + 0.45f));
        sum += GetIslandValue(new Vector2(x + 0.15f, y - 0.45f));

        return sum / 12f;
    }

    public bool IsWalkable(Vector2 pos)
    {
        return GetIslandValue(pos) <= 0;
    }

    public bool IsSandZone(Vector2 pos)
    {
        float value = GetIslandValue(pos);
        return value > -edgeBlend && value <= 0;
    }

    public bool IsGrassZone(Vector2 pos)
    {
        float value = GetIslandValue(pos);
        return value <= -edgeBlend;
    }

    // =========================
    // 🎨 SUELO
    // =========================

    void GenerateGround()
    {
        groundTilemap.ClearAllTiles();

        int halfX = Mathf.RoundToInt(mapSize.x / 2);
        int halfY = Mathf.RoundToInt(mapSize.y / 2);

        // Generar un poco más grande para que el shader redondee limpio
        int margin = 5;

        for (int x = -halfX - margin; x < halfX + margin; x++)
        {
            for (int y = -halfY - margin; y < halfY + margin; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float edge = GetIslandValue(pos);

                // Generar tiles hasta un poco fuera del borde
                // El shader se encarga de suavizar el fade
                if (edge > 3f)
                    continue;

                Vector3Int tilePos = new Vector3Int(x, y, 0);

                if (edge > -edgeBlend)
                {
                    float t = Mathf.InverseLerp(0f, -edgeBlend, edge);

                    float noise = Mathf.PerlinNoise(
                        (x + seedX) * 0.2f,
                        (y + seedY) * 0.2f
                    );
                    t += (noise - 0.5f) * 0.3f;
                    t = Mathf.Clamp01(t);

                    if (Random.value > t)
                        groundTilemap.SetTile(tilePos, sandTile);
                    else
                        groundTilemap.SetTile(tilePos, grassTiles[Random.Range(0, grassTiles.Length)]);
                }
                else
                {
                    groundTilemap.SetTile(tilePos, grassTiles[Random.Range(0, grassTiles.Length)]);
                }

                float v = Random.Range(0.88f, 1.08f);
                groundTilemap.SetColor(tilePos, new Color(v, v * 0.96f, v));

                groundTilemap.SetTransformMatrix(
                    tilePos,
                    Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.Euler(0, 0, 90 * Random.Range(0, 4)),
                        Vector3.one
                    )
                );
            }
        }
    }

    // =========================
    // 🌲 OBSTÁCULOS
    // =========================

    void SpawnObstacle(Vector2 pos)
    {
        if (!IsWalkable(pos))
            return;

        if (IsSandZone(pos))
            return;

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

    // =========================
    // 🧠 LÓGICA AUXILIAR
    // =========================

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

    public Vector2 GetSafeSpawnPosition()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 pos = GetRandomPosition();

            if (IsWalkable(pos) && !IsSandZone(pos))
                return pos;
        }

        return Vector2.zero;
    }

    // =========================
    // 🧱 MAZE
    // =========================

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