using UnityEngine;

public class TerrainDataMap : MonoBehaviour
{
    public Vector2Int resolution = new Vector2Int(512, 512);
    public Vector2 worldSize = new Vector2(320, 320);
    
    private Texture2D terrainMap;
    private RenderTexture blendMap;
    
    private float seedX;
    private float seedY;
    
    public void Initialize(float seedX, float seedY)
    {
        this.seedX = seedX;
        this.seedY = seedY;
        
        GenerateTerrainMap();
        CreateBlendMap();
    }
    
    void GenerateTerrainMap()
    {
        terrainMap = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBAFloat, false);
        terrainMap.filterMode = FilterMode.Bilinear;
        terrainMap.wrapMode = TextureWrapMode.Clamp;
        
        for (int y = 0; y < resolution.y; y++)
        {
            for (int x = 0; x < resolution.x; x++)
            {
                Vector2 worldPos = new Vector2(
                    (x / (float)resolution.x - 0.5f) * worldSize.x,
                    (y / (float)resolution.y - 0.5f) * worldSize.y
                );
                
                float islandValue = CalculateIslandGradient(worldPos);
                float terrainNoise = GetTerrainNoise(worldPos);
                float sandBlend = CalculateSandBlend(worldPos, islandValue);
                float detail = GetDetailNoise(worldPos);
                
                terrainMap.SetPixel(x, y, new Color(
                    islandValue,
                    terrainNoise,
                    sandBlend,
                    detail
                ));
            }
        }
        
        terrainMap.Apply();
    }
    
    float CalculateIslandGradient(Vector2 pos)
    {
        float noise1 = Mathf.PerlinNoise(
            (pos.x + seedX) * 0.05f,
            (pos.y + seedY) * 0.05f
        );
        
        float noise2 = Mathf.PerlinNoise(
            (pos.x + seedX) * 0.1f,
            (pos.y + seedY) * 0.1f
        ) * 0.5f;
        
        float noise3 = Mathf.PerlinNoise(
            (pos.x + seedX) * 0.2f,
            (pos.y + seedY) * 0.2f
        ) * 0.25f;
        
        float combinedNoise = noise1 + noise2 + noise3;
        
        float maxDist = Mathf.Min(worldSize.x, worldSize.y) * 0.5f;
        float dist = pos.magnitude;
        
        float island = maxDist * (0.7f + combinedNoise * 0.4f);
        
        float gradient = 1f - Mathf.Clamp01(dist / island);
        
        return Mathf.SmoothStep(0f, 1f, gradient);
    }
    
    float CalculateSandBlend(Vector2 pos, float islandValue)
    {
        float edgeStart = 0.3f;
        float edgeEnd = 0.1f;
        
        if (islandValue > edgeStart)
            return 0f;
        
        if (islandValue < edgeEnd)
            return 1f;
        
        float blend = Mathf.InverseLerp(edgeStart, edgeEnd, islandValue);
        
        float noise = Mathf.PerlinNoise(
            (pos.x + seedX) * 0.15f,
            (pos.y + seedY) * 0.15f
        );
        
        blend += (noise - 0.5f) * 0.3f;
        
        return Mathf.Clamp01(blend);
    }
    
    float GetTerrainNoise(Vector2 pos)
    {
        return Mathf.PerlinNoise(
            (pos.x + seedX) * 0.3f,
            (pos.y + seedY) * 0.3f
        );
    }
    
    float GetDetailNoise(Vector2 pos)
    {
        return Mathf.PerlinNoise(
            (pos.x + seedX) * 0.5f,
            (pos.y + seedY) * 0.5f
        );
    }
    
    void CreateBlendMap()
    {
        blendMap = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
        blendMap.filterMode = FilterMode.Bilinear;
        blendMap.wrapMode = TextureWrapMode.Clamp;
    }
    
    public Texture2D GetTerrainMap() => terrainMap;
    public RenderTexture GetBlendMap() => blendMap;
}