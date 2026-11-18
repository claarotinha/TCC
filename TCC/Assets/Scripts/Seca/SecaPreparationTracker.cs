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
        // O pedido e o contador pertencem ao mesmo objeto na Seca.
        // Também aceita o pedido em outro objeto da mesma cena.
        if (preparationRequest == null)
            preparationRequest = GetComponent<SecaPreparationRequest>();

        if (preparationRequest == null)
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                SecaPreparationRequest[] requests =
                    root.GetComponentsInChildren<SecaPreparationRequest>(true);
                foreach (SecaPreparationRequest request in requests)
                {
                    if (preparationRequest != null)
                    {
                        ReportConfigurationError("há mais de um pedido na cena; selecione o correto no Inspector.");
                        return;
                    }
                    preparationRequest = request;
                }
            }
        }

        if (preparationRequest == null)
        {
            ReportConfigurationError("o pedido da mãe não foi encontrado nesta cena.");
            return;
        }

        if (!ItemsAreValid())
            RestorePreparationItems();

        if (!ItemsAreValid())
        {
            ReportConfigurationError("faltam a cabaça vazia, o facão ou o pano no catálogo de itens.");
            return;
        }

        configured = true;
        Refresh();
    }

    private bool ItemsAreValid()
    {
        if (requiredItems == null || requiredItems.Length != 3)
            return false;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == null)
                return false;
            for (int j = 0; j < i; j++)
                if (requiredItems[i] == requiredItems[j])
                    return false;
        }
        return true;
    }

    private void RestorePreparationItems()
    {
        InventorySaveCatalog catalog =
            Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalog == null)
            return;

        // Identificadores estáveis dos mesmos assets usados pelos coletáveis.
        // Não cria cópias de ItemData, pois o inventário compara suas referências.
        if (catalog.TryGetItem("f0c6e92250400ab42871c53f7ebf8161", out ItemData cabaca) &&
            catalog.TryGetItem("479344094161c0740a7c1d933ed41f9b", out ItemData facao) &&
            catalog.TryGetItem("290faf3e464dc4c4f97c0f50675e5395", out ItemData pano))
        {
            requiredItems = new[] { cabaca, facao, pano };
        }
    }

    private void ReportConfigurationError(string detail)
    {
        configured = false;
        HideObjective();
        Debug.LogError("SecaPreparationTracker: " + detail, this);
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