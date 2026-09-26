using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject highlight;

    private ItemData item;
    private GameObject dragIcon;

    public void Setup(ItemData newItem)
    {
        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (highlight != null)
            highlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.SelectItem(item);
        UpdateAllSlots();
        StartCoroutine(SelectedAnimation());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || item.icon == null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        dragIcon = new GameObject(
            "ItemArrastado",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        dragIcon.transform.SetParent(canvas.rootCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 120);

        Image dragImage = dragIcon.GetComponent<Image>();
        dragImage.sprite = item.icon;
        dragImage.preserveAspect = true;
        dragImage.raycastTarget = false;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool wasDragging = dragIcon != null;

        if (dragIcon != null)
            Destroy(dragIcon);

        dragIcon = null;

        if (!wasDragging || item == null)
            return;

        // Primeiro, verifica se o item foi solto sobre outro slot.
        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;
        InventorySlot otherSlot = dropObject != null
            ? dropObject.GetComponentInParent<InventorySlot>()
            : null;

        if (otherSlot != null)
        {
            if (otherSlot != this && otherSlot.item != null)
            {
                CombinationPrompt prompt = GetComponentInParent<CombinationPrompt>();
                if (prompt != null)
                    prompt.Show(item, otherSlot.item);
            }

            return;
        }

        // Soltar no restante do inventário não usa o item no cenário.
        InventoryUI inventoryUI = GetComponentInParent<InventoryUI>();
        RectTransform inventoryRect = inventoryUI != null
            ? inventoryUI.GetComponentInParent<RectTransform>()
            : null;

        if (inventoryRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                inventoryRect, eventData.position, null))
            return;

        if (Camera.main == null)
            return;

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);

        foreach (Collider2D hit in Physics2D.OverlapPointAll(worldPoint))
        {
            ItemUseTarget target = hit.GetComponent<ItemUseTarget>();

            if (target != null && target.TryUse(item))
                return;
        }
    }

    private void OnDisable()
    {
        if (dragIcon != null)
            Destroy(dragIcon);

        dragIcon = null;
    }

    private void UpdateAllSlots()
    {
        InventorySlot[] slots = FindObjectsByType<InventorySlot>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (InventorySlot slot in slots)
        {
            if (slot.highlight != null)
            {
                slot.highlight.SetActive(
                    InventoryManager.Instance.SelectedItem == slot.item
                );
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
