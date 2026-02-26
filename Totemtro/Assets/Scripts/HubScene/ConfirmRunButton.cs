using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmRunButton : MonoBehaviour
{
    public void OnConfirm()
    {
        bool ready =
            RunPreparationSystem.Instance.PrepareRun(
                RunLoadoutSystem.Instance.loadoutSlots);

        if (!ready)
        {
            Debug.Log("Not enough items.");
            return;
        }

        HubUIManager.Instance.ShowFade();
        Invoke(nameof(LoadCombat), 0.6f);
    }

    void LoadCombat()
    {
        SceneManager.LoadScene("CombatScene");
    }
}