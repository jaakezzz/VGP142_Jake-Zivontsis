using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("What to disable on death")]
    public MonoBehaviour[] controlScripts;   // e.g., PlayerMotor, camera look, combat
    public CharacterController cc;

    [Header("Animation")]
    public Animator anim;                    // (optional) reference to wolf child Animator
    [Tooltip("Seconds to show the death animation before GM respawn starts")]
    public float deathAnimDuration = 1.0f;

    Collider[] colliders;
    Rigidbody rb;
    Health hp;
    bool isDead;

    void Awake()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        colliders = GetComponentsInChildren<Collider>(true);
        rb = GetComponent<Rigidbody>();

        // Fallback: auto-find a child animator with a controller (nice-to-have)
        if (!anim || !anim.runtimeAnimatorController)
        {
            anim = GetComponentsInChildren<Animator>(true)
                   .FirstOrDefault(a => a && a.runtimeAnimatorController);
        }
    }

    void Start()
    {
        hp = GetComponent<Health>();
        hp.onDeath.AddListener(OnDied);
    }

    void OnDied()
    {
        if (isDead) return;
        isDead = true;

        //Debug.Log("[PDH] OnDied at " + Time.time);

        // Stop control/collisions but KEEP renderers visible for death anim
        if (cc) cc.enabled = false;
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        foreach (var c in colliders) if (c) c.enabled = false;
        foreach (var s in controlScripts) if (s) s.enabled = false;

        // Fire the Animator trigger via the bridge (preferred)
        var bridge = GetComponent<WolfAnimatorBridge>();
        if (bridge != null)
        {
            //Debug.Log($"[PDH] Triggering Die via WolfAnimatorBridge for {deathAnimDuration}s");
            bridge.TriggerDie();
        }
        else if (anim && anim.runtimeAnimatorController)
        {
            // Fallback: call trigger directly if bridge not present
            Debug.Log($"[PDH] Triggering Die on Animator directly for {deathAnimDuration}s");
            anim.SetTrigger("Die");
        }
        else
        {
            Debug.LogWarning("[PDH] No WolfAnimatorBridge or valid Animator found to play death animation.");
        }

        // Wait so the death anim is visible, then let GM handle respawn timing/position/HP
        StartCoroutine(NotifyGMAfter(deathAnimDuration));
    }

    IEnumerator NotifyGMAfter(float delay)
    {
        //Debug.Log("[PDH] Waiting " + delay + "s before notifying GM");
        yield return new WaitForSecondsRealtime(delay);
        //Debug.Log("[PDH] Now calling GM.OnPlayerDied");
        GameManager.I.OnPlayerDied();
    }

    // Called by GameManager right before showing the player again
    public void SetDeadState(bool dead)
    {
        isDead = dead;

        if (rb) rb.isKinematic = dead;
        if (cc) cc.enabled = !dead;
        foreach (var c in colliders) if (c) c.enabled = !dead;
        foreach (var s in controlScripts) if (s) s.enabled = !dead;

        if (!dead && anim && anim.runtimeAnimatorController)
        {
            // Clear any stale state and return to locomotion cleanly
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
