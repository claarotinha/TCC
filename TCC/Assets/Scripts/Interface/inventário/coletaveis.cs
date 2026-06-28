using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void OnMouseDown()
    {
        // Verifica se o painel de exame está aberto
        if (ExamineObject.IsShowing())
        {
            Debug.Log("⛔ Não é possível coletar enquanto examina um objeto.");
            return;
        }

        Debug.Log("=== COLETA ===");

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("❌ InventoryManager não existe na cena!");
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("❌ ItemData não foi atribuído ao coletável!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);
        Debug.Log("✅ " + itemData.itemName + " coletado com sucesso!");
        Destroy(gameObject);
    }
}