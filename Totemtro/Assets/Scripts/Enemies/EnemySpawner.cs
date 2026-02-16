using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Prefabs")]
    public GameObject thrallPrefab;
    public GameObject stalkerPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float minSpawnDistance = 8f;
    public float maxSpawnDistance = 14f;
    public int maxEnemiesAlive = 20;

    [Header("Difficulty Scaling")]
    public float difficultyRampTime = 30f;
    public float spawnIntervalReduction = 0.2f;
    public int extraEnemiesPerRamp = 3;

    [Header("Director AI")]
    public float pressureRadius = 6f;
    public int maxEnemiesNearPlayer = 6;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private float difficultyTimer;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player == null)
        {
            Debug.LogError("EnemySpawner: Player no encontrado.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        difficultyTimer += Time.deltaTime;

        if (difficultyTimer >= difficultyRampTime)
        {
            difficultyTimer = 0f;

            spawnInterval = Mathf.Max(0.6f, spawnInterval - spawnIntervalReduction);
            maxEnemiesAlive += extraEnemiesPerRamp;

            Debug.Log("Dificultad aumentada");
        }

        // Limpia enemigos destruidos
        aliveEnemies.RemoveAll(e => e == null);
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (aliveEnemies.Count >= maxEnemiesAlive)
                continue;

            if (CountEnemiesNearPlayer() >= maxEnemiesNearPlayer)
                continue;

            SpawnEnemy();
        }
    }


    void SpawnEnemy()
    {
        Vector2 spawnPos = GetSpawnPosition();

        GameObject prefab = ChooseEnemy();

        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        aliveEnemies.Add(enemy);
    }

    Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * distance;

        return (Vector2)player.position + offset;
    }

    GameObject ChooseEnemy()
    {
        // Al principio casi todo Thrall
        // Poco a poco más Stalker

        float stalkerChance = Mathf.Clamp01(Time.time / 90f);

        if (Random.value < stalkerChance && stalkerPrefab != null)
            return stalkerPrefab;

        return thrallPrefab;
    }

    int CountEnemiesNearPlayer()
    {
        int count = 0;

        foreach (var enemy in aliveEnemies)
        {
            if (enemy == null) continue;

            if (Vector2.Distance(enemy.transform.position, player.position) < pressureRadius)
                count++;
        }

        return count;
    }

}
