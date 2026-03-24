using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmRunButton : MonoBehaviour
{
    public void OnConfirm()
    {
        if (RunPreparationSystem.Instance == null)
        {
            Debug.LogError("RunPreparationSystem is NULL");
            return;
        }

        if (RunLoadoutSystem.Instance == null)
        {
            Debug.LogError("RunLoadoutSystem is NULL");
            return;
        }

        bool ready =
            RunPreparationSystem.Instance.PrepareRun(
                RunLoadoutSystem.Instance.loadoutSlots);

        if (!ready)
        {
            Debug.Log("Not enough items.");
            return;
        }

        // Guardar el héroe seleccionado para que sobreviva entre escenas
        if (HeroSelectionManager.Instance != null &&
            GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetSelectedHero(
                HeroSelectionManager.Instance.SelectedHero); 
        }

        if (HubUIManager.Instance != null)
            HubUIManager.Instance.ShowFade();

        Invoke(nameof(LoadCombat), 0.6f);
    }

    void LoadCombat()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRun();
        }
        else
        {
            Debug.LogError("GameManager.Instance is NULL");
            SceneManager.LoadScene("CombatScene");
        }
    }
}