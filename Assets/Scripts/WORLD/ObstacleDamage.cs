using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    public float dps = 20f;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<Health>();
            if (hp) hp.Damage(dps * Time.deltaTime);
        }
    }
}
