using UnityEngine;

public class ObstacleSlow : MonoBehaviour
{
    public float multiplier = 0.5f;
    void OnTriggerEnter(Collider other) { if (other.TryGetComponent<PlayerMotor>(out var m)) m.speed *= multiplier; }
    void OnTriggerExit(Collider other) { if (other.TryGetComponent<PlayerMotor>(out var m)) m.speed /= multiplier; }
}