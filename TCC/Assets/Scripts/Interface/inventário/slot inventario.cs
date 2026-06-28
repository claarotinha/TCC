using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image highlight;

    private ItemData item;

    private Vector3 targetScale;
    private Vector3 normalScale = Vector3.one;
    private Vector3 selectedScale = Vector3.one * 1.1f;

    private void Update()
    {
        // animação suave de escala
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * 12f
        );
    }

    public void Setup(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.icon;

        UpdateVisualInstant();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.SelectItem(item);
        UpdateAllSlots();
    }

    public void UpdateVisual()
    {
        bool selected = InventoryManager.Instance.SelectedItem == item;

        if (highlight != null)
            highlight.enabled = selected;

        targetScale = selected ? selectedScale : normalScale;
    }

    private void UpdateVisualInstant()
    {
        bool selected = InventoryManager.Instance.SelectedItem == item;

        if (highlight != null)
            highlight.enabled = selected;

        targetScale = selected ? selectedScale : normalScale;

        transform.localScale = targetScale;
    }

    private void UpdateAllSlots()
    {
        // 🔥 CORRIGIDO: Usa FindObjectsByType em vez de FindObjectsOfType
        InventorySlot[] slots = FindObjectsByType<InventorySlot>(FindObjectsSortMode.None);

        foreach (var s in slots)
            s.UpdateVisual();
    }
}