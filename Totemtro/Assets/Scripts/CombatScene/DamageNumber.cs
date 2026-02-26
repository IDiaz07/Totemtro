using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifetime = 0.4f;
    public float gravity = 3f;
    public float spreadForce = 2f;

    TextMeshPro tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void SetDamage(float amount, bool isCritical)
    {
        tmp.text = Mathf.RoundToInt(amount).ToString();

        tmp.color = isCritical ? Color.red : Color.white;

        StartCoroutine(LifeRoutine());
    }

    IEnumerator LifeRoutine()
    {
        float timer = 0f;

        while (timer < lifetime)
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        ExplodeDigits();
    }

    void ExplodeDigits()
    {
        string text = tmp.text;

        float spacing = 0.1f;

        for (int i = 0; i < text.Length; i++)
        {
            GameObject digitObj = new GameObject("Digit");

            // MISMA POSICIÓN
            digitObj.transform.position = transform.position;
            digitObj.transform.rotation = Quaternion.identity;
            digitObj.transform.localScale = transform.localScale;

            TextMeshPro digit = digitObj.AddComponent<TextMeshPro>();

            // COPIAR PROPIEDADES IMPORTANTES
            digit.text = text[i].ToString();
            digit.font = tmp.font;
            digit.fontSize = tmp.fontSize;
            digit.color = tmp.color;
            digit.alignment = TextAlignmentOptions.Center;
            digit.sortingLayerID = tmp.sortingLayerID;
            digit.sortingOrder = tmp.sortingOrder;

            // Ajustar posición lateral
            float centerOffset = (i - (text.Length - 1) / 2f);
            digitObj.transform.position += new Vector3(centerOffset * spacing, 0f, 0f);

            Rigidbody2D rb = digitObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = gravity;
            rb.mass = 0.1f;
            rb.angularVelocity = Random.Range(-200f, 200f);

            Vector2 force = new Vector2(centerOffset * spreadForce, 0.2f);
            rb.AddForce(force, ForceMode2D.Impulse);

            Destroy(digitObj, 0.5f);
        }

        Destroy(gameObject);
    }
}
