using UnityEngine;
public class SpawnPointGizmo : MonoBehaviour
{
    public Color color = Color.yellow;
    public float radius = 1.0f;
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
