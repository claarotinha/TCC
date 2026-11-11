using System;
using System.Collections;
using UnityEngine;

// Continuação do mesmo componente e Canvas. As posições e as artes pertencem à cena.
public partial class EntradaCurrais
{
    [Header("Etapa 3 - busca dentro do campo")]
    [SerializeField] private GameObject setorBusca;
    [SerializeField] private Collider2D areaRaimundo, areaLavadeira, areaPanoLavagem, areaLembranca, areaPatio, areaPanoAtendimento;
    [SerializeField] private Transform[] limitesBusca;
    [SerializeField] private Transform paiTrabalhoMarca, bento, bentoCaido, bentoEmPe, fiscalTrabalho;
    [SerializeField] private GameObject panoCarregado, manchaRoda, corpoCoberto, trabalhadorRetorno;
    [SerializeField] private GameObject sombraGalho, pessoaNaCerca, reflexoIrma, reflexoZefinha, sinalGuarda;
    [SerializeField] private Fala[] raimundoInicio, raimundoBando, raimundoMulher, raimundoSaida;
    [SerializeField] private Fala[] lavagemFalas, lembrancaCorpo, lembrancaComadre, lembrancaAgua, lembrancaRetorno;
    [SerializeField] private Fala[] patioInicio, patioAgressao, patioRetirada, corpoDistante, atendimentoFalas, noticiaBento;
    [SerializeField] private string[] opcoesRaimundo, objetivosCampo;
    private float limiteEsquerdoBusca;

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
        if (limitesBusca == null || limitesBusca.Length != 4 || opcoesRaimundo == null ||
            opcoesRaimundo.Length != 3 || objetivosCampo == null || objetivosCampo.Length != 7) valido = false;
        else foreach (Transform marca in limitesBusca) valido &= ConferirReferencia(marca, nameof(limitesBusca));
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
            return new[] { areaRaimundo, areaLavadeira, areaPanoLavagem, areaLembranca, areaPatio, areaPanoAtendimento };
        return new[] { areaLotePanos, areaLuzia, areaAgua, areaMae, areaMenino, areaRegistro, areaPanela };
    }

    private string ObjetivoCampo() => PostoDisponivel ? ObjetivoPosto() : objetivosCampo[Mathf.Clamp(Dados.curraisEtapaBusca, 0, 6)];

    private void AplicarLimitesBusca()
    {
        BoxCollider2D limite = limiteSetor as BoxCollider2D;
        if (limite == null || limitesBusca == null || limitesBusca.Length != 4) return;
        if (limiteEsquerdoBusca == 0f) limiteEsquerdoBusca = limite.bounds.min.x;
        int indice = !Dados.curraisPanelaConcluida ? 0 : Dados.curraisEtapaBusca == 0 ? 0 :
            Dados.curraisEtapaBusca <= 2 ? 1 : Dados.curraisEtapaBusca == 3 ? 2 : 3;
        float direita = limitesBusca[indice].position.x;
        Vector3 centro = limite.transform.InverseTransformPoint(new Vector3((limiteEsquerdoBusca + direita) * .5f,
            limite.bounds.center.y, limite.transform.position.z));
        limite.offset = new Vector2(centro.x, limite.offset.y);
        limite.size = new Vector2((direita - limiteEsquerdoBusca) / Mathf.Abs(limite.transform.lossyScale.x), limite.size.y);
        Physics2D.SyncTransforms();
        if (seguirCamera != null) seguirCamera.AtualizarLimites();
    }

    private void AtualizarCampo()
    {
        bool ativo = Dados.curraisPanelaConcluida;
        setorBusca.SetActive(ativo);
        AplicarLimitesBusca();
        if (!ativo) { AtualizarPosto(); return; }
        int etapa = Dados.curraisEtapaBusca;
        objetivoTexto.text = ObjetivoCampo();
        areaRaimundo.enabled = etapa == 0;
        areaLavadeira.enabled = etapa == 1;
        areaPanoLavagem.enabled = etapa == 2;
        areaLembranca.enabled = etapa == 3;
        areaPatio.enabled = etapa == 4;
        areaPanoAtendimento.enabled = etapa == 5;
        panoCarregado.SetActive(etapa >= 3 && etapa <= 4 && Dados.curraisEscolhaAgua == 1);
        manchaRoda.SetActive(etapa >= 5);
        corpoCoberto.SetActive(etapa >= 6);
        trabalhadorRetorno.SetActive(etapa >= 6);
        bento.gameObject.SetActive(etapa < 6);
        Posicionar(bento, etapa >= 5 ? bentoCaido : bentoEmPe);
        bento.rotation = (etapa >= 5 ? bentoCaido : bentoEmPe).rotation;
        if (etapa >= 1 && (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            if (seguidoraMae != null) seguidoraMae.enabled = false;
            Posicionar(mae, maeAbrigo);
            luzia.transform.localRotation = Quaternion.identity;
            if (etapa >= 4) Posicionar(pai, paiTrabalhoMarca);
            if (etapa >= 6) Posicionar(pai, paiAbrigo);
        }
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
            if (Mathf.Abs(mari.transform.position.x - area.bounds.center.x) > alcanceInteracao)
            { objetivoTexto.text = "Aproxime-se para interagir. " + ObjetivoCampo(); return true; }
            int etapa = Dados.curraisEtapaBusca;
            if (area == areaRaimundo && etapa == 0)
            {
                if (!Dados.curraisRaimundoConhecido) Iniciar(ConhecerRaimundo());
                else MenuRaimundo();
            }
            else if (area == areaLavadeira && etapa == 1) Iniciar(ConversarLavadeira());
            else if (area == areaPanoLavagem && etapa == 2) AvancarCampo(3);
            else if (area == areaLembranca && etapa == 3) Iniciar(LembrarLenda());
            else if (area == areaPatio && etapa == 4) Iniciar(TestemunharTrabalho());
            else if (area == areaPanoAtendimento && etapa == 5) Iniciar(AjudarBento());
            else objetivoTexto.text = ObjetivoCampo();
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
        posicao.x = Mathf.Clamp(mari.transform.position.x - seguidoraLuzia.DistanciaAtras,
            limiteSetor.bounds.min.x + 1f, limiteSetor.bounds.max.x - 1f);
        luzia.transform.position = posicao;
        Physics2D.SyncTransforms();
    }

    private IEnumerator ConversarLavadeira()
    {
        if (Dados.curraisTarefaPanosIniciada && !Dados.curraisLotePanosEntregue)
        {
            if (!TemItem(itemLotePanos) || InventoryManager.Instance.SelectedItem != itemLotePanos)
            {
                yield return Dizer(new[] { new Fala { personagem = "Lavadeira", texto =
                    "Você trouxe o lote da mesa dos nomes? Selecione os panos no inventário, feche com TAB e entregue aqui." } });
                yield break;
            }
            InventoryManager.Instance.RemoveItem(itemLotePanos);
            Dados.curraisLotePanosEntregue = true;
            AtualizarTarefaPanos();
            Salvar();
        }
        yield return Dizer(lavagemFalas);
        AvancarCampo(Dados.curraisEscolhaAgua == 1 ? 2 : 3);
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
        yield return Dizer(patioInicio);
        yield return Mover(bento, bentoCaido.position, tempoPlano);
        bento.rotation = bentoCaido.rotation;
        Quaternion fiscalOriginal = fiscalTrabalho.localRotation;
        try
        {
            fiscalTrabalho.localRotation = Quaternion.Euler(0, 0, -12);
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
        yield return Dizer(atendimentoFalas);
        yield return Esperar(tempoPlano);
        yield return Dizer(noticiaBento);
        GameProgress.Instance.UnlockDiaryPage("currais-bento");
        AvancarCampo(6);
    }
}
