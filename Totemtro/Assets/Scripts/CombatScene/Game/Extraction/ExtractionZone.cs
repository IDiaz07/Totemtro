using UnityEngine;
using System.Collections;

public class ExtractionZone : MonoBehaviour
{
    [Header("FX")]
    public GameObject beamEffect;
    public GameObject circleEffect;

    [Header("Beam Drop Settings")]
    public float beamStartHeight = 10f;
    public float beamDropDuration = 0.6f;
    public float beamDropShake = 0.3f;
    public float beamDropShakeIntensity = 0.15f;

    bool playerInside = false;
    bool beamActivated = false;
    bool beamReady = false;
    float channelDuration;

    Coroutine channelCoroutine;
    ParticleSystem[] beamParticles;

    public void Initialize(float channelTime)
    {
        channelDuration = channelTime;

        if (beamEffect != null)
        {
            beamParticles = beamEffect.GetComponentsInChildren<ParticleSystem>(true);
            beamEffect.SetActive(false);
        }

        // Diagnóstico mejorado
        Collider2D col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (col == null)
            Debug.LogError("ExtractionZone: NO tiene Collider2D!");
        else if (!col.isTrigger)
            Debug.LogError("ExtractionZone: Collider2D NO es trigger!");
        else
            Debug.Log($"ExtractionZone: Collider2D OK (type={col.GetType().Name}, trigger={col.isTrigger})");

        if (rb == null)
            Debug.LogError("ExtractionZone: NO tiene Rigidbody2D! Añade uno con Body Type = Kinematic");
        else
            Debug.Log($"ExtractionZone: Rigidbody2D OK (type={rb.bodyType})");

        // Verificar layer del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"ExtractionZone: Player encontrado - Layer: {LayerMask.LayerToName(player.layer)}, Tag: {player.tag}");
        }
        else
        {
            Debug.LogError("ExtractionZone: NO se encontró GameObject con tag 'Player'!");
        }

