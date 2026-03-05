using UnityEngine;
using UnityEngine.UI;

public class VexBarUI : MonoBehaviour
{
    public Image fillImage;
    public Transform cardHolder;

    public GameObject defaultPrefab;
    public GameObject skullPrefab;
    public GameObject starPrefab;
    public GameObject firePrefab;
    public GameObject fangPrefab;
    public GameObject spadePrefab;

    VexAttack vex;
    GameObject currentInstance;
    VexCardType lastCard;
    bool lastHasCard;

    void Start()
    {
        vex = FindObjectOfType<VexAttack>();

        if (vex == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Spawn(defaultPrefab);
    }

    void Update()
    {
        if (vex == null) return;

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            vex.GetBarPercent(),
            Time.deltaTime * 8f
        );

        UpdateCardVisual();
    }

    void UpdateCardVisual()
    {
        if (!vex.hasCard)
        {
            if (lastHasCard)
                Spawn(defaultPrefab);

            lastHasCard = false;
            return;
        }

        if (!lastHasCard || lastCard != vex.currentCard)
        {
            lastHasCard = true;
            lastCard = vex.currentCard;

            switch (vex.currentCard)
            {
                case VexCardType.Skull: Spawn(skullPrefab); break;
                case VexCardType.Star: Spawn(starPrefab); break;
                case VexCardType.Fire: Spawn(firePrefab); break;
                case VexCardType.Fang: Spawn(fangPrefab); break;
                case VexCardType.Spade: Spawn(spadePrefab); break;
            }
        }
    }

    void Spawn(GameObject prefab)
    {
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = Instantiate(prefab, cardHolder);
        currentInstance.transform.localPosition = Vector3.zero;
        currentInstance.transform.localScale = Vector3.one;
    }
}