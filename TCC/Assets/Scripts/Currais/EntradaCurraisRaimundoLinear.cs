using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Raimundo — Examine e escolhas do primeiro encontro")]
    [SerializeField] private ExamineObject examePrimeiroRaimundo;
    [SerializeField] private Fala[] aproximacaoRaimundoMae, aproximacaoRaimundoLuzia;
    [SerializeField] private Fala[] raimundoPrecisao, raimundoAjuda, raimundoApresentacao, raimundoApresentacaoBreve;
    [SerializeField] private Fala[] raimundoPassado, raimundoGuardas;
    [SerializeField, Min(.5f)] private float alcanceComentarioRaimundo = 6f;

    private bool PodeConversarRaimundo => Dados.curraisEscolhaAgua != 0 &&
        Dados.curraisLotePanosRecolhido && Dados.curraisPanelaConcluida;
    private bool NomeRaimundoConhecido => Dados.curraisRaimundoConhecido || ViuConversaEntrada("lavagem-indicacao-raimundo");
    private bool EncontroRaimundoEncerrado => ViuConversaEntrada("raimundo-encontro-encerrado");

    private bool ReferenciasRaimundoLinearValidas()
    {
        bool valido = ConferirReferencia(examePrimeiroRaimundo, nameof(examePrimeiroRaimundo)) &&
            ConferirReferencia(examePrimeiroRaimundo.examinePanel, "Painel de Examine de Raimundo") &&
            ConferirReferencia(examePrimeiroRaimundo.examineText, "Texto de Examine de Raimundo");
        foreach (Fala[] falas in new[] { aproximacaoRaimundoMae, aproximacaoRaimundoLuzia,
            raimundoPrecisao, raimundoAjuda, raimundoApresentacao, raimundoApresentacaoBreve,
            raimundoPassado, raimundoGuardas }) valido &= FalasValidas(falas);
        return valido;
    }

    private void PrimeiroContatoRaimundo()
    {
        if (EncontroRaimundoEncerrado) return;
        Iniciar(AbrirContatoRaimundo());
    }

    private IEnumerator AbrirContatoRaimundo()
    {
        // A descrição é sempre a primeira interação, inclusive antes de liberar a conversa.
        yield return ExamineNomesUmaVez("raimundo-examine-inicial", examePrimeiroRaimundo);
        if (!PodeConversarRaimundo || EncontroRaimundoEncerrado) yield break;
        if (!ViuConversaEntrada("raimundo-aproximacao")) yield return ComentarRaimundo();
        if (ViuConversaEntrada("raimundo-assunto-respondido"))
        { yield return DeixarRaimundo(); yield break; }
        if (ViuConversaEntrada("raimundo-assunto-passado"))
        { MenuPassadoRaimundo(); yield break; }
        if (ViuConversaEntrada("raimundo-abordagem-encerrar"))
        { yield return EncerrarAntesDosAssuntosRaimundo(); yield break; }
        if (Dados.curraisRaimundoConhecido)
        { MenuAssuntosRaimundoLinear(); yield break; }
        if (ViuConversaEntrada("raimundo-abordagem-precisao") || ViuConversaEntrada("raimundo-abordagem-ajuda"))
        { yield return ApresentarRaimundoLinear(); yield break; }
        if (ViuConversaEntrada("raimundo-pista-inicial"))
        { MenuAbordagemRaimundo(); yield break; }
        MostrarOpcoes("Homem", string.Empty, new[] { "Perguntar pela irmã", "Sair" },
            new Action[] { () => Iniciar(ConhecerRaimundo()), FecharMenu });
    }

    private IEnumerator ConversaInicialRaimundo()
    {
        if (!PodeConversarRaimundo || EncontroRaimundoEncerrado) yield break;
        yield return DizerEntradaUmaVez("raimundo-pista-inicial", raimundoInicio);
        MenuAbordagemRaimundo();
    }

    private void MenuAbordagemRaimundo() => MostrarOpcoes("Homem", string.Empty,
        new[] { "O que o senhor lembra dela?", "Me ajuda a encontrar essa mulher?", "Encerrar a conversa" },
        new Action[] { () => Iniciar(EscolherAbordagemRaimundo(true)),
            () => Iniciar(EscolherAbordagemRaimundo(false)), () => Iniciar(EncerrarAntesDosAssuntosRaimundo()) });

    private IEnumerator EscolherAbordagemRaimundo(bool precisao)
    {
        if (ViuConversaEntrada("raimundo-abordagem-precisao") || ViuConversaEntrada("raimundo-abordagem-ajuda")) yield break;
        yield return Dizer(precisao ? raimundoPrecisao : raimundoAjuda);
        RegistrarConversaEntrada(precisao ? "raimundo-abordagem-precisao" : "raimundo-abordagem-ajuda"); Salvar();
        yield return ApresentarRaimundoLinear();
    }

    private IEnumerator ApresentarRaimundoLinear()
    {
        if (!Dados.curraisRaimundoConhecido)
        {
            yield return Dizer(raimundoApresentacao);
            RegistrarNomeRaimundo();
        }
        MenuAssuntosRaimundoLinear();
    }

    private void RegistrarNomeRaimundo()
    {
        Dados.curraisRaimundoConhecido = true;
        GameProgress.Instance.UnlockDiaryPage("currais-raimundo"); Salvar();
    }

    private IEnumerator EncerrarAntesDosAssuntosRaimundo()
    {
        if (!Dados.curraisRaimundoConhecido)
        {
            yield return Dizer(raimundoApresentacaoBreve);
            RegistrarConversaEntrada("raimundo-abordagem-encerrar");
            RegistrarNomeRaimundo();
        }
        yield return DeixarRaimundo();
    }

    private void MenuAssuntosRaimundoLinear() => MostrarOpcoes("Raimundo", string.Empty,
        new[] { "O senhor já conhecia esse lugar?", "Essa mão tá machucada?", "Preciso continuar" },
        new Action[] { () => Iniciar(EntrarPassadoRaimundo()),
            () => Iniciar(PerguntarRaimundo(4, raimundoTrabalho)), () => Iniciar(DeixarRaimundo()) });

    private IEnumerator EntrarPassadoRaimundo()
    {
        yield return DizerEntradaUmaVez("raimundo-assunto-passado", raimundoPassado);
        MenuPassadoRaimundo();
    }

    private void MenuPassadoRaimundo() => MostrarOpcoes("Raimundo", string.Empty,
        new[] { "Por que entrou no bando?", "E num pode enfrentar os guardas?", "Prefiro continuar" },
        new Action[] { () => Iniciar(PerguntarRaimundo(1, raimundoBando)),
            () => Iniciar(PerguntarRaimundo(2, raimundoGuardas)), () => Iniciar(DeixarRaimundo()) });

    private bool AtualizarAproximacaoRaimundo(bool livre)
    {
        if (!livre || !PodeConversarRaimundo || EncontroRaimundoEncerrado ||
            ViuConversaEntrada("raimundo-aproximacao") || PainelAuxiliarAberto() || InventarioAbertoMemoria()) return false;
        if (Mathf.Abs(mari.transform.position.x - areaRaimundo.bounds.center.x) > alcanceComentarioRaimundo) return false;
        Iniciar(ComentarRaimundo()); return true;
    }

    private IEnumerator ComentarRaimundo()
    {
        yield return DizerEntradaUmaVez("raimundo-aproximacao",
            aproximacaoRaimundoMae);
    }
}

