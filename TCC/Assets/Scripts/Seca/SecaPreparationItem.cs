using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ItemDiscovery))]
public class SecaPreparationItem : MonoBehaviour
{
    [Header("Pedido da mãe")]
    [SerializeField] private SecaPreparationRequest preparationRequest;

    private static readonly HashSet<SecaPreparationItem> activeItems = new HashSet<SecaPreparationItem>();
    private void OnEnable() => activeItems.Add(this);
    private void OnDisable() => activeItems.Remove(this);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetItems() => activeItems.Clear();

    public static bool HasCollectableAt(Vector2 point)
    {
        foreach (SecaPreparationItem item in activeItems)
            if (item != null && item.released && item.discovery != null && item.discovery.enabled &&
                item.itemCollider != null && item.itemCollider.enabled && item.itemCollider.OverlapPoint(point)) return true;
        return false;
    }

    private ItemDiscovery discovery;
    private Collider2D itemCollider;
    private SpriteRenderer itemRenderer;
    private bool released;
    private int requestCompletedFrame = -1;

    private void Awake()
    {
        discovery = GetComponent<ItemDiscovery>();
        discovery.enabled = false;
        itemCollider = GetComponent<Collider2D>();
        itemRenderer = GetComponent<SpriteRenderer>();
        if (itemCollider != null) itemCollider.enabled = false;
        if (itemRenderer != null) itemRenderer.enabled = false;
    }

    private void Start()
    {
        if (preparationRequest != null)
            return;

        Debug.LogError(
            "SecaPreparationItem: configure o pedido da mãe em " + name + ".",
            this
        );

        enabled = false;
    }

    private void Update()
    {
        if (released || preparationRequest == null ||
            !preparationRequest.RequestCompleted)
        {
            return;
        }

        // O clique que fecha a fala não pode coletar um item atrás do painel.
        if (requestCompletedFrame < 0)
        {
            requestCompletedFrame = Time.frameCount;
            return;
        }

        if (Time.frameCount <= requestCompletedFrame)
            return;

        released = true;
        discovery.enabled = true;
        if (itemCollider != null) itemCollider.enabled = true;
        if (itemRenderer != null) itemRenderer.enabled = true;
    }
}
