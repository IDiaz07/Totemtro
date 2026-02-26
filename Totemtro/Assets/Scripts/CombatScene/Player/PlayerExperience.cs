using UnityEngine;
using System;

public class PlayerExperience : MonoBehaviour
{
    public int currentLevel = 1;

    public float currentXP = 0f;
    public float xpToNextLevel = 100f;

    public event Action OnLevelUp;
    [SerializeField] TotemSelectionUI totemUI;

    void Start()
    {
        OnLevelUp += OpenTotemSelection;
    }

    public void AddXP(float amount)
    {
        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        FindFirstObjectByType<LevelUpEffect>()?.Play();
        currentLevel++;
        currentXP -= xpToNextLevel;

        xpToNextLevel *= 1.25f;

        OnLevelUp?.Invoke();
    }

    void OpenTotemSelection()
    {
        if (totemUI != null)
            totemUI.Open();
    }


    public float GetXPPercent()
    {
        return currentXP / xpToNextLevel;
    }
}
