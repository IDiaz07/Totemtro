using UnityEngine;

public class OrinAttack : MonoBehaviour
{
    HeroController hero;
    Weapon weapon;

    int currentIndex = 0;

    void Awake()
    {
        hero = GetComponentInParent<HeroController>();
        weapon = GetComponent<Weapon>();
    }

    void Update()
    {
        // Solo funciona si el héroe actual es Orin
        if (hero == null) return;
        if (weapon == null) return;
        if (hero.currentHero == null) return;

        if (hero.currentHero.heroType != HeroType.Orin)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            SwitchWeapon();
        }
    }

    void SwitchWeapon()
    {
        if (hero.currentHero.weapons.Count < 2)
            return;

        currentIndex = (currentIndex == 0) ? 1 : 0;

        weapon.SetWeapon(hero.currentHero.weapons[currentIndex]);

        Debug.Log("Orin switched weapon → " + currentIndex);
    }
}