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

    [Header("Phase 2 FX")]
    public GameObject phase2AuraPrefab;
    public GameObject phase2ParticlesPrefab;
    public GameObject phase2ExplosionPrefab;
    [SerializeField] private SpriteRenderer bodyRenderer;

    bool isTransforming = false;

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
    CameraFollow cameraFollow;

    bool phaseTwo = false;
    bool enraged = false;
    bool canSphere = true;
    bool canGuardians = true;
    bool deathStarted = false;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No MainCamera found in scene!");
            return;
        }

        cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.target = player;

        originalCamSize = mainCamera.orthographicSize;
        originalCamPos = mainCamera.transform.position;

        ClearAllEnemies();
        StartCoroutine(Introduction());
    }

    void Update()
    {
        if (deathStarted) return;
        if (enemy == null) return;

        float currentHP = enemy.GetCurrentHealth();

        if (currentHP <= 0f)
        {
            deathStarted = true;
            StopAllCoroutines();   // 🔥 MUY IMPORTANTE
            StartCoroutine(DeathSequence());
            return;
        }

        if (isTransforming) return;

        if (!phaseTwo && currentHP <= enemy.maxHealth * 0.5f)
            EnterPhaseTwo();

        if (!enraged && currentHP <= enemy.maxHealth * 0.2f)
            EnterEnrage();
    }

    // ==============================
    // INTRO
    // ==============================

    IEnumerator Introduction()
    {
        Time.timeScale = 0f;
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        Camera cam = Camera.main;
        Vector3 originalPos = cam.transform.position;
        float originalSize = cam.orthographicSize;

        // 🔥 Spawn portal
        GameObject portal = Instantiate(
            portalPrefab,
            transform.position,
            Quaternion.identity
        );

        Debug.Log("Playing BOSS music");
        MusicManager.Instance.PlayMusic(bossMusic, 2f);

        yield return new WaitForSecondsRealtime(1.5f);

        // 🌑 Oscurecer pantalla
        yield return StartCoroutine(FadeScreen(1f, 0.5f));

        // 🎥 Cámara va al boss
        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            cam.transform.position = Vector3.Lerp(
                originalPos,
                new Vector3(transform.position.x, transform.position.y, originalPos.z),
                t / duration
            );

            cam.orthographicSize = Mathf.Lerp(originalSize, originalSize - 2f, t / duration);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cam.transform.position =
            new Vector3(transform.position.x, transform.position.y, originalPos.z);

        // 🔥 Boss emerge desde el portal
        transform.localScale = Vector3.zero;
        Destroy(portal);

        float emergeTime = 1f;
        t = 0f;

        while (t < emergeTime)
        {
            transform.localScale = Vector3.Lerp(
                Vector3.zero,
                Vector3.one,
                t / emergeTime
            );

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;

        // 💀 Mostrar nombre
        BossNameCinematic nameUI =
            FindFirstObjectByType<BossNameCinematic>();

        if (nameUI != null)
            yield return StartCoroutine(
                nameUI.ShowName("Astrael, The Null Sovereign")
            );

        // Crear arena ahora
        SpawnDynamicArena();

        yield return new WaitForSecondsRealtime(1f);

        // 🌑 Quitar oscuridad
        yield return StartCoroutine(FadeScreen(0f, 0.5f));

        // 🔥 Reactivar seguimiento normal
        cam.orthographicSize = originalSize;

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        cam.orthographicSize = originalSize;
        if (cameraFollow != null)
            cameraFollow.enabled = true;

        Time.timeScale = 1f;

        CreateBossUI();
        // ⏳ Espera 3 segundos antes de atacar
        yield return new WaitForSeconds(1f);

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
        while (!deathStarted)
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
        while (!deathStarted)
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
        if (isTransforming) return;

        phaseTwo = true;
        isTransforming = true;

        StartCoroutine(PhaseTwoTransformation());
    }

    IEnumerator PhaseTwoTransformation()
    {
        isTransforming = true;

        // 🧊 Freeze juego
        Time.timeScale = 0f;

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // 🎥 Cámara al boss
        Vector3 camTarget = new Vector3(
            transform.position.x,
            transform.position.y,
            mainCamera.transform.position.z
        );

        float zoomSize = originalCamSize * 0.7f;

        float t = 0f;
        float duration = 0.8f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            mainCamera.transform.position =
                Vector3.Lerp(originalCamPos, camTarget, t / duration);

            mainCamera.orthographicSize =
                Mathf.Lerp(originalCamSize, zoomSize, t / duration);

            yield return null;
        }

        mainCamera.transform.position = camTarget;
        mainCamera.orthographicSize = zoomSize;

        // 🌑 Aura
        if (phase2AuraPrefab != null)
            Instantiate(
                phase2AuraPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

        // 🔥 Partículas
        if (phase2ParticlesPrefab != null)
            Instantiate(
                phase2ParticlesPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

        // 📈 Pulsos de escala
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("No SpriteRenderer found in AstraelBoss or its children!");
            yield break;
        }

        Vector3 baseScale = transform.localScale;

        yield return StartCoroutine(ScalePulse(baseScale, baseScale * 2f, 0.25f));
        yield return StartCoroutine(ScalePulse(baseScale * 2f, baseScale * 1.6f, 0.25f));

        // 💜 Cambio de color en el segundo pulso
        sr.color = new Color(0.6f, 0f, 0.8f);

        yield return StartCoroutine(ScalePulse(baseScale * 1.6f, baseScale * 2f, 0.2f));
        yield return StartCoroutine(ScalePulse(baseScale * 2f, baseScale * 1.8f, 0.2f));
        yield return StartCoroutine(ScalePulse(baseScale * 1.8f, baseScale * 2f, 0.15f));

        transform.localScale = baseScale * 2f;

        // 💥 Explosión visual
        if (phase2ExplosionPrefab != null)
            Instantiate(
                phase2ExplosionPrefab,
                transform.position,
                Quaternion.identity
            );

        CameraShake.Instance.Shake(1.2f, 0.8f);

        yield return new WaitForSecondsRealtime(0.6f);

        // 🎥 Cámara vuelve al player
        t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            mainCamera.transform.position =
                Vector3.Lerp(camTarget, originalCamPos, t / duration);

            mainCamera.orthographicSize =
                Mathf.Lerp(zoomSize, originalCamSize, t / duration);

            yield return null;
        }

        mainCamera.transform.position = originalCamPos;
        mainCamera.orthographicSize = originalCamSize;

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        // 🔥 Buffs reales fase 2
        sphereCooldown *= 0.7f;
        guardianCooldown *= 0.7f;
        guardianCount += 2;

        // 🎮 Vuelve el control
        Time.timeScale = 1f;

        isTransforming = false;
    }


    IEnumerator ScalePulse(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(from, to, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = to;
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
        Enemy[] enemies =
            FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            if (e == enemy) continue;
            Debug.Log("Enemies found: " + enemies.Length);
            Destroy(e.gameObject);
        }
    }


    public bool isSpawning = true;

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void ResumeSpawning()
    {
        isSpawning = true;
    }

    IEnumerator DeathSequence()
    {
        Debug.Log("===== DEATH SEQUENCE START =====");

        canSphere = false;
        canGuardians = false;


        Time.timeScale = 0.5f;

        CameraShake.Instance.Shake(0.2f, 1f);

        yield return new WaitForSecondsRealtime(1f);

        if (arenaController != null)
        {
            arenaController.StartCollapse();
        }

        yield return new WaitForSecondsRealtime(1.5f);

        if (legendaryDropPrefab != null)
        {
            Instantiate(
                legendaryDropPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        EnemySpawner spawner =
            FindFirstObjectByType<EnemySpawner>();

        if (spawner != null)
        {
            spawner.ResumeSpawning();
            spawner.spawnInterval *= 0.7f;
            spawner.maxEnemiesAlive += 10;
            Debug.Log("ACT II BEGINS");
        }

        MusicManager.Instance?.PlayMusic(normalMusic, 2f);

        if (bossUI != null)
            Destroy(bossUI.gameObject);

        if (cameraFollow != null)
        {
            cameraFollow.target = player;
            cameraFollow.enabled = true;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        yield return StartCoroutine(UltraCinematicDeath());
        Destroy(gameObject);
    }

    IEnumerator UltraCinematicDeath()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Vector3 originalScale = transform.localScale;

        // 🎥 Cámara zoom brutal
        Vector3 camStart = mainCamera.transform.position;
        Vector3 camTarget = new Vector3(
            transform.position.x,
            transform.position.y,
            camStart.z
        );

        float camZoomStart = mainCamera.orthographicSize;
        float camZoomTarget = camZoomStart * 0.5f;

        float zoomTime = 0.6f;
        float t = 0f;

        Time.timeScale = 0.3f; // 🐢 Slow motion extremo

        while (t < zoomTime)
        {
            t += Time.unscaledDeltaTime;

            mainCamera.transform.position =
                Vector3.Lerp(camStart, camTarget, t / zoomTime);

            mainCamera.orthographicSize =
                Mathf.Lerp(camZoomStart, camZoomTarget, t / zoomTime);

            yield return null;
        }

        // 🌌 Partículas orbitando hacia el centro
        if (phase2ParticlesPrefab != null)
        {
            GameObject particles = Instantiate(
                phase2ParticlesPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(particles, 2f);
        }

        // 💜 Simulación de dissolve (fade + contracción)
        float dissolveTime = 1.2f;
        t = 0f;

        while (t < dissolveTime)
        {
            float progress = t / dissolveTime;

            // reducción progresiva
            transform.localScale = originalScale * (1f - progress);

            // fade alpha
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - progress;
                sr.color = c;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // ⚪ FLASH BLANCO
        if (screenFade != null)
        {
            screenFade.alpha = 1f;
            yield return new WaitForSecondsRealtime(0.15f);
            screenFade.alpha = 0f;
        }

        // 💥 Explosión final
        if (phase2ExplosionPrefab != null)
            Instantiate(
                phase2ExplosionPrefab,
                transform.position,
                Quaternion.identity
            );

        CameraShake.Instance.Shake(0.5f, 1.2f);

        yield return new WaitForSecondsRealtime(0.3f);

        // 🎮 Restaurar cámara y tiempo
        Time.timeScale = 1f;

        mainCamera.transform.position = originalCamPos;
        mainCamera.orthographicSize = originalCamSize;

        if (cameraFollow != null)
            cameraFollow.enabled = true;
    }


    IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (screenFade == null) yield break;

        float start = screenFade.alpha;
        float t = 0f;

        while (t < duration)
        {
            screenFade.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        screenFade.alpha = targetAlpha;
    }

    IEnumerator ScaleToPhaseTwo()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.one * 2f;

        float duration = 0.6f;
        float t = 0f;

        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t / duration
            );

            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

}
