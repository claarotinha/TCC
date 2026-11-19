using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ExamineObject))]
public class SecaCabinetItem : MonoBehaviour
{
    [SerializeField] private SecaPreparationRequest preparationRequest;
    [SerializeField] private ItemData item;
    [SerializeField] private GameObject collectable;
    private static SecaCabinetItem activeCabinet;
    private ExamineObject examine;
    private Collider2D cabinetCollider;
    private bool inspected;
    private bool collected;

    private void Awake()
    {
        cabinetCollider = GetComponent<Collider2D>();
        examine = GetComponent<ExamineObject>();
        examine.Opened += Open;
    }

    private void OnEnable() => activeCabinet = this;

    private void OnDisable()
    {
        if (activeCabinet == this) activeCabinet = null;
    }

    public bool Contains(Vector2 point) =>
        isActiveAndEnabled && cabinetCollider != null && cabinetCollider.enabled &&
        cabinetCollider.OverlapPoint(point);

    public static bool CoversBackgroundExamine(ExamineObject target, Vector2 point) =>
        activeCabinet != null && activeCabinet.examine != target && activeCabinet.Contains(point);

    private void Open()
    {
        if (!inspected)
        {
            inspected = true;
            return;
        }
        TryCollect();
    }

    public bool TryCollect()
    {
        if (!isActiveAndEnabled || !inspected || collected ||
            preparationRequest == null || !preparationRequest.RequestCompleted) return false;
        InventoryTabController inventory = InventoryTabController.Instance;
        CollectPrompt prompt = inventory != null ? inventory.GetComponent<CollectPrompt>() : null;
        if (prompt == null || item == null) return false;
        if (InventoryManager.Instance != null && InventoryManager.Instance.Items.Contains(item))
        {
            collected = true;
            examine.message = "O lugar onde o facão estava guardado ficou vazio. Já o levei comigo.";
            return false;
        }
        if (!prompt.Show(item, () =>
        {
            collected = true;
            examine.message = "O lugar onde o facão estava guardado ficou vazio. Já o levei comigo.";
            if (collectable != null)
            {
                GameProgress.Instance?.MarkObjectCollected(GameProgress.ObjectId(collectable));
                Destroy(collectable);
            }
        }, alreadyExamined: true)) return false;

        examine.HidePanel();
        return true;
    }

    private void OnDestroy()
    {
        if (examine != null) examine.Opened -= Open;
    }
}
