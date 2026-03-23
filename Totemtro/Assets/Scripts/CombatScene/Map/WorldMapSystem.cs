using UnityEngine;
using UnityEngine.UI;

public class WorldMapSystem : MonoBehaviour
{
    [Header("References")]
    public Camera worldCamera;
    public RawImage fogImage;
    public Transform player;

    [Header("Player Icon")]
    public GameObject playerIconPrefab;
    GameObject playerIconInstance;

    [Header("Map Control")]
    public float zoomSpeed = 5f;
    public float minZoom = 20f;
    public float maxZoom = 80f;

    [Header("Fog Settings")]
    public int textureSize = 512;
    public int gridSize = 128;
    public int revealRadius = 2;

    // Fog
    Texture2D fogTexture;
    Color32[] outputBuffer;
    bool[,] exploredGrid;

    static readonly Color32 FOG_BLACK = new Color32(0, 0, 0, 255);
    static readonly Color32 FOG_GREY = new Color32(0, 0, 0, 140);

    // Drag
    bool isDragging;
    Vector3 dragStartCamPos;
    Vector2 dragStartMouse;

    // Camera memory
    Vector3 savedCameraPos;
    bool hasSavedPosition;

    // =====================================================
    // UNITY
    // =====================================================

    void Awake()
    {
        GenerateFog();
    }

    void Start()
    {
        CreatePlayerIcon();

        if (worldCamera != null)
            savedCameraPos = worldCamera.transform.position;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    void LateUpdate()
    {
        if (player != null && worldCamera != null)
            Reveal();
    }

    void OnDestroy()
    {
        if (fogTexture != null)
            Destroy(fogTexture);
    }

    // =====================================================
    // PLAYER ICON
    // =====================================================

    void CreatePlayerIcon()
    {
        if (playerIconPrefab == null || player == null) return;

        playerIconInstance = Instantiate(playerIconPrefab, player);
        playerIconInstance.transform.localPosition = Vector3.zero;
        playerIconInstance.name = "WorldMap_PlayerIcon";
    }

    // =====================================================
    // DRAG
    // =====================================================

    void HandleDrag()
    {
        if (worldCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartMouse = Input.mousePosition;
            dragStartCamPos = worldCamera.transform.position;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - dragStartMouse;

            float camHeight = worldCamera.orthographicSize * 2f;
            float pixelToWorld = camHeight / Screen.height;

            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * pixelToWorld;

            worldCamera.transform.position = dragStartCamPos + move;

            savedCameraPos = worldCamera.transform.position;
            hasSavedPosition = true;
        }
    }

    // =====================================================
    // ZOOM
    // =====================================================

    void HandleZoom()
    {
        if (worldCamera == null) return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f) return;

        worldCamera.orthographicSize -= scroll * zoomSpeed;
        worldCamera.orthographicSize = Mathf.Clamp(
            worldCamera.orthographicSize,
            minZoom,
            maxZoom
        );

        savedCameraPos = worldCamera.transform.position;
        hasSavedPosition = true;
    }

    // =====================================================
    // FOG SYSTEM (GRID + CAMERA CORRECTO)
    // =====================================================

    void GenerateFog()
    {
        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        outputBuffer = new Color32[textureSize * textureSize];
        exploredGrid = new bool[gridSize, gridSize];

        for (int i = 0; i < outputBuffer.Length; i++)
            outputBuffer[i] = FOG_BLACK;

        fogTexture.SetPixels32(outputBuffer);
        fogTexture.Apply(false);

        if (fogImage != null)
            fogImage.texture = fogTexture;
    }

    void Reveal()
    {
        Vector2 norm = WorldToViewport(player.position);

        int gx = Mathf.Clamp(Mathf.FloorToInt(norm.x * gridSize), 0, gridSize - 1);
        int gy = Mathf.Clamp(Mathf.FloorToInt(norm.y * gridSize), 0, gridSize - 1);

        for (int y = -revealRadius; y <= revealRadius; y++)
        {
            for (int x = -revealRadius; x <= revealRadius; x++)
            {
                int nx = gx + x;
                int ny = gy + y;

                if (nx < 0 || ny < 0 || nx >= gridSize || ny >= gridSize)
                    continue;

                exploredGrid[nx, ny] = true;
            }
        }

        UpdateTexture(gx, gy);
    }

    Vector2 WorldToViewport(Vector3 worldPos)
    {
        Vector3 v = worldCamera.WorldToViewportPoint(worldPos);
        return new Vector2(v.x, v.y);
    }

    void UpdateTexture(int playerGX, int playerGY)
    {
        int cellSize = textureSize / gridSize;

        for (int gy = 0; gy < gridSize; gy++)
        {
            for (int gx = 0; gx < gridSize; gx++)
            {
                bool explored = exploredGrid[gx, gy];

                Color32 color = explored ? FOG_GREY : FOG_BLACK;

                float dist = Vector2.Distance(
                    new Vector2(gx, gy),
                    new Vector2(playerGX, playerGY)
                );

                if (dist < revealRadius)
                    color = new Color32(0, 0, 0, 0);

                for (int y = 0; y < cellSize; y++)
                {
                    for (int x = 0; x < cellSize; x++)
                    {
                        int px = gx * cellSize + x;
                        int py = gy * cellSize + y;

                        outputBuffer[py * textureSize + px] = color;
                    }
                }
            }
        }

        fogTexture.SetPixels32(outputBuffer);
        fogTexture.Apply(false);
    }

    // =====================================================
    // PUBLIC API
    // =====================================================

    public void CenterOnPlayer()
    {
        if (worldCamera == null || player == null) return;

        Vector3 pos = player.position;
        pos.z = worldCamera.transform.position.z;

        worldCamera.transform.position = pos;
        savedCameraPos = pos;
    }

    public void RestoreCameraPosition()
    {
        if (worldCamera != null && hasSavedPosition)
            worldCamera.transform.position = savedCameraPos;
    }

    public void ResetFog()
    {
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                exploredGrid[x, y] = false;

        for (int i = 0; i < outputBuffer.Length; i++)
            outputBuffer[i] = FOG_BLACK;

        fogTexture.SetPixels32(outputBuffer);
        fogTexture.Apply(false);
    }
}