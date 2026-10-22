using System.Text;
using TMPro;
using UnityEngine;

public class SecaPreparationTracker : MonoBehaviour
{
    [Header("Pedido da mãe")]
    [SerializeField] private SecaPreparationRequest preparationRequest;

    [Header("Cinco itens necessários")]
    [SerializeField] private ItemData[] requiredItems = new ItemData[5];

    [Header("Objetivo na tela")]
    [SerializeField] private TMP_Text objectiveText;

    public int CollectedCount { get; private set; }

    public bool AllItemsCollected =>
        configured &&
        preparationRequest != null &&
        preparationRequest.RequestCompleted &&
        CollectedCount == 5;

    private bool configured;
    private bool objectiveHidden;
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
            objectiveText != null &&
            requiredItems != null &&
            requiredItems.Length == 5;

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
                "SecaPreparationTracker: configure o pedido, o texto " +
                "e cinco ItemData distintos.",
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
        objectiveHidden = true;

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        if (!configured)
            return;

        requestWasCompleted = preparationRequest.RequestCompleted;
        objectiveText.gameObject.SetActive(requestWasCompleted && !objectiveHidden);

        CollectedCount = 0;
        bool[] found = new bool[requiredItems.Length];

        InventoryManager inventory = InventoryManager.Instance;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (inventory == null)
                continue;

            foreach (ItemData current in inventory.Items)
            {
                if (current != requiredItems[i])
                    continue;

                found[i] = true;
                CollectedCount++;
                break;
            }
        }

        StringBuilder text = new StringBuilder();

        text.Append("Preparativos: ");
        text.Append(CollectedCount);
        text.AppendLine("/5");

        for (int i = 0; i < requiredItems.Length; i++)
        {
            text.Append(found[i] ? "[OK] " : "[ ] ");
            text.AppendLine(requiredItems[i].itemName);
        }

        if (AllItemsCollected)
        {
            text.AppendLine();
            text.Append("Peguei tudo. Vou avisar a mainha.");
        }

        objectiveText.text = text.ToString();
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= Refresh;

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }
}