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
        return ok;
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
        if (Dados.curraisEtapaBusca == 5 && !Dados.curraisTrabalhoPatioIniciado)
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

    private string ObjetivoSocorro()
    {
        if (Dados.curraisEtapaBusca == 4)
        {
            if (!Dados.curraisTrabalhoPatioIniciado) return "Aproxime-se do pátio e converse com seu pai.";
            if (!Dados.curraisAguaPatioPreparada) return "Apresente a cabaça vazia no ponto de água dos trabalhadores.";
            if (!Dados.curraisPanosPatioEntregues) return "Selecione o pano de auxílio e entregue no ponto de apoio do pátio.";
            return "Volte até seu pai no pátio.";
        }
        if (!Dados.curraisBentoMorto)
            return "Pegue o pano de auxílio e use-o em Bento. Apresente também a cabaça com água no atendimento.";
        if (!Dados.curraisPanoSepultamentoEntregue) return "Pegue um pano limpo e entregue junto de Bento, para os pertences dele.";
        if (!Dados.curraisCabacaSocorroRecolhida) return "Recolha a cabaça deixada no atendimento.";
        return "Aproxime-se de Bento para acompanhar a despedida.";
    }

    private void AtualizarSocorro()
    {
        bool patio = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 4;
        bool socorro = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca == 5;
        areaAguaPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado && !Dados.curraisAguaPatioPreparada;
        areaPanosPatio.enabled = patio && Dados.curraisTrabalhoPatioIniciado && !Dados.curraisPanosPatioEntregues;
        areaSocorroBento.enabled = socorro;
        cabacaSocorro.SetActive(socorro && (Dados.curraisItensAtendimentoBento & 2) != 0 && !Dados.curraisCabacaSocorroRecolhida);
        areaCabacaSocorro.enabled = cabacaSocorro.activeSelf && Dados.curraisBentoMorto;
        panosEntreguesPatio.SetActive(Dados.curraisPanosPatioEntregues);
    }

    private IEnumerator PrepararAguaPatio()
    {
        if (!TemItem(cabacaVaziaBusca) || InventoryManager.Instance.SelectedItem != cabacaVaziaBusca)
        {
            yield return Dizer(new[] { new Fala { personagem = "Trabalhador", texto =
                "Apresente a cabaça vazia. Esta porção é para os homens do serviço e o atendimento daqui." } });
            yield break;
        }
        InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
        InventoryManager.Instance.AddItem(cabacaCheiaBusca);
        Dados.curraisAguaPatioPreparada = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(aguaPatioFalas);
        if (Dados.curraisPanosPatioEntregues) yield return AgressaoNoPatio();
    }

    private IEnumerator EntregarPanosPatio()
    {
        if (!TemItem(panoAuxilio) || InventoryManager.Instance.SelectedItem != panoAuxilio)
        {
            yield return Dizer(tarefaPatioFalas);
            yield break;
        }
        InventoryManager.Instance.RemoveItem(panoAuxilio);
        Dados.curraisPanosPatioEntregues = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Trabalhador", texto = "Deixe o pano aqui. Obrigado, menina." } });
        if (Dados.curraisAguaPatioPreparada) yield return AgressaoNoPatio();
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
                InventoryManager.Instance.RemoveItem(panoAuxilio);
                Dados.curraisItensAtendimentoBento |= 1;
                AtualizarEstadoBusca(); Salvar();
                yield return Dizer(new[] { new Fala { personagem = "Luzia", texto = "Segure o pano aqui. Isso. Deixe os homens atenderem ele." } });
            }
            else if (selecionado == cabacaCheiaBusca && TemItem(cabacaCheiaBusca) && (Dados.curraisItensAtendimentoBento & 2) == 0)
            {
                InventoryManager.Instance.RemoveItem(cabacaCheiaBusca);
                Dados.curraisItensAtendimentoBento |= 2;
                AtualizarEstadoBusca(); Salvar();
                yield return Dizer(new[] { new Fala { personagem = "Luzia", texto = "Deixe a água com nós, Josefa. Obrigada." } });
            }
            else
            {
                yield return Dizer(new[] { new Fala { personagem = "Luzia", texto =
                    "Precisamos do pano e da cabaça com água. Selecione um deles no inventário e apresente aqui." } });
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
                yield return Dizer(new[] { new Fala { personagem = "Trabalhador", texto =
                    "Pode trazer um pano limpo para os pertences de Bento? Os homens vão levar ele ao sepultamento." } });
                yield break;
            }
            InventoryManager.Instance.RemoveItem(panoAuxilio);
            Dados.curraisPanoSepultamentoEntregue = true;
            AtualizarEstadoBusca(); Salvar();
            yield return Dizer(despedidaPatio);
        }
        if (Dados.curraisCabacaSocorroRecolhida) yield return SepultarBento();
    }

    private IEnumerator ConcluirAtendimentoBento()
    {
        yield return Dizer(atendimentoFalas);
        yield return Escurecer(1f);
        Dados.curraisBentoMorto = true;
        GameProgress.Instance.UnlockDiaryPage("currais-bento");
        AtualizarEstadoBusca(); Salvar();
        yield return Esperar(tempoPlano);
        yield return Escurecer(0f);
        yield return Dizer(noticiaBento);
    }

    private IEnumerator RecolherCabacaSocorro()
    {
        if (!Dados.curraisBentoMorto || Dados.curraisCabacaSocorroRecolhida) yield break;
        if (!TemItem(cabacaVaziaBusca)) InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        Dados.curraisCabacaSocorroRecolhida = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Zefinha", texto = "Vou guardar a cabaça da família." } });
    }

    private IEnumerator SepultarBento()
    {
        if (Dados.curraisSepultamentoConcluido) yield break;
        bool objetivoVisivel = objetivoTexto.gameObject.activeSelf;
        try
        {
            yield return Escurecer(1f);
            sepultamentoNaTela = true;
            planoSepultamento.SetActive(true);
            corpoNoPlano.SetActive(true);
            objetivoTexto.gameObject.SetActive(false);
            yield return Escurecer(0f);
            yield return Dizer(sepultamentoFalas);
            yield return Escurecer(1f);
            corpoNoPlano.SetActive(false);
            yield return Esperar(tempoPlano);
            yield return Escurecer(0f);
            yield return Dizer(retornoSepultamento);
            yield return Escurecer(1f);
            RestaurarPlanoSepultamento();
            Dados.curraisSepultamentoConcluido = true;
            AvancarCampo(6);
            yield return Escurecer(0f);
        }
        finally
        {
            RestaurarPlanoSepultamento();
            if (objetivoTexto != null) objetivoTexto.gameObject.SetActive(objetivoVisivel);
        }
    }

    private IEnumerator AjudarNoQuarto()
    {
        if (Dados.curraisAjudouAtendimentoQuarto || !TemItem(cabacaVaziaBusca)) yield break;
        yield return Escolher("Trabalhadora", "Você pode emprestar esse recipiente? Preciso buscar água para quem tá aqui.",
            new[] { "Emprestar a cabaça", "Preciso voltar para minha família" });
        if (escolhaRecebida != 0) yield break;
        // Empréstimo breve durante a conversa: a mesma cabaça é devolvida, sem consumi-la.
        yield return Dizer(emprestimoQuarto);
        yield return Esperar(tempoPlano);
        yield return Dizer(devolucaoQuarto);
        Dados.curraisAjudouAtendimentoQuarto = true;
        Salvar();
    }

    private void RestaurarPlanoSepultamento()
    {
        if (planoSepultamento != null) planoSepultamento.SetActive(false);
        if (sepultamentoNaTela && objetivoTexto != null) objetivoTexto.gameObject.SetActive(true);
        sepultamentoNaTela = false;
    }
}
