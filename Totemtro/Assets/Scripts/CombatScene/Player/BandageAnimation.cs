using UnityEngine;

public class BandageAnimation : MonoBehaviour
{
    public Transform body;
    public Transform weapon;

    public float tiltAngle = -10f;
    public float weaponLowerOffset = -0.2f;
    public float wrapSpeed = 6f;

    bool isBandaging = false;

    Quaternion originalBodyRotation;
    Vector3 originalWeaponPos;

    float wrapTimer;

    void Start()
    {
        if (body != null)
            originalBodyRotation = body.localRotation;

        if (weapon != null)
            originalWeaponPos = weapon.localPosition;
    }

    void Update()
    {
        if (!isBandaging)
            return;

        wrapTimer += Time.deltaTime * wrapSpeed;

        float wrapMotion =
            Mathf.Sin(wrapTimer) * 0.05f;

        if (body != null)
        {
            body.localRotation =
                Quaternion.Euler(0f, 0f, tiltAngle);
        }

        if (weapon != null)
        {
            Vector3 targetPos =
                originalWeaponPos +
                new Vector3(0f,
                            weaponLowerOffset + wrapMotion,
                            0f);

            weapon.localPosition =
                Vector3.Lerp(weapon.localPosition,
                             targetPos,
                             Time.deltaTime * 8f);
        }
    }

    public void StartBandaging()
    {
        isBandaging = true;
        wrapTimer = 0f;
    }

    public void StopBandaging()
    {
        isBandaging = false;

        if (body != null)
            body.localRotation = originalBodyRotation;

        if (weapon != null)
            weapon.localPosition = originalWeaponPos;
    }
}