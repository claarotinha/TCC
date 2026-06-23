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

        bool isWalking = speed > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isWalking;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
    }
}