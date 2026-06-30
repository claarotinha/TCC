using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void OnMouseDown()
    {
        // Verifica se está examinando algo
        if (ExamineObject.IsShowing() || CollectableExamine.IsShowing())
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