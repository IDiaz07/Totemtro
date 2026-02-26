using UnityEngine;
using System.Collections;

public class CraftingBookAnimator : MonoBehaviour
{
    public GameObject closedBook;
    public GameObject openBook;

    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip pageSound;

    public float openDuration = 0.4f;

    bool isOpen = false;

    public void PlayPageTurn()
    {
        audioSource.PlayOneShot(pageSound);
    }

    public void PlayOpenBook()
    {
        audioSource.PlayOneShot(openSound);
    }

    public void PlayCloseBook()
    {
        audioSource.PlayOneShot(closeSound);
    }

    void Start()
    {
        closedBook.SetActive(true);
        openBook.SetActive(false);
    }

    public void ToggleBook()
    {
        if (!isOpen)
            StartCoroutine(OpenBook());
        else
            StartCoroutine(CloseBook());
    }

    IEnumerator OpenBook()
    {
        isOpen = true;

        closedBook.SetActive(false);
        openBook.SetActive(true);

        RectTransform rt = openBook.GetComponent<RectTransform>();

        rt.localScale = Vector3.zero;

        float t = 0f;

        while (t < openDuration)
        {
            t += Time.unscaledDeltaTime;
            float ease = 1f - Mathf.Pow(1f - t / openDuration, 3f);
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    IEnumerator CloseBook()
    {
        isOpen = false;

        RectTransform rt = openBook.GetComponent<RectTransform>();

        float t = 0f;

        while (t < openDuration)
        {
            t += Time.unscaledDeltaTime;
            float ease = t / openDuration;
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, ease);
            yield return null;
        }

        openBook.SetActive(false);
        closedBook.SetActive(true);
    }
}
