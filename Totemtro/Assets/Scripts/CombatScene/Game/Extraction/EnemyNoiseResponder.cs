using UnityEngine;
using System.Collections;

public class EnemyNoiseResponder : MonoBehaviour, INoiseListener
{
    Vector2 noiseTarget;
    bool responding = false;

    public float investigateDuration = 4f;
    public float investigateSpeed = 2.5f;

    public void OnNoiseHeard(Vector2 position)
    {
        noiseTarget = position;

        if (!responding)
            StartCoroutine(InvestigateRoutine());
    }

    IEnumerator InvestigateRoutine()
    {
        responding = true;

        float timer = 0f;

        while (timer < investigateDuration)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                noiseTarget,
                investigateSpeed * Time.deltaTime
            );

            timer += Time.deltaTime;
            yield return null;
        }

        responding = false;
    }
}