        StartCoroutine(SpawnFX());
    }

    // =========================================
    // TRIGGER - MEJORADO
    // =========================================

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"<color=yellow>ExtractionZone: OnTriggerEnter2D con {col.name} (tag={col.tag}, layer={LayerMask.LayerToName(col.gameObject.layer)})</color>");

        if (!col.CompareTag("Player"))
        {
            Debug.Log($"ExtractionZone: Tag '{col.tag}' no coincide con 'Player' - ignorando");
            return;
        }

        Debug.Log("<color=green>ExtractionZone: PLAYER ENTRÓ en la zona ✓</color>");

        playerInside = true;

        if (!beamActivated)
        {
            Debug.Log("ExtractionZone: Activando beam drop sequence...");
            beamActivated = true;
            StartCoroutine(BeamDropSequence());
        }
        else if (beamReady)
        {
            Debug.Log($"ExtractionZone: Beam ya está listo. channelCoroutine es null? {channelCoroutine == null}");
        }
        else
        {
            Debug.Log("ExtractionZone: Beam activado pero aún no está listo");
        }

        if (beamReady && channelCoroutine == null)
        {
            Debug.Log("ExtractionZone: Iniciando channel routine inmediatamente");
            channelCoroutine = StartCoroutine(ChannelRoutine());
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        // Fallback: intentar iniciar el channel si el jugador está dentro y todo está listo
        if (col.CompareTag("Player") && beamReady && channelCoroutine == null && playerInside)
        {
            Debug.Log("<color=cyan>ExtractionZone: OnTriggerStay2D detectó player listo - iniciando channel</color>");
            channelCoroutine = StartCoroutine(ChannelRoutine());
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        Debug.Log("<color=red>ExtractionZone: PLAYER SALIÓ de la zona</color>");

        playerInside = false;
        CancelChannel();
    }

    // =========================================
    // CHANNEL
    // =========================================

    IEnumerator ChannelRoutine()
    {
        while (!beamReady)
        {
            Debug.Log("ExtractionZone: Esperando que beam esté listo...");
            yield return null;
        }

        Debug.Log("<color=lime>ExtractionZone: Channeling INICIADO ✓</color>");

        if (ExtractionUI.Instance != null)
            ExtractionUI.Instance.ShowChannel(channelDuration);
        else
            Debug.LogError("ExtractionZone: ExtractionUI.Instance es NULL!");

        float timer = 0f;

        while (timer < channelDuration)
        {
            if (!playerInside)
            {
                Debug.Log("ExtractionZone: Channeling CANCELADO (salió de zona)");
                yield break;
            }

            timer += Time.deltaTime;

            if (ExtractionUI.Instance != null)
                ExtractionUI.Instance.UpdateChannel(timer);

            if (Mathf.FloorToInt(timer) !=
                Mathf.FloorToInt(timer - Time.deltaTime))
            {
                if (NoiseSystem.Instance != null)
                    NoiseSystem.Instance.EmitNoise(transform.position, 25f);
            }

            yield return null;
        }

        Debug.Log("<color=lime>ExtractionZone: Channeling COMPLETADO ✓</color>");

        if (ExtractionUI.Instance != null)
            ExtractionUI.Instance.HideChannel();

        channelCoroutine = null;

        ExtractionManager.Instance.CompleteExtraction();
    }

    void CancelChannel()
    {
        if (channelCoroutine != null)
        {
            Debug.Log("ExtractionZone: Cancelando channel...");
            StopCoroutine(channelCoroutine);
            channelCoroutine = null;
        }

        if (ExtractionUI.Instance != null)
            ExtractionUI.Instance.HideChannel();
    }

    // =========================================
    // FX
    // =========================================

    IEnumerator SpawnFX()
    {
        if (circleEffect != null)
        {
            circleEffect.transform.localScale = Vector3.zero;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime;
                circleEffect.transform.localScale =
                    Vector3.one * Mathf.Lerp(0f, 1.2f, t);
                yield return null;
            }
        }
    }

    // =========================================
    // BEAM DROP
    // =========================================

    IEnumerator BeamDropSequence()
    {
        if (beamEffect == null)
        {
            Debug.LogWarning("ExtractionZone: beamEffect es null, marcando beam como listo inmediatamente");
            beamReady = true;
            yield break;
        }

        Debug.Log("ExtractionZone: Iniciando beam drop sequence...");

        Vector3 finalPos = beamEffect.transform.localPosition;

        beamEffect.transform.localPosition =
            finalPos + Vector3.up * beamStartHeight;

        beamEffect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        beamEffect.SetActive(true);
        StopBeamParticles();

        float t = 0f;

        while (t < beamDropDuration)
        {
            t += Time.deltaTime;
            float eased = (t / beamDropDuration) * (t / beamDropDuration);

            beamEffect.transform.localPosition =
                Vector3.Lerp(
                    finalPos + Vector3.up * beamStartHeight,
                    finalPos,
                    eased
                );

            float scale = Mathf.Lerp(0.3f, 1f, eased);
            beamEffect.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        beamEffect.transform.localPosition = finalPos;
        beamEffect.transform.localScale = Vector3.one;

        PlayBeamParticles();

        CameraShake.ShakeCamera(beamDropShakeIntensity, beamDropShake);

        yield return StartCoroutine(ImpactPunch());

        beamReady = true;
        Debug.Log("<color=lime>ExtractionZone: Beam listo! ✓</color>");

        if (playerInside && channelCoroutine == null)
        {
            Debug.Log("ExtractionZone: Player ya está dentro, iniciando channel...");
            channelCoroutine = StartCoroutine(ChannelRoutine());
        }
    }

    // =========================================
    // PARTICLES
    // =========================================

    void StopBeamParticles()
    {
        if (beamParticles == null) return;

        foreach (var ps in beamParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void PlayBeamParticles()
    {
        if (beamParticles == null) return;

        foreach (var ps in beamParticles)
        {
            ps.Clear();
            ps.Play();
        }
    }

    // =========================================
    // IMPACT
    // =========================================

    IEnumerator ImpactPunch()
    {
        if (beamEffect == null)
            yield break;

        float punchScale = 1.4f;
        beamEffect.transform.localScale = Vector3.one * punchScale;

        float t = 0f;
        float punchDuration = 0.2f;

        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(punchScale, 1f, t / punchDuration);
            beamEffect.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        beamEffect.transform.localScale = Vector3.one;

        if (circleEffect != null)
        {
            Vector3 orig = circleEffect.transform.localScale;
            circleEffect.transform.localScale = orig * 1.3f;

            t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                circleEffect.transform.localScale =
                    Vector3.Lerp(orig * 1.3f, orig, t / 0.15f);
                yield return null;
            }

            circleEffect.transform.localScale = orig;
        }
    }

    // =========================================
    // CLOSE
    // =========================================

    public void CloseZone()
    {
        CancelChannel();
        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        if (NoiseSystem.Instance != null)
            NoiseSystem.Instance.EmitNoise(transform.position, 15f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            transform.localScale =
                Vector3.one * Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}