using System;
using System.Collections;
using UnityEngine;

// Continuação do mesmo componente e Canvas. As posições e as artes pertencem à cena.
public partial class EntradaCurrais
{
    [Header("Etapa 3 - busca dentro do campo")]
    [SerializeField] private GameObject setorBusca;
    [SerializeField] private Collider2D areaRaimundo, areaLavadeira, areaPanoLavagem, areaLembranca, areaPatio, areaPanoAtendimento;
    [SerializeField] private Transform paiTrabalhoMarca, bento, bentoCaido, bentoEmPe, fiscalTrabalho;
    [SerializeField] private GameObject panoCarregado, manchaRoda, corpoCoberto, trabalhadorRetorno;
    [SerializeField] private GameObject sombraGalho, pessoaNaCerca, reflexoIrma, reflexoZefinha, sinalGuarda;
    [SerializeField] private Fala[] raimundoInicio, raimundoBando, raimundoMulher, raimundoSaida;
    [SerializeField] private Fala[] lavagemFalas, lembrancaCorpo, lembrancaComadre, lembrancaAgua, lembrancaRetorno;
    [SerializeField] private Fala[] patioInicio, patioAgressao, patioRetirada, corpoDistante, atendimentoFalas, noticiaBento;
    [SerializeField] private string[] opcoesRaimundo, objetivosCampo;

    private bool ReferenciasCampoValidas()
    {
        UnityEngine.Object[] referencias = { setorBusca, areaRaimundo, areaLavadeira, areaPanoLavagem,
            areaLembranca, areaPatio, areaPanoAtendimento, paiTrabalhoMarca, bento,
            bentoCaido, bentoEmPe, fiscalTrabalho, panoCarregado, manchaRoda, corpoCoberto,
            trabalhadorRetorno, sombraGalho, pessoaNaCerca, reflexoIrma, reflexoZefinha, sinalGuarda };
        string[] nomes = { nameof(setorBusca), nameof(areaRaimundo), nameof(areaLavadeira), nameof(areaPanoLavagem),
            nameof(areaLembranca), nameof(areaPatio), nameof(areaPanoAtendimento),
            nameof(paiTrabalhoMarca), nameof(bento), nameof(bentoCaido), nameof(bentoEmPe), nameof(fiscalTrabalho),
            nameof(panoCarregado), nameof(manchaRoda), nameof(corpoCoberto), nameof(trabalhadorRetorno),
            nameof(sombraGalho), nameof(pessoaNaCerca), nameof(reflexoIrma), nameof(reflexoZefinha), nameof(sinalGuarda) };
        bool valido = true;
        for (int i = 0; i < referencias.Length; i++) valido &= ConferirReferencia(referencias[i], nomes[i]);
        if (opcoesRaimundo == null ||
            opcoesRaimundo.Length != 4 || objetivosCampo == null || objetivosCampo.Length != 7) valido = false;
        Fala[][] sequencias = { raimundoInicio, raimundoBando, raimundoMulher, raimundoSaida, lavagemFalas,
            lembrancaCorpo, lembrancaComadre, lembrancaAgua, lembrancaRetorno, patioInicio, patioAgressao,
            patioRetirada, corpoDistante, atendimentoFalas, noticiaBento };
        foreach (Fala[] sequencia in sequencias) valido &= FalasValidas(sequencia);
        return valido;
    }

    private Collider2D[] AlvosAtuais()
    {
        if (PostoDisponivel) return AlvosPosto();
        if (Dados.curraisPanelaConcluida)
            return new[] { areaAguaPatio, areaPanosPatio, areaSocorroBento, areaCabacaSocorro, areaPanosUso, areaPanosAtendimento, areaRaimundo, areaLavadeira, areaPanoLavagem, areaLembranca, areaPatio, areaPanoAtendimento };
        return new[] { areaLotePanos, areaLuzia, areaAgua, areaMae, areaMenino, areaRegistro, areaPanela };
    }

    private string ObjetivoCampo() => string.Empty;

