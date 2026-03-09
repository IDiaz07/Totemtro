using UnityEngine;
using System.Collections.Generic;

public class MurrayAnchor : MonoBehaviour
{
    Transform player;

    float damage;
    float range;

    Vector2 direction;

    Vector3 startPos;

    bool returning = false;
    bool isEnding = false;

    HashSet<Enemy> hookedEnemies = new HashSet<Enemy>();

    LineRenderer chain;

    public float speed = 14f;
    public float pullForce = 40f;

    public float chainWidth = 0.4f;

    public float explosionRadius = 2f;
    public float explosionDamageMultiplier = 1.5f;

    public void Initialize(
        Transform playerTransform,
        Vector2 dir,
        float dmg,
        float rng,
        float anchorRadiusValue,
        float chainWidthValue
    )
    {
        player = playerTransform;
        direction = dir;
        damage = dmg;
        range = rng;
        chainWidth = chainWidthValue;

        startPos = transform.position;

        chain = GetComponent<LineRenderer>();

        // seguridad para que nunca se quede vivo
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (isEnding) return;

        Move();

        if (isEnding) return;

        UpdateChain();
        HookChainEnemies();
        PullEnemies();
    }

    void Move()
    {
        if (!returning)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            if (Vector2.Distance(startPos, transform.position) >= range)
                returning = true;
        }
        else
        {
            Vector2 dirToPlayer =
                ((Vector2)player.position - (Vector2)transform.position).normalized;

            transform.position += (Vector3)(dirToPlayer * speed * Time.deltaTime);

            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= 0.45f)
            {
                isEnding = true;
                Explode();
                Destroy(gameObject);
            }
        }
    }

    void UpdateChain()
    {
        if (chain == null) return;

        chain.SetPosition(0, player.position);
        chain.SetPosition(1, transform.position);
    }

    // --------------------------------
    // ANCHOR HIT
    // --------------------------------

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isEnding) return;

        Enemy enemy = col.GetComponent<Enemy>();

        if (enemy == null) return;
        if (hookedEnemies.Contains(enemy)) return;

        hookedEnemies.Add(enemy);

        Vector2 dir =
            ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

        enemy.TakeDamage(damage, dir, false);

        HitStop.Instance?.Stop(0.03f);
    }

    // --------------------------------
    // CHAIN HOOK
    // --------------------------------

    void HookChainEnemies()
    {
        Vector2 start = player.position;
        Vector2 end = transform.position;

        RaycastHit2D[] hits =
            Physics2D.CircleCastAll(start, chainWidth, end - start, Vector2.Distance(start, end));

        foreach (var hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();

            if (enemy == null) continue;
            if (hookedEnemies.Contains(enemy)) continue;

            hookedEnemies.Add(enemy);

            Vector2 dir =
                ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

            enemy.TakeDamage(damage * 0.7f, dir, false);
        }
    }

    // --------------------------------
    // PULL
    // --------------------------------

    void PullEnemies()
    {
        foreach (Enemy enemy in hookedEnemies)
        {
            if (enemy == null) continue;

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 dir =
                ((Vector2)player.position - rb.position).normalized;

            // mover enemigo sin física
            rb.MovePosition(rb.position + dir * pullForce * Time.deltaTime);
        }
    }

    // --------------------------------
    // EXPLOSION
    // --------------------------------

    void Explode()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(player.position, explosionRadius);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null) continue;

            Vector2 dir =
                ((Vector2)enemy.transform.position - (Vector2)player.position).normalized;

            enemy.TakeDamage(damage * explosionDamageMultiplier, dir, false);
        }

        HitStop.Instance?.Stop(0.05f);
    }
}