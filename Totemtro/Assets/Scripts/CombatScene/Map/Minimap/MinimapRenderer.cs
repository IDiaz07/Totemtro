using UnityEngine;
using UnityEngine.UI;

public class MinimapRenderer : MonoBehaviour
{
    [Header("References")]
    public RawImage mapImage;
    public MapGenerator mapGenerator;
    public Transform player;
    public RectTransform mapRect;

    [Header("Settings")]
    public int textureSize = 256;
    public float worldSize = 80f;
    public float moveScale = 8f;

    Texture2D mapTexture;

    void Start()
    {
        GenerateTexture();
    }

    void Update()
    {
        if (player == null || mapRect == null) return;

        Vector2 offset = new Vector2(
            player.position.x,
            player.position.y
        ) * moveScale;

        mapRect.anchoredPosition = -offset;
    }

    void GenerateTexture()
    {
        mapTexture = new Texture2D(textureSize, textureSize);
        mapTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                Vector2 worldPos = TextureToWorld(x, y);
                mapTexture.SetPixel(x, y, GetColor(worldPos));
            }
        }

        mapTexture.Apply();
        mapImage.texture = mapTexture;
    }

    Vector2 TextureToWorld(int x, int y)
    {
        float wx = (x / (float)textureSize - 0.5f) * worldSize;
        float wy = (y / (float)textureSize - 0.5f) * worldSize;

        return new Vector2(wx, wy);
    }

    Color GetColor(Vector2 pos)
    {
        if (mapGenerator != null && mapGenerator.IsInsideMazeArea(pos))
            return Color.gray;

        float noise = Mathf.PerlinNoise(pos.x * 0.05f, pos.y * 0.05f);

        if (noise < 0.3f)
            return new Color(0.05f, 0.05f, 0.05f);

        if (noise < 0.6f)
            return new Color(0.1f, 0.2f, 0.1f);

        return new Color(0.2f, 0.15f, 0.1f);
    }
}