    private void AtualizarCampo()
    {
        bool ativo = Dados.curraisPanelaConcluida;
        setorBusca.SetActive(ativo);
        if (!ativo) { AtualizarPosto(); return; }
        int etapa = Dados.curraisEtapaBusca;
        objetivoTexto.text = string.Empty;
        areaRaimundo.enabled = etapa == 0;
        areaLavadeira.enabled = etapa == 1;
        areaPanoLavagem.enabled = etapa == 2 && Dados.curraisPanosSeparados == 3;
        AtualizarVisuaisLavagem();
        areaLembranca.enabled = etapa == 3;
        areaPatio.enabled = etapa == 4;
        areaPanoAtendimento.enabled = etapa == 5 && PrecisaRecolherPanoSocorro;
        panoCarregado.SetActive(etapa >= 3 && etapa <= 4 && Dados.curraisEscolhaAgua == 1);
        manchaRoda.SetActive(etapa >= 5);
        corpoCoberto.SetActive((etapa >= 6 || Dados.curraisBentoMorto) && !Dados.curraisSepultamentoConcluido);
        trabalhadorRetorno.SetActive(etapa >= 6);
        bento.gameObject.SetActive(etapa < 6 && !Dados.curraisBentoMorto);
        Posicionar(bento, etapa >= 5 ? bentoCaido : bentoEmPe);
        // Não gira nem inclina o sprite para representar ferimentos.
        Vector3 baseBento = bento.position;
        baseBento.y = bentoEmPe.position.y;
        bento.position = baseBento;
        bento.rotation = bentoEmPe.rotation;
        if (etapa >= 1 && (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            if (seguidoraMae != null) seguidoraMae.enabled = false;
            Posicionar(mae, maeAbrigo);
            luzia.transform.localRotation = Quaternion.identity;
            if (etapa >= 4) Posicionar(pai, paiTrabalhoMarca);
            if (etapa >= 6) Posicionar(pai, paiAbrigo);
        }
        AtualizarSocorro();
        OcultarPercepcao();
        AtualizarPosto();
        Physics2D.SyncTransforms();
    }

    private void OcultarPercepcao()
    {
        sombraGalho.SetActive(false); pessoaNaCerca.SetActive(false);
        reflexoIrma.SetActive(false); reflexoZefinha.SetActive(false); sinalGuarda.SetActive(false);
    }

    private bool InteragirCampo(Vector2 ponto)
    {
        if (PostoDisponivel) return InteragirPosto(ponto);
        foreach (Collider2D area in AlvosAtuais())
        {
            if (area == null || !area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            int etapa = Dados.curraisEtapaBusca;
            if (area == areaAguaPatio && etapa == 4) Iniciar(PrepararAguaPatio());
            else if (area == areaPanosPatio && etapa == 4) Iniciar(EntregarPanosPatio());
            else if (area == areaSocorroBento && etapa == 5) Iniciar(UsarNoSocorro());
            else if (area == areaCabacaSocorro && etapa == 5) Iniciar(RecolherCabacaSocorro());
            else if (area == areaPanosUso && etapa == 2) Iniciar(SepararPanoLavagem(false));
            else if (area == areaPanosAtendimento && etapa == 2) Iniciar(SepararPanoLavagem(true));
            else if (area == areaRaimundo && etapa == 0)
            {
                if (!Dados.curraisRaimundoConhecido) Iniciar(ConhecerRaimundo());
                else MenuRaimundo();
            }
            else if (area == areaLavadeira && etapa == 1) Iniciar(ConversarLavadeira());
            else if (area == areaPanoLavagem && etapa == 2) Iniciar(RecolherTiraLavagem());
            else if (area == areaLembranca && etapa == 3)
            {
                Iniciar(LembrarLenda());
            }
            else if (area == areaPatio && etapa == 4) Iniciar(TestemunharTrabalho());
            else if (area == areaPanoAtendimento && etapa == 5) Iniciar(AjudarBento());
            else objetivoTexto.text = string.Empty;
            return true;
        }
        return false;
    }

    private void AvancarCampo(int etapa)
    {
        Dados.curraisEtapaBusca = etapa;
        AtualizarEstadoBusca();
        Salvar();
    }

    private IEnumerator ConhecerRaimundo()
    {
        yield return Dizer(raimundoInicio);
        Dados.curraisRaimundoConhecido = true;
        GameProgress.Instance.UnlockDiaryPage("currais-raimundo");
        Salvar();
        MenuRaimundo();
    }

    private void MenuRaimundo()
    {
        MostrarOpcoes("Raimundo", raimundoInicio[raimundoInicio.Length - 1].texto, opcoesRaimundo, new Action[] {
            () => Iniciar(PerguntarRaimundo(1, raimundoBando)),
            () => Iniciar(PerguntarRaimundo(2, raimundoMulher)),
            () => Iniciar(PerguntarRaimundo(4, raimundoTrabalho)),
            () => Iniciar(DeixarRaimundo()) });
    }

    private IEnumerator PerguntarRaimundo(int pergunta, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisPerguntasRaimundo |= pergunta;
        Salvar(); MenuRaimundo();
    }

    private IEnumerator DeixarRaimundo()
    {
        yield return Dizer(raimundoSaida);
        AvancarCampo(1);
        RestaurarLuziaNaBusca();
    }

    private void RestaurarLuziaNaBusca()
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca < 1) return;
        Vector3 posicao = luzia.transform.position;
        posicao.x = mari.transform.position.x - seguidoraLuzia.DistanciaAtras;
        luzia.transform.position = posicao;
        Physics2D.SyncTransforms();
    }

