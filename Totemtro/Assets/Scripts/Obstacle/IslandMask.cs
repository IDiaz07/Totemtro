using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IslandMask : MonoBehaviour
{
    [Header("Mask Settings")]
    public int textureSize = 512;
    public float fadeWidth = 8f;
    public float noiseScale = 0.05f;
    public float noiseStrength = 6f;

    private SpriteRenderer sr;

    public void Generate(Vector2 mapSize, float seedX, float seedY)
    {
        sr = GetComponent<SpriteRenderer>();

        float radius = Mathf.Min(mapSize.x, mapSize.y) * 0.5f;

        // Crear textura con borde circular suave
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float halfSize = textureSize * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Posición normalizada (-1 a 1)
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;

                // Posición en mundo
                float wx = nx * radius;
                float wy = ny * radius;

                // Distancia al centro (circular perfecto)
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                // Ruido para bordes orgánicos
                float noise = Mathf.PerlinNoise(
                    (wx + seedX) * noiseScale,
                    (wy + seedY) * noiseScale
                );

                // Radio variable con ruido (forma orgánica)
                float edgeRadius = 0.7f + noise * 0.4f;

                // Fade suave en el borde
                float fadeNorm = fadeWidth / radius;
                float alpha;

                if (dist < edgeRadius - fadeNorm)
                {
                    // Dentro de la isla: completamente transparente
                    // (deja ver el tilemap debajo)
                    alpha = 0f;
                }
                else if (dist < edgeRadius)
                {
                    // Zona de transición: fade gradual
                    float t = (dist - (edgeRadius - fadeNorm)) / fadeNorm;
                    // Curva suave
                    t = t * t * (3f - 2f * t);
                    alpha = t;
                }
                else
                {
                    // Fuera de la isla: completamente opaco (tapa el tilemap)
                    alpha = 1f;
                }

                // Color de la máscara = color del fondo (negro/azul oscuro)
                tex.SetPixel(x, y, new Color(0f, 0f, 0.05f, alpha));
            }
        }

        tex.Apply();

        // Crear sprite que cubra todo el mapa
        float pixelsPerUnit = textureSize / (radius * 2f);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );

        sr.sprite = sprite;
        sr.sortingOrder = 1; // Encima del tilemap
        transform.position = Vector3.zero;
    }
}
