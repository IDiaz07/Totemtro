using UnityEngine;
using System.Collections;

public class SummonerAI : MonoBehaviour
{
    [Header("Summon")]
    public GameObject thrallPrefab;
    public int thrallsToSummon = 5;
    public float summonCooldown = 6f;
    public float summonCastTime = 1.5f;

    [Header("Movement")]
    public float keepDistance = 5f;
    public float moveSpeed = 2f;

    [Header("Shake")]
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 50f;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;

    bool canSummon = true;

    SummonerDirectionalSprite spriteController;
    GameObject invokerFloor;
    Transform body;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();

        spriteController = GetComponentInChildren<SummonerDirectionalSprite>();
        invokerFloor = transform.Find("InvokerFloor")?.gameObject;
        body = transform.Find("Body");

        if (invokerFloor != null)
            invokerFloor.SetActive(false);
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (enemy.IsKnocked()) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < keepDistance)
        {
            Vector2 dir =
                (transform.position - player.position).normalized;

            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (canSummon)
                StartCoroutine(Summon());
        }
    }

    IEnumerator Summon()
    {
        canSummon = false;
        rb.linearVelocity = Vector2.zero;

        if (spriteController != null)
            spriteController.SetSummoning(true);

        if (invokerFloor != null)
            invokerFloor.SetActive(true);

        yield return StartCoroutine(ShakeDuringCast());

        SpawnThralls();

        if (spriteController != null)
            spriteController.SetSummoning(false);

        if (invokerFloor != null)
            invokerFloor.SetActive(false);

        yield return new WaitForSeconds(summonCooldown);

        canSummon = true;
    }

    IEnumerator ShakeDuringCast()
    {
        if (body == null)
        {
            yield return new WaitForSeconds(summonCastTime);
            yield break;
        }

        Vector3 originalPos = body.localPosition;
        float timer = 0f;

        while (timer < summonCastTime)
        {
            float shakeX =
                Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;

            float shakeY =
                Mathf.Cos(Time.time * shakeSpeed * 0.8f) * shakeIntensity;

            body.localPosition =
                originalPos + new Vector3(shakeX, shakeY, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        body.localPosition = originalPos;
    }

    void SpawnThralls()
    {
        if (thrallPrefab == null) return;

        float radius = 1.8f;

        for (int i = 0; i < thrallsToSummon; i++)
        {
            float angle = i * Mathf.PI * 2f / thrallsToSummon;

            Vector2 offset = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;

            Instantiate(
                thrallPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }
}
