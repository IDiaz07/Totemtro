using UnityEngine;
using System.Collections.Generic;

public static class MazeGenerator
{
    static int width = 25;
    static int height = 25;

    static int[,] maze;

    static float cellSize = 2f;

    public static void Generate(MapGenerator map, Vector2 center)
    {
        maze = new int[width, height];

        GenerateMaze();

        BreakRandomWalls();

        CreateEntrances(); 

        float rotation = Random.Range(0, 4) * 90f;

        BuildMaze(map, center, rotation);

        SpawnRewards(map, center, rotation);

        SpawnDecorations(map, center, rotation);
    }

    static void GenerateMaze()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        Carve(1, 1);
    }

    static void Carve(int x, int y)
    {
        int[] dirs = { 0, 1, 2, 3 };

        Shuffle(dirs);

        foreach (int dir in dirs)
        {
            int dx = 0;
            int dy = 0;

            switch (dir)
            {
                case 0: dx = 2; break;
                case 1: dx = -2; break;
                case 2: dy = 2; break;
                case 3: dy = -2; break;
            }

            int nx = x + dx;
            int ny = y + dy;

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[nx, ny] == 1)
            {
                maze[nx, ny] = 0;
                maze[x + dx / 2, y + dy / 2] = 0;

                Carve(nx, ny);
            }
        }
    }

    static void BreakRandomWalls()
    {
        int openings = Random.Range(4, 8);

        for (int i = 0; i < openings; i++)
        {
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);

            maze[x, y] = 0;
        }
    }

    static bool IsDeadEnd(int x, int y)
    {
        if (maze[x, y] == 1)
            return false;

        int exits = 0;

        if (maze[x + 1, y] == 0) exits++;
        if (maze[x - 1, y] == 0) exits++;
        if (maze[x, y + 1] == 0) exits++;
        if (maze[x, y - 1] == 0) exits++;

        return exits == 1;
    }

    static void BuildMaze(MapGenerator map, Vector2 center, float rotation)
    {
        Quaternion rot = Quaternion.Euler(0, 0, rotation);

        Vector2 mazeOffset = new Vector2(width, height) * cellSize * 0.5f;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 1)
                {
                    Vector2 offset = new Vector2(x * cellSize, y * cellSize);

                    offset -= mazeOffset;

                    offset = rot * offset;

                    Vector2 pos = center + offset;

                    GameObject.Instantiate(
                        map.mazeWallPrefab,
                        pos,
                        Quaternion.identity,
                        map.obstacleParent
                    );
                }
            }
    }

    static void SpawnRewards(MapGenerator map, Vector2 center, float rotation)
    {
        List<Vector2> deadEnds = new List<Vector2>();
        List<Vector2> paths = new List<Vector2>();

        // Detectar paths y dead ends
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 0)
                {
                    paths.Add(new Vector2(x, y));

                    if (IsDeadEnd(x, y))
                        deadEnds.Add(new Vector2(x, y));
                }
            }

        ShuffleList(deadEnds);
        ShuffleList(paths);

        Quaternion rot = Quaternion.Euler(0, 0, rotation);
        Vector2 mazeOffset = new Vector2(width, height) * cellSize * 0.5f;

        // 🧰 COFRES → SOLO en dead ends
        int chestCount = Mathf.Min(4, deadEnds.Count);

        for (int i = 0; i < chestCount; i++)
        {
            Vector2 offset = deadEnds[i] * cellSize;
            offset -= mazeOffset;
            offset = rot * offset;

            Vector2 pos = center + offset;

            GameObject.Instantiate(
                map.chestPrefab,
                pos,
                Quaternion.identity
            );
        }

        // 🎰 SLOT MACHINE → crear zona abierta
        if (paths.Count > 0)
        {
            Vector2 slotCell = paths[0];

            ClearArea(slotCell, 2);

            Vector2 offset = slotCell * cellSize;
            offset -= mazeOffset;
            offset = rot * offset;

            Vector2 pos = center + offset;

            GameObject.Instantiate(
                map.slotMachinePrefab,
                pos,
                Quaternion.identity
            );
        }
    }

    static void ClearArea(Vector2 cell, int radius)
    {
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                int nx = (int)cell.x + x;
                int ny = (int)cell.y + y;

                if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1)
                {
                    maze[nx, ny] = 0;
                }
            }
    }

    static void CreateEntrances()
    {
        int entrances = Random.Range(2, 4);

        for (int i = 0; i < entrances; i++)
        {
            int side = Random.Range(0, 4);

            int x = 0;
            int y = 0;

            switch (side)
            {
                case 0: // abajo
                    x = Random.Range(1, width - 2);
                    y = 1;
                    break;

                case 1: // arriba
                    x = Random.Range(1, width - 2);
                    y = height - 2;
                    break;

                case 2: // izquierda
                    x = 1;
                    y = Random.Range(1, height - 2);
                    break;

                case 3: // derecha
                    x = width - 2;
                    y = Random.Range(1, height - 2);
                    break;
            }

            maze[x, y] = 0;
            maze[x + (x == 1 ? -1 : x == width - 2 ? 1 : 0),
                 y + (y == 1 ? -1 : y == height - 2 ? 1 : 0)] = 0;
        }
    }

    static void SpawnDecorations(MapGenerator map, Vector2 center, float rotation)
    {
        if (map.mazeDecorations == null || map.mazeDecorations.Length == 0)
            return;

        Quaternion rot = Quaternion.Euler(0, 0, rotation);

        Vector2 mazeOffset = new Vector2(width, height) * cellSize * 0.5f;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0 && Random.value < 0.06f)
                {
                    Vector2 offset = new Vector2(x, y) * cellSize;

                    offset -= mazeOffset;

                    offset = rot * offset;

                    Vector2 pos = center + offset;

                    GameObject prefab = map.mazeDecorations[
                        Random.Range(0, map.mazeDecorations.Length)
                    ];

                    GameObject.Instantiate(
                        prefab,
                        pos,
                        Quaternion.identity,
                        map.obstacleParent
                    );
                }
            }
    }

    static void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int r = Random.Range(i, array.Length);

            int tmp = array[i];
            array[i] = array[r];
            array[r] = tmp;
        }
    }

    static void ShuffleList(List<Vector2> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);

            Vector2 tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}