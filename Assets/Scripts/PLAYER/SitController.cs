using UnityEngine;

public class SitController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;           // wolf child Animator (with Wolf_animation controller)
    [SerializeField] CharacterController cc;  // optional; used for movement checks
    [SerializeField] PlayerMotor motor;       // optional; to lock movement/jump

    [Header("Input")]
    public KeyCode sitKey = KeyCode.LeftControl;

    [Header("Behavior")]
    [Tooltip("If true: sitting hard-locks movement until you press the sit key again. If false: sitting ends as soon as you move.")]
    public bool sitLocksMovement = false;
    [Tooltip("How much input speed exits sit when sitLocksMovement is false.")]
    public float moveExitThreshold = 0.15f;

    [Header("Animator")]
    public string sitBoolParam = "Sit";

    bool isSitting;

    void Reset()
    {
        anim = GetComponentInChildren<Animator>(true);
        cc = GetComponent<CharacterController>();
        motor = GetComponent<PlayerMotor>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!cc) cc = GetComponent<CharacterController>();
        if (!motor) motor = GetComponent<PlayerMotor>();
    }

    void Update()
    {
        // Toggle with sit key
        if (Input.GetKeyDown(sitKey))
        {
            SetSitting(!isSitting);
        }

        // If auto-exit mode, leave sit when we detect movement
        if (!sitLocksMovement && isSitting)
        {
            float planarSpeed = 0f;
            if (cc) { var v = cc.velocity; v.y = 0f; planarSpeed = v.magnitude; }

            // fallback: sample input axes if CC is absent or not moving yet
            if (!cc) planarSpeed = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).magnitude;

            if (planarSpeed > moveExitThreshold)
                SetSitting(false);
        }
    }

    public void SetSitting(bool sit)
    {
        if (isSitting == sit) return;
        isSitting = sit;

        // Drive Animator
        if (anim) anim.SetBool(sitBoolParam, isSitting);

        // Optional: lock player controls
        if (motor)
        {
            // Hard-lock movement when sitting? (jump disabled either way)
            motor.SetExternalLock(isSitting && sitLocksMovement);
            motor.SetJumpEnabled(!isSitting); // disable jump while sitting
        }
    }

    public bool IsSitting() => isSitting;
}
