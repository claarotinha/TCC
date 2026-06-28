using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void OnMouseEnter()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
        }
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("=== TESTE DE COLETA ===");

        Debug.Log("InventoryManager.Instance = " + InventoryManager.Instance);

        Debug.Log("itemData = " + itemData);

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Não existe InventoryManager na cena!");
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("ItemData não foi atribuído ao coletável!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);

        Destroy(gameObject);
    }
}