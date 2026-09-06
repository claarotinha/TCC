using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject highlight;

    private ItemData item;

    public void Setup(ItemData newItem)
    {
        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }

        if (highlight != null)
            highlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null)
            return;

        InventoryManager.Instance.SelectItem(item);

        UpdateAllSlots();

        StartCoroutine(SelectedAnimation());
    }

    private void UpdateAllSlots()
    {
        InventorySlot[] slots =
            FindObjectsByType<InventorySlot>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (InventorySlot slot in slots)
        {
            if (slot.highlight != null)
            {
                bool selected =
                    InventoryManager.Instance.SelectedItem ==
                    slot.item;

                slot.highlight.SetActive(selected);
            }
        }
    }

    private IEnumerator SelectedAnimation()
    {
        Vector3 originalScale = transform.localScale;

        transform.localScale = originalScale * 1.1f;

        yield return new WaitForSeconds(0.1f);

        transform.localScale = originalScale;
    }
}