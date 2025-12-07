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

    // Lauren
    Animator anim;
    private Vector2 lastMoveDirection;
    public AudioClip walkSFX;
    private AudioSource audioSource;
    public float stepInterval = 0.4f;
    private float stepTimer = 0f;


    void Start()
    {
        anim = GetComponent<Animator>();
    }
    //lauren end
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveSpeed = baseMoveSpeed;
        // Important: Make sure Rigidbody has Freeze Rotation X & Z checked in Inspector
        audioSource = GetComponent<AudioSource>();
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

        //Lauren
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        /* if ((moveX == 0 && moveY == 0) && (moveInput.x != 0 || moveInput.y != 0))
        {
            lastMoveDirection = moveInput;
        } */

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = new Vector2(moveInput.x, moveInput.z);
        }

        Animate();
        WalkSFX();
        // Lauren end
    }

    void FixedUpdate()
    {
        // Move the player using physics
        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * moveSpeed;
        vel.z = moveInput.z * moveSpeed;
        rb.linearVelocity = vel;

        /*
        // Turn player to face the direction they're moving (if enabled)
        if (faceMoveDirection && moveInput.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.2f));
        } */ //lauren note: this is opposite of what we wanted for our game
    }

    // Lauren
    void Animate()
    {
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.z);
        anim.SetFloat("MoveMagnitude", moveInput.magnitude);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }

    void WalkSFX()
    {
        if (moveInput.sqrMagnitude > 0.01f && walkSFX != null)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                audioSource.PlayOneShot(walkSFX);
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
}