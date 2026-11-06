using UnityEngine;

// Os ambientes são exclusivos; a câmera e a personagem usam o mesmo limite.
[DefaultExecutionOrder(-500)]
public class SecaEnvironmentLimits : MonoBehaviour
{
    [SerializeField] private GameObject[] environments;
    [SerializeField] private SpriteRenderer[] backgrounds;
    [SerializeField] private BoxCollider2D movementArea;
    [SerializeField] private CameraFollow cameraFollow;
    private int current = -1;

    private void Start() => Refresh();
    private void Update() => Refresh();
    private void LateUpdate() => Refresh();

    private void Refresh()
    {
        if (movementArea == null || environments == null || backgrounds == null) return;
        for (int i = 0; i < environments.Length && i < backgrounds.Length; i++)
        {
            if (environments[i] == null || !environments[i].activeInHierarchy ||
                backgrounds[i] == null || backgrounds[i].sprite == null) continue;
            if (current == i) return;
            current = i;
            Bounds bounds = backgrounds[i].bounds;
            movementArea.offset = (Vector2)bounds.center - (Vector2)movementArea.transform.position;
            movementArea.size = new Vector2(bounds.size.x, bounds.size.y);
            Physics2D.SyncTransforms();
            if (cameraFollow == null) return;
            cameraFollow.AtualizarLimites();
            Camera camera = cameraFollow.GetComponent<Camera>();
            Transform target = cameraFollow.target;
            if (camera == null || target == null) return;
            float halfWidth = camera.orthographicSize * camera.aspect;
            float min = bounds.min.x + halfWidth;
            float max = bounds.max.x - halfWidth;
            Vector3 position = cameraFollow.transform.position;
            position.x = min <= max ? Mathf.Clamp(target.position.x, min, max) : bounds.center.x;
            cameraFollow.transform.position = position;
            return;
        }
    }
}
