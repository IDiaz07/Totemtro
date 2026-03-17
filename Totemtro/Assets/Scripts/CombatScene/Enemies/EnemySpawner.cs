using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float spawnBossOffset = 6f;

    private List<GameObject> aliveEnemies = new List<GameObject>();

    float difficultyTimer;
    bool bossSpawned = false;
    bool spawningStopped = false;
    bool isPaused = false;

    Coroutine spawnRoutine;

    void Start()
    {
        Debug.Log($"EnemySpawner START - gameObject.activeInHierarchy={gameObject.activeInHierarchy}, enabled={enabled}");
        // Buscar player si no está asignado
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        Debug.Log(player != null ? $"EnemySpawner: player encontrado ({player.name})" : "EnemySpawner: player NO encontrado en Start");

        // No hacemos return aquí. Arrancamos el loop que esperará al player si es necesario.
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (player == null) return;

        difficultyTimer += Time.deltaTime;

        if (difficultyTimer >= difficultyRampTime)
        {
            difficultyTimer = 0f;

            spawnInterval = Mathf.Max(0.6f, spawnInterval - spawnIntervalReduction);
            maxEnemiesAlive += extraEnemiesPerRamp;

            Debug.Log("EnemySpawner: dificultad aumentada");
        }

        // limpiar enemigos muertos
        aliveEnemies.RemoveAll(e => e == null);

        // spawn boss
        if (!bossSpawned && Time.time >= bossSpawnTime)
        {
            SpawnBoss();
        }
    }

    IEnumerator SpawnLoop()
    {

        Debug.Log("EnemySpawner: esperando intro...");

        yield return new WaitUntil(() => !GameIntroState.IsIntroPlaying);

        Debug.Log("EnemySpawner: intro terminada, empezando spawn");

        while (true)
        {
            // Esperar antes de cada intento (usa tiempo escalado)
            yield return new WaitForSeconds(spawnInterval);

            // Diagnóstico: log de estado para entender por qué se salta spawn
            if (player == null)
            {
                Debug.Log("EnemySpawner: player aun null, intentando encontrarlo de nuevo...");
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    player = p.transform;
                    Debug.Log($"EnemySpawner: player encontrado dinámicamente ({player.name})");
                }
                else
                {
                    Debug.Log("EnemySpawner: player no encontrado en esta iteración - saltando spawn");
                    continue;
                }
            }

            if (spawningStopped)
            {
                Debug.Log("EnemySpawner: spawningStopped == true, saltando spawn");
                continue;
            }

            if (isPaused)
            {
                Debug.Log("EnemySpawner: isPaused == true, saltando spawn");
                continue;
            }

            if (aliveEnemies.Count >= maxEnemiesAlive)
            {
                Debug.Log($"EnemySpawner: aliveEnemies ({aliveEnemies.Count}) >= maxEnemiesAlive ({maxEnemiesAlive}) - saltando spawn");
                continue;
            }

            int near = CountEnemiesNearPlayer();
            if (near >= maxEnemiesNearPlayer)
            {
                Debug.Log($"EnemySpawner: enemies near player ({near}) >= maxEnemiesNearPlayer ({maxEnemiesNearPlayer}) - saltando spawn");
                continue;
            }

            SpawnEnemy();
        }
    }

    public void PauseSpawning()
    {
        isPaused = true;
    }

    public void ResumeSpawning()
    {
        isPaused = false;
        spawningStopped = false;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawningStopped = true;
    }

    void SpawnEnemy()
    {
        if (player == null)
        {
            Debug.LogWarning("EnemySpawner: SpawnEnemy abortado porque player == null");
            return;
        }

        Vector2 spawnPos = GetSpawnPosition();

        GameObject prefab = ChooseEnemy();

        if (prefab == null)
        {
            Debug.LogWarning("EnemySpawner: prefab null");
            return;
        }

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        aliveEnemies.Add(enemy);

        Debug.Log($"EnemySpawner: enemigo spawneado ({prefab.name}) en {spawnPos}, totalAlive={aliveEnemies.Count}");
    }

    Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

        return (Vector2)player.position + offset;
    }

    GameObject ChooseEnemy()
    {
        float time = Time.time;

        float thrallWeight = 40f;
        float stalkerWeight = 20f;
        float bruteWeight = 10f;
        float spitterWeight = 8f;
        float exploderWeight = 6f;
        float summonerWeight = 2f;
        float eliteThrallWeight = 0.5f;

        bruteWeight += time / 30f;
        spitterWeight += time / 45f;
        exploderWeight += time / 60f;
        summonerWeight += time / 180f;
        eliteThrallWeight += time / 220f;

        float totalWeight =
            thrallWeight +
            stalkerWeight +
            bruteWeight +
            spitterWeight +
            exploderWeight +
            summonerWeight +
            eliteThrallWeight;

        float roll = Random.Range(0f, totalWeight);

        float r = roll;
        GameObject chosen = null;

        if (r < thrallWeight) chosen = thrallPrefab;
        else
        {
            r -= thrallWeight;
            if (r < stalkerWeight) chosen = stalkerPrefab;
            else
            {
                r -= stalkerWeight;
                if (r < bruteWeight) chosen = brutePrefab;
                else
                {
                    r -= bruteWeight;
                    if (r < spitterWeight) chosen = spitterPrefab;
                    else
                    {
                        r -= spitterWeight;
                        if (r < exploderWeight) chosen = exploderPrefab;
                        else
                        {
                            r -= exploderWeight;
                            if (r < summonerWeight) chosen = summonerPrefab;
                            else
                            {
                                r -= summonerWeight;
                                if (r < eliteThrallWeight) chosen = eliteThrall;
                                else chosen = thrallPrefab;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"ChooseEnemy: roll={roll:F2}, total={totalWeight:F2}, chosen={(chosen != null ? chosen.name : "NULL")}, eliteWeight={eliteThrallWeight:F2}");

        return chosen;
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

        StopSpawning();

        foreach (var e in aliveEnemies)
        {
            if (e != null)
                Destroy(e);
        }

        aliveEnemies.Clear();

        Vector2 bossPosition =
            player.position + Vector3.up * spawnBossOffset;

        if (astraelBossPrefab != null)
        {
            Instantiate(astraelBossPrefab, bossPosition, Quaternion.identity);
            Debug.Log("EnemySpawner: boss spawneado");
        }
        else
        {
            Debug.LogError("EnemySpawner: astraelBossPrefab no asignado");
        }
    }
}