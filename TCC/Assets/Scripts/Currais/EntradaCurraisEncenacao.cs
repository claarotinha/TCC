using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Entrada — imagens e gestos editáveis")]
    [SerializeField] private PoseEntradaCurrais poseIrmaChamando, poseMaeAmparando, posePaiFerido, poseGuardaBloqueio;
    [SerializeField] private Transform maeAmparoEntrada;
    [SerializeField] private GameObject detalheIrmaFerida, detalheIrmaCurativo;
    [SerializeField] private Fala[] chegadaFamilia;

    private bool ReferenciasEncenacaoValidas()
    {
        return ConferirReferencia(poseIrmaChamando, nameof(poseIrmaChamando)) &
            ConferirReferencia(poseMaeAmparando, nameof(poseMaeAmparando)) &
            ConferirReferencia(posePaiFerido, nameof(posePaiFerido)) &
            ConferirReferencia(poseGuardaBloqueio, nameof(poseGuardaBloqueio)) &
            ConferirReferencia(maeAmparoEntrada, nameof(maeAmparoEntrada)) &
            ConferirReferencia(detalheIrmaFerida, nameof(detalheIrmaFerida)) &
            ConferirReferencia(detalheIrmaCurativo, nameof(detalheIrmaCurativo)) &
            FalasValidas(chegadaFamilia);
    }

    private void PrepararEncenacaoEntrada()
    {
        // Todos os checkpoints após a separação preservam a lesão do pai.
        if (Dados.curraisEtapaEntrada >= 2) Dados.curraisPaiFerido = true;
        OcultarGestosDaEntrada();
        AtualizarFerimentoEntrada();
    }

    private void MostrarChamadoDaIrma(bool mostrar)
    {
        poseIrmaChamando.DefinirVisivel(mostrar);
        detalheIrmaFerida.SetActive(mostrar && Dados.estradaMariquinhaFerida && !Dados.estradaCurativoAplicado);
        detalheIrmaCurativo.SetActive(mostrar && Dados.estradaMariquinhaFerida && Dados.estradaCurativoAplicado);
    }

    private IEnumerator AmpararPaiNaEntrada()
    {
        yield return Mover(mae, maeAmparoEntrada.position, tempoPlano);
        poseMaeAmparando.DefinirVisivel(true);
    }

    private void AtualizarFerimentoEntrada()
    {
        if (posePaiFerido != null && GameProgress.Instance != null)
            posePaiFerido.DefinirVisivel(Dados.curraisPaiFerido && !planoMemoriaAtivo && !encerrando);
    }

    private void OcultarGestosDaEntrada()
    {
        if (poseIrmaChamando != null) poseIrmaChamando.DefinirVisivel(false);
        if (poseMaeAmparando != null) poseMaeAmparando.DefinirVisivel(false);
        if (poseGuardaBloqueio != null) poseGuardaBloqueio.DefinirVisivel(false);
        if (detalheIrmaFerida != null) detalheIrmaFerida.SetActive(false);
        if (detalheIrmaCurativo != null) detalheIrmaCurativo.SetActive(false);
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
