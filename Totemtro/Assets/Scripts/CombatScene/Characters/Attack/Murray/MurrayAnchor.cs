using UnityEngine;
using System.Collections.Generic;

public class MurrayAnchor : MonoBehaviour
{
    Transform player;

    float damage;
    float range;

    float baseAngle;

    float startAngle = -15f;
    float endAngle = 15f;

    float timer;

    enum Phase
    {
        Out,
        Sweep,
        Return
    }

    Phase phase;

    HashSet<Enemy> hookedEnemies = new HashSet<Enemy>();

    public float outTime = 0.25f;
    public float sweepTime = 0.35f;
    public float returnTime = 0.25f;

    public float pullForce = 12f;
    public float chainWidth = 0.4f;

    LineRenderer chain;

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
        damage = dmg;
        range = rng;

        chainWidth = chainWidthValue;

        baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        phase = Phase.Out;

        chain = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Out:
                PhaseOut();
                break;

            case Phase.Sweep:
                PhaseSweep();
                break;

            case Phase.Return:
                PhaseReturn();
                break;
        }

        Vector2 dir =
            ((Vector2)transform.position - (Vector2)player.position).normalized;

        RotateAnchor(dir);

        UpdateChain();
        HookChainEnemies();
        PullEnemies();
    }

    void RotateAnchor(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void PhaseOut()
    {
        float t = timer / outTime;

        float angle = baseAngle + startAngle;

        Vector2 pos = AngleToDir(angle) * range * t;

        transform.position = player.position + (Vector3)pos;

        if (t >= 1f)
        {
            phase = Phase.Sweep;
            timer = 0f;
        }
    }

    void PhaseSweep()
    {
        float t = timer / sweepTime;

        float angle = Mathf.Lerp(startAngle, endAngle, t);

        Vector2 pos = AngleToDir(baseAngle + angle) * range;

        transform.position = player.position + (Vector3)pos;

        if (t >= 1f)
        {
            phase = Phase.Return;
            timer = 0f;
        }
    }

    void PhaseReturn()
    {
        float t = timer / returnTime;

        Vector2 startPos = AngleToDir(baseAngle + endAngle) * range;

        Vector2 pos = Vector2.Lerp(startPos, Vector2.zero, t);

        transform.position = player.position + (Vector3)pos;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    Vector2 AngleToDir(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void UpdateChain()
    {
        if (chain == null) return;

        chain.SetPosition(0, player.position);
        chain.SetPosition(1, transform.position);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();

        if (enemy == null) return;
        if (hookedEnemies.Contains(enemy)) return;

        hookedEnemies.Add(enemy);

        enemy.TakeDamage(damage, Vector2.zero, false);
    }

    void PullEnemies()
    {
        foreach (Enemy enemy in hookedEnemies)
        {
            if (enemy == null) continue;

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 target = player.position;

            Vector2 newPos = Vector2.MoveTowards(
                rb.position,
                target,
                pullForce * Time.deltaTime
            );

            rb.MovePosition(newPos);
        }
    }

    void HookChainEnemies()
    {
        Vector2 start = player.position;
        Vector2 end = transform.position;

        Vector2 segment = end - start;
        float length = segment.magnitude;

        if (length < 0.01f) return;

        Vector2 dir = segment.normalized;

        RaycastHit2D[] hits =
            Physics2D.CircleCastAll(start, chainWidth, dir, length);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();

            if (enemy == null) continue;
            if (hookedEnemies.Contains(enemy)) continue;

            hookedEnemies.Add(enemy);

            Vector2 hitDir =
                ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

            enemy.TakeDamage(damage * 0.7f, hitDir, false);
        }
    }
}