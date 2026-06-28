using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject slotPrefab;

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in InventoryManager.Instance.Items)
        {
            GameObject slot = Instantiate(slotPrefab, content);

            slot.GetComponent<InventorySlot>()
                .Setup(item);
        }
    }
}