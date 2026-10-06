using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ItemUseTarget : MonoBehaviour
{
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private bool consumeOnUse = true;
    [SerializeField] private UnityEvent onUsed;

    private string progressId;
    private void Awake() => progressId = GameProgress.ObjectId(gameObject);

    public bool TryUse(ItemData item)
    {
        if (item == null || item != requiredItem ||
            InventoryManager.Instance == null)
            return false;

        QuartoBaguncaDoor roomDoor = GetComponent<QuartoBaguncaDoor>();
        if ((roomDoor != null && QuartoBaguncaDoor.Unlocked) ||
            (consumeOnUse && GameProgress.Instance != null &&
             GameProgress.Instance.WasTargetUsed(progressId)))
            return false;

        TutorialBoysDialogue boys = GetComponent<TutorialBoysDialogue>();
        if (boys != null && !boys.CanReceiveBall)
            return false;

        bool owned = false;
        foreach (ItemData inventoryItem in InventoryManager.Instance.Items)
        {
            if (inventoryItem == item)
            {
                owned = true;
                break;
            }
        }

        if (!owned)
            return false;

        if (consumeOnUse)
            InventoryManager.Instance.RemoveItem(item);

        if (consumeOnUse)
            GameProgress.Instance?.MarkTargetUsed(progressId);
        onUsed?.Invoke();
        if (boys != null)
            boys.OnBallReturned();
        Debug.Log("Item usado: " + item.itemName);
        return true;
    }
}

