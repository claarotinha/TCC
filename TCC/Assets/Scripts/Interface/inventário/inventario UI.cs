using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private TextMeshProUGUI itemsText;

    private bool isOpen;

    private void Start()
    {
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        itemsText.text = "";

        foreach (var item in Inventory.Instance.Items)
        {
            itemsText.text += item.itemName + "\n";
        }
    }
}