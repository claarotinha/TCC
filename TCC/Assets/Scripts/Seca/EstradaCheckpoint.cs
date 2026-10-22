using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EstradaCheckpoint : MonoBehaviour
{
    [SerializeField] private ItemData[] requiredItems = new ItemData[5];

    private IEnumerator Start()
    {
        // Continuar restaura o arquivo existente; não deve sobrescrevê-lo.
        bool restoring = GameSaveSystem.IsRestoring;
        yield return null;
        if (restoring || GameSaveSystem.IsRestoring)
            yield break;

        GameProgress progress = GameProgress.Instance;
        if (progress == null || !progress.Data.tutorialCompleted)
        {
            Debug.LogWarning("EstradaCheckpoint: teste chegando pela Seca, após o tutorial.", this);
            yield break;
        }
        if (!progress.Data.secaMealCompleted || progress.Data.secaFoodChoice < 1 ||
            progress.Data.secaFoodChoice > 3 || !ValidateItems())
        {
            Debug.LogError("EstradaCheckpoint: confira a escolha da refeição e os cinco itens antes de partir.", this);
            yield break;
        }
        Scene scene = gameObject.scene;
        if (SceneManager.GetActiveScene() != scene ||
            !Application.CanStreamedLevelBeLoaded(scene.name))
            yield break;

        GameSaveSystem save = progress.GetComponent<GameSaveSystem>();
        if (save == null || !save.TrySaveCheckpoint(scene.name))
            Debug.LogError("EstradaCheckpoint: a chegada não foi salva. Confira o Console.", this);
    }

    private bool ValidateItems()
    {
        if (requiredItems == null || requiredItems.Length != 5 ||
            InventoryManager.Instance == null)
            return false;
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == null)
                return false;
            for (int j = 0; j < i; j++)
                if (requiredItems[j] == requiredItems[i])
                    return false;
            bool found = false;
            foreach (ItemData item in InventoryManager.Instance.Items)
                if (item == requiredItems[i])
                {
                    found = true;
                    break;
                }
            if (!found)
                return false;
        }
        return true;
    }
}
