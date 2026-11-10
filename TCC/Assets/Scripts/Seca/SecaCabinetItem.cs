using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ExamineObject))]
public class SecaCabinetItem : MonoBehaviour
{
    [SerializeField] private SecaPreparationRequest preparationRequest;
    [SerializeField] private ItemData item;
    [SerializeField] private GameObject collectable;
    private ExamineObject examine;
    private bool inspected;
    private bool collected;

    private void Awake()
    {
        examine = GetComponent<ExamineObject>();
        examine.Opened += Open;
    }

    private void Open()
    {
        if (!inspected)
        {
            inspected = true;
            return;
        }
        if (collected || preparationRequest == null || !preparationRequest.RequestCompleted) return;
        InventoryTabController inventory = InventoryTabController.Instance;
        CollectPrompt prompt = inventory != null ? inventory.GetComponent<CollectPrompt>() : null;
        if (prompt == null || item == null) return;
        if (InventoryManager.Instance != null && InventoryManager.Instance.Items.Contains(item))
        {
            collected = true;
            examine.message = "O lugar onde o facão estava guardado ficou vazio. Já o levei comigo.";
            return;
        }
        examine.HidePanel();
        prompt.Show(item, () =>
        {
            collected = true;
            examine.message = "O lugar onde o facão estava guardado ficou vazio. Já o levei comigo.";
            if (collectable != null)
            {
                GameProgress.Instance?.MarkObjectCollected(GameProgress.ObjectId(collectable));
                Destroy(collectable);
            }
        });
    }

    private void OnDestroy()
    {
        if (examine != null) examine.Opened -= Open;
    }
}
