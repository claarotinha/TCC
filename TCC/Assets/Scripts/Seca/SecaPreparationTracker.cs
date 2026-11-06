using TMPro;
using UnityEngine;

public class SecaPreparationTracker : MonoBehaviour
{
    [Header("Pedido da mãe")]
    [SerializeField] private SecaPreparationRequest preparationRequest;

    [Header("Itens necessários")]
    [SerializeField] private ItemData[] requiredItems = new ItemData[3];

    [Header("Objetivo na tela")]
    [SerializeField] private TMP_Text objectiveText;

    public int CollectedCount { get; private set; }

    public bool AllItemsCollected =>
        configured &&
        preparationRequest != null &&
        preparationRequest.RequestCompleted &&
        CollectedCount == requiredItems.Length;

    private bool configured;
    private bool requestWasCompleted;

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged += Refresh;

        if (configured)
            Refresh();
    }

    private void Start()
    {
        configured =
            preparationRequest != null &&
            requiredItems != null &&
            requiredItems.Length > 0;

        if (configured)
        {
            for (int i = 0; i < requiredItems.Length; i++)
            {
                if (requiredItems[i] == null)
                {
                    configured = false;
                    break;
                }

                for (int j = 0; j < i; j++)
                {
                    if (requiredItems[i] == requiredItems[j])
                        configured = false;
                }
            }
        }

        if (!configured)
        {
            if (objectiveText != null)
                objectiveText.gameObject.SetActive(false);

            Debug.LogError(
                "SecaPreparationTracker: configure o pedido " +
                "e os ItemData distintos.",
                this
            );

            return;
        }

        Refresh();
    }

    private void Update()
    {
        if (!configured)
            return;

        bool completed = preparationRequest.RequestCompleted;

        if (completed != requestWasCompleted)
            Refresh();
    }

    public void HideObjective()
    {
        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        if (!configured)
            return;

        requestWasCompleted = preparationRequest.RequestCompleted;
        if (objectiveText != null) objectiveText.gameObject.SetActive(false);

        CollectedCount = 0;

        InventoryManager inventory = InventoryManager.Instance;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (inventory == null)
                continue;

            foreach (ItemData current in inventory.Items)
            {
                if (current != requiredItems[i])
                    continue;

                CollectedCount++;
                break;
            }
        }

    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= Refresh;

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }
}