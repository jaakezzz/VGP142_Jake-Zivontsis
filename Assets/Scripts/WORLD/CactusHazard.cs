using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CactusHazard : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 6f;
    public float cooldown = 0.5f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float upwardBoost = 0.5f;

    float lastHitTime;

    // --- TRIGGERS ---
    void OnTriggerEnter(Collider other) { TryHit(other); }
    void OnTriggerStay(Collider other) { TryHit(other); }

    // --- COLLISIONS ---
    void OnCollisionEnter(Collision c) { TryHit(c.collider); }
    void OnCollisionStay(Collision c) { TryHit(c.collider); }

    void TryHit(Collider other)
    {
        if (Time.time < lastHitTime + cooldown) return;

        var hp = other.GetComponentInParent<Health>();
        if (!hp) return;

        lastHitTime = Time.time;
        hp.Damage(damage);

        // Knockback via your PlayerMotor
        var motor = other.GetComponentInParent<PlayerMotor>();
        if (motor)
        {
            Vector3 dir = other.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) dir = transform.forward; // fallback
            dir.Normalize();
            dir.y += upwardBoost;                      // tiny pop upward
            motor.ApplyExternalImpulse(dir * knockbackForce);
        }
    }
}
