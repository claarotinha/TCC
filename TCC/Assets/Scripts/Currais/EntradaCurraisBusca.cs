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
        return ReferenciasRaimundoLinearValidas() & valido;
    }

    private Collider2D[] AlvosAtuais()
    {
        if (PostoDisponivel) return ComCriancaERegistros(AlvosPosto());
        if (Dados.curraisPanelaConcluida)
            return ComCriancaERegistros(ComAlvosDoPatio(AlvosLavagemEPassagem()));
        return ComCriancaERegistros(new[] { AreaCabacaMesa, areaPanoAbrigo, areaLotePanos, areaLuzia, areaAgua, areaDistribuicaoAgua, areaMae, areaPaiAbrigo, areaMenino, areaRegistro, areaPanela, areaRaimundo, areaLavadeira, areaRecebimentoLavagem });
    }

    private string ObjetivoCampo() => string.Empty;

    private void AtualizarCampo()
    {
        bool ativo = Dados.curraisPanelaConcluida;
        setorBusca.SetActive(true);
        bool levandoLote = Dados.curraisEtapaEntrada == 4 && Dados.curraisLotePanosRecolhido && !ativo;
        if (levandoLote)
        {
            areaRaimundo.enabled = true;
            areaLavadeira.enabled = true;
            areaRecebimentoLavagem.enabled = true;
        }
        if (!ativo) { AtualizarPosto(); return; }
        int etapa = Dados.curraisEtapaBusca;
        objetivoTexto.text = string.Empty;
        areaRaimundo.enabled = PrimeiraBuscaEmCurso;
        areaLavadeira.enabled = PrimeiraBuscaEmCurso;
        areaPanoLavagem.enabled = etapa == 2 && RetalhoAutorizado;
        AtualizarVisuaisLavagem();
        areaLembranca.enabled = etapa == 3;
        areaPatio.enabled = etapa == 4;
        areaPanoAtendimento.enabled = false;
        panoCarregado.SetActive(false);
        manchaRoda.SetActive(etapa >= 5);
        corpoCoberto.SetActive((etapa >= 6 || Dados.curraisBentoMorto) && !Dados.curraisSepultamentoConcluido);
        trabalhadorRetorno.SetActive(true);
        bento.gameObject.SetActive(true);
        Posicionar(bento, etapa >= 5 ? bentoCaido : bentoEmPe);
        // Não gira nem inclina o sprite para representar ferimentos.
        Vector3 baseBento = bento.position;
        baseBento.y = bentoEmPe.position.y;
        bento.position = baseBento;
        bento.rotation = bentoEmPe.rotation;
        if (etapa >= 1 && (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            if (seguidoraMae != null) seguidoraMae.enabled = false;
            if (Dados.curraisEscolhaAgua != 1 || Dados.curraisEtapaBusca > 5)
                Posicionar(mae, maeAbrigo);
            luzia.transform.localRotation = Quaternion.identity;
            if (etapa >= 4)
                Posicionar(pai, paiTrabalhoMarca);
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
        if (InteragirPessoasDoPatio(ponto)) return true;
        if (areaRecebimentoLavagem.enabled && areaRecebimentoLavagem.gameObject.activeInHierarchy && areaRecebimentoLavagem.OverlapPoint(ponto))
        { MenuLavadeiraNaBusca(); return true; }
        foreach (Collider2D area in AlvosAtuais())
        {
            if (area == null || !area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            int etapa = Dados.curraisEtapaBusca;
            if (area == areaAguaPatio && etapa == 4) MenuAguaDoPatio();
            else if (area == areaPanosPatio && etapa == 4) MenuApoioDoPatio();
            else if (area == areaSocorroBento && etapa == 5) AbrirSocorroOuUsarItem();
            else if (area == areaCabacaSocorro && etapa == 5) Iniciar(RecolherCabacaSocorro());
            else if (area == areaPanosUso && etapa == 2) MenuPilhaLavagem(false);
            else if (area == areaPanosAtendimento && etapa == 2) MenuPilhaLavagem(true);
            else if (area == areaRaimundo && PrimeiraBuscaEmCurso)
            {
                PrimeiroContatoRaimundo();
            }
            else if (area == areaLavadeira && PrimeiraBuscaEmCurso) MenuLavadeiraNaBusca();
            else if (area == areaLuzia && PrimeiraBuscaEmCurso) ConversarLuziaNaBusca();
            else if (area == areaGuardaRonda && PrimeiraBuscaEmCurso) Iniciar(AbrirGuardaPassagem());
            else if (area == areaPanoLavagem && etapa == 2) Iniciar(RecolherTiraLavagem());
            else if (area == areaLembranca && etapa == 3)
            {
                Iniciar(LembrarLenda());
            }
            else if (area == areaPatio && etapa == 4) Iniciar(AbrirPaiPatioLinear());
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

    private IEnumerator ConhecerRaimundo() { yield return ConversaInicialRaimundo(); }

    private void MenuRaimundo() => PrimeiroContatoRaimundo();

    private IEnumerator PerguntarRaimundo(int pergunta, Fala[] falas)
    {
        if (EncontroRaimundoEncerrado) yield break;
        yield return Dizer(falas);
        Dados.curraisPerguntasRaimundo |= pergunta;
        RegistrarConversaEntrada("raimundo-assunto-respondido"); Salvar();
        yield return DeixarRaimundo();
    }

    private IEnumerator DeixarRaimundo()
    {
        if (EncontroRaimundoEncerrado) yield break;
        Fala[] despedida = raimundoSaida;
        if (Dados.curraisLotePanosEntregue)
        {
            despedida = (Fala[])raimundoSaida.Clone();
            despedida[0] = new Fala { personagem = "Zefinha",
                texto = "Já deixei os panos na lavagem. Se souber da menina, procure nós perto das esteiras." };
        }
        yield return Dizer(despedida);
        RegistrarConversaEntrada("raimundo-encontro-encerrado");
        if (Dados.curraisEtapaBusca == 0) AvancarCampo(1);
        else Salvar();
    }

    private void RestaurarLuziaNaBusca()
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca < 1) return;
        if (Dados.curraisEscolhaAgua == 1 && Dados.curraisEtapaBusca <= 5)
        {
            Vector3 posicaoMae = mae.position;
            SpriteRenderer visual = mari.GetComponent<SpriteRenderer>();
            float direcao = visual != null && visual.flipX ? -1f : 1f;
            posicaoMae.x = mari.transform.position.x - direcao * seguidoraMae.DistanciaAtras;
            mae.position = posicaoMae;
            Physics2D.SyncTransforms();
            return;
        }
        Vector3 posicao = luzia.transform.position;
        posicao.x = mari.transform.position.x - seguidoraLuzia.DistanciaAtras;
        luzia.transform.position = posicao;
        Physics2D.SyncTransforms();
    }

    private IEnumerator ConversarLavadeira() { yield return ReceberLoteLavagemLinear(); }

    private IEnumerator LembrarLenda()
    {
        if (lembrancaEmCurso || Dados.curraisEtapaBusca != 3 ||
            !Dados.curraisTiraPanoRecolhida || Dados.curraisPassagemVigiadaConcluida) yield break;
        EncerrarOportunidadeLavadeira();
        lembrancaEmCurso = true;
        try
        {
            int historia = Dados.estradaHistoriaEscolhida;
            if (historia == 1)
            {
                yield return MostrarPercepcaoGradual(sombraGalho);
                yield return Dizer(lembrancaCorpo);
                yield return MostrarPercepcaoGradual(pessoaNaCerca);
                yield return Esperar(tempoPlano);
            }
            else if (historia == 2)
            {
                yield return MostrarPercepcaoGradual(sinalGuarda);
                yield return Dizer(lembrancaComadre);
                yield return Esperar(tempoPlano);
            }
            else if (historia == 3)
            {
                yield return MostrarPercepcaoGradual(reflexoIrma);
                yield return Dizer(lembrancaAgua);
                reflexoIrma.SetActive(false);
                yield return MostrarPercepcaoGradual(reflexoZefinha);
                yield return Esperar(tempoPlano);
            }
            yield return OrientarAcompanhanteNaLembranca();
            yield return Dizer(Dados.curraisEscolhaAgua == 1 ? chamadaPassagemMae : chamadaPassagemLuzia);
            Fala[] reacao = historia == 1 ? reacaoCorpoPassagem :
                historia == 2 ? reacaoComadrePassagem : historia == 3 ? reacaoAguaPassagem : reacaoSemLendaPassagem;
            yield return Dizer(new[] { reacao[0] });
            OcultarPercepcao();
            Fala[] resposta = new Fala[reacao.Length - 1];
            Array.Copy(reacao, 1, resposta, 0, resposta.Length);
            yield return Dizer(ComAcompanhante(resposta));
            yield return Dizer(Dados.curraisEscolhaAgua == 1 ? continuarPassagemMae : continuarPassagemLuzia);
            Dados.curraisPassagemVigiadaConcluida = true;
            GameProgress.Instance.UnlockDiaryPage("currais-corredor");
            AvancarCampo(4);
        }
        finally
        {
            lembrancaEmCurso = false;
            OcultarPercepcao();
        }
    }

    private IEnumerator TestemunharTrabalho() { yield return ConversarPaiPatioLinear(); }

    private IEnumerator AgressaoNoPatio()
    {
        if (agressaoPatioEmCurso || Dados.curraisEtapaBusca != 4 ||
            !Dados.curraisAguaPatioPreparada || !Dados.curraisPanosPatioEntregues ||
            !TemItem(cabacaCheiaBusca) || !PaiNoAlcancePatio) yield break;
        agressaoPatioEmCurso = true;
        QuadrosAmbiente cicloBento = bento.GetComponent<QuadrosAmbiente>();
        bool cicloAtivo = cicloBento != null && cicloBento.enabled;
        try
        {
            yield return Dizer(patioLevarAgua);
            yield return Dizer(patioAntesQueda);
            yield return Escurecer(1f);
            if (cicloBento != null) cicloBento.enabled = false;
            Posicionar(bento, bentoCaido);
            bento.rotation = bentoEmPe.rotation;
            bentoEmSocorroEmCena = true;
            manchaRoda.SetActive(true);
            AtualizarDetalhesPatio();
            yield return Esperar(.35f);
            yield return Escurecer(0f);
            yield return Dizer(patioDepoisQueda);
            yield return Dizer(patioRetirada);
            panoCarregado.SetActive(false);
            yield return Dizer(patioPensamentoMari);
            yield return Dizer(Dados.curraisEscolhaAgua == 1 ? patioReacaoMae : patioReacaoLuzia);
            yield return FalarPosto(NomeAcompanhantePatio,
                "Tu tá com o pano limpo e a água. Eu fico aqui com ele.");
            AvancarCampo(5);
        }
        finally
        {
            if (cicloBento != null) cicloBento.enabled = cicloAtivo;
            agressaoPatioEmCurso = false;
            bentoEmSocorroEmCena = false;
            if (!encerrando && GameProgress.Instance != null) AtualizarEstadoBusca();
        }
    }

    private IEnumerator AjudarBento()
    {
        if (!PrecisaRecolherPanoSocorro || Dados.curraisEtapaBusca != 5 || Dados.curraisBentoMorto) yield break;
        if (!Dados.curraisBentoMorto && !PanoDoApoioDisponivel) yield break;
        yield return ExaminarOuColetar(panoAuxilio);
        if (!coletaDoCenarioConfirmada) yield break;
        if (!Dados.curraisBentoMorto) RegistrarConversaEntrada("patio-pano-retomado");
        AtualizarEstadoBusca(); Salvar();
    }
}
