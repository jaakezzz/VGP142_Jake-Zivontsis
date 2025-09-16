using UnityEngine;
using System.Collections;

public class BooEnemyAI : MonoBehaviour
{
    public Transform player;
    public Renderer[] renderersToHide;     // fallback only
    public GhostDissolve visuals;

    [Header("Perception")]
    [Range(10f, 100f)] public float visibilityRange = 25f;   // how far Boo can be "seen"
    [Range(10f, 179f)] public float visibilityConeDegrees = 120f; // total cone angle
    public float shootRange = 18f;
    public float fireInterval = 1.5f;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    bool visibleToPlayer = true;
    bool lastVisible = true;    // debounce
    Coroutine fireLoop;

    void Start()
    {
        if (!visuals) visuals = GetComponentInChildren<GhostDissolve>();

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (renderersToHide == null || renderersToHide.Length == 0)
            renderersToHide = GetComponentsInChildren<Renderer>(true);

        fireLoop = StartCoroutine(FireRoutine());
    }

    void Update()
    {
        if (!player) return;

        // Direction & distance to player (flatten Y so height doesn't skew angle)
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        toPlayer.Normalize();

        // Angle test (cone): dot >= cos(half-angle)
        float halfAngleRad = 0.5f * visibilityConeDegrees * Mathf.Deg2Rad;
        float dotThreshold = Mathf.Cos(halfAngleRad);         // 120° -> 60° half-angle -> 0.5
        float dot = Vector3.Dot(transform.forward, toPlayer);

        // Must be inside both the cone AND the distance
        bool nowVisible = (dot >= dotThreshold) && (dist <= visibilityRange);

        // Update visuals only on change
        if (nowVisible != lastVisible)
        {
            if (visuals) visuals.SetVisible(nowVisible);
            else foreach (var r in renderersToHide) if (r) r.enabled = nowVisible;

            visibleToPlayer = nowVisible;
            lastVisible = nowVisible;
        }
        else
        {
            visibleToPlayer = nowVisible;
        }
    }

    IEnumerator FireRoutine()
    {
        var wait = new WaitForSeconds(fireInterval);
        while (true)
        {
            yield return wait;
            if (!player || !projectilePrefab || !firePoint) continue;

            float dist = Vector3.Distance(transform.position, player.position);
            if (visibleToPlayer && dist <= shootRange)
            {
                var aim = Quaternion.LookRotation((player.position + Vector3.up / 3f) - firePoint.position);
                Instantiate(projectilePrefab, firePoint.position, aim);
            }
        }
    }
}
