using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject slotPrefab;

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (content == null || slotPrefab == null)
        {
            Debug.LogError("InventoryUI: Content ou Slot Prefab não configurado.");
            return;
        }

        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null)
            return;

        foreach (ItemData item in InventoryManager.Instance.Items)
        {
            GameObject slot = Instantiate(slotPrefab, content);
            slot.GetComponent<InventorySlot>().Setup(item);
        }
    }
}
