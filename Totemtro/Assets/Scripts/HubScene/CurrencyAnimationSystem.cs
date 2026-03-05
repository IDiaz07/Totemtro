using UnityEngine;
using TMPro;
using System.Collections;

public class CurrencyAnimationSystem : MonoBehaviour
{
    public static CurrencyAnimationSystem Instance;

    [Header("References")]
    public Canvas canvas;
    public Camera uiCamera;

    [Header("Targets")]
    public RectTransform goldTarget;
    public RectTransform gemsTarget;

    [Header("Prefabs (UI - Image + RectTransform)")]
    public GameObject goldCoinPrefab;
    public GameObject gemPrefab;

    [Header("Counter UI")]
    public TMP_Text goldText;
    public TMP_Text gemsText;

    RectTransform canvasRect;

    int displayedGold;
    int displayedGems;

    void Awake()
    {
        Instance = this;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Start()
    {
        displayedGold = MetaCurrencySystem.Instance.Gold;
        displayedGems = MetaCurrencySystem.Instance.Gems;

        goldText.text = displayedGold.ToString("N0");
        gemsText.text = displayedGems.ToString("N0");
    }

    // =====================================================
    // PUBLIC
    // =====================================================

    public void PlayGoldAnimation(int amount, Vector3 worldStartPos)
    {
        if (goldTarget == null || goldCoinPrefab == null) return;

        StartCoroutine(SpawnCurrency(
            amount,
            goldCoinPrefab,
            worldStartPos,
            goldTarget,
            true
        ));
    }

    public void PlayGemsAnimation(int amount, Vector3 worldStartPos)
    {
        if (gemsTarget == null || gemPrefab == null) return;

        StartCoroutine(SpawnCurrency(
            amount,
            gemPrefab,
            worldStartPos,
            gemsTarget,
            false
        ));
    }

    // =====================================================
    // CORE
    // =====================================================

    IEnumerator SpawnCurrency(
        int amount,
        GameObject prefab,
        Vector3 worldStartPos,
        RectTransform target,
        bool isGold)
    {
        int visualCount = Mathf.Clamp(amount / 10, 5, 30);
        float duration = Mathf.Lerp(0.5f, 1.6f, Mathf.Clamp01(amount / 500f));

        Vector2 startPos = WorldToCanvasPosition(worldStartPos);
        Vector2 endPos = WorldToCanvasPosition(target.position);

        for (int i = 0; i < visualCount; i++)
        {
            GameObject obj = Instantiate(prefab, canvas.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.anchoredPosition =
                startPos + Random.insideUnitCircle * 40f;

            StartCoroutine(MoveToTarget(rect, endPos, duration));

            yield return new WaitForSeconds(0.02f);
        }

        yield return AnimateCounter(amount, isGold);
    }

    IEnumerator MoveToTarget(
        RectTransform rect,
        Vector2 targetPos,
        float duration)
    {
        Vector2 start = rect.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            rect.anchoredPosition =
                Vector2.Lerp(start, targetPos, t / duration);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(rect.gameObject);
    }

    IEnumerator AnimateCounter(int amount, bool isGold)
    {
        int startValue = isGold ? displayedGold : displayedGems;

        int realValue = isGold
            ? MetaCurrencySystem.Instance.Gold
            : MetaCurrencySystem.Instance.Gems;

        float duration = 0.9f;
        float t = 0f;

        while (t < duration)
        {
            int current = Mathf.RoundToInt(
                Mathf.Lerp(startValue, realValue, t / duration)
            );

            if (isGold)
            {
                displayedGold = current;
                goldText.text = current.ToString("N0");
            }
            else
            {
                displayedGems = current;
                gemsText.text = current.ToString("N0");
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (isGold)
        {
            displayedGold = realValue;
            goldText.text = realValue.ToString("N0");
        }
        else
        {
            displayedGems = realValue;
            gemsText.text = realValue.ToString("N0");
        }
    }

    // =====================================================
    // CONVERSION CORRECTA
    // =====================================================

    Vector2 WorldToCanvasPosition(Vector3 worldPos)
    {
        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}