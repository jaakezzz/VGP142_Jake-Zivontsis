using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -20f;
    public float jumpForce = 6f;
    public Transform cam;

    CharacterController cc;
    Vector3 velocity;

    void Start() { cc = GetComponent<CharacterController>(); if (!cam) cam = Camera.main.transform; }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 forward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 right = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 move = (forward * v + right * h) * speed;

        cc.Move(move * Time.deltaTime);

        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        if (Input.GetButtonDown("Jump") && cc.isGrounded) velocity.y = jumpForce;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
