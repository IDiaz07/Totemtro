using UnityEngine;
using System.Collections.Generic;

public class ActiveItemInventory : MonoBehaviour
{
    public List<ActiveAbilityBase> equippedAbilities = new List<ActiveAbilityBase>();
    public int maxAbilities = 2;

    public bool AddAbility(ActiveAbilityBase ability)
    {
        if (equippedAbilities.Count >= maxAbilities)
            return false;

        equippedAbilities.Add(ability);
        ability.Initialize(gameObject);

        return true;
    }
}
