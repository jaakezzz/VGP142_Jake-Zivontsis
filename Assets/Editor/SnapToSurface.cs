// Assets/Editor/SnapToSurface.cs
using UnityEngine;
using UnityEditor;

public static class SnapToSurface
{
    const float CastHeight = 2000f;          // start far above
    const float PivotPadding = 0.0f;         // extra lift if you want a tiny gap
    static readonly int Everything = ~0;

    [MenuItem("GameObject/Snap To Ground %g", false, 0)]   // Ctrl/Cmd + G
    public static void SnapDown() => SnapInternal(alignToNormal: false);

    [MenuItem("GameObject/Snap & Align To Surface %#g", false, 1)] // Ctrl/Cmd + Shift + G
    public static void SnapAndAlign() => SnapInternal(alignToNormal: true);

    static void SnapInternal(bool alignToNormal)
    {
        if (Selection.gameObjects.Length == 0)
            return;

        foreach (var go in Selection.gameObjects)
        {
            // only scene objects
            if (!go.scene.IsValid()) continue;

            var t = go.transform;
            // Cast from well above the current pivot
            Vector3 origin = t.position + Vector3.up * CastHeight;

            if (Physics.Raycast(origin, Vector3.down, out var hit, Mathf.Infinity, Everything, QueryTriggerInteraction.Ignore))
            {
                Undo.RecordObject(t, "Snap To Surface");

                // If the object pivot isn't at the bottom, compute how much to lift
                float pivotToBottom = 0f;
                if (TryGetBottomOffset(go, out var offset))
                    pivotToBottom = offset;

                t.position = hit.point + hit.normal * (pivotToBottom + PivotPadding);

                if (alignToNormal)
                {
                    // Keep current forward projected onto the surface so it doesn't spin randomly
                    Vector3 up = hit.normal;
                    Vector3 fwd = Vector3.ProjectOnPlane(t.forward, up);
                    if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.ProjectOnPlane(Vector3.forward, up);
                    t.rotation = Quaternion.LookRotation(fwd.normalized, up);
                }
            }
            else
            {
                Debug.LogWarning($"Snap: no collider found beneath '{go.name}'.");
            }
        }
    }

    // Try to estimate how far the pivot is above the lowest point of the renderers/colliders,
    // so we can rest the object on the surface instead of burying it.
    static bool TryGetBottomOffset(GameObject go, out float offset)
    {
        offset = 0f;

        // Prefer renderers (visual bounds usually best for props)
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            offset = Mathf.Max(0f, go.transform.position.y - b.min.y);
            return true;
        }

        // Fallback to colliders
        var cols = go.GetComponentsInChildren<Collider>();
        if (cols != null && cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            offset = Mathf.Max(0f, go.transform.position.y - b.min.y);
            return true;
        }

        return false;
    }
}
