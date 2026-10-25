using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EstradaCheckpoint : MonoBehaviour
{
    [Header("Os cinco itens levados da casa")]
    [SerializeField] private ItemData[] requiredItems = new ItemData[5];

    private IEnumerator Start()
    {
        // Não sobrescreve o arquivo ao carregar pelo Continuar.
        bool enteredByContinue = GameSaveSystem.IsRestoring;

        // Aguarda a inicialização do inventário e dos objetos da cena.
        yield return null;

        if (enteredByContinue || GameSaveSystem.IsRestoring)
            yield break;

        GameProgress progress = GameProgress.Instance;

        if (progress == null || !progress.Data.tutorialCompleted)
        {
            Debug.LogWarning(
                "EstradaCheckpoint: teste chegando pela Seca, " +
                "com o tutorial concluído.",
                this
            );
            yield break;
        }

        if (!progress.Data.secaMealCompleted ||
            progress.Data.secaFoodChoice < 1 ||
            progress.Data.secaFoodChoice > 3)
        {
            Debug.LogError(
                "EstradaCheckpoint: a escolha da refeição " +
                "não foi registrada. Confira o SecaMealProgress.",
                this
            );
            yield break;
        }

        if (!ValidateItems())
            yield break;

        Scene scene = gameObject.scene;

        if (SceneManager.GetActiveScene() != scene ||
            !Application.CanStreamedLevelBeLoaded(scene.name))
        {
            Debug.LogError(
                "EstradaCheckpoint: confira se a Estrada está " +
                "incluída na lista de cenas do build.",
                this
            );
            yield break;
        }

        GameSaveSystem saveSystem =
            progress.GetComponent<GameSaveSystem>();

        if (saveSystem == null ||
            !saveSystem.TrySaveCheckpoint(scene.name))
        {
            Debug.LogError(
                "EstradaCheckpoint: não foi possível salvar. " +
                "Confira as mensagens anteriores no Console.",
                this
            );
        }
    }

    private bool ValidateItems()
    {
        if (requiredItems == null || requiredItems.Length != 5 ||
            InventoryManager.Instance == null)
        {
            Debug.LogError(
                "EstradaCheckpoint: configure os cinco itens " +
                "e confira se existe um InventoryManager.",
                this
            );
            return false;
        }

        for (int i = 0; i < requiredItems.Length; i++)
        {
            ItemData required = requiredItems[i];

            if (required == null)
            {
                Debug.LogError(
                    "EstradaCheckpoint: existe um campo de item vazio.",
                    this
                );
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                if (requiredItems[j] == required)
                {
                    Debug.LogError(
                        "EstradaCheckpoint: os cinco itens " +
                        "precisam ser diferentes.",
                        this
                    );
                    return false;
                }
            }

            bool found = false;

            foreach (ItemData item in InventoryManager.Instance.Items)
            {
                if (item == required)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogError(
                    "EstradaCheckpoint: falta no inventário o item " +
                    required.name + ".",
                    this
                );
                return false;
            }
        }

        return true;
    }
}