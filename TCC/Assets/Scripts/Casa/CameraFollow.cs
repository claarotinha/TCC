using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Collider2D levelBounds;

    private float fixedY;
    private float fixedZ;
    private Camera sceneCamera;

    // A área jogável corresponde aos cenários visíveis, e não ao antigo collider maior.
    public static Bounds GetArtBounds(Collider2D fallback)
    {
        GameObject background = GameObject.Find("Background");
        if (background != null)
        {
            SpriteRenderer[] parts = background.GetComponentsInChildren<SpriteRenderer>();
            if (parts.Length > 0)
            {
                Bounds visible = parts[0].bounds;
                foreach (SpriteRenderer part in parts) visible.Encapsulate(part.bounds);
                return visible;
            }
        }
        return fallback != null ? fallback.bounds : new Bounds(Vector3.zero, Vector3.one * 100f);
    }

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        sceneCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float desiredX = target.position.x;
        if (levelBounds != null && sceneCamera != null && sceneCamera.orthographic)
        {
            float halfWidth = sceneCamera.orthographicSize * sceneCamera.aspect;
            Bounds bounds = GetArtBounds(levelBounds);
            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;
            desiredX = minX <= maxX
                ? Mathf.Clamp(desiredX, minX, maxX)
                : bounds.center.x;
        }

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
        if (levelBounds != null && sceneCamera != null && sceneCamera.orthographic)
        {
            float halfWidth = sceneCamera.orthographicSize * sceneCamera.aspect;
            Bounds bounds = GetArtBounds(levelBounds);
            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;
            Vector3 position = transform.position;
            position.x = minX <= maxX
                ? Mathf.Clamp(position.x, minX, maxX)
                : bounds.center.x;
            transform.position = position;
        }
    }
}
