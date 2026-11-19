using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class EntradaCurrais
{
    [Header("Bento — atendimento, pertences e despedida")]
    [SerializeField] private ExamineObject exameAtendimentoBento, examePertencesBento, exameFamiliarBento;
    [SerializeField] private ItemData itemEmbrulhoBento;
    [SerializeField] private Collider2D areaEmbrulhoBento, areaFamiliarBento;
    [SerializeField] private Transform familiarBento, origemFamiliarBento, chegadaFamiliarBento, paiRetornoBento;
    [SerializeField] private SpriteRenderer arteFamiliarBento, marcadorFamiliarBento;
    [SerializeField] private Image familiarNoSepultamento;
    [SerializeField, Min(.1f)] private float pausaAtendimentoBento = 2f, tempoCaminhadaBento = 2.5f;
    [SerializeField] private Fala[] bentoReceberPano, bentoReceberAgua, bentoOuvirMae, bentoOuvirLuzia;
    [SerializeField] private Fala[] bentoPaiBuscarFamilia, bentoAtendimentoParou, bentoPerguntaMae, bentoPerguntaLuzia, bentoNoticiaLinear;
    [SerializeField] private Fala[] bentoFicar, bentoPerguntarFamilia, bentoPertencesPedido, bentoPertencesRecusados, bentoPertencesEntregues;
    [SerializeField] private Fala[] bentoCabacaRecuperada, bentoLembrarCabaca, bentoAnuncioSaida, bentoEsperar;
    [SerializeField] private Fala[] bentoPermissaoSepultamento, bentoReunirGrupo, bentoSepultamentoInicio, bentoNomeSepultamento;
    [SerializeField] private Fala[] bentoSepultamentoFamilia, bentoRetornoCruzes;
    private bool familiarBentoEmMovimento;

    private bool PaiBuscandoFamiliaBento => Dados.curraisEtapaBusca == 5 && !Dados.curraisBentoMorto &&
        ViuConversaEntrada("bento-pai-foi-buscar") && !ViuConversaEntrada("bento-pai-voltou");
    private bool PertencesBentoResolvidos => Dados.curraisPanoSepultamentoEntregue;
    private bool DespedidaBentoPronta => Dados.curraisEtapaBusca == 5 && Dados.curraisBentoMorto &&
        PertencesBentoResolvidos && Dados.curraisCabacaSocorroRecolhida && !Dados.curraisSepultamentoConcluido;

    private bool ReferenciasAtendimentoBentoValidas()
    {
        bool ok = true;
        foreach (UnityEngine.Object obj in new UnityEngine.Object[] { itemEmbrulhoBento, areaEmbrulhoBento, areaFamiliarBento,
            familiarBento, origemFamiliarBento, chegadaFamiliarBento, paiRetornoBento, arteFamiliarBento, marcadorFamiliarBento,
            familiarNoSepultamento }) ok &= ConferirReferencia(obj, "Atendimento de Bento");
        foreach (ExamineObject exame in new[] { exameAtendimentoBento, examePertencesBento, exameFamiliarBento })
            ok &= ConferirReferencia(exame, "Examine de Bento") &&
                ConferirReferencia(exame.examinePanel, "Painel de Examine de Bento") &&
                ConferirReferencia(exame.examineText, "Texto de Examine de Bento");
        foreach (Fala[] falas in new[] { bentoReceberPano, bentoReceberAgua, bentoOuvirMae, bentoOuvirLuzia,
            bentoPaiBuscarFamilia, bentoAtendimentoParou, bentoPerguntaMae, bentoPerguntaLuzia, bentoNoticiaLinear,
            bentoFicar, bentoPerguntarFamilia, bentoPertencesPedido, bentoPertencesRecusados, bentoPertencesEntregues,
            bentoCabacaRecuperada, bentoLembrarCabaca, bentoAnuncioSaida, bentoEsperar, bentoPermissaoSepultamento,
            bentoReunirGrupo, bentoSepultamentoInicio, bentoNomeSepultamento, bentoSepultamentoFamilia, bentoRetornoCruzes })
            ok &= FalasValidas(falas);
        return ok;
    }

    private void PrepararAtendimentoLinear()
    {
        if (Dados.curraisEtapaBusca == 5 && ViuConversaEntrada("bento-embrulho-recolhido") &&
            !PertencesBentoResolvidos && !TemItem(itemEmbrulhoBento))
            InventoryManager.Instance.AddItem(itemEmbrulhoBento);
    }

    private IEnumerator AbrirAtendimentoBentoLinear()
    {
        if (Dados.curraisEtapaBusca != 5) yield break;
        if (Dados.curraisBentoMorto && !ViuConversaEntrada("bento-exame-atendimento"))
            exameAtendimentoBento.message = "Bento está coberto à sombra. Quem veio com ele espera perto do atendimento.";
        yield return ExamineNomesUmaVez("bento-exame-atendimento", exameAtendimentoBento);
        if (!Dados.curraisBentoMorto)
        {
            if ((Dados.curraisItensAtendimentoBento & 3) == 3)
            { yield return ConcluirAtendimentoBento(); yield break; }
            AbrirAssuntosEntrada("bento-ouvir", NomeAcompanhantePatio, string.Empty,
                new[] { "bento-ouvir" }, new[] { "Ele tá ouvindo nós?" },
                new Func<IEnumerator>[] { () => Dizer(bentoOuvirMae) });
            yield break;
        }
        if (PertencesBentoResolvidos)
        {
            if (!Dados.curraisCabacaSocorroRecolhida)
            { yield return LembrarCabacaBento(); yield break; }
            if (!ViuConversaEntrada("bento-reacao-escolhida") && !ViuConversaEntrada("bento-despedida-iniciada"))
                MostrarOpcoes(NomeAcompanhantePatio, string.Empty,
                    new[] { "Falar de Bento", "Acompanhar a família", "Sair" },
                    new Action[] { MostrarReacaoBento, () => Iniciar(AnunciarDespedidaBento()), FecharMenu });
            else yield return AnunciarDespedidaBento();
            yield break;
        }
        MostrarReacaoBento();
    }

    private void MostrarReacaoBento()
    {
        if (!Dados.curraisBentoMorto || ViuConversaEntrada("bento-reacao-escolhida") ||
            ViuConversaEntrada("bento-despedida-iniciada")) return;
        MostrarOpcoes(NomeAcompanhantePatio, string.Empty,
            new[] { "Ficar um pouco", "Perguntar pela família dele", "Sair da conversa" },
            new Action[] { () => Iniciar(ReagirMorteBento(true)), () => Iniciar(ReagirMorteBento(false)), FecharMenu });
    }

    private IEnumerator ReagirMorteBento(bool ficar)
    {
        if (ViuConversaEntrada("bento-reacao-escolhida")) yield break;
        yield return Dizer(ficar ? ComAcompanhante(bentoFicar) : bentoPerguntarFamilia);
        if (ficar) yield return Esperar(tempoPlano);
        RegistrarConversaEntrada("bento-reacao-escolhida");
        RegistrarConversaEntrada(ficar ? "bento-reacao-ficar" : "bento-reacao-familia"); Salvar();
    }

    private IEnumerator TrazerFamiliarBento()
    {
        familiarBentoEmMovimento = true;
        try
        {
            // A família vem após o aviso do trabalhador; o pai nunca sai do atendimento.
            FecharDialogo();
            yield return Mover(familiarBento, chegadaFamiliarBento.position, tempoCaminhadaBento);
            yield return Esperar(1.5f);
        }
        finally { familiarBentoEmMovimento = false; Physics2D.SyncTransforms(); }
    }

    private bool InteragirPertencesBento(Vector2 ponto)
    {
        if (Dados.curraisEtapaBusca != 5 || !Dados.curraisBentoMorto) return false;
        if (areaEmbrulhoBento.enabled && areaEmbrulhoBento.gameObject.activeInHierarchy && areaEmbrulhoBento.OverlapPoint(ponto))
        { Iniciar(AbrirPertencesBento()); return true; }
        if (areaFamiliarBento.enabled && areaFamiliarBento.gameObject.activeInHierarchy && areaFamiliarBento.OverlapPoint(ponto))
        { Iniciar(ExamineNomesUmaVez("bento-exame-familiar", exameFamiliarBento)); return true; }
        return false;
    }

    private IEnumerator AbrirPertencesBento()
    {
        if (PertencesBentoResolvidos || !Dados.curraisBentoMorto) yield break;
        yield return ExamineNomesUmaVez("bento-exame-pertences", examePertencesBento);
        MostrarOpcoes("Pertences de Bento", string.Empty,
            new[] { "Levar os pertences", "Prefiro ficar aqui", "Sair" },
            new Action[] { () => Iniciar(RecolherEmbrulhoBento()), () => Iniciar(TrabalhadorLevarEmbrulhoBento()), FecharMenu });
    }

    private IEnumerator RecolherEmbrulhoBento()
    {
        if (PertencesBentoResolvidos || TemItem(itemEmbrulhoBento)) yield break;
        yield return ExaminarOuColetar(itemEmbrulhoBento);
        if (!coletaDoCenarioConfirmada) yield break;
        RegistrarConversaEntrada("bento-embrulho-recolhido");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator EntregarEmbrulhoBento()
    {
        if (!Dados.curraisBentoMorto || PertencesBentoResolvidos || !TemItem(itemEmbrulhoBento)) yield break;
        InventoryManager.Instance.RemoveItem(itemEmbrulhoBento);
        Dados.curraisPanoSepultamentoEntregue = true;
        RegistrarConversaEntrada("bento-pertences-pela-jogadora");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(bentoPertencesEntregues);
    }

    private IEnumerator TrabalhadorLevarEmbrulhoBento()
    {
        if (PertencesBentoResolvidos || TemItem(itemEmbrulhoBento)) yield break;
        yield return Dizer(bentoPertencesRecusados);
        yield return Mover(pertencesBentoCobertos.transform, chegadaFamiliarBento.position + Vector3.down * .2f, .8f);
        Dados.curraisPanoSepultamentoEntregue = true;
        RegistrarConversaEntrada("bento-pertences-pelo-trabalhador");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator LembrarCabacaBento()
    {
        yield return DizerEntradaUmaVez("bento-lembrar-cabaca", ComAcompanhante(bentoLembrarCabaca));
        SpriteRenderer visual = cabacaSocorro.GetComponent<SpriteRenderer>();
        Color original = visual.color;
        try
        {
            float tempo = 0f;
            while (tempo < .7f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    visual.color = new Color(original.r, original.g, original.b,
                        original.a * (.35f + .65f * Mathf.PingPong(tempo * 4f, 1f)));
                }
                yield return null;
            }
        }
        finally { visual.color = original; }
    }

    private IEnumerator AnunciarDespedidaBento()
    {
        if (!DespedidaBentoPronta) yield break;
        yield return DizerEntradaUmaVez("bento-anuncio-saida", bentoAnuncioSaida);
        MostrarPartidaBento();
    }

    private void MostrarPartidaBento()
    {
        if (!DespedidaBentoPronta) return;
        Vector3 pertoDaZefinha = mae.position;
        pertoDaZefinha.x = mari.transform.position.x - seguidoraMae.DistanciaAtras;
        mae.position = pertoDaZefinha;
        Physics2D.SyncTransforms();
        MostrarOpcoes("Mãe", string.Empty,
            new[] { "Eu quero ir", "Não vou ao funeral", "Sair" },
            new Action[] { () => Iniciar(SepultarBento()), () => Iniciar(RecusarFuneralBento()), FecharMenu });
    }

    private IEnumerator RecusarFuneralBento()
    {
        if (!DespedidaBentoPronta || sepultamentoEmCurso) yield break;
        sepultamentoEmCurso = true;
        try
        {
            yield return FalarPosto("Zefinha", "Eu num consigo ir.", "Mãe",
                "Tá bem, minha filha. Eu e teu pai vamos com a família de Bento. Quando nós voltar, tu conta se soube de alguma coisa.");
            Dados.curraisEscolhaFuneral = 2;
            RegistrarConversaEntrada("bento-funeral-recusado");
            yield return Escurecer(1f);
            yield return Esperar(2f);
            // Resolvido também por recusa, sem fingir que Josefa acompanhou o enterro.
            Dados.curraisSepultamentoConcluido = true;
            GameProgress.Instance.UnlockDiaryPage("josefa-09");
            AvancarCampo(6);
            yield return Escurecer(0f);
            yield return RetomarBuscaPosto();
        }
        finally { sepultamentoEmCurso = false; }
    }

    private IEnumerator EsperarDespedidaBento()
    {
        yield return Dizer(ComAcompanhante(bentoEsperar));
        yield return Esperar(tempoPlano);
        RegistrarConversaEntrada("bento-esperou"); Salvar();
        MostrarPartidaBento();
    }

    private bool AtualizarFluxoBento(bool livre)
    {
        if (Dados.curraisEtapaBusca != 5) return false;
        AtualizarVisuaisBentoLinear();
        if (!livre || InventarioAbertoMemoria() || PainelAuxiliarAberto()) return false;
        if (!Dados.curraisBentoMorto && (Dados.curraisItensAtendimentoBento & 3) == 3)
        { Iniciar(ConcluirAtendimentoBento()); return true; }
        if (DespedidaBentoPronta && !ViuConversaEntrada("bento-anuncio-saida"))
        { Iniciar(AnunciarDespedidaBento()); return true; }
        return false;
    }

    private void AtualizarVisuaisBentoLinear()
    {
        bool etapa = Dados.curraisEtapaBusca == 5 && !Dados.curraisSepultamentoConcluido;
        bool chegou = ViuConversaEntrada("bento-pai-voltou") || Dados.curraisBentoMorto;
        if (!familiarBentoEmMovimento)
            familiarBento.position = chegou ? chegadaFamiliarBento.position : origemFamiliarBento.position;
        marcadorFamiliarBento.gameObject.SetActive(etapa && (chegou || familiarBentoEmMovimento) && arteFamiliarBento.sprite == null);
        areaFamiliarBento.enabled = etapa && Dados.curraisBentoMorto && !PertencesBentoResolvidos;
        bool recolhido = ViuConversaEntrada("bento-embrulho-recolhido") || TemItem(itemEmbrulhoBento);
        pertencesBentoCobertos.gameObject.SetActive(etapa && Dados.curraisBentoMorto && (!recolhido || PertencesBentoResolvidos));
        areaEmbrulhoBento.enabled = etapa && Dados.curraisBentoMorto && !PertencesBentoResolvidos && !recolhido;
        if (etapa && PertencesBentoResolvidos)
            pertencesBentoCobertos.transform.position = chegadaFamiliarBento.position + Vector3.down * .2f;
    }

    private void AjustarCompanhiaBentoNoPlano()
    {
        familiarNoSepultamento.sprite = arteFamiliarBento.sprite != null ? arteFamiliarBento.sprite : marcadorFamiliarBento.sprite;
        familiarNoSepultamento.gameObject.SetActive(sepultamentoNaTela && quadroFuneralProvisorio == null);
    }
}
