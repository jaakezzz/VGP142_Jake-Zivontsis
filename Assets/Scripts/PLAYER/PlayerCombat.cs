using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator anim;
    public float punchDamage = 20f;
    public float kickDamage = 30f;
    public float range = 2f;
    public LayerMask enemyMask;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) { anim.SetTrigger("Punch"); DoHit(punchDamage); }
        if (Input.GetMouseButtonDown(1)) { anim.SetTrigger("Kick"); DoHit(kickDamage); }
    }

    void DoHit(float dmg)
    {
        var hits = Physics.SphereCastAll(transform.position + Vector3.up, 0.5f, transform.forward, range, enemyMask);
        foreach (var h in hits)
        {
            var hp = h.collider.GetComponentInParent<Health>();
            if (hp) hp.Damage(dmg);
        }
    }
}
