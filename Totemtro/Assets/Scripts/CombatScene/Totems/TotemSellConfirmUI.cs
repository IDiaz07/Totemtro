using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TotemSellConfirmUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    Action onConfirmAction;

    void Awake()
    {
        panel.SetActive(false);

        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);
    }

    // ===============================
    // SINGLE SELL
    // ===============================

    public void Open(TotemData data, Action onConfirm)
    {
        if (data == null) return;

        panel.SetActive(true);

        messageText.text = $"Sell {data.totemName}?";

        onConfirmAction = onConfirm;
    }

    // ===============================
    // SELL ALL
    // ===============================

    public void OpenSellAll(Action onConfirm)
    {
        panel.SetActive(true);

        messageText.text = "Sell ALL totems?";

        onConfirmAction = onConfirm;
    }

    // ===============================
    // CONFIRM
    // ===============================

    void Confirm()
    {
        panel.SetActive(false);

        onConfirmAction?.Invoke();

        // 🔥 MUY IMPORTANTE
        onConfirmAction = null;
    }

    void Close()
    {
        panel.SetActive(false);

        // 🔥 MUY IMPORTANTE
        onConfirmAction = null;
    }
}
