using UnityEngine;
using UnityEngine.UI;

public class VexBarUI : MonoBehaviour
{
    public Image fillImage;
    public Transform cardHolder;
    public Weapon weapon;

    public GameObject glow;

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

    void Update()
    {
        if (weapon == null || weapon.currentWeapon == null)
            return;

        bool isVex =
            weapon.currentWeapon.weaponType == WeaponType.VexProyectile;

        // activar / desactivar UI
        gameObject.SetActive(isVex);

        if (!isVex) return;

        if (vex == null)
            vex = weapon.GetComponent<VexAttack>();

        if (vex == null) return;

        float targetFill = vex.hasCard ? 1f : vex.GetBarPercent();

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFill,
            Time.deltaTime * 8f
        );

        if (glow != null)
            glow.SetActive(vex.hasCard);

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