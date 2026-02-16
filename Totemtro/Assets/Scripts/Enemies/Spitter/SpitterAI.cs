using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SpitterAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float attackRange = 6f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;

    bool canAttack = true;

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

        if (dist > attackRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (canAttack)
                StartCoroutine(Shoot());
        }
    }

    IEnumerator Shoot()
    {
        canAttack = false;

        yield return new WaitForSeconds(0.4f); // windup

        if (projectilePrefab != null)
        {
            Vector2 dir =
                (player.position - transform.position).normalized;

            GameObject proj = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            proj.GetComponent<EnemyProjectile>()
                .Init(dir);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
