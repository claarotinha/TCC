using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image icon;

    private ItemData item;

    public void Setup(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
    }

    public void Select()
    {
        InventoryManager.Instance.SelectItem(item);

        Debug.Log(item.itemName + " selecionado");
    }
}