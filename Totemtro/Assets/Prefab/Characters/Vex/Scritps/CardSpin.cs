using UnityEngine;

public class CardSpin : MonoBehaviour
{
    public float rotationSpeed = 360f; // grados por segundo

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}