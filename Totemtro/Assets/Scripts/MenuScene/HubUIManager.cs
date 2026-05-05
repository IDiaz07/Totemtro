using UnityEngine;
using System.Collections;

public class HubUIManager : MonoBehaviour
{
    public static HubUIManager Instance;

    public GameObject inventoryPanel;
    public GameObject confirmationPanel;
    public GameObject fadePanel;

    [Header("Gold Animation Source")]
    [SerializeField] private RectTransform playButtonRect;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Si hay reward pendiente del Summary, animar monedas
        if (SummarySceneUI.PendingGoldReward > 0)
        {
            StartCoroutine(PlayGoldEntryAnimation());
        }
    }

    IEnumerator PlayGoldEntryAnimation()
    {
        // Esperar un frame para que CurrencyAnimationSystem se inicialice
        yield return null;
        yield return new WaitForSeconds(0.5f);

        int reward = SummarySceneUI.PendingGoldReward;
        SummarySceneUI.PendingGoldReward = 0;

        if (CurrencyAnimationSystem.Instance != null && playButtonRect != null)
        {
            // Posición del botón PLAY como origen de las monedas
            Vector3 worldPos = playButtonRect.position;

            CurrencyAnimationSystem.Instance
                .PlayGoldAnimation(reward, worldPos);
        }
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void OpenConfirmation()
    {
        confirmationPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        confirmationPanel.SetActive(false);
    }

    public void ShowFade()
    {
        if (fadePanel != null)
            fadePanel.SetActive(true);
    }
}