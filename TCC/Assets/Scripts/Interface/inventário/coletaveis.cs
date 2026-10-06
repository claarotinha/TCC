using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private string progressId;

    private void Awake() => progressId = GameProgress.ObjectId(gameObject);

    private void Start()
    {
        if (GameProgress.Instance != null && GameProgress.Instance.WasObjectCollected(progressId))
            gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        // Verifica se está examinando algo
        if (InvestigationGuard.Blocked)
        {
            Debug.Log("⛔ Não é possível coletar enquanto examina um objeto.");
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("❌ ItemData não atribuído!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("❌ InventoryManager não existe!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);
        Debug.Log("✅ " + itemData.itemName + " coletado com sucesso!");
        GameProgress.Instance?.MarkObjectCollected(progressId);
        Destroy(gameObject);
    }

    void OnMouseEnter()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }
}

