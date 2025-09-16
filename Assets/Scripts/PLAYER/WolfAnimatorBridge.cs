using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class WolfAnimatorBridge : MonoBehaviour
{
    Animator anim;
    CharacterController cc;
    Vector3 lastPos;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();
        lastPos = transform.position;
    }

    void Update()
    {
        // Calculate horizontal speed
        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPos = transform.position;

        // Normalize into 0 (idle), 1 (walk), 2 (run)
        anim.SetFloat("Speed", speed / 6f * 2f); // adjust 6f if PlayerMotor speed changes
    }

    public void TriggerAttack() => anim.SetTrigger("Attack");
    public void TriggerDie() => anim.SetTrigger("Die");
}
