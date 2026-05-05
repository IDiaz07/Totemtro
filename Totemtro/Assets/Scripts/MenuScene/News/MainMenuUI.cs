using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject newsPanel;
    [SerializeField] private GameObject newsDetailPanel;

    [Header("UI Controllers")]
    [SerializeField] private NewsPanelUI newsPanelUI;

    public void OpenNews()
    {
        if (newsPanel == null || newsPanelUI == null)
            return;

        newsPanel.SetActive(true);
        newsPanelUI.Open();
    }

    public void CloseNews()
    {
        if (newsPanel == null)
            return;

        newsPanel.SetActive(false);
    }

    public void CloseNewsDetail()
    {
        if (newsDetailPanel == null)
            return;

        newsDetailPanel.SetActive(false);
    }

    public void OpenSettings(GameObject settingsPanel)
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(false);
    }
}