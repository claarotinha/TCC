using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    void Update()
    {
        // Não permite coletar itens durante o pause
        if (PauseManager.IsPaused)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        // Segurança extra: impede coleta caso este método seja chamado por outro script
        if (PauseManager.IsPaused)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager não encontrado!");
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("ItemData não atribuído!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);
        Debug.Log("📦 " + itemData.itemName + " coletado!");
        Destroy(gameObject);
    }
}