using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Collider2D levelBounds;
    [SerializeField] private SpriteRenderer[] backgroundPieces;

    [SerializeField] private float horizontalOffset;

    private float fixedY;
    private float fixedZ;
    private Camera sceneCamera;
    private Bounds cameraBounds;
    private bool hasBounds;
    private bool preparada;
    [SerializeField] private bool limitarAltura;
    private float zoomDosLimites;

    public void LiberarLimites()
    {
        levelBounds = null;
        backgroundPieces = new SpriteRenderer[0];
        hasBounds = false;
    }

    public void DefinirLimites(Collider2D limites, bool restringirAltura = false)
    {
        if (sceneCamera == null) sceneCamera = GetComponent<Camera>();
        if (zoomDosLimites <= 0f && sceneCamera != null) zoomDosLimites = sceneCamera.orthographicSize;
        levelBounds = limites;
        backgroundPieces = new SpriteRenderer[0];
        hasBounds = limites != null;
        if (hasBounds) cameraBounds = limites.bounds;
        limitarAltura = restringirAltura;
    }

    private float ClampY(float y)
    {
        if (!limitarAltura || !hasBounds || sceneCamera == null || !sceneCamera.orthographic) return y;
        float metade = sceneCamera.orthographicSize;
        float minimo = cameraBounds.min.y + metade, maximo = cameraBounds.max.y - metade;
        return minimo <= maximo ? Mathf.Clamp(y, minimo, maximo) : cameraBounds.center.y;
    }

    void Start()
    {
        if (preparada) return;
        PrepararCamera();
        // Outras cenas preservam o posicionamento inicial já utilizado.
        if (target != null)
            transform.position = new Vector3(ClampX(target.position.x + horizontalOffset), fixedY, fixedZ);
    }

    public void RetomarSemReposicionar()
    {
        if (!preparada) PrepararCamera();
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        enabled = true;
    }

    public void RestaurarEnquadramentoDoAlvo()
    {
        if (!preparada) PrepararCamera();
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        if (target != null)
            transform.position = new Vector3(ClampX(target.position.x + horizontalOffset), fixedY, fixedZ);
    }

    private void PrepararCamera()
    {
        preparada = true;
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
    }

    // Chamado pelo setor quando uma passagem é liberada; outras cenas mantêm seus limites.
    public void AtualizarLimites()
    {
        if (levelBounds == null) return;
        cameraBounds = levelBounds.bounds;
        hasBounds = true;
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

        if (limitarAltura && hasBounds && sceneCamera != null && zoomDosLimites > 0f)
        {
            // Em telas largas, a rua curta precisa caber sem expor as bordas vazias.
            sceneCamera.orthographicSize = Mathf.Min(zoomDosLimites, cameraBounds.extents.y,
                cameraBounds.extents.x / Mathf.Max(.01f, sceneCamera.aspect));
        }
        float desiredX = ClampX(target.position.x + horizontalOffset);

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
        position.y = ClampY(position.y);
        transform.position = position;
    }
}
