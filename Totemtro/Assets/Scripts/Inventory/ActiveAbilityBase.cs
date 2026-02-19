using UnityEngine;

public abstract class ActiveAbilityBase : ScriptableObject
{
    public string abilityName;
    public Sprite icon;

    public float cooldown = 5f;

    protected float lastUseTime;

    protected GameObject owner;

    public virtual void Initialize(GameObject player)
    {
        owner = player;
    }

    public bool CanUse()
    {
        return Time.time >= lastUseTime + cooldown;
    }

    public void TryUse()
    {
        if (!CanUse())
            return;

        Activate();
        lastUseTime = Time.time;
    }

    protected abstract void Activate();
}
