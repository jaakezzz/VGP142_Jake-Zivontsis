using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float life = 5f;
    public float damage = 15f;
    public LayerMask hitMask;

    void Start() { Destroy(gameObject, life); }

    void Update() { transform.position += transform.forward * speed * Time.deltaTime; }

    void OnTriggerEnter(Collider other)
    {
        // 1) If we touched something with Health (e.g., the Player), apply damage no matter the layer.
        var hp = other.GetComponentInParent<Health>();
        if (hp)
        {
            hp.Damage(damage);
            Destroy(gameObject);
            return;
        }

        // 2) Otherwise, use the mask to decide if we should disappear (terrain, props, etc.)
        if (((1 << other.gameObject.layer) & hitMask) != 0)
        {
            Destroy(gameObject);
        }
    }

}
