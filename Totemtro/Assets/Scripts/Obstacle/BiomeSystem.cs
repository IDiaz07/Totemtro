using UnityEngine;

public enum BiomeType
{
    Empty,
    Forest,
    Ruins,
    Clearing
}

public class BiomeSystem
{
    float macroScale;
    float microScale;

    public BiomeSystem(float macro, float micro)
    {
        macroScale = macro;
        microScale = micro;
    }

    public BiomeType GetBiome(Vector2 pos)
    {
        float macroNoise = Mathf.PerlinNoise(
            pos.x * macroScale,
            pos.y * macroScale
        );

        if (macroNoise < 0.3f)
            return BiomeType.Clearing;

        if (macroNoise < 0.55f)
            return BiomeType.Forest;

        if (macroNoise < 0.8f)
            return BiomeType.Ruins;

        return BiomeType.Empty;
    }

    public float GetClutter(Vector2 pos)
    {
        return Mathf.PerlinNoise(
            pos.x * microScale + 200,
            pos.y * microScale + 200
        );
    }
}