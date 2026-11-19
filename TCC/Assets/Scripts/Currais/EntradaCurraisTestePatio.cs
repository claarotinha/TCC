using UnityEngine;

// Atalho temporário isolado. Remover este arquivo e as duas chamadas em Start
// elimina o teste; desmarcar a opção no Inspector já restaura o fluxo normal.
public partial class EntradaCurrais
{
    [Header("Teste temporário — começar com o pai e Bento")]
    [Tooltip("Somente no Editor. Inicia no pátio com cabaça, pano do pai e retalho. Não substitui um Continuar nem grava sobre a partida salva.")]
    [SerializeField] private bool iniciarNoPatioParaTeste;

    private bool PrepararTestePatio(bool restaurandoPartida)
    {
        if (!Application.isEditor || !iniciarNoPatioParaTeste || restaurandoPartida)
            return false;

        // Uma sessão nova em memória evita misturar etapas posteriores com o pátio.
        // CanSave permanece falso: o save real não é sobrescrito por este atalho.
        GameProgress.Instance.ResetProgress();
        Dados.ato1Concluido = true;
        Dados.curraisEtapaEntrada = 4;
        Dados.curraisRevistaApresentada = true;
        Dados.curraisPertencesRetidos = true;
        Dados.curraisAbrigoApresentado = true;
        Dados.curraisLuziaApresentada = true;
        Dados.curraisFamiliaParou = true;
        Dados.curraisPaiAcomodado = true;
        Dados.curraisPaiFerido = true;
        Dados.curraisDistribuicaoIniciada = true;
        Dados.curraisAguaOrientada = true;
        Dados.curraisAguaRecebida = true;
        Dados.curraisCabacaDevolvida = true;
        Dados.curraisEscolhaAgua = 2; // Luzia acompanha este teste.
        Dados.curraisRegistroConcluido = true;
        Dados.curraisTarefaPanosIniciada = true;
        Dados.curraisLotePanosRecolhido = true;
        Dados.curraisLotePanosEntregue = true;
        Dados.curraisPanelaConcluida = true;
        Dados.curraisPanosSeparados = 3;
        Dados.curraisTiraPanoRecolhida = true;
        Dados.curraisPassagemVigiadaConcluida = true;
        Dados.curraisRaimundoConhecido = true;
        Dados.curraisEtapaBusca = 4;
        RegistrarConversaEntrada("raimundo-encontro-encerrado");
        InventoryManager.Instance.ReplaceItems(new[] { cabacaVaziaBusca, panoAuxilio, tiraPanoLavagem });
        Debug.Log("Teste do pátio ativo: fale com o pai para iniciar Bento. Cabaça, pano e retalho no inventário. Salvamento desativado nesta sessão.", this);
        return true;
    }

    private void PosicionarInicioTestePatio()
    {
        Vector3 posicao = mari.transform.position;
        posicao.x = paiTrabalhoMarca.position.x - alcanceChegadaPatio - 1f;
        mari.transform.position = posicao;
        Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.position = posicao;
            corpo.linearVelocity = Vector2.zero;
        }
        Physics2D.SyncTransforms();
        if (seguirCamera != null) seguirCamera.RestaurarEnquadramentoDoAlvo();
        CorFade(0f);
    }
}
