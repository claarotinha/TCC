using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Amanhecer — reencontro com Raimundo")]
    [SerializeField] private ExamineObject exameRaimundoManha;
    private bool AntesDaUltimaConsulta => ConsultaDisponivel && Dados.curraisEtapaConsulta == 1 && !Dados.curraisConsultaApresentada;
    private bool RetalhoDisponivelManha => ConsultaDisponivel && Dados.curraisEtapaConsulta == 1 &&
        RetalhoAutorizado && !Dados.curraisTiraPanoRecolhida && !Dados.curraisBaldeApoiado &&
        !TemItem(tiraPanoLavagem) && !TemItem(baldeProtegidoItem);
    private bool PodeOferecerAlca => !Dados.curraisBaldeApoiado && !Dados.curraisBaldeSolicitado &&
        (TemItem(tiraPanoLavagem) || RetalhoDisponivelManha);
    private bool NomeNovoParaFamilia => Dados.curraisIdentidadeConfirmada && !ViuConversaEntrada("familia-nair-contada");

    private IEnumerator AbrirRaimundoManha()
    {
        if (Dados.curraisEtapaConsulta != 1) yield break;
        if (!ViuConversaEntrada("raimundo-manha-examine"))
        {
            string original = exameRaimundoManha.message;
            if (Dados.curraisBaldeApoiado) exameRaimundoManha.message =
                "A tira de pano continua enrolada na alça. Raimundo ajeita a mão antes de levantar o balde.";
            try { yield return ExamineNomesUmaVez("raimundo-manha-examine", exameRaimundoManha); }
            finally { exameRaimundoManha.message = original; }
        }
        if (!ViuConversaEntrada("raimundo-manha-encontro"))
        {
            if (!Dados.curraisIdentidadeConfirmada)
                yield return FalarPosto("Raimundo", Dados.curraisEscolhaPosto == 1 ?
                    "Bom dia, menina. Consegui perguntar de novo por aquela mulher." :
                    "Menina, consegui perguntar por aquela mulher que tava com tua irmã.");
            RegistrarConversaEntrada("raimundo-manha-encontro");
            AtualizarEstadoBusca(); Salvar();
        }
        MenuRaimundoLinear();
    }

    private void MenuRaimundoLinear()
    {
        var rotulos = new List<string>(); var acoes = new List<Action>();
        rotulos.Add(Dados.curraisIdentidadeConfirmada ? "Relembrar o nome da mulher" : "O que descobriu da mulher?");
        acoes.Add(() => Iniciar(Dados.curraisIdentidadeConfirmada ?
            FalarPosto("Raimundo", "Nair Pereira. Foi a mulher que eu vi com tua irmã. Pra onde chegaram, eu num vi.") : ConversarRaimundoManha()));
        if (PodeOferecerAlca)
        { rotulos.Add("Posso proteger a alça?"); acoes.Add(() => Iniciar(PedirBaldeRaimundo())); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarOpcoes("Raimundo", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator ConfirmarNomeRaimundo()
    {
        if (Dados.curraisIdentidadeConfirmada) yield break;
        yield return FalarPosto("Raimundo", "Falei com outra mulher da lavagem. Ela conhece Nair Pereira.",
            "Zefinha", "Nair Pereira?", "Raimundo", "Mostrei como era a mulher que eu vi junto de tua irmã. Ela confirmou o nome.");
        if (Dados.curraisEscolhaAgua == 1)
            yield return FalarPosto("Mãe", "Foi com essa mulher que o senhor viu minha filha?",
                "Raimundo", "Foi. A menina tava viva, andando junto dela.");
        else yield return FalarPosto("Luzia", "Foi essa mulher que tu viu com a menina?",
            "Raimundo", "Foi. A menina tava viva, andando junto dela.");
        yield return FalarPosto("Zefinha", "E pra onde elas foram?", "Raimundo", "Pra onde chegaram, eu num vi.");
        if (Dados.curraisDestinoConfirmado)
            yield return FalarPosto("Zefinha", "Nós já tem o nome de Lagoa do Meio. Agora tem o dela também.",
                "Raimundo", "Pergunte pelos dois. Pode ser que alguém reconheça.");
        else yield return FalarPosto("Zefinha", "Agora eu posso perguntar pelo nome dela.", "Raimundo", "Nair Pereira. Guarde esse nome.");
        Dados.curraisIdentidadeConfirmada = true;
        GameProgress.Instance.UnlockDiaryPage("currais-nair"); Salvar();
        if (Dados.curraisEscolhaAgua == 1)
            yield return FalarPosto("Zefinha", "Vamos contar a painho.", "Mãe", "Vamos. Depois nós pergunta por esse nome.");
        else yield return FalarPosto("Zefinha", "Vou contar a mainha e painho.", "Luzia", "Vai lá, menina.");
    }

    private IEnumerator SolicitarBaldeLinear()
    {
        if (!PodeOferecerAlca) yield break;
        if (TemItem(tiraPanoLavagem)) yield return Dizer(pedidoBaldeFalas);
        else yield return FalarPosto("Zefinha", "Um pano enrolado aí num ajudava?",
            "Raimundo", "Ajudava. Mas precisa ser um pedaço limpo.", "Zefinha", "A lavadeira separou um pra mim. Vou buscar.");
        Dados.curraisBaldeSolicitado = true;
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator RecolherRetalhoManha()
    {
        if (!RetalhoDisponivelManha) yield break;
        yield return ExaminarOuColetar(tiraPanoLavagem);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisTiraPanoRecolhida = true;
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator DevolverBaldeLinear()
    {
        if (Dados.curraisBaldeApoiado || !Dados.curraisBaldeRecolhido) yield break;
        if (!TemItem(baldeProtegidoItem))
        {
            yield return DizerEntradaUmaVez("raimundo-balde-sem-pano", baldeAindaSemPano);
            yield break;
        }
        InventoryManager.Instance.RemoveItem(baldeProtegidoItem);
        Dados.curraisBaldeApoiado = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(baldeDevolvidoFalas);
        if (!Dados.curraisIdentidadeConfirmada)
        {
            yield return FalarPosto("Raimundo", "E consegui uma notícia daquela mulher.");
            yield return ConfirmarNomeRaimundo();
        }
    }

    private IEnumerator ContarNomeFamiliaManha()
    {
        if (!NomeNovoParaFamilia) yield break;
        if (Dados.curraisEscolhaAgua == 1)
            yield return FalarPosto("Pai", "Raimundo conseguiu saber?", "Zefinha",
                "A mulher se chama Nair Pereira. Foi com ela que ele viu Mariquinha.");
        else yield return FalarPosto("Mãe", "Vocês falaram com Raimundo?", "Zefinha",
            "Falamos. A mulher que ele viu com Mariquinha se chama Nair Pereira.");
        yield return FalarPosto("Pai", "Então nós pergunta por ela também.");
        RegistrarConversaEntrada("familia-nair-contada"); Salvar();
    }

    private bool InteragirFamiliaNomeNovo(Vector2 ponto)
    {
        if (!AntesDaUltimaConsulta || FamiliaNaConsulta || !NomeNovoParaFamilia) return false;
        Collider2D alvo = Dados.curraisEscolhaAgua == 1 ? areaPaiAbrigo : areaMae;
        if (!alvo.enabled || !alvo.gameObject.activeInHierarchy || !alvo.OverlapPoint(ponto)) return false;
        MenuAssuntoNPC("Família", string.Empty, "Contar o nome da mulher", ContarNomeFamiliaManha);
        return true;
    }

    private void AtualizarCompanhiaManha(bool livre)
    {
        bool maeAcompanha = Dados.curraisEscolhaAgua == 1;
        seguidorPaiConsulta.enabled = false;
        seguidoraMae.enabled = livre && maeAcompanha;
        seguidoraLuzia.enabled = false;
    }
}

