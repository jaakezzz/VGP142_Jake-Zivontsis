using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DebugThrowOnPickup : MonoBehaviour
{
    static int globalThrows = 0;
    public static int maxThrows = 2;

    void Reset()
    {
        // Make setup foolproof
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (globalThrows >= maxThrows) return;
        if (!other.CompareTag("Player")) return;

        globalThrows++;
        Debug.LogException(new System.Exception(
            $"Intentional lab exception from pickup: {name} (#{globalThrows})"
        ));
        Destroy(gameObject);
    }
}
