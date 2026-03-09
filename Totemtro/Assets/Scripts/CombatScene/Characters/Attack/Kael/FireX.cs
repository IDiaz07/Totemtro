using UnityEngine;
using System.Collections.Generic;

public class FireX : MonoBehaviour
{
    float damage;
    Vector2 direction;
    Transform player;

    float angle;
    float spiralRadius;

    [Header("Spiral")]
    public float spiralRotationSpeed = 6f;
    public int spiralDirection = 1;
    public float spriteRotationOffset = 90f;

    [Header("Visual")]
    public float targetScale = 2.5f;

    [Header("Lifetime")]
    public float lifeTime = 1.5f;

    [Header("Damage")]
    public float hitCooldown = 0.4f;
    public float damageRadius = 0.8f;

    static Dictionary<Enemy, float> globalLastHitTime =
        new Dictionary<Enemy, float>();

    Collider2D[] hitBuffer = new Collider2D[20];

    public void Initialize(
        float dmg,
        Vector2 dir,
        Transform playerTransform,
        float radius)
    {
        damage = dmg;
        direction = dir.normalized;
        player = playerTransform;

        spiralRadius = radius;

        angle = Mathf.Atan2(dir.y, dir.x);

        transform.position = player.position;

        RotateSprite(direction);

        transform.localScale = Vector3.zero;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        MoveSpiral();
        Grow();
        CheckDamage();
    }

    void MoveSpiral()
    {
        angle += spiralRotationSpeed * spiralDirection * Time.deltaTime;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * spiralRadius;

        transform.position = (Vector2)player.position + offset;

        RotateSprite(offset);
    }

    void RotateSprite(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        float rot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, rot + spriteRotationOffset);
    }

    void Grow()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            12f * Time.deltaTime
        );
    }

    void CheckDamage()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            damageRadius,
            hitBuffer
        );

        float currentTime = Time.time;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            if (globalLastHitTime.TryGetValue(enemy, out float lastHit))
            {
                if (currentTime - lastHit < hitCooldown)
                    continue;

                globalLastHitTime[enemy] = currentTime;
            }
            else
            {
                globalLastHitTime.Add(enemy, currentTime);
            }

            enemy.TakeDamage(damage, direction, false);
        }
    }
}