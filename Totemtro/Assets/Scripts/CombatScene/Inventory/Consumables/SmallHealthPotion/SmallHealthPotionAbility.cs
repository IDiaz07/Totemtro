using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Consumables/Small Health Potion")]
public class SmallHealthPotionAbility : ActiveAbilityBase
{
    public float castTime = 2.5f;

    protected override bool Activate(GameObject user)
    {
        PlayerHealth health = user.GetComponent<PlayerHealth>();
        StatusEffectController effects =
            user.GetComponent<StatusEffectController>();

        if (health == null || effects == null)
            return false;

        if (health.GetCurrentHealthPercent() >= 1f)
            return false;

        effects.ApplyPotionHeal(castTime);

        return true;
    }
}