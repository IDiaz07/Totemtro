using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonHandler : MonoBehaviour
{
    public string combatSceneName = "CombatScene";

    public void StartGame()
    {
        HeroData hero =
            HeroSelectionManager.Instance.SelectedHero;

        GameSessionManager.Instance.SetSelectedHero(hero);

        SceneManager.LoadScene(combatSceneName);
    }
}