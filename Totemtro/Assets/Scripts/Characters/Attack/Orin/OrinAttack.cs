using UnityEngine;

public class OrinAttack : MonoBehaviour
{
    HeroController hero;
    int currentIndex = 0; // 0 = Rifle, 1 = SMG

    void Awake()
    {
        hero = GetComponentInParent<HeroController>();
    }

    void Update()
    {
        if (hero == null) return;

        // 🔁 Cambiar arma con click derecho (una sola vez)
        if (Input.GetMouseButtonDown(1))
        {
            currentIndex = currentIndex == 0 ? 1 : 0;
            hero.EquipWeapon(currentIndex);
        }
    }
}
