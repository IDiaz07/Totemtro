using UnityEngine;

public class BodySway : MonoBehaviour
{
    public float maxRotation = 5f;      // grados máximos de giro
    public float swaySpeed = 6f;         // velocidad del balanceo
    public float smooth = 10f;           // suavizado al volver a 0

    Vector3 originalRotation;

    void Start()
    {
        originalRotation = transform.localEulerAngles;
    }

    void Update()
    {
        // Leemos input de movimiento
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isMoving = h != 0 || v != 0;

        float targetZ = 0f;

        if (isMoving)
        {
            // Balanceo izquierda-derecha
            targetZ = Mathf.Sin(Time.time * swaySpeed) * maxRotation;
        }

        // Suavizamos la rotación
        Vector3 currentRot = transform.localEulerAngles;
        float z = Mathf.LerpAngle(currentRot.z, targetZ, Time.deltaTime * smooth);

        transform.localEulerAngles = new Vector3(
            originalRotation.x,
            originalRotation.y,
            z
        );
    }
}
