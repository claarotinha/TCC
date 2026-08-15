using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Parallax")]
    [SerializeField] private float parallaxStrength = 0.1f;
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 lastPlayerPosition;
    private Vector3 velocity;

    private void Start()
    {
        lastPlayerPosition = player.position;
    }

    private void LateUpdate()
    {
        Vector3 playerMovement = player.position - lastPlayerPosition;

        Vector3 targetPosition = transform.position;

        targetPosition.x -= playerMovement.x * parallaxStrength;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        lastPlayerPosition = player.position;
    }
}