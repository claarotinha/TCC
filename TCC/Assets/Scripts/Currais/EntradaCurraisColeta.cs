using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    private bool coletaDoCenarioConfirmada;

    private IEnumerator ExaminarOuColetar(ItemData item)
    {
        coletaDoCenarioConfirmada = false;
        CollectPrompt prompt = InventoryTabController.Instance != null
            ? InventoryTabController.Instance.GetComponent<CollectPrompt>() : null;
        if (prompt == null || item == null) yield break;
        // CollectPrompt.Confirm já adiciona uma unidade antes do callback.
        // O chamador apenas registra o progresso; não adiciona o item novamente.
        if (!prompt.Show(item, () => coletaDoCenarioConfirmada = true)) yield break;
        while (prompt.IsOpen) yield return null;
    }
}
