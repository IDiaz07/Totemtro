using UnityEngine;

public class HeroUnlockManager : MonoBehaviour
{
    public void UnlockHero(HeroData hero)
    {
        PlayerPrefs.SetInt(hero.heroName, 1);
    }

    public bool IsUnlocked(HeroData hero)
    {
        if (hero.unlockedByDefault)
            return true;

        return PlayerPrefs.GetInt(hero.heroName, 0) == 1;
    }
}
