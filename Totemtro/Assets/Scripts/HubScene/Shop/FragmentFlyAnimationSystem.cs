using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FragmentFlyAnimationSystem : MonoBehaviour
{
    public static FragmentFlyAnimationSystem Instance;

    public Canvas canvas;
    public RectTransform animationRoot;
    public GameObject fragmentPrefab;

    public Transform globalFragmentTarget; // 👈 EL ÚNICO TARGET

    void Awake()
    {
        Instance = this;
    }

    public void PlayFragmentFly(
        Sprite fragmentSprite,
        Vector3 startWorldPos,
        HeroType heroType,
        int amount)
    {
        StartCoroutine(FlyRoutine(fragmentSprite, startWorldPos, heroType, amount));
    }

    IEnumerator FlyRoutine(
    Sprite sprite,
    Vector3 startScreenPos,
    HeroType heroType,
    int amount)
    {
        int visualCount = 6; // 👈 cantidad visual base

        // Puedes escalar según amount si quieres
        if (amount >= 20) visualCount = 12;
        else if (amount >= 10) visualCount = 8;

        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransform targetRect = globalFragmentTarget as RectTransform;
        Vector2 targetPos = targetRect.anchoredPosition;

        for (int i = 0; i < visualCount; i++)
        {
            GameObject obj = Instantiate(fragmentPrefab, animationRoot);
            Image img = obj.GetComponent<Image>();

            RectTransform rect = obj.GetComponent<RectTransform>();

            // Posición inicial
            Vector2 localStart;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                startScreenPos,
                canvas.worldCamera,
                out localStart);

            rect.anchoredPosition = localStart;

            // Pequeña dispersión inicial random
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 40f;
            Vector2 midPoint = Vector2.Lerp(localStart, targetPos, 0.5f)
                               + new Vector2(0, 220f)
                               + randomOffset;

            StartCoroutine(AnimateSingleFragment(
                rect,
                localStart,
                midPoint,
                targetPos,
                heroType,
                i == 0 ? amount : 0
            ));

            yield return new WaitForSeconds(0.05f); // pequeño stagger
        }
    }

    IEnumerator AnimateSingleFragment(
    RectTransform rect,
    Vector2 start,
    Vector2 mid,
    Vector2 end,
    HeroType heroType,
    int amountToAdd)
    {
        float duration = 1.6f;
        float time = 0.5f;

        rect.localScale = Vector3.one * 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float eased = Mathf.SmoothStep(0f, 1f, t);

            Vector2 p0 = Vector2.Lerp(start, mid, eased);
            Vector2 p1 = Vector2.Lerp(mid, end, eased);
            Vector2 finalPos = Vector2.Lerp(p0, p1, eased);

            rect.anchoredPosition = finalPos;

            float scale = Mathf.Lerp(0.8f, 1.2f, eased);
            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        rect.anchoredPosition = end;

        yield return ImpactPunch(rect);

        Destroy(rect.gameObject);

        // 🔥 AQUÍ VA
        if (amountToAdd > 0)
        {
            HeroProgressSystem.Instance.AddFragments(heroType, amountToAdd);
        }
    }

    IEnumerator ImpactPunch(RectTransform rect)
    {
        float punchTime = 0.15f;
        float t = 0f;

        Vector3 original = rect.localScale;

        while (t < punchTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 0.9f, t / punchTime);
            rect.localScale = Vector3.one * scale;
            yield return null;
        }

        rect.localScale = original;
    }
}