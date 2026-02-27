using UnityEngine;
using System.Collections.Generic;

public enum MainMenuPanelType
{
    Menu,
    Inventory,
    Shop,
    Champs,
    News,
    Friends,
    Quests,
    Profile,
    ChampsDetail
}

public class MainMenuPanelController : MonoBehaviour
{
    public static MainMenuPanelController Instance;

    [System.Serializable]
    public class PanelEntry
    {
        public MainMenuPanelType panelType;
        public GameObject panelObject;
    }

    [Header("Panel References")]
    public List<PanelEntry> panels;

    Dictionary<MainMenuPanelType, GameObject> panelDictionary;

    MainMenuPanelType currentPanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeDictionary();
    }

    void Start()
    {
        DisableAllPanels();
        OpenPanel(MainMenuPanelType.Menu);
    }

    void DisableAllPanels()
    {
        foreach (var entry in panels)
        {
            if (entry.panelObject != null)
                entry.panelObject.SetActive(false);
        }
    }

    void InitializeDictionary()
    {
        panelDictionary = new Dictionary<MainMenuPanelType, GameObject>();

        foreach (var entry in panels)
        {
            if (!panelDictionary.ContainsKey(entry.panelType))
                panelDictionary.Add(entry.panelType, entry.panelObject);
        }
    }

    void OpenPanel(MainMenuPanelType type)
    {
        if (!panelDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Panel not registered: " + type);
            return;
        }

        DisableAllPanels();

        if (panelDictionary[type] != null)
            panelDictionary[type].SetActive(true);
        else
            Debug.LogError("Panel reference is NULL for: " + type);

        currentPanel = type;
    }

    // 🔥 MÉTODOS PÚBLICOS PARA BOTONES

    public void OpenMenu() => OpenPanel(MainMenuPanelType.Menu);
    public void OpenInventory() => OpenPanel(MainMenuPanelType.Inventory);
    public void OpenShop() => OpenPanel(MainMenuPanelType.Shop);
    public void OpenChamps() => OpenPanel(MainMenuPanelType.Champs);
    public void OpenNews() => OpenPanel(MainMenuPanelType.News);
    public void OpenFriends() => OpenPanel(MainMenuPanelType.Friends);
    public void OpenQuests() => OpenPanel(MainMenuPanelType.Quests);
    public void OpenProfile() => OpenPanel(MainMenuPanelType.Profile);
}