    private IEnumerator ConversarLavadeira()
    {
        if (Dados.curraisTarefaPanosIniciada && !Dados.curraisLotePanosEntregue)
        {
            if (!TemItem(itemLotePanos) || InventoryManager.Instance.SelectedItem != itemLotePanos)
            {

                yield break;
            }
            InventoryManager.Instance.RemoveItem(itemLotePanos);
            Dados.curraisLotePanosEntregue = true;
            AtualizarTarefaPanos();
            Salvar();
        }
        yield return Dizer(lavagemFalas);
        yield return Dizer(instrucoesLavagem);
        Dados.curraisEtapaBusca = 2;
        GarantirPanosLavagem();
        AvancarCampo(2);
        GameProgress.Instance.UnlockDiaryPage("currais-lavagem");
        Salvar();
    }

    private IEnumerator LembrarLenda()
    {
        int historia = Dados.estradaHistoriaEscolhida;
        if (historia == 1)
        {
            sombraGalho.SetActive(true);
            yield return Dizer(lembrancaCorpo);
            pessoaNaCerca.SetActive(true);
            yield return Esperar(tempoPlano);
        }
        else if (historia == 2)
        {
            sinalGuarda.SetActive(true);
            yield return Dizer(lembrancaComadre);
            yield return Esperar(tempoPlano);
        }
        else if (historia == 3)
        {
            reflexoIrma.SetActive(true);
            yield return Dizer(lembrancaAgua);
            reflexoIrma.SetActive(false); reflexoZefinha.SetActive(true);
            yield return Esperar(tempoPlano);
        }
        // Testes diretos sem lenda escolhida seguem pelo chamado de Luzia, sem inventar uma escolha.
        yield return Dizer(lembrancaRetorno);
        GameProgress.Instance.UnlockDiaryPage("currais-corredor");
        AvancarCampo(4);
    }

    private IEnumerator TestemunharTrabalho()
    {
        if (!Dados.curraisTrabalhoPatioIniciado)
        {
            Dados.curraisTrabalhoPatioIniciado = true;
            if (!TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
            AtualizarEstadoBusca(); Salvar();
            yield return Dizer(patioInicio);
            yield return Dizer(tarefaPatioFalas);
            yield break;
        }
        if (!Dados.curraisAguaPatioPreparada || !Dados.curraisPanosPatioEntregues)
        { yield return Dizer(tarefaPatioFalas); yield break; }
        yield return AgressaoNoPatio();
    }

    private IEnumerator AgressaoNoPatio()
    {
        Vector3 destinoBento = bentoCaido.position;
        destinoBento.y = bentoEmPe.position.y;
        yield return Mover(bento, destinoBento, tempoPlano);
        bento.rotation = bentoEmPe.rotation;
        Quaternion fiscalOriginal = fiscalTrabalho.localRotation;
        try
        {
            fiscalTrabalho.localRotation = Quaternion.identity;
            yield return Dizer(patioAgressao);
            manchaRoda.SetActive(true);
            yield return Esperar(tempoPlano);
        }
        finally { if (fiscalTrabalho != null) fiscalTrabalho.localRotation = fiscalOriginal; }
        yield return Dizer(patioRetirada);
        panoCarregado.SetActive(false);
        yield return Dizer(corpoDistante);
        AvancarCampo(5);
    }

    private IEnumerator AjudarBento()
    {
        if (!PrecisaRecolherPanoSocorro) yield break;
        yield return ExaminarOuColetar(panoAuxilio);
        if (!coletaDoCenarioConfirmada) yield break;
        if (!TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
        AtualizarEstadoBusca(); Salvar();

    }
}
