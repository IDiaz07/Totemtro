using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    Enemy enemy;

    float duration = 5f;
    float tickRate = 1f;
    float damage = 5f;

    GameObject visual;

    public void Initialize(Enemy target, GameObject visualPrefab)
    {
        enemy = target;

        if (visualPrefab != null)
        {
            visual = Instantiate(visualPrefab, enemy.transform);
            visual.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(BurnRoutine());
    }

    IEnumerator BurnRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (enemy == null)
                break;

            enemy.TakeDamage(damage, Vector2.zero, false);

            yield return new WaitForSeconds(tickRate);
            timer += tickRate;
        }

        if (visual != null)
            Destroy(visual);

        Destroy(this);
    }
}