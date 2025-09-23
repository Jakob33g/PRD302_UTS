using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;              // units/second
    public bool faceMoveDirection = true;     // rotate toward movement

    Rigidbody rb;
    Vector3 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Make sure Rigidbody has Freeze Rotation X & Z checked in Inspector
    }

    void Update()
    {
        // WASD/Arrows: Horizontal = X, Vertical = Z
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0f, v).normalized; // no faster diagonals
    }

    void FixedUpdate()
    {
        // Apply XZ velocity, keep gravity on Y
        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * moveSpeed;
        vel.z = moveInput.z * moveSpeed;
        rb.linearVelocity = vel;

        // Optional: face movement direction
        if (faceMoveDirection && moveInput.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.2f));
        }
    }
}