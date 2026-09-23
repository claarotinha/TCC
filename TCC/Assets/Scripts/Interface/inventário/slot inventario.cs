using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || item.icon == null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        dragIcon = new GameObject("ItemArrastado", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dragIcon.transform.SetParent(canvas.rootCanvas.transform, false);
        Image image = dragIcon.GetComponent<Image>();
        image.sprite = item.icon;
        image.preserveAspect = true;
        image.raycastTarget = false;
        dragIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
        dragIcon.transform.SetAsLastSibling();
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null) dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);
        dragIcon = null;
        if (item == null) return;
        InventorySlot other = eventData.pointerCurrentRaycast.gameObject != null
            ? eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<InventorySlot>() : null;
        if (other != null && other != this && InventoryManager.Instance != null &&
            InventoryManager.Instance.TryCombine(item, other.item)) return;
        if (!CasaInventoryController.IsOpen || Camera.main == null ||
            !MotherDialogue.FalouSobreTrabalho) return;
        Vector2 point = Camera.main.ScreenToWorldPoint(eventData.position);
        foreach (Collider2D hit in Physics2D.OverlapPointAll(point))
        {
            QuartoBaguncaDoor door = hit.GetComponent<QuartoBaguncaDoor>();
            if (door == null || door.key != item) continue;
            CasaInventoryController.CloseInventory();
            door.UnlockWithKey();
            return;
        }
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