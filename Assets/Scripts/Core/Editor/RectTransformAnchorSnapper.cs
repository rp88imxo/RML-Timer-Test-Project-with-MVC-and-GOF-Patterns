using UnityEditor;
using UnityEngine;

namespace RML.Core
{
public static class RectTransformAnchorSnapper
{
    [MenuItem("CONTEXT/RectTransform/Snap Anchors/Fully")]
    private static void SnapAnchorsFully(MenuCommand command)
    {
        var rect = (RectTransform)command.context;

        var parent = rect.parent as RectTransform;

        if (parent == null)
        {
            Debug.LogWarning($"Unable to snap anchors for {rect.name}");
            return;
        }

        SnapAnchorsVertically(command);
        SnapAnchorsHorizontally(command);
    }

    [MenuItem("CONTEXT/RectTransform/Snap Anchors/Vertically")]
    private static void SnapAnchorsVertically(MenuCommand command)
    {
        var rect = (RectTransform)command.context;

        var parent = rect.parent as RectTransform;

        if (parent == null)
        {
            Debug.LogWarning($"Unable to snap anchors for {rect.name}");
            return;
        }

        var corners = new Vector3[4];
        rect.GetLocalCorners(corners);

        var parentRect = parent.rect;
        float height = parentRect.height;

        float centerY = height * parent.pivot.y;

        var localPosition = rect.localPosition;

        float minAnchorY =
            (centerY + localPosition.y + corners[0].y) / height;
        float maxAnchorY =
            (centerY + localPosition.y + corners[2].y) / height;

        var anchorMin = new Vector2(rect.anchorMin.x, minAnchorY);
        var anchorMax = new Vector2(rect.anchorMax.x, maxAnchorY);

        Undo.RecordObject(rect, "Snap anchors vertically");

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;

        rect.offsetMin = new Vector2(rect.offsetMin.x, 0);
        rect.offsetMax = new Vector2(rect.offsetMax.x, 0);
    }

    [MenuItem("CONTEXT/RectTransform/Snap Anchors/Horizontally")]
    private static void SnapAnchorsHorizontally(MenuCommand command)
    {
        var rect = (RectTransform)command.context;

        var parent = rect.parent as RectTransform;

        if (parent == null)
        {
            Debug.LogWarning($"Unable to snap anchors for {rect.name}");
            return;
        }

        var corners = new Vector3[4];
        rect.GetLocalCorners(corners);

        var parentRect = parent.rect;
        float width = parentRect.width;

        float centerX = width * parent.pivot.x;

        var localPosition = rect.localPosition;

        float minAnchorX =
            (centerX + localPosition.x + corners[0].x) / width;
        float maxAnchorX =
            (centerX + localPosition.x + corners[2].x) / width;

        var anchorMin = new Vector2(minAnchorX, rect.anchorMin.y);
        var anchorMax = new Vector2(maxAnchorX, rect.anchorMax.y);

        Undo.RecordObject(rect, "Snap anchors vertically");

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;

        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0, rect.offsetMax.y);
    }
}
}