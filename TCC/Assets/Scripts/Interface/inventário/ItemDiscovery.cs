using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemDiscovery : MonoBehaviour
{
    [SerializeField] private ItemData item;
    private Collider2D itemCollider;
    private string progressId;
    private ExamineObject examine;
    private bool inspected;

    private void Awake()
    {
        itemCollider = GetComponent<Collider2D>();
        examine = GetComponent<ExamineObject>();
        if (examine != null) examine.enabled = false;
        progressId = GameProgress.ObjectId(gameObject);
    }

    private void Start()
    {
        if (GameProgress.Instance != null && GameProgress.Instance.WasObjectCollected(progressId))
            gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || Camera.main == null ||
            itemCollider == null || item == null ||
            InventoryTabController.Instance == null ||
            (InvestigationGuard.Blocked && !(examine != null && examine.PanelIsOpen)))
            return;

        if (!InvestigationGuard.TryGetPointerPosition(out Vector2 mousePosition)) return;
        if (!itemCollider.OverlapPoint(mousePosition))
            return;

        if (!inspected && examine != null)
        {
            inspected = true;
            examine.ShowMessage(string.IsNullOrWhiteSpace(examine.message) ? item.description : examine.message);
            return;
        }
        if (examine != null && examine.PanelIsOpen) examine.HidePanel();

        CollectPrompt prompt =
            InventoryTabController.Instance.GetComponent<CollectPrompt>();

        if (prompt == null)
        {
            Debug.LogError("CollectPrompt não encontrado no InventoryCanvas.");
            return;
        }

        prompt.Show(item, () =>
        {
            GameProgress.Instance?.MarkObjectCollected(progressId);
            Destroy(gameObject);
        }, alreadyExamined: examine != null && inspected);
    }
}

