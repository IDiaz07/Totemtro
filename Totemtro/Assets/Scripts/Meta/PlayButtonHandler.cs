using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonHandler : MonoBehaviour
{
    public string combatSceneName = "CombatScene";

    public void StartGame()
    {
        if (HeroSelectionManager.Instance == null)
        {
            Debug.LogError("HeroSelectionManager not found");
            return;
        }

        HeroData hero = HeroSelectionManager.Instance.SelectedHero;

        if (hero == null)
        {
            Debug.LogWarning("No hero selected");
            return;
        }

        GameSessionManager.Instance.SetSelectedHero(hero);

        SceneManager.LoadScene(combatSceneName);
    }
}