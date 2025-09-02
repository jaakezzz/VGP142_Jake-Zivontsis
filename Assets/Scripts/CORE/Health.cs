using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    public UnityEvent onDeath;
    public UnityEvent<float, float> onHealthChanged; // current, max

    void Awake() { currentHP = maxHP; onHealthChanged?.Invoke(currentHP, maxHP); }

    public void Damage(float dmg)
    {
        currentHP = Mathf.Max(0f, currentHP - dmg);
        onHealthChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0f) Die();
    }

    public void Heal(float amt)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amt);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Die() { onDeath?.Invoke(); }
}
