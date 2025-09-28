using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // new Input System
#endif

[DefaultExecutionOrder(50)]
public class FollowCamera : MonoBehaviour
{
    [Header("Follow target")]
    public Transform target;                 // drag your Player
    public Vector3 lookOffset = Vector3.zero;

    [Header("World-locked view (DS-style)")]
    [Range(0f, 360f)] public float yaw = 40f;      // world yaw (fixed; camera doesn't rotate with player)
    [Range(10f, 85f)] public float tilt = 55f;     // pitch downwards

    [Header("Distance / zoom")]
    public float distance = 12f;                   // start distance
    public float minDistance = 8f;
    public float maxDistance = 18f;
    public float zoomStep = 1.25f;                 // how much a single wheel notch changes distance

    [Header("Follow smoothing")]
    [Tooltip("0 = snap; higher = smoother")]
    public float damping = 0.12f;

    [Header("Simple occlusion (optional)")]
    public LayerMask collisionMask;                // leave empty to disable
    public float collisionBuffer = 0.3f;           // pull camera off the wall a bit

    Vector3 _vel;
    Transform _t;

    void Awake()
    {
        _t = transform;

        // auto-find player by tag if not set
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- handle zoom (mouse wheel), supports both input systems ---
        float scroll = GetScroll();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Mouse scroll is typically +/-120 on the new system; normalize a bit
            float steps = Mathf.Sign(scroll) * Mathf.Ceil(Mathf.Abs(scroll) / 120f);
            distance = Mathf.Clamp(distance - steps * zoomStep, minDistance, maxDistance);
        }

        // Build a world-locked rotation (fixed yaw & tilt, like DS)
        Quaternion viewRot = Quaternion.Euler(tilt, yaw, 0f);

        // Desired camera position: back away from the target by 'distance' along the view's -Z
        Vector3 backDir = viewRot * Vector3.back; // unit vector pointing from target to camera
        Vector3 desired = target.position + lookOffset + backDir * distance;

        // Optional: keep line of sight clear with a single raycast
        if (collisionMask.value != 0)
        {
            Vector3 origin = target.position + lookOffset;
            Vector3 toCam  = desired - origin;
            float len = toCam.magnitude;
            if (len > 0.001f && Physics.Raycast(origin, toCam.normalized, out RaycastHit hit, len, collisionMask))
            {
                desired = hit.point - toCam.normalized * collisionBuffer;
            }
        }

        // Smooth follow on position only (rotation is locked)
        if (damping <= 0f)
            _t.position = desired;
        else
            _t.position = Vector3.SmoothDamp(_t.position, desired, ref _vel, damping);

        // Lock rotation so it never spins with the player
        _t.rotation = viewRot;

        // Optionally, look slightly ahead (here just at the player + offset)
        _t.LookAt(target.position + lookOffset);
    }

    float GetScroll()
    {
        // Works with either input backend
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        #else
        return Input.mouseScrollDelta.y;
        #endif
    }
}