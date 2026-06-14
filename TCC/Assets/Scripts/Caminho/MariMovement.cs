using UnityEngine;

public class MariMovement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float runSpeed = 6f;

    private Rigidbody2D rb;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, 0f);
    }
}