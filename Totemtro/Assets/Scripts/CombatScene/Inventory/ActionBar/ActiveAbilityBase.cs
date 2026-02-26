using UnityEngine;

public abstract class ActiveAbilityBase : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public float cooldown = 5f;

    protected float lastUseTime = -999f;

    public bool CanUse()
    {
        if (lastUseTime > Time.time)
            lastUseTime = -999f;

        return Time.time >= lastUseTime + cooldown;
    }

    public bool TryActivate(GameObject user)
    {
        if (!CanUse())
            return false;

        bool success = Activate(user);

        if (success)
            lastUseTime = Time.time;

        return success;
    }

    protected abstract bool Activate(GameObject user);
}
