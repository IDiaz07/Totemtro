using UnityEngine;
using System.Collections.Generic;

public class MinimapSystem : MonoBehaviour
{
    public Camera minimapCamera;
    public RectTransform iconsContainer;
    public Transform player; // 🔥 AÑADIDO

    List<MinimapIcon> icons = new List<MinimapIcon>();

    public void Register(MinimapIcon icon)
    {
        if (icon == null) return;
        icons.Add(icon);
    }

    public void Unregister(MinimapIcon icon)
    {
        if (icon == null) return;
        icons.Remove(icon);
    }

    void LateUpdate()
    {
        if (minimapCamera == null || player == null) return;

        for (int i = icons.Count - 1; i >= 0; i--)
        {
            if (icons[i] == null || icons[i].Target == null)
            {
                icons.RemoveAt(i);
                continue;
            }

            UpdateIcon(icons[i]);
        }
    }

    void UpdateIcon(MinimapIcon icon)
    {
        Vector3 playerPos = player.position;
        Vector3 worldPos = icon.Target.position;

        Vector2 delta = worldPos - playerPos;

        float mapHeight = minimapCamera.orthographicSize * 2f;
        float mapWidth = mapHeight * minimapCamera.aspect;

        Vector2 normalized = new Vector2(
            delta.x / mapWidth,
            delta.y / mapHeight
        );

        Rect rect = iconsContainer.rect;

        Vector2 uiPos = new Vector2(
            normalized.x * rect.width,
            normalized.y * rect.height
        );

        icon.SetVisible(true);
        icon.UpdatePosition(uiPos);
    }
}