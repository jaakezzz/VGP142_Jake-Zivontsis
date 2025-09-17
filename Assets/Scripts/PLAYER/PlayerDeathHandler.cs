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
    [Tooltip("Seconds to show the death animation before GM decides (respawn or game over).")]
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

        // Prefer the bridge trigger; fallback to direct trigger
        var bridge = GetComponent<WolfAnimatorBridge>();
        if (bridge != null) bridge.TriggerDie();
        else if (anim && anim.runtimeAnimatorController) anim.SetTrigger("Die");

        // After the short death anim delay, let GameManager decide (lives -> respawn or game over)
        StartCoroutine(NotifyGMAfter(deathAnimDuration));
    }

    IEnumerator NotifyGMAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        GameManager.I.OnPlayerDied();
    }

    // Called by GameManager before showing the player again
    public void SetDeadState(bool dead)
    {
        isDead = dead;

        if (rb) rb.isKinematic = dead;
        if (cc) cc.enabled = !dead;
        foreach (var c in colliders) if (c) c.enabled = !dead;
        foreach (var s in controlScripts) if (s) s.enabled = !dead;

        if (!dead && anim && anim.runtimeAnimatorController)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
