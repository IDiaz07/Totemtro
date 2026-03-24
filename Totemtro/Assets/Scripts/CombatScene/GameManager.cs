using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    [SerializeField] private string hubSceneName = "HubScene";
    [SerializeField] private string combatSceneName = "CombatScene";
    [SerializeField] private string summarySceneName = "SummaryScene";
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

    // =========================================
    // DEATH → SummaryScene (DEFEAT) — directo
    // =========================================

    public void HandlePlayerDeath()
    {
        Debug.Log("🔴 Player died — triggering defeat sequence");

        if (RunManager.Instance != null)
            RunManager.Instance.EndRunByDeath();
        else
            Debug.LogError("RunManager.Instance is NULL");

        Time.timeScale = 1f;

        // Ir directo a SummaryScene sin pantalla de carga
        SceneManager.LoadScene(summarySceneName);
    }

    // =========================================
    // EXTRACTION → SummaryScene (VICTORY) — directo
    // =========================================

    public void ExtractRun()
    {
        Debug.Log("🟢 Extraction — triggering victory sequence");

        if (RunManager.Instance != null)
            RunManager.Instance.EndRunByExtraction();
        else
            Debug.LogError("RunManager.Instance is NULL");

        Time.timeScale = 1f;

        // Usar LoadSceneAsync para que no corte la corrutina del caller
        SceneManager.LoadScene(summarySceneName);
    }

    // =========================================
    // NAVIGATION
    // =========================================

    public void RestartRun()
    {
        Time.timeScale = 1f;
        LoadSceneWithLoading(combatSceneName);
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;

        if (MetaInventory.Instance != null)
            MetaInventory.Instance.SaveMetaInventory();

        RunSummaryManager.Clear();

        // Limpiar loadout de la run anterior
        if (RunLoadoutSystem.Instance != null)
            RunLoadoutSystem.Instance.ClearLoadout();

        LoadSceneWithLoading(hubSceneName);
    }
}
