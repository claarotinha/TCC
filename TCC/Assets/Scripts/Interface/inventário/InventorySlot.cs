using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null;
            icon.raycastTarget = true;

            icon.color = item != null && item.itemName == "Cabaça cheia"
                ? new Color(0.65f, 0.85f, 1f, 1f)
                : Color.white;
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
    }

    public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
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

        // Verifica a combinação com outro item do inventário.
        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;

        InventorySlot otherSlot = dropObject != null
            ? dropObject.GetComponentInParent<InventorySlot>()
            : null;

        if (otherSlot != null)
        {
            if (otherSlot != this && otherSlot.item != null)
            {
                CombinationPrompt prompt =
                    GetComponentInParent<CombinationPrompt>();

                if (prompt != null)
                    prompt.Show(item, otherSlot.item);
            }

            return;
        }

        // A revista aceita a ferramenta apenas quando solta sobre o cabo.
        EntradaCurrais entradaCurrais = FindAnyObjectByType<EntradaCurrais>();
        if (entradaCurrais != null && entradaCurrais.SoltarItemNoCenario(item, eventData.position))
            return;

        // Usa os métodos existentes da jornada.
        SegundaParada parada = FindAnyObjectByType<SegundaParada>();

        if (parada != null)
        {
            if (parada.SoltarNoRiacho(item, eventData.position))
                return;

            if (parada.SoltarNaBarriguda(item, eventData.position))
                return;

            if (parada.SoltarCurativo(item, eventData.position))
                return;

            if (parada.SoltarNoAcampamento(item, eventData.position))
                return;
        }

        // Soltar no restante do inventário não usa o item no cenário.
        InventoryUI inventoryUI = GetComponentInParent<InventoryUI>();

        RectTransform inventoryRect = inventoryUI != null
            ? inventoryUI.GetComponentInParent<RectTransform>()
            : null;

        if (inventoryRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                inventoryRect,
                eventData.position,
                eventData.pressEventCamera))
        {
            return;
        }

        Camera cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
            return;

        Vector2 worldPoint =
            cameraPrincipal.ScreenToWorldPoint(eventData.position);

        foreach (Collider2D hit in Physics2D.OverlapPointAll(worldPoint))
        {
            CacaPassarinho passarinho =
                hit.GetComponent<CacaPassarinho>();

            if (passarinho != null && passarinho.TentarCacar(item))
                return;

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
        InventoryManager manager = InventoryManager.Instance;

        if (manager == null)
            return;

        InventorySlot[] slots =
            FindObjectsByType<InventorySlot>(FindObjectsInactive.Include);

        foreach (InventorySlot slot in slots)
        {
            if (slot.highlight != null)
            {
                slot.highlight.SetActive(
                    manager.SelectedItem == slot.item
                );
            }
        }
    }
}