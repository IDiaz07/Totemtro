using UnityEngine;

public class SpiralParticleSystem : MonoBehaviour
{
    [Header("Spiral Settings")]
    public ParticleSystem targetParticleSystem;
    public float spiralSpeed = 3f;
    public float startRadius = 3f;
    public float spiralHeight = 6f;

    [Header("Visual Polish")]
    [Range(0f, 1f)] public float startAlpha = 0.8f;
    [Range(0f, 1f)] public float endAlpha = 0f;
    public float startSize = 1f;
    public float endSize = 0.1f;
    public Color baseColor = new Color(0.6f, 0.85f, 1f, 1f);
    public Color tipColor = new Color(1f, 1f, 1f, 0f);

    [Header("Motion")]
    public float jitterStrength = 0.15f;
    public float accelerationCurve = 2f;
    public float rotationOffset = 0f;

    ParticleSystem.Particle[] particles;
    float globalAngle;

    void LateUpdate()
    {
        if (targetParticleSystem == null) return;

        int maxParticles = targetParticleSystem.main.maxParticles;

        if (particles == null || particles.Length < maxParticles)
            particles = new ParticleSystem.Particle[maxParticles];

        int count = targetParticleSystem.GetParticles(particles);

        // Rotación global lenta para que el vórtice se sienta vivo
        globalAngle += Time.deltaTime * 0.5f;

        for (int i = 0; i < count; i++)
        {
            // 0 = recién nacida, 1 = a punto de morir
            float life = 1f - (particles[i].remainingLifetime / particles[i].startLifetime);

            // Curva de aceleración: empieza lento, termina rápido
            float curved = Mathf.Pow(life, 1f / accelerationCurve);

            // Radio se reduce con ease-in
            float radius = Mathf.Lerp(startRadius, 0f, curved);

            // Ángulo por partícula + offset global + seed único
            float seed = particles[i].randomSeed * 0.001f;
            float angle = (curved * spiralSpeed * Mathf.PI * 2f)
                        + rotationOffset
                        + globalAngle
                        + seed;

            // Altura sube con curva suave
            float y = Mathf.Lerp(0f, spiralHeight, curved);

            // Jitter orgánico basado en seed
            float jx = Mathf.PerlinNoise(seed * 100f, Time.time * 2f) - 0.5f;
            float jz = Mathf.PerlinNoise(Time.time * 2f, seed * 100f) - 0.5f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius + jx * jitterStrength * radius,
                y,
                Mathf.Sin(angle) * radius + jz * jitterStrength * radius
            );

            particles[i].position = transform.position + offset;

            // Tamaño se reduce progresivamente
            float size = Mathf.Lerp(startSize, endSize, curved);
            particles[i].startSize = size;

            // Color + Alpha fade profesional
            Color col = Color.Lerp(baseColor, tipColor, curved);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, curved);

            // Pico de brillo a mitad del recorrido (glow sutil)
            float glowPeak = Mathf.Sin(life * Mathf.PI) * 0.3f;
            alpha = Mathf.Clamp01(alpha + glowPeak);

            col.a = alpha;
            particles[i].startColor = col;
        }

        targetParticleSystem.SetParticles(particles, count);
    }
}