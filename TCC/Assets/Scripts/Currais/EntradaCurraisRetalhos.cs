using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Lavagem — puzzle de retalhos no Canvas")]
    [SerializeField] private PuzzleRetalhos puzzleRetalhos;

    private IEnumerator RemendarPanoLavagem()
    {
        if (Dados.curraisEtapaBusca != 2 || Dados.curraisPanosSeparados == 3) yield break;
        FecharDialogo();
        InventoryTabController inventario = InventoryTabController.Instance;
        bool inventarioHabilitado = inventario != null && inventario.enabled;
        if (inventario != null)
        {
            inventario.Close();
            inventario.enabled = false;
        }
        bool concluido = false;
        try
        {
            puzzleRetalhos.Abrir();
            while (puzzleRetalhos.Aberto) yield return null;
            concluido = puzzleRetalhos.Concluido;
        }
        finally
        {
            if (puzzleRetalhos != null) puzzleRetalhos.Fechar();
            if (inventario != null) inventario.enabled = inventarioHabilitado;
            quadroBloqueado = Time.frameCount;
            InvestigationGuard.BlockCurrentClick();
        }
        if (!concluido) yield break;
        // Mantém o contrato de progresso antigo: os dois lotes estão resolvidos.
        Dados.curraisPanosSeparados = 3;
        RegistrarConversaEntrada("lavagem-retalhos-concluidos");
        GarantirPanosLavagem();
        AtualizarEstadoBusca(); Salvar();
        yield return ConcluirLavagemLinear();
    }
}
