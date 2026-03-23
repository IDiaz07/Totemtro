using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grenade : MonoBehaviour
{
    [Header("Explosion")]
    public float delay = 2f;
    public float radius = 6f;
    public float damage = 30f;
    public float knockbackForce = 6f;

    [Header("Visual")]
    public GameObject explosionPrefab;
    public ParticleSystem explosionParticles;
    public SpriteRenderer spriteRenderer;

    [Header("Feedback")]
    public AudioClip explosionSound;
    public float screenShakeIntensity = 0.3f;
    public float screenShakeDuration = 0.2f;

    float timer;
    bool hasExploded = false;

    void Start()
    {
        timer = delay;
    }

    void Update()
    {
        if (hasExploded) return;

        timer -= Time.deltaTime;

        // Parpadeo cuando queda poco
        if (timer <= 0.5f && spriteRenderer != null)
        {
            float blink = Mathf.PingPong(Time.time * 20f, 1f);
            spriteRenderer.color = new Color(1f, blink, blink);
        }

        if (timer <= 0f)
        {
            Explode();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.GetComponent<Enemy>() != null)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 pos = transform.position;

        // 🐢 Mini slow motion
        TimeManager.Instance?.DoSlowMotion(0.2f, 0.05f);

        // 🔥 Daño en área con falloff
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, radius);

        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                if (hitEnemies.Contains(enemy))
                    continue;

                hitEnemies.Add(enemy);

                float distance = Vector2.Distance(pos, enemy.transform.position);

                float falloff = 1f - (distance / radius);
                falloff = Mathf.Clamp01(falloff);

                Vector2 dir = (enemy.transform.position - pos).normalized;

                float finalDamage = damage * falloff;

                enemy.TakeDamage(
                    finalDamage,
                    dir * knockbackForce * falloff,
                    false
                );
            }
        }

        // 💥 Spawn explosión visual
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, pos, Quaternion.identity);

        // ✨ Partículas
        if (explosionParticles != null)
        {
            ParticleSystem p =
                Instantiate(explosionParticles, pos, Quaternion.identity);
            p.Play();
        }

        // 📳 Screen Shake escalado por distancia al player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float dist =
                Vector2.Distance(pos, player.transform.position);

            float intensity =
                Mathf.Clamp01(1f - (dist / radius)) * 0.6f;

            CameraShake.ShakeCamera(intensity, 0.25f);
        }

        // 🔊 Sonido con ligera variación de pitch
        if (explosionSound != null)
{
    GameObject tempAudio = new GameObject("TempAudio");
    tempAudio.transform.position = pos;

    AudioSource audio = tempAudio.AddComponent<AudioSource>();
    audio.clip = explosionSound;
    audio.pitch = Random.Range(0.9f, 1.1f);
    audio.Play();

    Destroy(tempAudio, explosionSound.length);
}

        // ⚡ Flash blanco rápido
        ScreenFlash.Instance?.Flash();

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    IEnumerator DoSlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;
    }
}
