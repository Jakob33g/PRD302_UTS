using UnityEngine;
using UnityEngine.InputSystem;


/*
CODE WRITTEN BY LAUREN
*/

public class FixedPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    private Vector2 input;
    private Vector2 lastMoveDirection;
    private Rigidbody rb;
    private Quaternion fixedRotation;

    [SerializeField] private SpriteRenderer spriteRenderer;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        fixedRotation = transform.rotation;
    }

    private void Update()
    {
        Inputs();
        Animate();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(input.x, 0f, input.y).normalized;
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
         transform.rotation = fixedRotation;
    }

    private void Inputs()
    {
        if (Keyboard.current == null) return;

        input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;

        if (input.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = input.normalized;
        }
    }

    private void Animate()
    {
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);
        animator.SetFloat("MoveMagnitude", input.magnitude);
        animator.SetFloat("LastMoveX", lastMoveDirection.x);
        animator.SetFloat("LastMoveY", lastMoveDirection.y);

    }
}
