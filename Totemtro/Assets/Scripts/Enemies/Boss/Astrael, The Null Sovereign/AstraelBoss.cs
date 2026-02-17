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

    [Header("Cinematic")]
    public GameObject portalPrefab;
    public CanvasGroup screenFade;       // imagen negra fullscreen
    public float introDelayBeforeAttack = 3f;

    Camera mainCamera;
    float originalCamSize;
    Vector3 originalCamPos;

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

        mainCamera = Camera.main;
        originalCamSize = mainCamera.orthographicSize;
        originalCamPos = mainCamera.transform.position;

        ClearAllEnemies();

        StartCoroutine(BossIntroSequence());
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

    IEnumerator BossIntroSequence()
    {
        Time.timeScale = 0f;

        // Pequeño delay dramático
        yield return new WaitForSecondsRealtime(0.5f);

        // Cámara va hacia el boss
        Camera mainCam = Camera.main;
        Vector3 originalPos = mainCam.transform.position;
        float originalSize = mainCam.orthographicSize;

        float zoomSize = originalSize - 2f;
        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            mainCam.transform.position = Vector3.Lerp(
                originalPos,
                new Vector3(transform.position.x, transform.position.y, originalPos.z),
                t / duration
            );

            mainCam.orthographicSize = Mathf.Lerp(originalSize, zoomSize, t / duration);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCam.transform.position =
            new Vector3(transform.position.x, transform.position.y, originalPos.z);

        mainCam.orthographicSize = zoomSize;

        // 🔥 AQUÍ SE CREA LA ARENA
        SpawnDynamicArena();

        // Espera dramática
        yield return new WaitForSecondsRealtime(3f);

        // Cámara vuelve al jugador
        t = 0f;

        while (t < duration)
        {
            mainCam.transform.position = Vector3.Lerp(
                mainCam.transform.position,
                new Vector3(player.position.x, player.position.y, originalPos.z),
                t / duration
            );

            mainCam.orthographicSize = Mathf.Lerp(zoomSize, originalSize, t / duration);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCam.orthographicSize = originalSize;

        Time.timeScale = 1f;

        // Empiezan ataques
        StartCoroutine(SphereLoop());
        StartCoroutine(GuardianLoop());
    }

    IEnumerator CameraFocusOnBoss()
    {
        float duration = 1.2f;
        float t = 0f;

        Vector3 bossCamPos =
            new Vector3(transform.position.x,
                        transform.position.y,
                        originalCamPos.z);

        float zoomSize = originalCamSize * 0.6f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            mainCamera.transform.position =
                Vector3.Lerp(originalCamPos, bossCamPos, t / duration);

            mainCamera.orthographicSize =
                Mathf.Lerp(originalCamSize, zoomSize, t / duration);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.6f);

        // Volver al player
        t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            mainCamera.transform.position =
                Vector3.Lerp(bossCamPos, originalCamPos, t / duration);

            mainCamera.orthographicSize =
                Mathf.Lerp(zoomSize, originalCamSize, t / duration);

            yield return null;
        }

        mainCamera.transform.position = originalCamPos;
        mainCamera.orthographicSize = originalCamSize;
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
        CameraShake.Instance.Shake(1f, 1f);

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
        }

        Destroy(gameObject);


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
