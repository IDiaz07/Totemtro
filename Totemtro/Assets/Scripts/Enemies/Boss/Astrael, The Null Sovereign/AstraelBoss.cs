using UnityEngine;
using System.Collections;

public class AstraelBoss : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject nullSpherePrefab;
    public GameObject nullGuardianPrefab;
    public GameObject bossHealthUIPrefab;
    public GameObject shockwavePrefab;
    public GameObject dynamicArenaPrefab;
    public GameObject legendaryDropPrefab;


    [Header("Attack Settings")]
    public float sphereCooldown = 4f;
    public float guardianCooldown = 6f;
    public int guardianCount = 4;

    [Header("Music")]
    public AudioClip bossMusic;
    public AudioClip normalMusic;


    Enemy enemy;
    Transform player;
    BossHealthUI bossUI;
    ArenaController arenaController;

    bool phaseTwo = false;
    bool enraged = false;
    bool canSphere = true;
    bool canGuardians = true;
    bool isDead = false;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        ClearAllEnemies();
        CreateBossUI();
        SpawnDynamicArena();
        MusicManager.Instance.PlayMusic(bossMusic, 1.5f);

        StartCoroutine(Introduction());
    }

    void Update()
    {
        if (enemy == null) return;

        float currentHP = enemy.GetCurrentHealth();

        // Fase 2
        if (!phaseTwo && currentHP <= enemy.maxHealth * 0.5f)
        {
            EnterPhaseTwo();
        }

        // Enrage
        if (!enraged && currentHP <= enemy.maxHealth * 0.2f)
        {
            EnterEnrage();
        }

        // Muerte
        if (currentHP <= 0f)
        {
            OnBossDeath();
        }
    }

    // ==============================
    // INTRO
    // ==============================

    IEnumerator Introduction()
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(1f);

        CameraShake.Instance.Shake(0.8f, 1.2f);

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        StartCoroutine(SphereLoop());
        StartCoroutine(GuardianLoop());
    }

    // ==============================
    // ATAQUES
    // ==============================

    IEnumerator SphereLoop()
    {
        while (true)
        {
            if (canSphere)
            {
                yield return StartCoroutine(CastNullSphere());
                yield return new WaitForSeconds(sphereCooldown);
            }

            yield return null;
        }
    }

    IEnumerator GuardianLoop()
    {
        while (true)
        {
            if (canGuardians)
            {
                yield return StartCoroutine(SpawnGuardians());
                yield return new WaitForSeconds(guardianCooldown);
            }

            yield return null;
        }
    }

    IEnumerator CastNullSphere()
    {
        canSphere = false;

        yield return new WaitForSeconds(0.6f);

        GameObject sphere = Instantiate(
            nullSpherePrefab,
            transform.position,
            Quaternion.identity
        );

        sphere.GetComponent<NullSphere>()
              .Init(player.position);

        canSphere = true;
    }

    IEnumerator SpawnGuardians()
    {
        canGuardians = false;

        for (int i = 0; i < guardianCount; i++)
        {
            GameObject g = Instantiate(
                nullGuardianPrefab,
                transform.position,
                Quaternion.identity
            );

            g.GetComponent<NullGuardian>()
             .Init(transform, player);

            yield return new WaitForSeconds(0.25f);
        }

        canGuardians = true;
    }

    // ==============================
    // FASES
    // ==============================

    void EnterPhaseTwo()
    {
        phaseTwo = true;

        sphereCooldown *= 0.7f;
        guardianCooldown *= 0.7f;
        guardianCount += 2;

        Instantiate(
            shockwavePrefab,
            transform.position,
            Quaternion.identity
        );

        CameraShake.Instance.Shake(1f, 0.8f);
    }

    void EnterEnrage()
    {
        enraged = true;

        sphereCooldown *= 0.5f;
        guardianCooldown *= 0.5f;
        guardianCount += 3;

        if (arenaController != null)
            arenaController.SetEnraged(true);
    }

    // ==============================
    // ARENA
    // ==============================

    void SpawnDynamicArena()
    {
        if (dynamicArenaPrefab == null) return;

        GameObject arenaInstance = Instantiate(
            dynamicArenaPrefab,
            transform.position,
            Quaternion.identity
        );

        arenaController =
            arenaInstance.GetComponent<ArenaController>();

        if (arenaController != null)
            arenaController.Initialize(transform);

    }

    // ==============================
    // UI
    // ==============================

    void CreateBossUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("No se encontró Canvas en la escena.");
            return;
        }

        if (bossHealthUIPrefab == null)
        {
            Debug.LogError("BossHealthUIPrefab no asignado.");
            return;
        }

        GameObject ui = Instantiate(
            bossHealthUIPrefab,
            canvas.transform
        );

        bossUI = ui.GetComponent<BossHealthUI>();

        if (bossUI == null)
        {
            Debug.LogError("BossHealthUI component missing.");
            return;
        }

        bossUI.Init(enemy, "Astrael, The Null Sovereign");
    }


    // ==============================
    // LIMPIEZA
    // ==============================

    void ClearAllEnemies()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var e in enemies)
        {
            if (e.gameObject == gameObject) continue;

            Destroy(e);
        }
    }

    void OnBossDeath()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(DeathSequence());
    }


    IEnumerator DeathSequence()
    {
        // Detener ataques
        StopAllCoroutines();

        // Congelar enemigos
        Time.timeScale = 0.5f;

        // Shake fuerte
        CameraShake.Instance.Shake(1.5f, 1f);

        yield return new WaitForSecondsRealtime(1f);

        // Colapso arena
        if (arenaController != null)
            arenaController.StartCollapse();

        yield return new WaitForSecondsRealtime(1.5f);

        // Drop legendario
        SpawnLegendaryDrop();

        yield return new WaitForSecondsRealtime(1f);

        // Transición acto 2
        EnterActTwo();

        MusicManager.Instance.PlayMusic(normalMusic, 2f);

        // Desactivar UI del boss
        if (bossUI != null)
        {
            Destroy(bossUI.gameObject);
            bossUI = null;

            Destroy(gameObject);
        }

        void SpawnLegendaryDrop()
        {
            if (legendaryDropPrefab == null) return;

            Instantiate(
                legendaryDropPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        void EnterActTwo()
        {
            EnemySpawner spawner =
                FindFirstObjectByType<EnemySpawner>();

            if (spawner != null)
                spawner.ResumeSpawning();

            // Aumentar dificultad base
            spawner.spawnInterval *= 0.7f;
            spawner.maxEnemiesAlive += 10;

            Time.timeScale = 1f;

            Debug.Log("ACT II BEGINS");
        }

    }
}
