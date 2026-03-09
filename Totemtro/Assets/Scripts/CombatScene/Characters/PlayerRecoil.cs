using UnityEngine;
using System.Collections;

public class PlayerRecoil : MonoBehaviour
{
    Vector3 originalPos;
    Coroutine recoilRoutine;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Recoil(Vector2 shootDirection, float distance, float duration)
    {
        if (recoilRoutine != null)
            StopCoroutine(recoilRoutine);

        recoilRoutine =
            StartCoroutine(RecoilRoutine(shootDirection, distance, duration));
    }

    IEnumerator RecoilRoutine(Vector2 dir, float distance, float duration)
    {
        Vector3 recoilOffset = -(Vector3)dir * distance;

        transform.localPosition = originalPos + recoilOffset;

        float timer = 0f;

        while (timer < duration)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPos,
                timer / duration
            );

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}