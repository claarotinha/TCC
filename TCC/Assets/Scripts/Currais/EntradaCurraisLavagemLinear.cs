using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Lavagem — Examine, recebimento e variações")]
    [SerializeField] private ExamineObject exameLavadeiraLinear, examePilhaUso, examePilhaAtendimento;
    [SerializeField] private Collider2D areaRecebimentoLavagem;
    [SerializeField] private Fala[] chegadaLavagemMae, chegadaLavagemLuzia, loteLavagemOrientacao;
    [SerializeField] private Fala[] loteLavagemInicio, loteLavagemMae, loteLavagemLuzia, loteLavagemPista;
    [SerializeField] private Fala[] conversaLavagemIrma, correcaoPilhaLavagem;
    [SerializeField] private Fala[] conclusaoLavagem, retalhoPedidoZefinha, retalhoOferecidoLavadeira;
    [SerializeField] private Fala[] saidaLavagemMae, saidaLavagemLuzia;
    [SerializeField, Min(.5f)] private float alcanceChegadaLavagem = 5f;
    private bool RetalhoAutorizado => ViuConversaEntrada("lavagem-retalho-autorizado") ||
        Dados.curraisTiraPanoRecolhida || Dados.curraisEtapaBusca >= 3;

    private bool ReferenciasLavagemLinearValidas()
    {
        bool ok = ConferirReferencia(areaRecebimentoLavagem, nameof(areaRecebimentoLavagem)) &
            ConferirReferencia(puzzleRetalhos, nameof(puzzleRetalhos));
        foreach (ExamineObject exame in new[] { exameLavadeiraLinear, examePilhaUso, examePilhaAtendimento })
            ok &= ConferirReferencia(exame, "Examine da lavagem") &&
                ConferirReferencia(exame.examinePanel, "Painel de Examine da lavagem") &&
                ConferirReferencia(exame.examineText, "Texto de Examine da lavagem");
        foreach (Fala[] falas in new[] { chegadaLavagemMae, chegadaLavagemLuzia, loteLavagemOrientacao,
            loteLavagemInicio, loteLavagemMae, loteLavagemLuzia, loteLavagemPista, conversaLavagemIrma,
            correcaoPilhaLavagem, conclusaoLavagem, retalhoPedidoZefinha, retalhoOferecidoLavadeira,
            saidaLavagemMae, saidaLavagemLuzia }) ok &= FalasValidas(falas);
        return ok;
    }

    private IEnumerator RetomarCaminhoDoLote(bool procurarRaimundo)
    {
        // Também recupera partidas que já estavam com o lote antes da correção.
        if (!Dados.curraisPanelaConcluida) yield return DecidirCanhotoNomes();
        if (!Dados.curraisPanelaConcluida) yield break;
        if (procurarRaimundo) yield return AbrirContatoRaimundo();
        else yield return TentarEntregarLoteLavagem();
    }

    private IEnumerator TentarEntregarLoteLavagem()
    {
        yield return ReceberLoteLavagemLinear();
    }

    private IEnumerator AbrirLavadeiraLinear()
    {
        yield return ExamineNomesUmaVez("lavagem-examine-mulher", exameLavadeiraLinear);
        if (!Dados.curraisLotePanosRecolhido || Dados.curraisEtapaBusca > 3 ||
            ViuConversaEntrada("lavagem-oportunidade-encerrada")) yield break;
        if (!Dados.curraisLotePanosEntregue)
        {
            MostrarOpcoes("Lavadeira", string.Empty,
                new[] { "Entregar o lote de panos", "Falar do lote", "Sair" },
                new Action[] { () => Iniciar(ReceberLoteLavagemLinear()),
                    () => Iniciar(Dizer(loteLavagemOrientacao)), FecharMenu });
            yield break;
        }
        if (Dados.curraisEtapaBusca <= 1)
        { yield return ReceberLoteLavagemLinear(); yield break; }
        if (Dados.curraisEtapaBusca == 2 && Dados.curraisPanosSeparados != 3)
        {
            MostrarOpcoes("Lavadeira", string.Empty, new[] { "Remendar o pano", "Sair" },
                new Action[] { () => Iniciar(RemendarPanoLavagem()), FecharMenu });
            yield break;
        }
        if (Dados.curraisPanosSeparados == 3 && !RetalhoAutorizado)
            yield return ConcluirLavagemLinear();
        MostrarConversaOpcionalLavagem();
    }

    private void MostrarConversaOpcionalLavagem()
    {
        if (ViuConversaEntrada("lavagem-oportunidade-encerrada")) return;
        AbrirAssuntosEntrada("lavagem-irma", "Lavadeira", string.Empty,
            new[] { "lavagem-irma-ouvida" }, new[] { "Perguntar pela menina" },
            new Func<IEnumerator>[] { () => Dizer(conversaLavagemIrma) });
    }

    private IEnumerator ReceberLoteLavagemLinear()
    {
        if (!Dados.curraisLotePanosRecolhido || Dados.curraisEtapaBusca > 1) yield break;
        if (!Dados.curraisLotePanosEntregue &&
            !TemItem(itemLotePanos)) yield break;
        yield return ExamineNomesUmaVez("lavagem-examine-mulher", exameLavadeiraLinear);
        if (!ViuConversaEntrada("lavagem-chegada")) yield return ComentarChegadaLavagem();
        if (!Dados.curraisLotePanosEntregue)
        {
            InventoryManager.Instance.RemoveItem(itemLotePanos);
            Dados.curraisLotePanosEntregue = true;
            AtualizarTarefaPanos(); Salvar();
        }
        yield return DizerEntradaUmaVez("lavagem-lote-inicio", loteLavagemInicio);
        yield return DizerEntradaUmaVez("lavagem-lote-acompanhante",
            loteLavagemMae);
        yield return DizerEntradaUmaVez("lavagem-lote-pista", loteLavagemPista);
        yield return DizerEntradaUmaVez("lavagem-instrucoes", instrucoesLavagem);
        Dados.curraisEtapaBusca = 2;
        GarantirPanosLavagem(); AvancarCampo(2);
        GameProgress.Instance.UnlockDiaryPage("currais-lavagem"); Salvar();
        yield return RemendarPanoLavagem();
    }

    private IEnumerator CorrigirPilhaLavagem(ItemData item)
    {
        if (item != panoUsoLavagem && item != panoAtendimentoLavagem) yield break;
        if (!ViuConversaEntrada("lavagem-correcao-pilha"))
        { yield return DizerEntradaUmaVez("lavagem-correcao-pilha", correcaoPilhaLavagem); yield break; }
        SpriteRenderer visual = (item == panoUsoLavagem ? areaPanosUso : areaPanosAtendimento).GetComponent<SpriteRenderer>();
        if (visual == null) yield break;
        Color original = visual.color;
        try
        {
            float tempo = 0f;
            while (tempo < .6f)
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

    private IEnumerator ConcluirLavagemLinear()
    {
        if (Dados.curraisPanosSeparados != 3 || RetalhoAutorizado) yield break;
        yield return DizerEntradaUmaVez("lavagem-conclusao", conclusaoLavagem);
        if (!EncontroRaimundoEncerrado)
            yield return DizerEntradaUmaVez("lavagem-indicacao-raimundo", new[]
            {
                new Fala { personagem = "Lavadeira", texto = "Ah, tu tá procurando tua irmã? Tente falar com Raimundo. Ele conhece muita gente por aqui." },
                new Fala { personagem = "Lavadeira", texto = "É aquele homem perto dos baldes." }
            });
        yield return Dizer((Dados.curraisPerguntasRaimundo & 4) != 0 ? retalhoPedidoZefinha : retalhoOferecidoLavadeira);
        RegistrarConversaEntrada("lavagem-retalho-autorizado");
        yield return EntregarRetalhoDaLavadeira();
    }

    private bool AtualizarAproximacaoLavagem(bool livre)
    {
        if (!livre || !Dados.curraisLotePanosRecolhido || Dados.curraisEtapaBusca > 3 ||
            PainelAuxiliarAberto() || InventarioAbertoMemoria()) return false;
        if (Dados.curraisTiraPanoRecolhida)
        {
            float direcao = Mathf.Sign(areaLembranca.bounds.center.x - areaLavadeira.bounds.center.x);
            if (direcao != 0f && direcao * (mari.transform.position.x - areaLembranca.bounds.center.x) >= 0f)
                EncerrarOportunidadeLavadeira();
        }
        if (Dados.curraisEtapaBusca == 2 && Dados.curraisPanosSeparados == 3)
        {
            Iniciar(RetalhoAutorizado ? EntregarRetalhoDaLavadeira() : ConcluirLavagemLinear());
            return true;
        }
        if (ViuConversaEntrada("lavagem-chegada") ||
            Mathf.Abs(mari.transform.position.x - areaLavadeira.bounds.center.x) > alcanceChegadaLavagem) return false;
        Iniciar(ComentarChegadaLavagem()); return true;
    }

    private IEnumerator ComentarChegadaLavagem() => DizerEntradaUmaVez("lavagem-chegada",
        chegadaLavagemMae);

    private void EncerrarOportunidadeLavadeira()
    {
        if (ViuConversaEntrada("lavagem-oportunidade-encerrada")) return;
        RegistrarConversaEntrada("lavagem-oportunidade-encerrada"); Salvar();
    }
}

