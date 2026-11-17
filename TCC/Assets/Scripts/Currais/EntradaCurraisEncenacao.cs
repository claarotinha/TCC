using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Entrada — animações existentes e marcas editáveis")]
    [SerializeField] private Transform maeAmparoEntrada;
    [SerializeField] private Fala[] chegadaFamilia;

    private bool ReferenciasEncenacaoValidas()
    {
        return ConferirReferencia(maeAmparoEntrada, nameof(maeAmparoEntrada)) &
            FalasValidas(chegadaFamilia);
    }

    private void PrepararEncenacaoEntrada()
    {
        Dados.curraisPaiFerido = true;
    }

    private IEnumerator AmpararPaiNaEntrada()
    {
        yield return Mover(mae, maeAmparoEntrada.position, tempoPlano);
        AnimarEscolta(mae, false);
    }

    private void AnimarSaidaDaFila(bool andando)
    {
        AnimarEscolta(irma, andando);
        AnimarEscolta(guardaRetaguarda, andando);
        foreach (Animator pessoa in outraFila.GetComponentsInChildren<Animator>(true))
        {
            if (!pessoa.enabled) continue;
            pessoa.SetBool("IsWalking", andando);
            pessoa.SetBool("IsRunning", false);
        }
    }
}
