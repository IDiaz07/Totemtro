using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Consumables/Thralls Bandage")]
public class ThrallsBandageAbility : ActiveAbilityBase
{
    public int healAmount = 10;
    public float castTime = 2.5f;

    protected override bool Activate(GameObject user)
    {
        PlayerHealth health = user.GetComponent<PlayerHealth>();
        StatusEffectController effects =
            user.GetComponent<StatusEffectController>();

        if (health == null || effects == null)
            return false;

        // 🔥 Si vida completa → no activar
        if (health.GetCurrentHealthPercent() >= 1f)
        {
            return false;   // ❌ No activar
        }

        effects.ApplyCastHeal(healAmount, castTime);

        return true; // ✔ Inicia cast
    }
}