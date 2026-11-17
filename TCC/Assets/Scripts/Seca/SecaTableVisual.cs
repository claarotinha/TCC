using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SecaTableVisual : MonoBehaviour
{
    [SerializeField] private SecaMealDialogue mealDialogue;
    [SerializeField] private Sprite comMari;
    [SerializeField] private Sprite semMari;
    [SerializeField] private Sprite semMariSemCabaca;
    [SerializeField] private ItemData cabaca;
    [SerializeField] private SpriteRenderer mari;
    [Header("Enquadramento da refeição")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField, Min(0.1f)] private float duracaoZoom = 0.65f;
    [SerializeField, Min(0f)] private float margem = 0.08f;
    [SerializeField] private Color corFundoRefeicao = new Color(0.294f, 0.263f, 0.247f, 1f);
    private Color corCameraAnterior;
    private CameraClearFlags limpezaAnterior;
    private Camera cameraRefeicao;
    private CameraFollow seguirCamera;
    private Vector3 posicaoCamera;
    private float zoomCamera;
    private float progressoZoom;
    private bool enquadrando, seguiaAntes;
    private readonly Vector3[] cantos = new Vector3[4];
    private static SecaTableVisual activeTable;
    private ExamineObject interaction;
    private Collider2D area;
    private void OnEnable() => activeTable = this;

    public static bool CoversBackgroundExamine(ExamineObject target, Vector2 point)
    {
        return activeTable != null && activeTable.interaction != null &&
            activeTable.interaction != target && activeTable.interaction.enabled &&
            activeTable.area != null && activeTable.area.OverlapPoint(point);
    }

    private SpriteRenderer table;
    private bool hidMari;

    private void Awake()
    {
        table = GetComponent<SpriteRenderer>();
        if (mealDialogue != null)
        {
            interaction = mealDialogue.GetComponent<ExamineObject>();
            area = mealDialogue.GetComponent<Collider2D>();
        }
    }

    private void LateUpdate()
    {
        bool seated = mealDialogue != null && mealDialogue.MealInProgress;
        bool recolheuCabaca = false;
        if (InventoryManager.Instance != null && cabaca != null)
            foreach (ItemData item in InventoryManager.Instance.Items)
                if (item == cabaca) recolheuCabaca = true;
        table.sprite = seated ? comMari : recolheuCabaca && semMariSemCabaca != null ? semMariSemCabaca : semMari;
        AjustarAreaDaMesa();
        if (seated) EnquadrarMesa();
        else RestaurarCamera();
        if (seated && mari != null && !hidMari)
        {
            hidMari = mari.enabled;
            mari.enabled = false;
        }
        else if (!seated) RestoreMari();
    }

    private void AjustarAreaDaMesa()
    {
        BoxCollider2D caixa = area as BoxCollider2D;
        if (caixa == null || table.sprite == null) return;
        // O desenho e a interação continuam juntos quando a mesa é redimensionada na Hierarchy.
        Vector3 minimo = caixa.transform.InverseTransformPoint(table.bounds.min);
        Vector3 maximo = caixa.transform.InverseTransformPoint(table.bounds.max);
        caixa.offset = (Vector2)((minimo + maximo) * 0.5f);
        caixa.size = new Vector2(Mathf.Abs(maximo.x - minimo.x), Mathf.Abs(maximo.y - minimo.y));
    }

    private void EnquadrarMesa()
    {
        if (!enquadrando)
        {
            cameraRefeicao = Camera.main;
            if (cameraRefeicao == null || !cameraRefeicao.orthographic) return;
            posicaoCamera = cameraRefeicao.transform.position;
            zoomCamera = cameraRefeicao.orthographicSize;
            corCameraAnterior = cameraRefeicao.backgroundColor;
            limpezaAnterior = cameraRefeicao.clearFlags;
            cameraRefeicao.clearFlags = CameraClearFlags.SolidColor;
            cameraRefeicao.backgroundColor = corFundoRefeicao;
            seguirCamera = cameraRefeicao.GetComponent<CameraFollow>();
            seguiaAntes = seguirCamera != null && seguirCamera.enabled;
            if (seguirCamera != null) seguirCamera.enabled = false;
            progressoZoom = 0f;
            enquadrando = true;
        }
        float fracaoPainel = 0f;
        if (dialoguePanel != null)
        {
            RectTransform painel = dialoguePanel.GetComponent<RectTransform>();
            painel.GetWorldCorners(cantos);
            Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            fracaoPainel = Mathf.Clamp(RectTransformUtility.WorldToScreenPoint(uiCamera, cantos[1]).y / Mathf.Max(1f, Screen.height), 0f, 0.65f);
        }
        Bounds area = table.bounds;
        float zoom = Mathf.Max((area.size.y + margem * 2f) / (2f * (1f - fracaoPainel)),
            (area.size.x + margem * 2f) / (2f * cameraRefeicao.aspect));
        Vector3 destino = new Vector3(area.center.x, area.center.y - zoom * fracaoPainel, posicaoCamera.z);
        progressoZoom += Time.deltaTime / duracaoZoom;
        float t = Mathf.SmoothStep(0f, 1f, progressoZoom);
        cameraRefeicao.orthographicSize = Mathf.Lerp(zoomCamera, zoom, t);
        cameraRefeicao.transform.position = Vector3.Lerp(posicaoCamera, destino, t);
    }

    private void RestaurarCamera()
    {
        if (!enquadrando) return;
        if (cameraRefeicao != null)
        {
            cameraRefeicao.transform.position = posicaoCamera;
            cameraRefeicao.orthographicSize = zoomCamera;
            cameraRefeicao.backgroundColor = corCameraAnterior;
            cameraRefeicao.clearFlags = limpezaAnterior;
        }
        if (seguirCamera != null) seguirCamera.enabled = seguiaAntes;
        enquadrando = false;
    }

    private void RestoreMari()
    {
        if (hidMari && mari != null) mari.enabled = true;
        hidMari = false;
    }

    private void OnDisable()
    {
        if (activeTable == this) activeTable = null;
        RestoreMari();
        RestaurarCamera();
    }
}
