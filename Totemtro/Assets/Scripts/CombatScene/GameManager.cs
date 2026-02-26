using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RunManager runManager;
    [SerializeField] private RunSummaryUI runSummaryUI;

    [Header("Scene Names")]
    [SerializeField] private string hubSceneName = "HubScene";
    [SerializeField] private string combatSceneName = "CombatScene";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartRun()
    {
        SceneManager.LoadScene(combatSceneName);
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("GAME MANAGER HANDLE DEATH CALLED");

        if (runManager != null)
            runManager.EndRunByDeath();
        else
            Debug.LogError("RunManager not found in scene!");

        ShowSummary(false); // murió
    }

    public void ExtractRun()
    {
        if (runManager != null)
            runManager.EndRunByExtraction();

        ShowSummary(true); // extrajo
    }

    void ShowSummary(bool extracted)
    {
        Debug.Log("SHOW SUMMARY CALLED");

        Time.timeScale = 0f;

        if (runSummaryUI != null)
            runSummaryUI.Show(extracted);
        else
            Debug.LogError("RunSummaryUI not found in scene!");
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(hubSceneName);
    }
}