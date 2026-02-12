using UnityEngine;

public class HeroController : MonoBehaviour
{
    public HeroData currentHero;
    public SpriteRenderer bodyRenderer;
    public float currentHealth;

    PlayerMovement movement;
    Weapon weapon;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponentInChildren<Weapon>();

        ApplyHero();
    }

    public void ApplyHero()
    {
        currentHealth = currentHero.maxHealth;

        movement.speed = currentHero.moveSpeed;

        weapon.SetWeapon(currentHero.startingWeapon);

        bodyRenderer.sprite = currentHero.bodySprite;
    }

}
