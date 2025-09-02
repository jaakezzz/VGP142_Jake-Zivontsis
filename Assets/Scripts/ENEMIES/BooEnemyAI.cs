using UnityEngine;
using System.Collections;

public class BooEnemyAI : MonoBehaviour
{
    public Transform player;
    public Renderer[] renderersToHide; // mesh renderers
    public float shootRange = 18f;
    public float fireInterval = 1.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    bool visibleToPlayer = true;
    Coroutine fireLoop;

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        fireLoop = StartCoroutine(FireRoutine());
    }

    void Update()
    {
        if (!player) return;
        Vector3 toEnemy = (transform.position - player.position).normalized;
        Vector3 playerForward = player.forward;
        float dot = Vector3.Dot(playerForward, toEnemy); // >0 => enemy in front of player

        visibleToPlayer = dot > 0.25f; // tweak cone
        foreach (var r in renderersToHide) r.enabled = visibleToPlayer;

        // Optionally freeze when seen
        if (visibleToPlayer) { /* stand still or play idle */ }
        else { /* optionally creep towards player */ }
    }

    IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);
            if (!player) continue;
            float dist = Vector3.Distance(transform.position, player.position);
            if (visibleToPlayer && dist <= shootRange)
            {
                var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation((player.position + Vector3.up) - firePoint.position));
            }
        }
    }
}
