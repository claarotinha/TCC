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
        // Recupera somente o pano da tarefa interrompida; nunca altera a tira de Raimundo.
        if (Dados.curraisEtapaBusca == 4 && Dados.curraisTrabalhoPatioIniciado &&
            !Dados.curraisPanosPatioEntregues && !TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
        AtualizarSocorro();
    }

    private string ObjetivoSocorro() => string.Empty;

    private void AtualizarSocorro()
    {
        bool patio = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 4;
        bool socorro = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 5;
        areaAguaPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado;
        areaPanosPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado;
        areaSocorroBento.enabled = socorro;
        cabacaSocorro.SetActive(socorro && (Dados.curraisItensAtendimentoBento & 2) != 0 && !Dados.curraisCabacaSocorroRecolhida);
        areaCabacaSocorro.enabled = cabacaSocorro.activeSelf && Dados.curraisBentoMorto;
        bool panoDisponivel = Dados.curraisPanosPatioEntregues &&
            !ViuConversaEntrada("patio-pano-retomado") && !(socorro && TemItem(panoAuxilio));
        panosEntreguesPatio.SetActive(panoDisponivel);
        areaPanoLimpoApoio.enabled = PanoDoApoioDisponivel;
        if ((patio || socorro) && Dados.curraisEscolhaAgua == 1) areaMae.enabled = true;
        areaPanoAtendimento.gameObject.SetActive(socorro && Dados.curraisBentoMorto && PrecisaRecolherPanoSocorro);
        AtualizarDetalhesPatio();
    }

    private IEnumerator PrepararAguaPatio()
    {
        if (Dados.curraisEtapaBusca != 4 || !ApoioPatioAutorizado || Dados.curraisAguaPatioPreparada ||
            !TemItem(cabacaVaziaBusca) || InventoryManager.Instance.SelectedItem != cabacaVaziaBusca) yield break;
        InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
        InventoryManager.Instance.AddItem(cabacaCheiaBusca);
        Dados.curraisAguaPatioPreparada = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(patioAguaRecebida);
    }

    private IEnumerator EntregarPanosPatio()
    {
        if (Dados.curraisEtapaBusca != 4 || !ApoioPatioAutorizado || Dados.curraisPanosPatioEntregues ||
            !TemItem(panoAuxilio) || InventoryManager.Instance.SelectedItem != panoAuxilio) yield break;
        Dados.curraisPanosPatioEntregues = true;
        InventoryManager.Instance.RemoveItem(panoAuxilio);
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(patioPanoEntregue);
    }

    private IEnumerator UsarNoSocorro()
    {
        ItemData selecionado = InventoryManager.Instance.SelectedItem;
        if (!Dados.curraisBentoMorto)
        {
            if (Dados.curraisItensAtendimentoBento == 3)
            { yield return ConcluirAtendimentoBento(); yield break; }
            if (selecionado == panoAuxilio && TemItem(panoAuxilio) && (Dados.curraisItensAtendimentoBento & 1) == 0)
            {
                Dados.curraisItensAtendimentoBento |= 1;
                InventoryManager.Instance.RemoveItem(panoAuxilio);
                AtualizarEstadoBusca(); Salvar();
                yield return Dizer(new[] { new Fala { personagem = NomeAcompanhantePatio, texto = "Segure o pano aqui. Isso. Deixe os homens atenderem ele." } });
            }
            else if (selecionado == cabacaCheiaBusca && TemItem(cabacaCheiaBusca) && (Dados.curraisItensAtendimentoBento & 2) == 0)
            {
                Dados.curraisItensAtendimentoBento |= 2;
                InventoryManager.Instance.RemoveItem(cabacaCheiaBusca);
                AtualizarEstadoBusca(); Salvar();
                yield return Dizer(new[] { new Fala { personagem = NomeAcompanhantePatio, texto = "Deixe a água com nós, Josefa. Obrigada." } });
            }
            else
            {
                yield return Dizer(EstadoSocorroBento());
                yield break;
            }
            if (Dados.curraisItensAtendimentoBento != 3) yield break;
            yield return ConcluirAtendimentoBento();
            yield break;
        }
        if (!Dados.curraisPanoSepultamentoEntregue)
        {
            if (selecionado != panoAuxilio || !TemItem(panoAuxilio))
            {
                yield return Dizer(pertencesSemPano);
                yield break;
            }
            Dados.curraisPanoSepultamentoEntregue = true;
            InventoryManager.Instance.RemoveItem(panoAuxilio);
            AtualizarEstadoBusca(); Salvar();
            yield return Dizer(despedidaPatio);
        }
        if (Dados.curraisCabacaSocorroRecolhida) yield return Dizer(despedidaPronta);
    }

    private IEnumerator ConcluirAtendimentoBento()
    {
        if (atendimentoBentoEmCurso || Dados.curraisBentoMorto || (Dados.curraisItensAtendimentoBento & 3) != 3)
            yield break;
        atendimentoBentoEmCurso = true;
        SpriteRenderer cobertura = corpoCoberto.GetComponent<SpriteRenderer>();
        Color corCobertura = cobertura.color;
        try
        {
            yield return Dizer(atendimentoFalas);
            yield return Escurecer(1f);
            Dados.curraisBentoMorto = true;
            GameProgress.Instance.UnlockDiaryPage("currais-bento");
            AtualizarEstadoBusca(); Salvar();
            cobertura.color = new Color(corCobertura.r, corCobertura.g, corCobertura.b, 0f);
            yield return Esperar(tempoPlano);
            yield return Escurecer(0f);
            yield return MostrarCoberturaBento(corCobertura);
            yield return Dizer(noticiaBento);
            yield return Dizer(pertencesSemPano);
        }
        finally { cobertura.color = corCobertura; atendimentoBentoEmCurso = false; }
    }

    private IEnumerator RecolherCabacaSocorro()
    {
        if (!Dados.curraisBentoMorto) { yield return Dizer(socorroFaltaPano); yield break; }
        if (Dados.curraisCabacaSocorroRecolhida) { yield return Dizer(cabacaGuardada); yield break; }
        yield return ExaminarOuColetar(cabacaVaziaBusca);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisCabacaSocorroRecolhida = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(cabacaGuardada);
        if (Dados.curraisPanoSepultamentoEntregue) yield return Dizer(despedidaPronta);
    }

    private IEnumerator SepultarBento()
    {
        if (Dados.curraisSepultamentoConcluido || sepultamentoEmCurso) yield break;
        if (!Dados.curraisBentoMorto || !Dados.curraisPanoSepultamentoEntregue || !Dados.curraisCabacaSocorroRecolhida)
        { yield return Dizer(EstadoSocorroBento()); yield break; }
        sepultamentoEmCurso = true;
        try
        {
            yield return Escurecer(1f);
            sepultamentoNaTela = true;
            planoSepultamento.SetActive(true);
            corpoNoPlano.SetActive(true);
            cruzDeBento.SetActive(false);
            AtualizarCompanhiaSepultamento();
            objetivoTexto.gameObject.SetActive(false);
            yield return Escurecer(0f);
            yield return Dizer(sepultamentoFalas);
            yield return BaixarBentoNoPlano();
            yield return Esperar(tempoPlano);
            yield return Dizer(retornoSepultamento);
            yield return ChapterCard.Show("josefa-09");
            yield return Escurecer(1f);
            RestaurarPlanoSepultamento();
            Dados.curraisSepultamentoConcluido = true;
            AvancarCampo(6);
            yield return Escurecer(0f);
        }
        finally
        {
            sepultamentoEmCurso = false;
            RestaurarPlanoSepultamento();
            if (objetivoTexto != null) objetivoTexto.gameObject.SetActive(false);
        }
    }

    private IEnumerator AjudarNoQuarto()
    {
        if (emprestandoNoQuarto) yield break;
        if (Dados.curraisAjudouAtendimentoQuarto) { yield return Dizer(devolucaoQuarto); yield break; }
        if (!TemItem(cabacaVaziaBusca)) { yield return Dizer(quartoSemCabaca); yield break; }
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

    private void RestaurarPlanoSepultamento()
    {
        if (planoSepultamento != null) planoSepultamento.SetActive(false);
        if (sepultamentoNaTela && objetivoTexto != null) objetivoTexto.gameObject.SetActive(false);
        sepultamentoNaTela = false;
    }
}

