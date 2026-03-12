using UnityEngine;

public class SeleneChargeOrb : MonoBehaviour
{
    public ParticleSystem spiralParticles;

    public float maxScale = 1.6f;
    public float chargeSpeed = 2f;

    float charge = 0f;
    bool charging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCharge();
        }

        if (Input.GetMouseButton(0) && charging)
        {
            Charge();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }

    void StartCharge()
    {
        charging = true;
        charge = 0f;

        if (spiralParticles != null)
            spiralParticles.Play();
    }

    void Charge()
    {
        charge += Time.deltaTime * chargeSpeed;

        float scale = Mathf.Lerp(1f, maxScale, charge);

        transform.localScale = Vector3.one * scale;
    }

    void Release()
    {
        charging = false;

        if (spiralParticles != null)
            spiralParticles.Stop();

        // aquí dispararías el proyectil
    }
}