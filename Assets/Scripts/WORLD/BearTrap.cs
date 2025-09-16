using System.Collections;
using UnityEngine;

public class BearTrap : MonoBehaviour
{
    [Header("Models")]
    public GameObject openModel;     // trap1 (open)
    public GameObject closedModel;   // trap1 active (closed)

    [Header("Trigger")]
    public Collider triggerArea;     // leave null to use the root collider
    public LayerMask triggerLayers;  // set to Player layer
    public float damage = 35f;

    [Header("Lifecycle")]
    public bool destroyAfterTrigger = false;
    public bool autoReset = false;
    public float resetDelay = 6f;    // re-open after this many seconds

    [Header("SFX")]
    public AudioSource sfx;
    public AudioClip snapClip;

    bool armed = true;
    bool busy;

    void Reset()
    {
        triggerArea = GetComponent<Collider>();
        if (triggerArea) triggerArea.isTrigger = true;
        SetVisual(open: true);
    }

    void OnValidate()
    {
        if (!triggerArea) triggerArea = GetComponent<Collider>();
        SetVisual(armed);
    }

    void Start()
    {
        if (!triggerArea) triggerArea = GetComponent<Collider>();
        Arm(true);
    }

    void Arm(bool state)
    {
        armed = state;
        busy = false;
        if (triggerArea) triggerArea.enabled = state;
        SetVisual(open: state);
    }

    void SetVisual(bool open)
    {
        if (openModel) openModel.SetActive(open);
        if (closedModel) closedModel.SetActive(!open);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!armed || busy) return;
        if ((triggerLayers.value & (1 << other.gameObject.layer)) == 0) return;

        StartCoroutine(Snap(other));
    }

    IEnumerator Snap(Collider victim)
    {
        busy = true;
        armed = false;
        SetVisual(open: false);

        if (sfx && snapClip) sfx.PlayOneShot(snapClip);

        // Damage the player (uses your existing Health component)
        var hp = victim.GetComponentInParent<Health>();
        if (hp) hp.Damage(damage);

        // Stun the player if they have a PlayerMotor
        var motor = victim.GetComponentInParent<PlayerMotor>();
        if (motor) motor.Stun(1f);  // stun duration

        if (destroyAfterTrigger)
        {
            yield return null;
            Destroy(gameObject);
            yield break;
        }

        if (autoReset)
        {
            // disable trigger for a moment so we don't insta-retrigger
            if (triggerArea) triggerArea.enabled = false;
            yield return new WaitForSeconds(resetDelay);
            Arm(true);
        }
        else
        {
            // one-shot trap: leave closed & disable trigger
            if (triggerArea) triggerArea.enabled = false;
        }
    }
}
