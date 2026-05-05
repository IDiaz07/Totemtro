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
        // Restar el reward pendiente para que el display arranque
        // con el valor ANTERIOR y la animación haga subir el número
        int pendingGold = SummarySceneUI.PendingGoldReward;

        displayedGold = MetaCurrencySystem.Instance.Gold - pendingGold;
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
        // Más monedas visibles cuanto mayor sea el amount
        int visualCount = Mathf.Clamp(amount / 5, 8, 50);

        // Vuelo más lento: de 1s a 2.5s según cantidad
        float flyDuration = Mathf.Lerp(1f, 2.5f, Mathf.Clamp01(amount / 500f));

        // Delay entre spawns más pronunciado para que se vea la cascada
        float spawnDelay = Mathf.Lerp(0.04f, 0.06f, Mathf.Clamp01(amount / 500f));

        Vector2 startPos = WorldToCanvasPosition(worldStartPos);
        Vector2 endPos = WorldToCanvasPosition(target.position);

        for (int i = 0; i < visualCount; i++)
        {
            GameObject obj = Instantiate(prefab, canvas.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.anchoredPosition =
                startPos + Random.insideUnitCircle * 40f;

            StartCoroutine(MoveToTarget(rect, endPos, flyDuration));

            yield return new WaitForSeconds(spawnDelay);
        }

        // Esperar a que la última moneda llegue antes de contar
        yield return new WaitForSeconds(flyDuration * 0.5f);

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
            // Ease-in para que acelere al final (más satisfactorio)
            float progress = t / duration;
            float eased = progress * progress;

            rect.anchoredPosition =
                Vector2.Lerp(start, targetPos, eased);

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

        // Duración del contador proporcional a la cantidad de monedas
        // Más monedas = más tiempo subiendo el número
        float duration = Mathf.Lerp(1.2f, 3f, Mathf.Clamp01(amount / 500f));
        float t = 0f;

        while (t < duration)
        {
            // Ease-out para que suba rápido al principio y desacelere
            float progress = t / duration;
            float eased = 1f - (1f - progress) * (1f - progress);

            int current = Mathf.RoundToInt(
                Mathf.Lerp(startValue, realValue, eased)
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