using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);

        bool isMoving = speed > 0.1f;

        // ✔ NÃO depende de velocidade pra corrida
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;

        animator.SetBool("IsWalking", isMoving);
        animator.SetBool("IsRunning", isRunning);
    }
}
