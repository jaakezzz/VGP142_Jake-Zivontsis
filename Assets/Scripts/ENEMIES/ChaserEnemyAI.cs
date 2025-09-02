using UnityEngine;
using UnityEngine.AI;

public class ChaserEnemyAI : MonoBehaviour
{
    public Transform player;
    public float meleeDamage = 15f;
    public float meleeRange = 1.8f;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < meleeRange)
        {
            var hp = player.GetComponent<Health>();
            if (hp) hp.Damage(meleeDamage * Time.deltaTime); // simple DPS
        }
    }
}
