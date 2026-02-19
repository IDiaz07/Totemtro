using UnityEngine;

public class NullGrenade : MonoBehaviour
{
    public float delay = 2f;
    public float radius = 3f;
    public float damagePerSecond = 10f;
    public float duration = 4f;
    public float pullForce = 8f;

    void Start()
    {
        Invoke(nameof(ActivateZone), delay);
    }

    void ActivateZone()
    {
        StartCoroutine(VoidZone());
    }

    System.Collections.IEnumerator VoidZone()
    {
        float timer = 0f;

        while (timer < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector2 dir = (transform.position - enemy.transform.position).normalized;

                    enemy.TakeDamage(damagePerSecond * Time.deltaTime, dir * pullForce, false);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
