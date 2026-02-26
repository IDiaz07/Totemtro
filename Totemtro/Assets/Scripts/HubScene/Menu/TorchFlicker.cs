using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    public Light2D light2D;
    public float baseIntensity = 1.2f;
    public float flickerAmount = 0.15f;
    public float flickerSpeed = 5f;

    float noiseOffset;

    void Start()
    {
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        light2D.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
    }
}