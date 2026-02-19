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
    public GameObject brutePrefab;
    public GameObject spitterPrefab;
    public GameObject summonerPrefab;
    public GameObject exploderPrefab;
    public GameObject eliteThrall;


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

    [Header("Boss")]
    public GameObject astraelBossPrefab;
    public float bossSpawnTime = 180f;
    public float spawnBoss = 0.5f;
    bool bossSpawned = false;



    private List<GameObject> aliveEnemies = new List<GameObject>();
    private float difficultyTimer;
    bool spawningStopped = false;

    bool isPaused = false;

    public void PauseSpawning()
    {
        isPaused = true;
    }

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

        // =====================
        // BOSS SPAWN
        // =====================
        if (!bossSpawned && Time.time >= bossSpawnTime)
        {
            SpawnBoss();
        }

    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawningStopped)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(spawnInterval);

            if (aliveEnemies.Count >= maxEnemiesAlive)
                continue;

            if (CountEnemiesNearPlayer() >= maxEnemiesNearPlayer)
                continue;

            if (isPaused)
                continue;

            SpawnEnemy();
        }
    }

    public void StopSpawning()
    {
        spawningStopped = true;
    }

    public void ResumeSpawning()
    {
        spawningStopped = false;
        isPaused = false;   // 🔥 ESTA LÍNEA FALTABA

        StopAllCoroutines();
        StartCoroutine(SpawnLoop());
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
        float time = Time.time;

        // ========= PESOS BASE =========
        float thrallWeight = 40f;
        float stalkerWeight = 20f;
        float bruteWeight = 10f;
        float spitterWeight = 8f;
        float exploderWeight = 6f;
        float summonerWeight = 2f;
        float eliteThrallWeight = 0.5f;

        // ========= ESCALADO POR TIEMPO =========

        // Brutes aumentan bastante
        bruteWeight += time / 30f;

        // Spitters aumentan moderado
        spitterWeight += time / 45f;

        // Exploders aumentan leve
        exploderWeight += time / 60f;

        // Summoner aumenta MUY lento
        summonerWeight += time / 180f;

        // EliteThrall aumenta MUY MUY lento
        eliteThrallWeight += time / 220f;

        // ========= SUMA TOTAL =========
        float totalWeight =
            thrallWeight +
            stalkerWeight +
            bruteWeight +
            spitterWeight +
            exploderWeight +
            summonerWeight +
            eliteThrallWeight;

        float roll = Random.Range(0f, totalWeight);

        if (roll < thrallWeight)
            return thrallPrefab;

        roll -= thrallWeight;

        if (roll < stalkerWeight)
            return stalkerPrefab;

        roll -= stalkerWeight;

        if (roll < bruteWeight)
            return brutePrefab;

        roll -= bruteWeight;

        if (roll < spitterWeight)
            return spitterPrefab;

        roll -= spitterWeight;

        if (roll < exploderWeight)
            return exploderPrefab;
        
        roll -= spitterWeight;

        if (roll < summonerWeight)
            return summonerPrefab;
        
        if (roll < eliteThrallWeight)
            return eliteThrall;

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

    void SpawnBoss()
    {
        bossSpawned = true;

        // Detener spawn normal
        StopSpawning();

        // Limpiar enemigos vivos
        foreach (var e in aliveEnemies)
        {
            if (e != null)
                Destroy(e);
        }

        aliveEnemies.Clear();

        // Spawnear boss en centro o cerca del jugador
        Vector2 bossPosition =
            player.position + Vector3.up * spawnBoss;

        Instantiate(
            astraelBossPrefab,
            bossPosition,
            Quaternion.identity
        );

        Debug.Log("Astrael has entered the arena.");
    }
}
