using UnityEngine;
using System.Collections;

public class ExtractionManager : MonoBehaviour
{
    public static ExtractionManager Instance;

    [Header("Prefabs")]
    public GameObject extractionZonePrefab;
    public ExtractionHelicopter helicopterPrefab;

    [Header("Timing")]
    public float countdownDuration = 20f;
    public float channelTime = 10f;
    public float zoneLifetime = 60f;

    [Header("Spawn")]
    public float minDistanceFromPlayer = 30f;
    public float maxDistanceFromPlayer = 60f;
    public Vector2 mapMin;
    public Vector2 mapMax;
    public float borderPadding = 10f;

    [Header("Boss Block")]
    public float bossBlockWindow = 5f;

    [Header("Spawner Boost")]
    public EnemySpawner enemySpawner;
    public float extractionSpawnMultiplier = 0.4f;

    [Header("Audio")]
    public AudioSource globalAudio;
    public AudioClip sirenClip;
    public AudioClip deniedClip;
    public AudioClip helicopterClip;

    [Header("UI")]
    public ExtractionPromptUI promptUI;

    // =========================================
    // STATE
    // =========================================

    bool extractionStarted = false;
    bool zoneActive = false;
    ExtractionZone currentZone;

    float originalSpawnInterval;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (enemySpawner != null)
            originalSpawnInterval = enemySpawner.spawnInterval;
    }

    void Update()
    {
        if (InputKeyBindings.Instance == null) return;
        if (GamePause.IsPaused) return;
        if (GameInputLock.IsLocked) return;

        if (!extractionStarted && promptUI != null)
        {
            string keyName = InputKeyBindings.Instance
                .GetKeyName(InputKeyBindings.Action.Extraction);
            promptUI.SetKeyText(keyName);
        }

        if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Extraction))
        {
            if (!extractionStarted && !zoneActive)
            {
                TryStartExtraction();
            }
        }
    }

    // =========================================
    // VALIDATION
    // =========================================

    void TryStartExtraction()
    {
        string denyReason = GetDenyReason();

        if (denyReason != null)
        {
            if (promptUI != null)
                promptUI.ShakeDenied(denyReason);

            if (globalAudio != null && deniedClip != null)
                globalAudio.PlayOneShot(deniedClip);

            return;
        }

        StartCoroutine(ExtractionSequence());
    }

    string GetDenyReason()
    {
        if (GameStateManager.Instance != null)
        {
            GameState state = GameStateManager.Instance.CurrentState;

            if (state == GameState.BossFight)
                return "No puedes extraer durante el boss";

            if (state == GameState.BossIntro)
                return "El boss está llegando...";
        }

        if (enemySpawner != null)
        {
            float timeUntilBoss = enemySpawner.bossSpawnTime - Time.time;

            if (timeUntilBoss > 0f && timeUntilBoss <= bossBlockWindow)
                return "Algo se acerca...";
        }

        return null;
    }

    // =========================================
    // SEQUENCE
    // =========================================

    IEnumerator ExtractionSequence()
    {
        extractionStarted = true;

        if (promptUI != null)
            promptUI.Hide();

        // Countdown bar (20s)
        if (ExtractionUI.Instance != null)
            ExtractionUI.Instance.Show(countdownDuration);

        if (enemySpawner != null)
            enemySpawner.spawnInterval *= extractionSpawnMultiplier;

        float timer = 0f;

        while (timer < countdownDuration)
        {
            timer += Time.deltaTime;

            if (ExtractionUI.Instance != null)
                ExtractionUI.Instance.UpdateBar(timer);

            yield return null;
        }

        if (ExtractionUI.Instance != null)
            ExtractionUI.Instance.Hide();

        if (enemySpawner != null)
            enemySpawner.spawnInterval = originalSpawnInterval;

        SpawnExtractionZone();

        zoneActive = true;

        if (globalAudio != null && sirenClip != null)
            globalAudio.PlayOneShot(sirenClip);

        StartCoroutine(ZoneExpirationRoutine());
    }

    // =========================================
    // SPAWN ZONE
    // =========================================

    void SpawnExtractionZone()
    {
        Vector2 spawnPos = FindValidSpawnPosition();

        GameObject zoneObj =
            Instantiate(extractionZonePrefab, spawnPos, Quaternion.identity);

        currentZone = zoneObj.GetComponent<ExtractionZone>();
        currentZone.Initialize(channelTime);

        if (ExtractionArrowUI.Instance != null)
            ExtractionArrowUI.Instance.SetTarget(zoneObj.transform);
    }

    Vector2 FindValidSpawnPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2 playerPos = playerObj != null
            ? (Vector2)playerObj.transform.position
            : Vector2.zero;

        for (int i = 0; i < 100; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(mapMin.x + borderPadding, mapMax.x - borderPadding),
                Random.Range(mapMin.y + borderPadding, mapMax.y - borderPadding)
            );

            float dist = Vector2.Distance(pos, playerPos);

            if (dist >= minDistanceFromPlayer && dist <= maxDistanceFromPlayer)
                return pos;
        }

        Vector2 dir = Random.insideUnitCircle.normalized;
        return playerPos + dir * minDistanceFromPlayer;
    }

    // =========================================
    // COMPLETION — HELICOPTER CINEMATIC
    // =========================================

    public void CompleteExtraction()
    {
        zoneActive = false;

        if (ExtractionArrowUI.Instance != null)
            ExtractionArrowUI.Instance.SetTarget(null);

        StartCoroutine(HelicopterExtractionSequence());
    }

    IEnumerator HelicopterExtractionSequence()
    {
        // Bloquear input del jugador
        GameInputLock.Lock();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            GameManager.Instance.ExtractRun();
            yield break;
        }

        Transform player = playerObj.transform;
        HeroController heroController = player.GetComponent<HeroController>();
        Weapon weapon = player.GetComponentInChildren<Weapon>();

        // Instanciar helicóptero
        ExtractionHelicopter helicopter = null;

        if (helicopterPrefab != null)
        {
            helicopter = Instantiate(helicopterPrefab);
            helicopter.gameObject.SetActive(true);

            // Audio del helicóptero
            if (globalAudio != null && helicopterClip != null)
                globalAudio.PlayOneShot(helicopterClip);

            // Ejecutar cinemática completa
            yield return StartCoroutine(
                helicopter.PlayFullSequence(player, heroController, weapon));

            Destroy(helicopter.gameObject);
        }

        // Cerrar zona
        if (currentZone != null)
            currentZone.CloseZone();

        yield return new WaitForSeconds(0.5f);

        // Ir a la escena de resumen DESPUÉS de la cinemática
        GameManager.Instance.ExtractRun();
    }

    IEnumerator ZoneExpirationRoutine()
    {
        yield return new WaitForSeconds(zoneLifetime);

        if (currentZone != null)
        {
            currentZone.CloseZone();
            zoneActive = false;
            extractionStarted = false;

            if (ExtractionArrowUI.Instance != null)
                ExtractionArrowUI.Instance.SetTarget(null);
        }
    }
}