using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private Collider2D movementBounds;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D bodyCollider;

    private float horizontalInput;
    private bool isRunning;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Se o jogo estiver pausado, não lê nenhum comando
        if (PauseHelper.BlockInput())
            return;

        HandleInput();
        CheckGround();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Se estiver pausado, garante que a Mari fique totalmente parada
        if (PauseHelper.BlockInput())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
        KeepInsideBounds();
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
        if (groundCheck == null)
            return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    private void KeepInsideBounds()
    {
        if (movementBounds == null)
            return;

        float halfWidth = bodyCollider != null ? bodyCollider.bounds.extents.x : 0f;
        Bounds bounds = CameraFollow.GetArtBounds(movementBounds);
        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float clampedX = minX <= maxX
            ? Mathf.Clamp(rb.position.x, minX, maxX)
            : bounds.center.x;

        if (!Mathf.Approximately(clampedX, rb.position.x))
        {
            rb.position = new Vector2(clampedX, rb.position.y);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
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
