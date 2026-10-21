using UnityEngine;

[RequireComponent(typeof(ItemDiscovery))]
public class SecaPreparationItem : MonoBehaviour
{
    [Header("Pedido da mãe")]
    [SerializeField] private SecaPreparationRequest preparationRequest;

    private ItemDiscovery discovery;
    private bool released;
    private int requestCompletedFrame = -1;

    private void Awake()
    {
        discovery = GetComponent<ItemDiscovery>();
        discovery.enabled = false;
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
    }
}
