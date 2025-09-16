using UnityEngine;

public class SimpleChaseCamera : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;        // PlayerRoot
    public Camera cam;              // Main Camera child

    [Header("Orbit")]
    public float height = 1.6f;     // orbit center above target
    public float distance = 7f;     // default follow distance
    public float minDistance = 2.5f, maxDistance = 12f;
    public float yaw = 0f, pitch = 20f;
    public float minPitch = -5f, maxPitch = 55f;
    public float yawSpeed = 180f, pitchSpeed = 140f;

    [Header("Collision")]
    public float collisionRadius = 0.35f;
    public float collisionPadding = 0.25f;
    public float minGroundClearance = 0.6f;
    public LayerMask collideMask;   // MUST include Terrain/Ground + Environment, exclude Player

    [Header("Smoothing")]
    public float followLerp = 12f;
    public float camLerp = 20f;

    Vector3 curPos; Quaternion curRot;

    void Start()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        curPos = cam ? cam.transform.position : transform.position;
        curRot = cam ? cam.transform.rotation : transform.rotation;
    }

    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.unscaledDeltaTime;
        pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.unscaledDeltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
            distance = Mathf.Clamp(distance - scroll, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target || !cam) return;

        // Anchor follows target (no tilt)
        transform.position = Vector3.Lerp(
            transform.position, target.position,
            1f - Mathf.Exp(-followLerp * Time.deltaTime));

        // World-up basis (stable)
        Vector3 up = Vector3.up;
        Quaternion yawQ = Quaternion.AngleAxis(yaw, up);
        Vector3 fwd = yawQ * Vector3.forward;
        Vector3 right = Vector3.Cross(up, fwd);
        Vector3 camFwd = Quaternion.AngleAxis(pitch, right) * fwd;

        // Orbit from a lifted center
        Vector3 orbitCenter = transform.position + up * height;
        Vector3 desired = orbitCenter - camFwd * distance;
        Quaternion drot = Quaternion.LookRotation(camFwd, up);

        // Collision from orbit center to desired
        Vector3 dir = desired - orbitCenter;
        float len = dir.magnitude;
        dir = (len > 1e-4f) ? dir / len : -camFwd;

        if (Physics.SphereCast(orbitCenter, collisionRadius, dir,
                out var hit, len, collideMask, QueryTriggerInteraction.Ignore))
            desired = hit.point - dir * collisionPadding;

        // Keep a little clearance above ground
        if (Physics.Raycast(desired + up * 0.5f, -up, out var floorHit, 2f, collideMask))
        {
            float h = Vector3.Dot(desired - floorHit.point, up);
            if (h < minGroundClearance) desired = floorHit.point + up * minGroundClearance;
        }

        // Smooth
        curPos = Vector3.Lerp(curPos, desired, 1f - Mathf.Exp(-camLerp * Time.deltaTime));
        curRot = Quaternion.Slerp(curRot, drot, 1f - Mathf.Exp(-camLerp * Time.deltaTime));
        cam.transform.SetPositionAndRotation(curPos, curRot);
    }
}
