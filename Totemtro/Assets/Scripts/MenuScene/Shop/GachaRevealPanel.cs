using UnityEngine;
using System.Collections;

public class GachaRevealPanel : MonoBehaviour
{
    public static GachaRevealPanel Instance;

    public GameObject root;
    public Transform rewardHolder;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show(ShopItemData item)
    {
        root.SetActive(true);
        StartCoroutine(RevealAnimation(item));
    }

    IEnumerator RevealAnimation(ShopItemData item)
    {
        // Aquí puedes:
        // - Mostrar icon
        // - Sonido
        // - Glow
        // - Scale animation

        yield return new WaitForSeconds(1f);
    }

    public void Close()
    {
        root.SetActive(false);
    }
}