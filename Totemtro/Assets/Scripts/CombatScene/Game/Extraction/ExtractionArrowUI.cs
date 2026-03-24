using UnityEngine;

public class ExtractionArrowUI : MonoBehaviour
{
    public static ExtractionArrowUI Instance;

    [Header("References")]
    public RectTransform arrow;

    [Header("Orbit Settings")]
    public float orbitRadius = 150f;

    [Header("Auto-hide")]
    public float hideDistance = 5f;

    Transform target;
    Transform player;
    RectTransform canvasRect;
    bool isOverlay;

    void Awake()
    {
        Instance = this;
        arrow.gameObject.SetActive(false);

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;

        if (t != null)
            arrow.gameObject.SetActive(true);
        else
            arrow.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        Vector3 dir = target.position - player.position;
        float distance = dir.magnitude;

        if (distance < hideDistance)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        else if (!arrow.gameObject.activeSelf)
        {
            arrow.gameObject.SetActive(true);
        }

        // Ángulo en world space
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Convertir posición del player a canvas
        Vector2 playerCanvasPos = WorldToCanvas(player.position);

        // Orbitar alrededor del jugador
        float rad = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;

        arrow.anchoredPosition = playerCanvasPos + offset;

        // Sprite apunta a la derecha → Atan2 ya da el ángulo correcto, sin offset
        arrow.localRotation = Quaternion.Euler(0, 0, angle);
    }

    Vector2 WorldToCanvas(Vector3 worldPos)
    {
        Camera cam = Camera.main;

        Vector2 screenPoint = cam.WorldToScreenPoint(worldPos);

        Camera canvasCam = isOverlay ? null : cam;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvasCam,
            out Vector2 localPoint
        );

        return localPoint;
    }
}