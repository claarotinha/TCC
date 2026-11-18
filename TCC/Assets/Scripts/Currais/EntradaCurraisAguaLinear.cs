using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Distribuição — sequência obrigatória e Examine")]
    [SerializeField] private ExamineObject exameDistribuicao;
    [SerializeField] private Fala[] autorizarCabacaAgua, chamadaDistribuicaoAgua;
    [SerializeField] private Fala[] apresentacaoPorcaoAgua, porcaoRecebidaAgua, retornoAbrigoAgua, necessidadeAguaAbrigo;
    [SerializeField] private Fala[] ofertaAguaMae, ofertaAguaLuzia, depoisAguaMae, depoisAguaLuzia, guardarCabacaAgua;
    [SerializeField, Min(.1f)] private float esperaChamadaAgua = 2f;
    [SerializeField, Min(.5f)] private float alcanceRetornoAgua = 4f;
    private bool exameDistribuicaoEmCurso;
    private int quadroExamineDistribuicao;

    // Estados antigos avançados continuam válidos, sem retirar água ou repetir a escolha.
    private bool CabacaAutorizada => ViuConversaEntrada("agua-cabaca-autorizada") ||
        Dados.curraisCabacaDevolvida || Dados.curraisAguaRecebida || Dados.curraisEscolhaAgua != 0;
    private bool VezAguaChamada => ViuConversaEntrada("agua-vez-chamada") ||
        Dados.curraisAguaRecebida || Dados.curraisEscolhaAgua != 0;
    private bool PodeEncherCabaca => ConviteAguaConhecido && Dados.curraisPaiAcomodado &&
        Dados.curraisEtapaEntrada == 4 && Dados.curraisCabacaDevolvida && VezAguaChamada && !Dados.curraisAguaRecebida;
    private bool PodeEscolherAgua => Dados.curraisAguaRecebida && Dados.curraisEscolhaAgua == 0 &&
        ViuConversaEntrada("agua-retorno-abrigo");

    private bool ReferenciasAguaLinearValidas() =>
        ConferirReferencia(exameDistribuicao, nameof(exameDistribuicao)) &&
        ConferirReferencia(exameDistribuicao.examinePanel, "Painel de Examine da distribuição") &&
        ConferirReferencia(exameDistribuicao.examineText, "Texto de Examine da distribuição") &&
        FalasValidas(autorizarCabacaAgua) && FalasValidas(chamadaDistribuicaoAgua) &&
        FalasValidas(apresentacaoPorcaoAgua) && FalasValidas(porcaoRecebidaAgua) &&
        FalasValidas(retornoAbrigoAgua) && FalasValidas(necessidadeAguaAbrigo) && FalasValidas(ofertaAguaMae) && FalasValidas(ofertaAguaLuzia) &&
        FalasValidas(depoisAguaMae) && FalasValidas(depoisAguaLuzia) && FalasValidas(guardarCabacaAgua);

    private void MenuDistribuicaoLinear()
    {
        if (!ConviteAguaConhecido || !Dados.curraisPaiAcomodado || Dados.curraisAguaRecebida) return;
        if (Dados.curraisCabacaDevolvida)
        {
            if (!VezAguaChamada) Iniciar(ChamarVezDaAgua());
            return;
        }
        if (CabacaAutorizada) return;
        if (!ViuConversaEntrada("agua-examine-distribuicao"))
        {
            FecharDialogo(); PausarExploracao();
            if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
            exameDistribuicaoEmCurso = true;
            quadroExamineDistribuicao = Time.frameCount;
            exameDistribuicao.ShowPanel();
            RegistrarConversaEntrada("agua-examine-distribuicao"); Salvar();
            return;
        }
        MostrarConversaCabaca();
    }

    private bool AtualizarExamineDistribuicao()
    {
        if (!exameDistribuicaoEmCurso) return false;
        PausarExploracao();
        if (exameDistribuicao.PanelIsOpen)
        {
            if (Time.frameCount > quadroExamineDistribuicao && Input.GetMouseButtonDown(0))
                ExamineObject.TryCloseCurrentPanelFromPointer();
            return true;
        }
        exameDistribuicaoEmCurso = false;
        MostrarConversaCabaca();
        return true;
    }

    private void MostrarConversaCabaca() => MostrarOpcoes("Funcionária", string.Empty,
        new[] { "Falar da cabaça", "Sair" }, new Action[] { () => Iniciar(AutorizarCabacaAgua()), FecharMenu });

    private IEnumerator AutorizarCabacaAgua()
    {
        if (CabacaAutorizada) yield break;
        yield return Dizer(autorizarCabacaAgua);
        RegistrarConversaEntrada("agua-cabaca-autorizada");
        AtualizarCabacaMesa(); Salvar();
    }

    private IEnumerator ChamarVezDaAgua()
    {
        if (VezAguaChamada || !Dados.curraisCabacaDevolvida || !ConviteAguaConhecido) yield break;
        yield return Esperar(esperaChamadaAgua);
        yield return Dizer(chamadaDistribuicaoAgua);
        RegistrarConversaEntrada("agua-vez-chamada"); Salvar();
    }

    private bool AtualizarRetornoAgua(bool livre)
    {
        if (livre && !Dados.curraisPanelaConcluida && ConviteAguaConhecido &&
            Dados.curraisCabacaDevolvida && !VezAguaChamada)
        { Iniciar(ChamarVezDaAgua()); return true; }
        if (!livre || Dados.curraisPanelaConcluida || Dados.curraisEtapaEntrada != 4 ||
            !Dados.curraisAguaRecebida || Dados.curraisEscolhaAgua != 0 ||
            ViuConversaEntrada("agua-retorno-abrigo")) return false;
        if (Mathf.Abs(mari.transform.position.x - mae.position.x) > alcanceRetornoAgua) return false;
        Iniciar(RetornarComAgua());
        return true;
    }

    private IEnumerator RetornarComAgua()
    {
        yield return Dizer(retornoAbrigoAgua);
        // A porção oferecida ao pai não esvazia o recipiente: resta água para a escolha.
        yield return BeberAguaAbrigo(1);
        yield return Dizer(necessidadeAguaAbrigo);
        RegistrarConversaEntrada("agua-retorno-abrigo"); Salvar();
    }
}
