using UnityEngine;
using System.Collections;

public class SummonerAI : MonoBehaviour
{
    public GameObject thrallPrefab;
    public float summonCooldown = 6f;
    public float keepDistance = 5f;
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;

    bool canSummon = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
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

        yield return new WaitForSeconds(1f);

        Instantiate(thrallPrefab,
            transform.position + (Vector3)Random.insideUnitCircle * 1.5f,
            Quaternion.identity);

        yield return new WaitForSeconds(summonCooldown);
        canSummon = true;
    }
}
