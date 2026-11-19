using System.Collections;
using UnityEngine;

// Cenas 11–14: trabalho, ajuda concreta e despedida, sem um minijogo de cura.
public partial class EntradaCurrais
{
    [Header("Pátio — tarefas antes do atendimento")]
    [SerializeField] private Collider2D areaAguaPatio, areaPanosPatio, areaSocorroBento, areaCabacaSocorro;
    [SerializeField] private ItemData panoAuxilio;
    [SerializeField] private GameObject cabacaSocorro, panosEntreguesPatio;
    [SerializeField] private Fala[] tarefaPatioFalas, aguaPatioFalas, despedidaPatio, sepultamentoFalas, retornoSepultamento;
    [SerializeField] private Fala[] emprestimoQuarto, devolucaoQuarto;

    [Header("Plano estático do sepultamento — mesma cena")]
    [SerializeField] private GameObject planoSepultamento, corpoNoPlano;
    [SerializeField] private GameObject quadroFuneralProvisorio;
    private bool sepultamentoNaTela;

    private bool ReferenciasSocorroValidas()
    {
        UnityEngine.Object[] refs = { areaAguaPatio, areaPanosPatio, areaSocorroBento, areaCabacaSocorro,
            panoAuxilio, cabacaSocorro, panosEntreguesPatio, planoSepultamento, corpoNoPlano };
        string[] nomes = { nameof(areaAguaPatio), nameof(areaPanosPatio), nameof(areaSocorroBento), nameof(areaCabacaSocorro),
            nameof(panoAuxilio), nameof(cabacaSocorro), nameof(panosEntreguesPatio), nameof(planoSepultamento), nameof(corpoNoPlano) };
        bool ok = true;
        for (int i = 0; i < refs.Length; i++) ok &= ConferirReferencia(refs[i], nomes[i]);
        foreach (Fala[] falas in new[] { tarefaPatioFalas, aguaPatioFalas, despedidaPatio, sepultamentoFalas, retornoSepultamento, emprestimoQuarto, devolucaoQuarto })
            ok &= FalasValidas(falas);
        return ok && ReferenciasPatioDetalhadoValidas();
    }

    private bool PrecisaRecolherPanoSocorro => !TemItem(panoAuxilio) &&
        (!Dados.curraisBentoMorto ? (Dados.curraisItensAtendimentoBento & 1) == 0 : !Dados.curraisPanoSepultamentoEntregue);

