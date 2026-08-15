using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isRunning;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Se o jogo estiver pausado, não lê nenhum comando
        if (UniversalPauseManager.IsPaused)
            return;

        HandleInput();
        CheckGround();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Se estiver pausado, garante que a Mari fique totalmente parada
        if (UniversalPauseManager.IsPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    private void Move()
    {
        float speed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(
            horizontalInput * speed,
            rb.linearVelocity.y
        );
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    private void UpdateAnimations()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);

        bool isMoving = speed > 0.1f;
        bool running = isRunning && isMoving;

        animator.SetBool("IsWalking", isMoving);
        animator.SetBool("IsRunning", running);
    }
}