using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    // --- External control (Sit/Trap/etc) ---
    bool externalLock;                      // hard lock: ignore movement & jump
    bool jumpEnabled = true;                // temporary on/off (e.g., traps, sit)
    [SerializeField] float groundStick = 3f;

    public void SetExternalLock(bool v) { externalLock = v; }
    public void SetJumpEnabled(bool v) { jumpEnabled = v; }
    public bool IsExternallyLocked() { return externalLock; }

    // --- Ability unlocks ---
    [Header("Ability Unlocks")]
    public bool runUnlocked = false;          // gate for running
    public bool jumpUnlocked = false;         // gate for jumping
    public bool startWithRunToggled = false; // only used if holdToRun = false

    // --- Movement ---
    [Header("Movement")]
    [Tooltip("Meters per second when walking.")]
    public float walkSpeed = 4f;
    [Tooltip("Meters per second when running.")]
    public float runSpeed = 6f;

    [Tooltip("Hold this key to run (or toggle if Hold To Run is off).")]
    public KeyCode runKey = KeyCode.LeftShift;
    public bool holdToRun = true;

    // Back-compat: current speed used this frame (read by other scripts)
    public float speed = 4f;

    public float gravity = -20f;       // negative
    public float jumpForce = 6f;
    public Transform cam;
    public Vector3 externalImpulse;

    [Header("Jump Reliability")]
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;

    // --- Attacks ---
    [Header("Attacks")]
    public Animator animator;                 // wolf Animator (for attack triggers only)
    public string attack1Trigger = "Attack1";
    public string attack2Trigger = "Attack2";
    public KeyCode attack1Key = KeyCode.Mouse0;
    public KeyCode attack2Key = KeyCode.Mouse1;

    [Tooltip("Temporarily lock movement during attack (optional).")]
    public bool lockMoveDuringAttack = false;
    public float attack1LockTime = 0.35f;
    public float attack2LockTime = 0.45f;

    [Header("Attack Lunges")]
    public float attack1LungeDelay = 0.25f;       // Time before the lunge triggers
    public float attack2LungeDelay = 0.4f;       // Time before the lunge triggers
    public float attack1LungeForce = 6.6f;   // Forward push
    public float attack2LungeForce = 12f;  // Forward push
    public float attack1UpwardForce = 3f;    // Slight lift off the ground
    public float attack2UpwardForce = 5f;    // Slight lift off the ground

    CharacterController cc;
    float vertVel;
    bool stunned;
    bool attackLocked;
    float coyoteTimer;
    float bufferTimer;
    int ignoreGroundedFrames;
    PlayerMelee melee;

    // run toggle state (if not hold-to-run)
    bool runToggled;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        melee = GetComponent<PlayerMelee>();

        runToggled = startWithRunToggled;

        // Initialize speed respecting run unlock + input mode
        if (!runUnlocked)
            speed = walkSpeed;
        else if (holdToRun)
            speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;
        else
            speed = (runToggled ? runSpeed : walkSpeed);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Run input (compute 'speed' BEFORE using it) ---
        if (!runUnlocked) speed = walkSpeed;
        else if (holdToRun) speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;
        else
        {
            if (Input.GetKeyDown(runKey)) runToggled = !runToggled;
            speed = runToggled ? runSpeed : walkSpeed;
        }

        // --- Handle attack input ---
        HandleAttackInput();

        Vector3 moveXZ = Vector3.zero;

        // Only allow WASD input if we are NOT locked
        if (!stunned && !externalLock && !attackLocked)
        {
            // ---- Input (WASD relative to camera)
            Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : transform.forward;
            Vector3 right = cam ? cam.right : transform.right;
            Vector2 in2 = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveXZ = (right * in2.x + fwd * in2.y).normalized * speed;

            // Buffer jump input (requires ability + temporary enable)
            if (jumpUnlocked && jumpEnabled && Input.GetButtonDown("Jump")) bufferTimer = jumpBuffer;
            else bufferTimer -= dt;
        }
        else
        {
            bufferTimer -= dt; // Still decay the jump buffer even if locked
        }

        // ---- Grounded check with short grace period
        bool grounded = cc.isGrounded;
        if (ignoreGroundedFrames > 0) { grounded = false; ignoreGroundedFrames--; }

        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= dt;

        // Apply buffered / coyote jump (only if unlocked + enabled)
        if (!stunned && !externalLock && !attackLocked && jumpUnlocked && jumpEnabled && bufferTimer > 0f && coyoteTimer > 0f)
        {
            vertVel = jumpForce;
            bufferTimer = 0f;
            coyoteTimer = 0f;
            ignoreGroundedFrames = 2;
        }

        // Gravity & stick-to-ground
        if (grounded && vertVel < 0f)
            vertVel = -2f;
        vertVel += gravity * dt;

        Vector3 velocity = moveXZ;
        velocity.y = vertVel;

        // add impulses (decay over time)
        if (externalImpulse.sqrMagnitude > 0.01f)
        {
            velocity += externalImpulse;
            externalImpulse = Vector3.Lerp(externalImpulse, Vector3.zero, 5f * dt);
        }

        cc.Move(velocity * dt);

        // NOTE: we are NOT feeding animator locomotion params anymore.
        // Keep your blend tree driven by whatever *else* you want (e.g., separate component).
    }


    // --- Attacks ---
    void HandleAttackInput()
    {
        if (Input.GetKeyDown(attack1Key))
            DoAttack(attack1Trigger, attack1LockTime);

        if (Input.GetKeyDown(attack2Key))
            DoAttack(attack2Trigger, attack2LockTime);
    }

    void DoAttack(string triggerName, float lockTime)
    {
        if (stunned || externalLock || attackLocked) return;

        if (animator && !string.IsNullOrEmpty(triggerName))
        {
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }

        // Hook into melee
        if (melee)
        {
            if (triggerName == attack1Trigger)
            {
                melee.TriggerAttack1();
                GetComponent<PlayerAudio>()?.PlayAttack1();
                StartCoroutine(LungeRoutine(attack1LungeDelay, attack1LungeForce, attack1UpwardForce));
            }
            else if (triggerName == attack2Trigger)
            {
                melee.TriggerAttack2();
                GetComponent<PlayerAudio>()?.PlayAttack2();
                StartCoroutine(LungeRoutine(attack2LungeDelay, attack2LungeForce, attack2UpwardForce));
            }
        }

        if (lockMoveDuringAttack && lockTime > 0f)
            StartCoroutine(AttackLock(lockTime));
    }

    IEnumerator LungeRoutine(float delay, float forwardForce, float upwardForce)
    {
        yield return new WaitForSeconds(delay);

        // Cancel the lunge if the player died or got stunned during the windup
        if (stunned || externalLock) yield break;

        var faceAlign = GetComponent<ModelGroundAlignAndFace>();
        Vector3 lungeDir = (faceAlign != null && faceAlign.model != null) ? faceAlign.model.forward : transform.forward;

        // Apply the forward momentum to the external impulse system
        Vector3 impulse = lungeDir * forwardForce;
        ApplyExternalImpulse(impulse);

        // Apply the upward force directly to the gravity tracking
        if (upwardForce > 0f)
        {
            vertVel = upwardForce;
            ignoreGroundedFrames = 2;
        }
    }

    IEnumerator AttackLock(float t)
    {
        attackLocked = true;
        bufferTimer = 0f; // cancel any jump buffer
        yield return new WaitForSeconds(t);
        attackLocked = false;
    }

    public void Stun(float duration)
    {
        if (!stunned) StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        stunned = true;
        yield return new WaitForSeconds(duration);
        stunned = false;
    }

    public void ApplyExternalImpulse(Vector3 impulse)
    {
        externalImpulse += impulse;
    }

    // --- Public API for pickups / scripts ---
    public void SetRunUnlocked(bool v) { runUnlocked = v; }
    public void SetJumpUnlocked(bool v) { jumpUnlocked = v; }
    public void UnlockRun() => runUnlocked = true;
    public void UnlockJump() => jumpUnlocked = true;
    public void LockRun() => runUnlocked = false;
    public void LockJump() => jumpUnlocked = false;
}
