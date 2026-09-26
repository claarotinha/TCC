using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Collider2D levelBounds;
    [SerializeField] private SpriteRenderer[] backgroundPieces;

    private float fixedY;
    private float fixedZ;
    private Camera sceneCamera;
    private Bounds cameraBounds;
    private bool hasBounds;

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        sceneCamera = GetComponent<Camera>();

        if (levelBounds != null)
        {
            cameraBounds = levelBounds.bounds;
            hasBounds = true;
        }
        else if (backgroundPieces != null)
        {
            foreach (SpriteRenderer piece in backgroundPieces)
            {
                if (piece == null || piece.sprite == null)
                    continue;

                if (!hasBounds)
                {
                    cameraBounds = piece.bounds;
                    hasBounds = true;
                }
                else
                    cameraBounds.Encapsulate(piece.bounds);
            }
        }

        // Começa no trecho onde a personagem está, sem mostrar a área vazia.
        if (target != null)
            transform.position = new Vector3(ClampX(target.position.x), fixedY, fixedZ);
    }

    private float ClampX(float x)
    {
        if (!hasBounds || sceneCamera == null || !sceneCamera.orthographic)
            return x;

        float halfWidth = sceneCamera.orthographicSize * sceneCamera.aspect;
        float minX = cameraBounds.min.x + halfWidth;
        float maxX = cameraBounds.max.x - halfWidth;
        return minX <= maxX ? Mathf.Clamp(x, minX, maxX) : cameraBounds.center.x;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float desiredX = ClampX(target.position.x);

        Vector3 desiredPosition = new Vector3(
            desiredX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // A interpolação não pode deixar a câmera parcialmente fora do cenário.
        Vector3 position = transform.position;
        position.x = ClampX(position.x);
        transform.position = position;
    }
}
