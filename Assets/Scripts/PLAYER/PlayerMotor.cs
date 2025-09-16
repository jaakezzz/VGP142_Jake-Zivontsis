using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    // --- External control (Sit/Trap/etc) ---
    bool externalLock;                 // hard lock: ignore movement & jump
    bool jumpEnabled = true;           // allow/disallow jump while otherwise movable
    [SerializeField] float groundStick = 3f; // small downward push while locked

    public void SetExternalLock(bool v) { externalLock = v; }
    public void SetJumpEnabled(bool v) { jumpEnabled = v; }
    public bool IsExternallyLocked() { return externalLock; }

    // --- Movement ---
    public float speed = 6f;
    public float gravity = -20f;       // negative
    public float jumpForce = 6f;
    public Transform cam;

    [Header("Jump Reliability")]
    public float coyoteTime = 0.12f;   // still jump shortly after leaving ground
    public float jumpBuffer = 0.12f;   // accept jump slightly before landing

    CharacterController cc;
    float vertVel;
    bool stunned;
    float coyoteTimer;
    float bufferTimer;
    int ignoreGroundedFrames;          // prevents slope from canceling jump

    void Awake() { cc = GetComponent<CharacterController>(); }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Hard locks (stun or external lock e.g. Sit/BearTrap) ---
        if (stunned || externalLock)
        {
            // keep controller grounded so we don’t hover on slopes
            if (cc && !cc.isGrounded)
                cc.Move(Vector3.down * groundStick * dt);
            return;
        }

        // ---- Input (WASD relative to camera)
        Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : transform.forward;
        Vector3 right = cam ? cam.right : transform.right;
        Vector2 in2 = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 moveXZ = (right * in2.x + fwd * in2.y).normalized * speed;

        // ---- Grounded check with short grace period
        bool grounded = cc.isGrounded;
        if (ignoreGroundedFrames > 0) { grounded = false; ignoreGroundedFrames--; }

        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= dt;

        // Jump buffer (only when jump is enabled)
        if (jumpEnabled && Input.GetButtonDown("Jump")) bufferTimer = jumpBuffer;
        else bufferTimer -= dt;

        // Apply buffered / coyote jump
        if (jumpEnabled && bufferTimer > 0f && coyoteTimer > 0f)
        {
            vertVel = jumpForce;
            bufferTimer = 0f;
            coyoteTimer = 0f;
            ignoreGroundedFrames = 2;  // let us fully leave the slope
        }

        // Gravity & stick-to-ground
        if (grounded && vertVel < 0f)
            vertVel = -2f;                 // small downward bias
        vertVel += gravity * dt;

        Vector3 velocity = moveXZ;
        velocity.y = vertVel;

        cc.Move(velocity * dt);
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
}
