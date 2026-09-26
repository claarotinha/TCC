using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemDiscovery : MonoBehaviour
{
    [SerializeField] private ItemData item;

    private void OnMouseDown()
    {
        if (item == null || InventoryTabController.Instance == null)
            return;

        CollectPrompt prompt =
            InventoryTabController.Instance.GetComponent<CollectPrompt>();

        if (prompt == null)
        {
            Debug.LogError("CollectPrompt não encontrado no InventoryCanvas.");
            return;
        }

        prompt.Show(item, () => Destroy(gameObject));
    }
}