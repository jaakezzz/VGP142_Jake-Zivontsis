using UnityEngine;

public class DebugThrowZone : MonoBehaviour
{
    static bool thrown;  // ensures one throw total for this zone

    void OnTriggerEnter(Collider other)
    {
        if (!thrown && other.CompareTag("Player"))
        {
            thrown = true;
            Debug.LogException(new System.Exception(
                "Intentional lab exception: entered Out of Bounds Zone."
            ));
        }
    }
}
