using UnityEngine;

public class RangerEnemyAI : MonoBehaviour
{
    public Transform player;
    public float desiredRange = 12f;
    public float moveSpeed = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 2.5f;
    float cd;

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        Vector3 dir = toPlayer.normalized;

        if (dist < desiredRange - 1f) transform.position -= dir * moveSpeed * Time.deltaTime;
        else if (dist > desiredRange + 1f) transform.position += dir * moveSpeed * Time.deltaTime;

        transform.forward = Vector3.Lerp(transform.forward, dir, 10f * Time.deltaTime);

        cd -= Time.deltaTime;
        if (cd <= 0f)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
            cd = fireCooldown;
        }
    }
}
