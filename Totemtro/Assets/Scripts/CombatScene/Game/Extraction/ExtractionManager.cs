using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExtractionManager : MonoBehaviour
{
    public static ExtractionManager Instance;

    public GameObject extractionZonePrefab;

    public float spawnDelay = 30f;
    public float channelTime = 5f;
    public float safeRadiusFromPlayer = 20f;

    public Vector2 mapMin;
    public Vector2 mapMax;
    public float borderPadding = 10f;

    bool extractionPending = false;
    bool extractionActive = false;

    public float zoneLifetime = 60f;
    ExtractionZone currentZone;

    public AudioSource globalAudio;
    public AudioClip sirenClip;

    void PlayGlobalSiren()
    {
        if (globalAudio != null && sirenClip != null)
            globalAudio.PlayOneShot(sirenClip);
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!extractionPending && !extractionActive)
            {
                StartCoroutine(StartExtractionCountdown());
            }
        }
    }

    IEnumerator StartExtractionCountdown()
    {
        extractionPending = true;

        yield return new WaitForSeconds(spawnDelay - 5f);

        //BossManager.Instance.BlockBossSpawn(true);

        yield return new WaitForSeconds(5f);

        SpawnExtractionZone();

        extractionPending = false;
        extractionActive = true;
    }

    void SpawnExtractionZone()
    {
        Vector2 spawnPos = FindValidSpawnPosition();

        GameObject zoneObj =
            Instantiate(extractionZonePrefab, spawnPos, Quaternion.identity);

        currentZone = zoneObj.GetComponent<ExtractionZone>();

        currentZone.Initialize(zoneLifetime);

        StartCoroutine(ZoneExpirationRoutine());

        PlayGlobalSiren();
        ExtractionArrowUI.Instance.SetTarget(zoneObj.transform);
    }

    Vector2 FindValidSpawnPosition()
    {
        Vector2 playerPos =
            GameObject.FindGameObjectWithTag("Player").transform.position;

        for (int i = 0; i < 100; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(mapMin.x + borderPadding, mapMax.x - borderPadding),
                Random.Range(mapMin.y + borderPadding, mapMax.y - borderPadding)
            );

            if (Vector2.Distance(pos, playerPos) > safeRadiusFromPlayer)
                return pos;
        }

        return mapMin;
    }

    public void CompleteExtraction()
    {
        GameManager.Instance.ExtractRun();
        extractionActive = false;
    }

    IEnumerator ZoneExpirationRoutine()
    {
        yield return new WaitForSeconds(zoneLifetime);

        if (currentZone != null)
        {
            currentZone.CloseZone();
            extractionActive = false;
        }
    }
}