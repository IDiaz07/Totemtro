using UnityEngine;
using System.Collections;

public class ExtractionZone : MonoBehaviour
{
    bool playerInside = false;
    bool isChanneling = false;

    float channelTimer = 0f;

    public GameObject beamEffect;
    public GameObject circleEffect;

    public void Initialize(float lifetime)
    {
        StartCoroutine(SpawnFX());
    }

    void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKey(KeyCode.R))
        {
            if (!isChanneling)
                StartCoroutine(ChannelRoutine());
        }
    }

    IEnumerator ChannelRoutine()
    {
        isChanneling = true;
        channelTimer = 0f;

        ExtractionUI.Instance.Show(ExtractionManager.Instance.channelTime);

        while (channelTimer < ExtractionManager.Instance.channelTime)
        {
            if (!playerInside)
                break;

            channelTimer += Time.deltaTime;

            ExtractionUI.Instance.UpdateBar(channelTimer);

            // 🔊 Ruido cada segundo
            if (Mathf.FloorToInt(channelTimer) != Mathf.FloorToInt(channelTimer - Time.deltaTime))
            {
                NoiseSystem.Instance.EmitNoise(transform.position, 25f);
            }

            yield return null;
        }

        ExtractionUI.Instance.Hide();

        if (channelTimer >= ExtractionManager.Instance.channelTime)
        {
            ExtractionManager.Instance.CompleteExtraction();
        }

        isChanneling = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInside = false;
    }

    IEnumerator SpawnFX()
    {
        // Luz vertical
        if (beamEffect != null)
            beamEffect.SetActive(true);

        // Círculo suelo expandiéndose
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

    public void CloseZone()
    {
        StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        // Sonido de apagado
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