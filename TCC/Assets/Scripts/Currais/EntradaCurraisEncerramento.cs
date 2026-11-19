using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Encerramento — família e nomes guardados")]
    [SerializeField] private Fala[] aberturaEncerramento, bentoEncerramento, fimFamiliaEncerramento;
    [Header("Passagens do tempo — imagens existentes")]
    [SerializeField] private GameObject[] imagensPassagem;

    private bool ReferenciasEncerramentoValidas() => FalasValidas(aberturaEncerramento) &&
        FalasValidas(bentoEncerramento) && FalasValidas(fimFamiliaEncerramento);

    private void MenuNomesFinais() => MostrarAssuntosFamiliaFinal();

    private void MostrarAssuntosFamiliaFinal()
    {
        if (!NegacaoAcolhida)
        {
            MenuAssuntoNPC("Zefinha", "Eu ainda preciso procurar Mariquinha.",
                "Continuar a conversa", RetomarNegacaoDaBusca);
            return;
        }
        var rotulos = new List<string>(); var acoes = new List<Action>();
        if ((Dados.curraisAssuntosEncerramento & 4) == 0)
        { rotulos.Add("Lembrar Bento"); acoes.Add(() => Iniciar(RelembrarAssuntoFinal(4, bentoEncerramento))); }
        if ((Dados.curraisAssuntosEncerramento & 8) == 0)
        { rotulos.Add("Ficar junto da família"); acoes.Add(() => Iniciar(RelembrarAssuntoFinal(8, fimFamiliaEncerramento))); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarOpcoes("Família", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator RelembrarAssuntoFinal(int bit, Fala[] falas)
    {
        if ((Dados.curraisAssuntosEncerramento & bit) != 0) yield break;
        yield return Dizer(falas);
        Dados.curraisAssuntosEncerramento |= bit;
        Salvar(); MostrarAssuntosFamiliaFinal();
    }

    private void MenuEsteiraFinal()
    {
        MostrarOpcoes("Mãe", string.Empty,
            new[] { "Tentar descansar", "Conversar com a família", "Sair" },
            new Action[] { () => Iniciar(FecharConversaFinal()), MenuNomesFinais, FecharMenu });
    }

    private IEnumerator FecharConversaFinal()
    {
        if (Dados.curraisEtapaConsulta < 3 || !ViuConversaEntrada("consulta-relato-final")) yield break;
        if (!NegacaoAcolhida)
        {
            yield return NegarFimDaBusca();
            if (!NegacaoAcolhida || Dados.ato2Concluido) yield break;
        }
        // Descansar não exige esgotar os assuntos opcionais.
        yield return Escurecer(1f);
        Posicionar(mari.transform, mariNoite); Posicionar(mae, maeNoite);
        Dados.curraisEtapaConsulta = 4;
        GameProgress.Instance.UnlockDiaryPage("currais-pistas-finais");
        AtualizarEstadoBusca(); Salvar();
        yield return ExibirPaginasMemoria();
    }

    private void MostrarPassagemMemoria(int indice)
    {
        if (imagensPassagem == null) return;
        for (int i = 0; i < imagensPassagem.Length; i++)
            if (imagensPassagem[i] != null) imagensPassagem[i].SetActive(i == indice);
    }
}
