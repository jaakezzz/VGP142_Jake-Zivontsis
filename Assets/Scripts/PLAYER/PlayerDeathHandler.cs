
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    Health hp;
    void Start()
    {
        hp = GetComponent<Health>();
        hp.onDeath.AddListener(() => { GameManager.I.OnPlayerDied(); gameObject.SetActive(false); });
    }
}
