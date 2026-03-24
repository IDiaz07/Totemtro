using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class InputKeyBindings : MonoBehaviour
{
    public static InputKeyBindings Instance;

    // =========================================
    // ACCIONES
    // =========================================

    public enum Action
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Inventory,
        Map,
        Extraction,
        Dash,
        Pause,
        Interact
    }

    // =========================================
    // DEFAULTS
    // =========================================

    static readonly Dictionary<Action, KeyCode> defaults = new Dictionary<Action, KeyCode>
    {
        { Action.MoveUp,      KeyCode.W },
        { Action.MoveDown,    KeyCode.S },
        { Action.MoveLeft,    KeyCode.A },
        { Action.MoveRight,   KeyCode.D },
        { Action.Inventory,   KeyCode.E },
        { Action.Map,         KeyCode.M },
        { Action.Extraction,  KeyCode.F },
        { Action.Dash,        KeyCode.Space },
        { Action.Pause,       KeyCode.Escape },
        { Action.Interact,    KeyCode.F },
    };

    Dictionary<Action, KeyCode> bindings = new Dictionary<Action, KeyCode>();

    // =========================================
    // LIFECYCLE
    // =========================================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBindings();
    }

    // =========================================
    // PUBLIC API
    // =========================================

    public KeyCode GetKey(Action action)
    {
        return bindings.ContainsKey(action) ? bindings[action] : KeyCode.None;
    }

    public bool GetKeyDown(Action action)
    {
        return Input.GetKeyDown(GetKey(action));
    }

    public bool GetKeyHeld(Action action)
    {
        return Input.GetKey(GetKey(action));
    }

    public bool GetKeyUp(Action action)
    {
        return Input.GetKeyUp(GetKey(action));
    }

    public string GetKeyName(Action action)
    {
        return GetKey(action).ToString().ToUpper();
    }

    /// <summary>
    /// Reasignar una tecla. Guarda automáticamente en PlayerPrefs.
    /// </summary>
    public void SetKey(Action action, KeyCode newKey)
    {
        bindings[action] = newKey;
        SaveBindings();
    }

    public void ResetToDefaults()
    {
        bindings = new Dictionary<Action, KeyCode>(defaults);
        SaveBindings();
    }

    // =========================================
    // MOVIMIENTO — ejes para PlayerMovement
    // =========================================

    public float GetHorizontalAxis()
    {
        float val = 0f;

        if (Input.GetKey(GetKey(Action.MoveRight))) val += 1f;
        if (Input.GetKey(GetKey(Action.MoveLeft)))  val -= 1f;

        return val;
    }

    public float GetVerticalAxis()
    {
        float val = 0f;

        if (Input.GetKey(GetKey(Action.MoveUp)))   val += 1f;
        if (Input.GetKey(GetKey(Action.MoveDown))) val -= 1f;

        return val;
    }

    // =========================================
    // PERSISTENCIA
    // =========================================

    void LoadBindings()
    {
        bindings.Clear();

        foreach (var kvp in defaults)
        {
            string saved = PlayerPrefs.GetString(
                "Key_" + kvp.Key, kvp.Value.ToString());

            if (Enum.TryParse(saved, out KeyCode key))
                bindings[kvp.Key] = key;
            else
                bindings[kvp.Key] = kvp.Value;
        }
    }

    void SaveBindings()
    {
        foreach (var kvp in bindings)
        {
            PlayerPrefs.SetString("Key_" + kvp.Key, kvp.Value.ToString());
        }

        PlayerPrefs.Save();
    }
}