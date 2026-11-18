using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Criança e mesa dos nomes — Examine e sequência")]
    [SerializeField] private ExamineObject exameMeninoNomes, exameLataNomes, exameFolhasNomes, exameCanhotoNomes;
    [SerializeField] private Fala[] pedidoNomeInicio, pedidoNomeMae, pedidoNomeLuzia, pedidoNomeServico;
    [SerializeField] private Fala[] pedidoNomeProtestoMae, pedidoNomeProtestoLuzia, pedidoNomeFinal;
    [SerializeField] private Fala[] reencontroComDestino, reencontroSemDestino, canhotoIntervencao;
    [SerializeField] private Fala[] canhotoDestino, canhotoGuardadoMae, canhotoGuardadoLuzia;
    private ExamineObject examineNomesAtual;
    private int quadroExamineNomes;

    private bool CanhotoPendenteDeColeta => ViuConversaEntrada("nomes-canhoto-ouvido") &&
        !ViuConversaEntrada("nomes-canhoto-recolhido") && !TemItem(itemCanhoto);

    private bool ReferenciasNomesValidas()
    {
        bool valido = true;
        foreach (ExamineObject exame in new[] { exameMeninoNomes, exameLataNomes, exameFolhasNomes, exameCanhotoNomes })
            valido &= ConferirReferencia(exame, "Examine — criança e nomes") &&
                ConferirReferencia(exame.examinePanel, "Painel de Examine — criança e nomes") &&
                ConferirReferencia(exame.examineText, "Texto de Examine — criança e nomes");
        foreach (Fala[] falas in new[] { pedidoNomeInicio, pedidoNomeMae, pedidoNomeLuzia, pedidoNomeServico,
            pedidoNomeProtestoMae, pedidoNomeProtestoLuzia, pedidoNomeFinal, reencontroComDestino,
            reencontroSemDestino, canhotoIntervencao, canhotoDestino, canhotoGuardadoMae, canhotoGuardadoLuzia })
            valido &= FalasValidas(falas);
        return valido;
    }

    private IEnumerator ExamineNomesUmaVez(string id, ExamineObject exame)
    {
        if (ViuConversaEntrada(id)) yield break;
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        FecharDialogo();
        examineNomesAtual = exame;
        quadroExamineNomes = Time.frameCount;
        exame.ShowPanel();
        while (exame.PanelIsOpen) yield return null;
        examineNomesAtual = null;
        RegistrarConversaEntrada(id); Salvar();
    }

    private bool AtualizarExamineNomes()
    {
        if (examineNomesAtual == null) return false;
        if (examineNomesAtual.PanelIsOpen && Time.frameCount > quadroExamineNomes &&
            Input.GetMouseButtonDown(0) && !SobreControle()) examineNomesAtual.HidePanel();
        return true;
    }

    private void MenuCriancaNomes()
    {
        if (Dados.curraisEscolhaAgua == 0)
        { Iniciar(ExamineNomesUmaVez("nomes-menino-antes-agua", exameMeninoNomes)); return; }
        if (!Dados.curraisMeninoVisitado)
        {
            MostrarOpcoes("Criança", string.Empty, new[] { "Conversar com o menino", "Sair" },
                new Action[] { () => Iniciar(ConhecerMenino()), FecharMenu });
            return;
        }
        if (Dados.curraisLotePanosEntregue && !ViuConversaEntrada("nomes-reencontro-menino"))
        {
            MostrarOpcoes("Criança", string.Empty, new[] { "Conversar de novo", "Sair" },
                new Action[] { () => Iniciar(ReencontrarMenino()), FecharMenu });
            return;
        }
        MostrarAssuntosMenino();
    }

    private void MostrarAssuntosMenino()
    {
        if (!Dados.curraisMeninoVisitado || Dados.curraisEscolhaAgua == 0) return;
        var rotulos = new List<string>(); var acoes = new List<Action>();
        if ((Dados.curraisPerguntasMenino & 1) == 0)
        { rotulos.Add("Tu espera aqui faz tempo?"); acoes.Add(() => Iniciar(ResponderMenino(1, meninoEspera))); }
        if ((Dados.curraisPerguntasMenino & 2) == 0)
        { rotulos.Add("Dizer o nome de Mariquinha"); acoes.Add(() => Iniciar(ResponderMenino(2, meninoNome))); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarMenuEntrada("nomes-menino-assuntos", "Criança", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator ConversarLataNomes()
    {
        if (ViuConversaEntrada("nomes-conversa-lata")) yield break;
        yield return Dizer(lataMeninoFalas);
        RegistrarConversaEntrada("nomes-conversa-lata");
        GameProgress.Instance.UnlockDiaryPage("currais-menino-espera"); Salvar();
    }

    private IEnumerator ReencontroNomes()
    {
        if (!Dados.curraisLotePanosEntregue || ViuConversaEntrada("nomes-reencontro-menino")) yield break;
        yield return Dizer(meninoReencontro);
        yield return Dizer(Dados.curraisDestinoConfirmado ? reencontroComDestino : reencontroSemDestino);
        if ((Dados.curraisPerguntasMenino & 2) != 0) yield return Dizer(meninoNomeLembrado);
        RegistrarConversaEntrada("nomes-reencontro-menino"); Salvar();
    }

    private IEnumerator DecidirCanhotoNomes()
    {
        if (!Dados.curraisLotePanosRecolhido || Dados.curraisEscolhaAgua == 0) yield break;
        if (CanhotoPendenteDeColeta) { yield return RecolherCanhotoNomes(); yield break; }
        if (Dados.curraisPanelaConcluida) yield break;
        yield return ExamineNomesUmaVez("nomes-examine-canhoto", exameCanhotoNomes);
        yield return DizerEntradaUmaVez("nomes-fiscal-canhoto", canhotoIntervencao);
        yield return Escolher("Zefinha", string.Empty,
            new[] { "Perguntar pelo destino do carro", "Seguir com os panos", "Voltar" });
        if (escolhaRecebida < 0 || escolhaRecebida > 1) yield break;
        bool perguntar = escolhaRecebida == 0;
        if (perguntar)
        {
            yield return Dizer(canhotoDestino);
            Dados.curraisDestinoConfirmado = true;
            RegistrarConversaEntrada("nomes-canhoto-ouvido");
        }
        Dados.curraisEscolhaPertence = perguntar ? 2 : 0;
        Dados.curraisLacoGuardado = false; Dados.curraisLacoNaPanela = false;
        Dados.curraisPanelaConcluida = true;
        AtualizarEstadoBusca(); Salvar();
        if (perguntar) yield return RecolherCanhotoNomes();
        else yield return Dizer(seguirPanos);
    }

    private IEnumerator RecolherCanhotoNomes()
    {
        if (!CanhotoPendenteDeColeta) yield break;
        yield return ExaminarOuColetar(itemCanhoto);
        if (!coletaDoCenarioConfirmada) yield break;
        RegistrarConversaEntrada("nomes-canhoto-recolhido");
        GameProgress.Instance.UnlockDiaryPage("currais-canhoto");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(Dados.curraisEscolhaAgua == 1 ? canhotoGuardadoMae : canhotoGuardadoLuzia);
    }
}
