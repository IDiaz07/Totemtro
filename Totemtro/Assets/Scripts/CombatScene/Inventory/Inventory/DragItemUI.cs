using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DragItemUI : MonoBehaviour
{
    public static DragItemUI Instance;

    public Image icon;

    public bool IsDragging { get; private set; }
    public ItemData draggedItem;
    public int draggedAmount;
    public AudioClip errorSound;
    AudioSource audioSource;


    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        Hide();
    }

    void Update()
    {
        if (IsDragging)
            transform.position = Input.mousePosition;
    }

    public void Show(ItemData item, int amount)
    {
        if (item == null) return;

        draggedItem = item;
        draggedAmount = amount;

        icon.sprite = item.icon;
        icon.color = Color.white;

        IsDragging = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        IsDragging = false;
        draggedItem = null;
        draggedAmount = 0;

        gameObject.SetActive(false);
    }

    public void Shake()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        Vector3 original = transform.position;

        float duration = 0.2f;
        float strength = 15f;

        float timer = 0;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float x =
                Mathf.Sin(timer * 40f) * strength;

            transform.position =
                original + new Vector3(x, 0, 0);

            yield return null;
        }

        transform.position = original;
    }

    public void PlayInvalidFeedback()
    {
        Shake();

        if (errorSound != null)
            audioSource.PlayOneShot(errorSound);
    }
}