    private void PrepararSocorro()
    {
        planoSepultamento.SetActive(false);
        if (Dados.curraisEtapaBusca >= 6)
        {
            Dados.curraisBentoMorto = true;
            Dados.curraisSepultamentoConcluido = true;
        }
        if (Dados.curraisEtapaBusca == 5)
        {
            Dados.curraisTrabalhoPatioIniciado = true;
            Dados.curraisAguaPatioPreparada = true;
            Dados.curraisPanosPatioEntregues = true;
            if ((Dados.curraisItensAtendimentoBento & 2) == 0 && !TemItem(cabacaCheiaBusca))
            {
                if (TemItem(cabacaVaziaBusca)) InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
                InventoryManager.Instance.AddItem(cabacaCheiaBusca);
            }
        }
        // Compatibilidade: o fluxo antigo retirava o pano ao entregá-lo no apoio.
        // Antes do atendimento, restaura apenas a unidade ainda não usada em Bento.
        if ((Dados.curraisEtapaBusca == 4 && Dados.curraisTrabalhoPatioIniciado) ||
            (Dados.curraisEtapaBusca == 5 && !Dados.curraisBentoMorto &&
             (Dados.curraisItensAtendimentoBento & 1) == 0))
        {
            if (!TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
            Dados.curraisPanosPatioEntregues = true;
        }
        // Recupera somente o pano da tarefa interrompida; nunca altera a tira de Raimundo.
        if (Dados.curraisEtapaBusca == 4 && Dados.curraisTrabalhoPatioIniciado &&
            !Dados.curraisPanosPatioEntregues && !TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
        // Migra saves em que a água já foi usada, mas a cabaça ainda estava no chão.
        if (Dados.curraisEtapaBusca == 5 && (Dados.curraisItensAtendimentoBento & 2) != 0)
        {
            if (!TemItem(cabacaVaziaBusca) && !TemItem(cabacaCheiaBusca))
                InventoryManager.Instance.AddItem(cabacaVaziaBusca);
            Dados.curraisCabacaSocorroRecolhida = true;
        }
        PrepararAtendimentoLinear();
        AtualizarSocorro();
    }

    private string ObjetivoSocorro() => string.Empty;

    private void AtualizarSocorro()
    {
        bool patio = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 4;
        bool socorro = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 5;
        areaAguaPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado;
        areaPanosPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado && !Dados.curraisPanosPatioEntregues;
        areaSocorroBento.enabled = socorro;
        cabacaSocorro.SetActive(false);
        areaCabacaSocorro.enabled = false;
        panosEntreguesPatio.SetActive(false);
        areaPanoLimpoApoio.enabled = false;
        if (patio || socorro) areaMae.enabled = true;
        areaPanoAtendimento.gameObject.SetActive(false);
        AtualizarDetalhesPatio();
        AtualizarVisuaisBentoLinear();
    }

    private IEnumerator PrepararAguaPatio()
    {
        if (Dados.curraisEtapaBusca != 4 || !ApoioPatioAutorizado || Dados.curraisAguaPatioPreparada ||
            !TemItem(cabacaVaziaBusca)) yield break;
        InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
        InventoryManager.Instance.AddItem(cabacaCheiaBusca);
        Dados.curraisAguaPatioPreparada = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(patioAguaRecebida);
    }

    private IEnumerator EntregarPanoNoApoio()
    {
        if (Dados.curraisEtapaBusca != 4 || !Dados.curraisTrabalhoPatioIniciado ||
            Dados.curraisPanosPatioEntregues || !TemItem(panoAuxilio)) yield break;
        if (!ApoioPatioAutorizado) yield return AutorizarApoioPatioLinear();
        yield return EntregarPanosPatio();
    }

    private IEnumerator EncherCabacaNoApoio()
    {
        if (Dados.curraisEtapaBusca != 4 || !Dados.curraisTrabalhoPatioIniciado ||
            Dados.curraisAguaPatioPreparada || !TemItem(cabacaVaziaBusca)) yield break;
        if (!ApoioPatioAutorizado) yield return AutorizarApoioPatioLinear();
        yield return PrepararAguaPatio();
    }

    private IEnumerator EntregarPanosPatio()
    {
        if (Dados.curraisEtapaBusca != 4 || !ApoioPatioAutorizado || Dados.curraisPanosPatioEntregues ||
            !TemItem(panoAuxilio)) yield break;
        Dados.curraisPanosPatioEntregues = true;
        // O pano fica reservado com Zefinha para o atendimento.
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(patioPanoEntregue);
    }

    private IEnumerator UsarNoSocorro(ItemData selecionado)
    {
        if (Dados.curraisEtapaBusca != 5 || Dados.curraisBentoMorto) yield break;
        bool pano = selecionado == panoAuxilio && TemItem(panoAuxilio) && (Dados.curraisItensAtendimentoBento & 1) == 0;
        bool agua = selecionado == cabacaCheiaBusca && TemItem(cabacaCheiaBusca) && (Dados.curraisItensAtendimentoBento & 2) == 0;
        if (!pano && !agua) yield break;
        yield return ExamineNomesUmaVez("bento-exame-atendimento", exameAtendimentoBento);
        if (!TemItem(selecionado)) yield break;
        Dados.curraisItensAtendimentoBento |= pano ? 1 : 2;
        InventoryManager.Instance.RemoveItem(selecionado);
        if (agua)
        {
            if (!TemItem(cabacaVaziaBusca)) InventoryManager.Instance.AddItem(cabacaVaziaBusca);
            Dados.curraisCabacaSocorroRecolhida = true;
        }
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(pano ? bentoReceberPano : bentoReceberAgua);
        if ((Dados.curraisItensAtendimentoBento & 3) == 3) yield return ConcluirAtendimentoBento();
    }

    private IEnumerator ConcluirAtendimentoBento()
    {
        if (atendimentoBentoEmCurso || Dados.curraisBentoMorto ||
            Dados.curraisEtapaBusca != 5 || (Dados.curraisItensAtendimentoBento & 3) != 3) yield break;
        atendimentoBentoEmCurso = true;
        SpriteRenderer cobertura = corpoCoberto.GetComponent<SpriteRenderer>();
        Color corCobertura = cobertura.color;
        try
        {
            yield return EnquadrarBentoNoPatio();
            yield return DizerEntradaUmaVez("bento-pai-vai-chamar", bentoPaiBuscarFamilia);
            if (!ViuConversaEntrada("bento-pai-foi-buscar"))
            {
                // O trabalhador avisa a família fora de quadro. O pai fica com Josefa.
                FecharDialogo();
                yield return Esperar(1.5f);
                RegistrarConversaEntrada("bento-pai-foi-buscar"); Salvar();
            }
            FecharDialogo();
            yield return Esperar(Mathf.Max(3f, pausaAtendimentoBento));
            yield return DizerEntradaUmaVez("bento-atendimento-parou", bentoAtendimentoParou);
            FecharDialogo();
            yield return Esperar(1.5f);
            yield return DizerEntradaUmaVez("bento-pergunta-levantar",
                bentoPerguntaMae);
            FecharDialogo();
            yield return Esperar(2f);
            yield return DizerEntradaUmaVez("bento-noticia", bentoNoticiaLinear);
            FecharDialogo();
            yield return Esperar(2.5f);
            yield return Escurecer(1f);
            Dados.curraisBentoMorto = true;
            RegistrarConversaEntrada("bento-pai-voltou");
            GameProgress.Instance.UnlockDiaryPage("currais-bento");
            AtualizarEstadoBusca(); Salvar();
            cobertura.color = new Color(corCobertura.r, corCobertura.g, corCobertura.b, 0f);
            yield return Esperar(.5f);
            yield return Escurecer(0f);
            CurraisEffectsDirector.Play("bento", 0.6f);
            yield return MostrarCoberturaBento(corCobertura);
            MostrarReacaoBento();
        }
        finally
        {
            cobertura.color = corCobertura;
            atendimentoBentoEmCurso = false;
        }
    }

    private IEnumerator RecolherCabacaSocorro()
    {
        if (Dados.curraisEtapaBusca != 5 || !Dados.curraisBentoMorto || Dados.curraisCabacaSocorroRecolhida) yield break;
        yield return ExaminarOuColetar(cabacaVaziaBusca);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisCabacaSocorroRecolhida = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(ComAcompanhante(bentoCabacaRecuperada));
    }

    private IEnumerator SepultarBento()
    {
        if (Dados.curraisSepultamentoConcluido || sepultamentoEmCurso || !DespedidaBentoPronta) yield break;
        sepultamentoEmCurso = true;
        try
        {
            RegistrarConversaEntrada("bento-despedida-iniciada"); Salvar();
            yield return DizerEntradaUmaVez("bento-permissao", bentoPermissaoSepultamento);
            yield return DizerEntradaUmaVez("bento-grupo-reunido", bentoReunirGrupo);
            // A transição mostra a reunião do grupo; a acompanhante continua ao lado de Josefa.
            yield return Escurecer(1f);
            CurraisMusicDirector.PlayMoment("funeral");
            sepultamentoNaTela = true;
            planoSepultamento.SetActive(true);
            corpoNoPlano.SetActive(true);
            cruzDeBento.SetActive(false);
            AtualizarCompanhiaSepultamento();
            AjustarCompanhiaBentoNoPlano();
            PrepararImagemFuneralProvisoria();
            objetivoTexto.gameObject.SetActive(false);
            yield return Escurecer(0f);
            yield return Dizer(ComAcompanhante(bentoSepultamentoInicio));
            if (quadroFuneralProvisorio == null) yield return BaixarBentoNoPlano();
            else yield return Esperar(2.5f);
            yield return Dizer(bentoNomeSepultamento);
            yield return Esperar(tempoPlano);
            yield return Dizer(bentoSepultamentoFamilia);
            yield return Dizer(bentoRetornoCruzes);
            yield return ChapterCard.Show("josefa-09");
            yield return Escurecer(1f);
            RestaurarPlanoSepultamento();
            Dados.curraisEscolhaFuneral = 1;
            Dados.curraisSepultamentoConcluido = true;
            AvancarCampo(6);
            yield return Escurecer(0f);
            yield return RetomarBuscaPosto();
        }
        finally
        {
            CurraisMusicDirector.StopMoment("funeral");
            sepultamentoEmCurso = false;
            RestaurarPlanoSepultamento();
            if (objetivoTexto != null) objetivoTexto.gameObject.SetActive(false);
        }
    }

    private IEnumerator AjudarNoQuarto()
    {
        if (emprestandoNoQuarto || !InvestigouQuarto || Dados.curraisAjudouAtendimentoQuarto ||
            !TemItem(cabacaVaziaBusca)) yield break;
        emprestandoNoQuarto = true;
        try
        {
            Dados.curraisCabacaEmprestadaQuarto = true;
            InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
            AtualizarDetalhesPosto(); Salvar();
            yield return Dizer(emprestimoQuarto);
            yield return ServirAguaNoQuarto();
            DevolverCabacaDoQuarto();
            Salvar();
            yield return Dizer(devolucaoQuarto);
        }
        finally { emprestandoNoQuarto = false; }
    }

    private void PrepararImagemFuneralProvisoria()
    {
        if (quadroFuneralProvisorio == null) return;
        // Fundo estático provisório; basta limpar a referência para voltar à encenação.
        foreach (Transform filho in planoSepultamento.transform)
            filho.gameObject.SetActive(filho.gameObject == quadroFuneralProvisorio);
    }

    private void RestaurarPlanoSepultamento()
    {
        if (planoSepultamento != null) planoSepultamento.SetActive(false);
        if (sepultamentoNaTela && objetivoTexto != null) objetivoTexto.gameObject.SetActive(false);
        sepultamentoNaTela = false;
    }
}


