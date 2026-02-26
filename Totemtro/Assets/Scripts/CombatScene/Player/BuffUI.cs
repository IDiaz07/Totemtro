using UnityEngine;
using System.Collections.Generic;

public class BuffUI : MonoBehaviour
{
    public static BuffUI Instance;

    public GameObject buffIconPrefab;
    public Transform container;

    Dictionary<string, BuffIconUI> activeBuffs =
        new Dictionary<string, BuffIconUI>();

    void Awake()
    {
        Instance = this;
    }

    public void AddBuff(string buffName, float duration)
    {
        if (activeBuffs.ContainsKey(buffName))
        {
            activeBuffs[buffName].Refresh(duration);
            return;
        }

        GameObject icon =
            Instantiate(buffIconPrefab, container);

        BuffIconUI ui =
            icon.GetComponent<BuffIconUI>();

        ui.Initialize(buffName, duration, RemoveBuff);

        activeBuffs.Add(buffName, ui);
    }

    void RemoveBuff(string buffName)
    {
        if (activeBuffs.ContainsKey(buffName))
            activeBuffs.Remove(buffName);
    }

    public void CancelBuff(string buffName)
    {
        if (!activeBuffs.ContainsKey(buffName))
            return;

        BuffIconUI ui = activeBuffs[buffName];

        activeBuffs.Remove(buffName);

        if (ui != null)
            ui.Cancel();   // 🔥 Animación pro
    }
}