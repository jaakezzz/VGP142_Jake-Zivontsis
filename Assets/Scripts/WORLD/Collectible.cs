using UnityEngine;

public enum CollectibleType { Heal, Speed, Points, Win }

public class Collectible : MonoBehaviour
{
    public CollectibleType type;
    public int points = 50;
    public float healAmount = 30f;
    public float speedBoost = 2f;
    public float speedDuration = 5f;
    public ParticleSystem pickupFX;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var motor = other.GetComponent<PlayerMotor>();
        var hp = other.GetComponent<Health>();
        switch (type)
        {
            case CollectibleType.Heal: if (hp) hp.Heal(healAmount); break;
            case CollectibleType.Speed: if (motor) StartCoroutine(SpeedBoost(motor)); break;
            case CollectibleType.Points: GameManager.I.AddScore(points); break;
            case CollectibleType.Win: GameManager.I.OnWin(); break;
        }
        if (pickupFX) Instantiate(pickupFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    System.Collections.IEnumerator SpeedBoost(PlayerMotor m)
    {
        float old = m.speed; m.speed *= speedBoost;
        yield return new WaitForSeconds(speedDuration);
        m.speed = old;
    }
}
