using System;
using System.Collections;
using UnityEngine;

// Cenas 19–21. Relembrar um assunto nunca inventa uma pista ou altera o destino da irmã.
public partial class EntradaCurrais
{
    [Header("Encerramento — assuntos da família")]
    [SerializeField] private Fala[] aberturaEncerramento, lugarEncerramento, nomeEncerramento;
    [SerializeField] private Fala[] faltaLugarEncerramento, faltaNomeEncerramento, bentoEncerramento, fimFamiliaEncerramento;
    [Header("Passagens do tempo — imagens existentes")]
    [SerializeField] private GameObject[] imagensPassagem;

    private bool ReferenciasEncerramentoValidas()
    {
        foreach (Fala[] falas in new[] { aberturaEncerramento, lugarEncerramento, nomeEncerramento,
            faltaLugarEncerramento, faltaNomeEncerramento, bentoEncerramento, fimFamiliaEncerramento })
            if (!FalasValidas(falas)) return false;
        return true;
    }

    private void MenuNomesFinais()
    {
        MostrarOpcoes("Família", "Ainda podemos conversar antes de descansar.",
            new[] { "O lugar e o nome", "Bento e as lembranças", "Sair" },
            new Action[] { MenuPistasFinais, MenuOutrosAssuntosFinais, FecharMenu });
    }

    private void MenuPistasFinais()
    {
        MostrarOpcoes("Família", "Vamos guardar o que foi confirmado, sem completar o que ainda falta.",
            new[] { Dados.curraisDestinoConfirmado ? "O lugar: Lagoa do Meio" : "Ainda falta o lugar",
                Dados.curraisIdentidadeConfirmada ? "O nome: Nair Pereira" : "Ainda falta o nome", "Voltar" },
            new Action[] { () => Iniciar(RelembrarAssuntoFinal(1, Dados.curraisDestinoConfirmado ? lugarEncerramento : faltaLugarEncerramento)),
                () => Iniciar(RelembrarAssuntoFinal(2, Dados.curraisIdentidadeConfirmada ? nomeEncerramento : faltaNomeEncerramento)), MenuNomesFinais });
    }

    private void MenuOutrosAssuntosFinais()
    {
        MostrarOpcoes("Família", "Bento também tem nome. Num vamos deixar ele se perder no meio disso tudo.",
            new[] { "Lembrar Bento", "Guardar os nomes e descansar", "Sair" },
            new Action[] { () => Iniciar(RelembrarAssuntoFinal(4, bentoEncerramento)),
                () => Iniciar(FecharConversaFinal()), FecharMenu });
    }

    private IEnumerator RelembrarAssuntoFinal(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisAssuntosEncerramento |= bit;
        Salvar(); MenuNomesFinais();
    }

    private IEnumerator FecharConversaFinal()
    {
        if (Dados.curraisEtapaConsulta != 3) yield break;
        yield return Dizer(fimFamiliaEncerramento);
        Dados.curraisEtapaConsulta = 4;
        GameProgress.Instance.UnlockDiaryPage("currais-pistas-finais");
        AtualizarEstadoBusca(); Salvar();
    }

    private void MostrarPassagemMemoria(int indice)
    {
        if (imagensPassagem == null) return;
        for (int i = 0; i < imagensPassagem.Length; i++)
            if (imagensPassagem[i] != null) imagensPassagem[i].SetActive(i == indice);
    }
}
