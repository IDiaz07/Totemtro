using UnityEngine;

public static class UILayerManager
{
    // Prioridad: el último en registrarse es el primero en cerrarse
    public enum Layer { SlotMachine, Inventory, PauseMenu }

    static Layer? activeLayer = null;

    public static void Open(Layer layer)
    {
        activeLayer = layer;
    }

    public static void Close(Layer layer)
    {
        if (activeLayer == layer)
            activeLayer = null;
    }

    public static bool IsAnyOpen() => activeLayer.HasValue;
    public static bool IsOpen(Layer layer) => activeLayer == layer;
    public static Layer? Current => activeLayer;
}