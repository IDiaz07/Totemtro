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
    [SerializeField] private string loadingSceneName = "LoadingScene";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void LoadSceneWithLoading(string sceneName)
    {
        Debug.Log("Loading via LoadingScene → " + sceneName);

        LoadingManager.TargetScene = sceneName;
        SceneManager.LoadScene(loadingSceneName);
    }


    public void StartRun()
    {
        Debug.Log("Hero before run: " + GameSessionManager.Instance.selectedHero);

        LoadingManager.loadingType = LoadingType.Fake;
        LoadingManager.TargetScene = combatSceneName;

        SceneManager.LoadScene("LoadingScene");
    }


    public void StartOnlineMatch()
    {
        LoadingManager.loadingType = LoadingType.SceneAsync;
        LoadingManager.TargetScene = "OnlineCombatScene";

        SceneManager.LoadScene("LoadingScene");
    }

    public void HandlePlayerDeath()
    {
        if (runManager != null)
            runManager.EndRunByDeath();

        ShowSummary(false);
    }

    public void ExtractRun()
    {
        if (runManager != null)
            runManager.EndRunByExtraction();

        ShowSummary(true);
    }

    void ShowSummary(bool extracted)
    {
        Time.timeScale = 0f;

        if (runSummaryUI != null)
            runSummaryUI.Show(extracted);
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;

        if (MetaInventory.Instance != null)
            MetaInventory.Instance.SaveMetaInventory();

        LoadSceneWithLoading(hubSceneName);
    }
}
