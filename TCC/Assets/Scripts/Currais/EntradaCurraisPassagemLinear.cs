using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Passagem vigiada — Examine e variações da acompanhante")]
    [SerializeField] private ExamineObject exameGuardaPassagem;
    [SerializeField] private Fala[] guardaPassagemPergunta, guardaPassagemMae, guardaPassagemLuzia;
    [SerializeField] private Fala[] chamadaPassagemMae, chamadaPassagemLuzia;
    [SerializeField] private Fala[] reacaoCorpoPassagem, reacaoComadrePassagem, reacaoAguaPassagem, reacaoSemLendaPassagem;
    [SerializeField] private Fala[] continuarPassagemMae, continuarPassagemLuzia;

    private bool ReferenciasPassagemLinearValidas()
    {
        bool valido = ConferirReferencia(exameGuardaPassagem, nameof(exameGuardaPassagem)) &&
            ConferirReferencia(exameGuardaPassagem.examinePanel, "Painel de Examine do guarda") &&
            ConferirReferencia(exameGuardaPassagem.examineText, "Texto de Examine do guarda");
        foreach (Fala[] falas in new[] { guardaPassagemPergunta, guardaPassagemMae, guardaPassagemLuzia,
            chamadaPassagemMae, chamadaPassagemLuzia, reacaoCorpoPassagem, reacaoComadrePassagem,
            reacaoAguaPassagem, reacaoSemLendaPassagem, continuarPassagemMae, continuarPassagemLuzia })
            valido &= FalasValidas(falas);
        // A primeira fala é a percepção de Zefinha; as demais vêm depois de ocultar a imagem.
        foreach (Fala[] reacao in new[] { reacaoCorpoPassagem, reacaoComadrePassagem, reacaoAguaPassagem, reacaoSemLendaPassagem })
            valido &= reacao != null && reacao.Length >= 2;
        return valido;
    }

    private IEnumerator AbrirGuardaPassagem()
    {
        yield return ExamineNomesUmaVez("passagem-examine-guarda", exameGuardaPassagem);
        if (Dados.curraisEtapaBusca != 3 || !Dados.curraisTiraPanoRecolhida ||
            Dados.curraisPassagemVigiadaConcluida) yield break;
        AbrirAssuntosEntrada("passagem-guarda", "Guarda", string.Empty,
            new[] { "passagem-pergunta-guarda" }, new[] { "Perguntar pela passagem" },
            new Func<IEnumerator>[] { ConversarGuardaPassagem });
    }

    private IEnumerator ConversarGuardaPassagem()
    {
        yield return Dizer(guardaPassagemPergunta);
        yield return Dizer(guardaPassagemMae);
    }
}

