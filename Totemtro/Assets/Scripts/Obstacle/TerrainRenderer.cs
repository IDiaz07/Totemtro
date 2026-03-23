using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainRenderer : MonoBehaviour
{
    public Material terrainMaterial;
    public Vector2 worldSize = new Vector2(320, 320);
    
    [Header("Textures")]
    public Texture2D grassTexture;
    public Texture2D sandTexture;
    
    [Header("Detail (optional)")]
    public Texture2D detailTexture;
    public bool generateDetailIfMissing = true;
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material instanceMaterial;
    
    public void Initialize(Texture2D terrainMap)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        GenerateMesh();
        
        // Crear instancia del material (no modificar el asset original)
        instanceMaterial = new Material(terrainMaterial);
        meshRenderer.material = instanceMaterial;
        
        // Asignar terrain data map
        instanceMaterial.SetTexture("_TerrainMap", terrainMap);
        
        // Asignar texturas de terreno
        if (grassTexture != null)
            instanceMaterial.SetTexture("_MainTex", grassTexture);
        
        if (sandTexture != null)
            instanceMaterial.SetTexture("_SandTex", sandTexture);
        
        // Detail texture
        if (detailTexture != null)
        {
            instanceMaterial.SetTexture("_DetailTex", detailTexture);
        }
        else if (generateDetailIfMissing)
        {
            instanceMaterial.SetTexture("_DetailTex", GenerateDetailTexture());
        }
        
        // Posicionar correctamente
        transform.position = new Vector3(-worldSize.x / 2f, -worldSize.y / 2f, 0);
    }
    
    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(worldSize.x, 0, 0),
            new Vector3(0, worldSize.y, 0),
            new Vector3(worldSize.x, worldSize.y, 0)
        };
        
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        int[] triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }
    
    Texture2D GenerateDetailTexture()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float n2 = Mathf.PerlinNoise(x * 0.1f + 100f, y * 0.1f + 100f) * 0.5f;
                float value = (n1 + n2) / 1.5f;
                value = 0.7f + value * 0.3f;
                
                tex.SetPixel(x, y, new Color(value, value, value));
            }
        }
        
        tex.Apply();
        return tex;
    }
    
    void OnDestroy()
    {
        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }
}