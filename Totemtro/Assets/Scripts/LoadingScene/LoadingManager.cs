using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public enum LoadingType
{
    Fake,
    SceneAsync
}

public class LoadingManager : MonoBehaviour
{
    public static string TargetScene;
    public static LoadingType loadingType;

    [Header("UI")]
    [SerializeField] private Image loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Tips")]
    [TextArea]
    [SerializeField] private string[] tips;

    void Start()
    {
        ShowRandomTip();

        if (loadingType == LoadingType.Fake)
            StartCoroutine(FakeLoading());

        if (loadingType == LoadingType.SceneAsync)
            StartCoroutine(RealLoading());
    }

    void ShowRandomTip()
    {
        if (tips == null || tips.Length == 0) return;

        int index = Random.Range(0, tips.Length);

        if (tipText != null)
            tipText.text = tips[index];
    }

    IEnumerator FakeLoading()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.7f;

            if (loadingBar != null)
                loadingBar.fillAmount = progress;

            if (loadingText != null)
                loadingText.text = "Loading " + Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        SceneManager.LoadScene(TargetScene);
    }

    IEnumerator RealLoading()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(TargetScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (loadingBar != null)
                loadingBar.fillAmount = progress;

            if (loadingText != null)
                loadingText.text = "Loading " + Mathf.RoundToInt(progress * 100f) + "%";

            if (op.progress >= 0.9f)
                op.allowSceneActivation = true;

            yield return null;
        }
    }
}
