using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    public GameObject dropPrefab; // assign a Collectible prefab
    public MonoBehaviour[] aiComponents; // BooEnemyAI, ChaserEnemyAI, etc.

    void Start()
    {
        var hp = GetComponent<Health>();
        hp.onDeath.AddListener(() =>
        {
            foreach (var a in aiComponents) a.enabled = false; // stop tracking (no rotations/movement)
            if (dropPrefab) Instantiate(dropPrefab, transform.position, Quaternion.identity);
            // play death anim via Animator parameter if you have one
            Destroy(gameObject, 3f);
        });
    }
}
