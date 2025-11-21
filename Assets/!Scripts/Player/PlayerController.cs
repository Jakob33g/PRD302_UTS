using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 6f;          // Starting speed before skills
    [HideInInspector] public float moveSpeed = 6f;  // Current speed (changed by skills)
    public bool faceMoveDirection = true;     // Turn to face the direction you're moving

    Rigidbody rb;
    Vector3 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveSpeed = baseMoveSpeed;
        // Important: Make sure Rigidbody has Freeze Rotation X & Z checked in Inspector
    }

    public void SetMoveSpeed(float newSpeed)
    {
        // Change movement speed (used by skills)
        moveSpeed = newSpeed;
    }

    void Update()
    {
        // Get movement input from WASD or arrow keys
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0f, v).normalized;  // Normalize so diagonal movement isn't faster
    }

    void FixedUpdate()
    {
        // Move the player using physics
        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * moveSpeed;
        vel.z = moveInput.z * moveSpeed;
        rb.linearVelocity = vel;

        // Turn player to face the direction they're moving (if enabled)
        if (faceMoveDirection && moveInput.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.2f));
        }
    }
}