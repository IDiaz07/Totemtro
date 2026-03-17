using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    public HeroData selectedHero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameSessionManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedHero(HeroData hero)
    {
        selectedHero = hero;
        Debug.Log("Hero selected: " + hero.heroName);
    }
}
