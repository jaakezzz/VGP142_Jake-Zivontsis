using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TerrainTreeColliderBaker : MonoBehaviour
{
    public Terrain terrain;
    public string containerName = "TreeColliders";

    [Header("Capsule defaults (scaled per tree)")]
    public float baseHeight = 6f;
    public float baseRadius = 0.35f;
    public float yOffset = 0f;

    [Header("Filter which prototypes get colliders")]
    public int[] includePrototypeIndices; // e.g., only real trees (not grass/bush)

    Transform _container;

    void OnEnable() { if (!terrain) terrain = GetComponent<Terrain>(); }

#if UNITY_EDITOR
    [ContextMenu("Log Tree Prototypes")]
    public void LogPrototypes()
    {
        if (!terrain || !terrain.terrainData) { Debug.LogWarning("Assign a Terrain."); return; }
        var protos = terrain.terrainData.treePrototypes;
        for (int i = 0; i < protos.Length; i++)
        {
            var name = protos[i].prefab ? protos[i].prefab.name : "(null)";
            Debug.Log($"Prototype {i}: {name}");
        }
    }

    [ContextMenu("Clear & Rebuild Colliders")]
    public void Rebuild()
    {
        if (!terrain || !terrain.terrainData) { Debug.LogWarning("Assign a Terrain."); return; }

        // container
        _container = transform.Find(containerName) ?? new GameObject(containerName).transform;
        _container.SetParent(transform, false);

        // clear old
        for (int i = _container.childCount - 1; i >= 0; i--) DestroyImmediate(_container.GetChild(i).gameObject);

        var td = terrain.terrainData;
        var size = td.size;
        var pos = terrain.transform.position;

        // quick lookup for included indices
        System.Array.Sort(includePrototypeIndices);

        int made = 0;
        foreach (var ti in td.treeInstances)
        {
            if (includePrototypeIndices != null && includePrototypeIndices.Length > 0)
            {
                if (System.Array.BinarySearch(includePrototypeIndices, ti.prototypeIndex) < 0) continue; // skip non-tree (grass/bush)
            }

            Vector3 wp = new Vector3(ti.position.x * size.x, ti.position.y * size.y, ti.position.z * size.z) + pos;

            var go = new GameObject($"TreeCol_{made++}");
            go.transform.SetParent(_container, true);
            go.transform.position = wp + new Vector3(0, yOffset, 0);
            go.transform.rotation = Quaternion.Euler(0f, ti.rotation * Mathf.Rad2Deg, 0f);

            var cap = go.AddComponent<CapsuleCollider>();
            cap.direction = 1; // Y
            cap.height = baseHeight * ti.heightScale;
            cap.radius = baseRadius * ti.widthScale;
            cap.center = new Vector3(0, cap.height * 0.5f, 0);
        }

        Debug.Log($"Baked {made} filtered tree colliders.");
    }
#endif
}
