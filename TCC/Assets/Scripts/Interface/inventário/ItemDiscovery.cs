using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemDiscovery : MonoBehaviour
{
    [SerializeField] private ItemData item;
    private Collider2D itemCollider;
    private string progressId;

    private void Awake()
    {
        itemCollider = GetComponent<Collider2D>();
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
            InvestigationGuard.Blocked)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!itemCollider.OverlapPoint(mousePosition))
            return;

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
        });
    }
}

