using UnityEngine;

public class FadeWhenBehind : MonoBehaviour
{
    public Transform sortPoint;
    public float fadeDistance = 3f;

    SpriteRenderer sr;
    Transform playerFeet;

    float targetAlpha = 1f;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        GameObject feet = GameObject.FindGameObjectWithTag("PlayerFeet");

        if (feet != null)
            playerFeet = feet.transform;
    }

    void Update()
    {
        if (playerFeet == null || sortPoint == null || sr == null)
            return;

        float distance = Vector2.Distance(playerFeet.position, sortPoint.position);

        bool playerBehind = playerFeet.position.y > sortPoint.position.y;

        if (playerBehind && distance < fadeDistance)
            targetAlpha = 0.35f;
        else
            targetAlpha = 1f;

        Color c = sr.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 6f);
        sr.color = c;
    }
}