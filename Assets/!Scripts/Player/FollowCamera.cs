using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // new Input System
#endif

[DefaultExecutionOrder(50)]
public class FollowCamera : MonoBehaviour
{
    [Header("What to Follow")]
    public Transform target;                 // Drag your Player here
    public Vector3 lookOffset = Vector3.zero;

    [Header("Camera Angle")]
    [Range(0f, 360f)] public float yaw = 40f;      // Camera rotation left/right (stays fixed, doesn't turn with player)
    [Range(10f, 85f)] public float tilt = 55f;     // Camera angle up/down (how much you look down at the player)

    [Header("Camera Distance")]
    public float distance = 12f;                   // How far the camera stays from the player

    // These fields are kept for backward compatibility but are no longer used (zoom is disabled)
    [System.Obsolete("Zoom is disabled. This field is kept for scene compatibility only.")]
    [HideInInspector] public float minDistance = 8f;
    [System.Obsolete("Zoom is disabled. This field is kept for scene compatibility only.")]
    [HideInInspector] public float maxDistance = 18f;
    [System.Obsolete("Zoom is disabled. This field is kept for scene compatibility only.")]
    [HideInInspector] public float zoomStep = 1.25f;

    [Header("Camera Movement Smoothing")]
    [Tooltip("How smooth the camera follows. 0 = instant snap, higher numbers = smoother")]
    public float damping = 0.12f;

    [Header("Wall Collision (Optional)")]
    public LayerMask collisionMask;                // What layers count as walls. Leave empty to disable wall detection
    public float collisionBuffer = 0.3f;           // How much to pull camera back from walls

    Vector3 _vel;
    Transform _t;

    void Awake()
    {
        _t = transform;

        // Find the player automatically if not assigned
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // Camera zoom is disabled - distance stays fixed

        // Set the camera rotation based on yaw and tilt
        Quaternion viewRot = Quaternion.Euler(tilt, yaw, 0f);

        // Calculate where the camera should be (behind the player at a distance)
        Vector3 backDir = viewRot * Vector3.back; // Direction from player to camera
        Vector3 desired = target.position + lookOffset + backDir * distance;

        // Check if there's a wall blocking the camera view
        if (collisionMask.value != 0)
        {
            Vector3 origin = target.position + lookOffset;
            Vector3 toCam  = desired - origin;
            float len = toCam.magnitude;
            if (len > 0.001f && Physics.Raycast(origin, toCam.normalized, out RaycastHit hit, len, collisionMask))
            {
                // Move camera closer if there's a wall
                desired = hit.point - toCam.normalized * collisionBuffer;
            }
        }

        // Move camera smoothly to the desired position
        if (damping <= 0f)
            _t.position = desired;
        else
            _t.position = Vector3.SmoothDamp(_t.position, desired, ref _vel, damping);

        // Keep camera rotation fixed (doesn't rotate with player)
        _t.rotation = viewRot;

        // Make camera look at the player
        _t.LookAt(target.position + lookOffset);
    }
}