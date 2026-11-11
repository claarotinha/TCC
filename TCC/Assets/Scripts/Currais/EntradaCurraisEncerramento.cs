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
        MostrarOpcoes("Família", InformacoesConsulta() + "\nO que vamos guardar desta conversa?",
            new[] { Dados.curraisDestinoConfirmado ? "Contar o lugar: Lagoa do Meio" : "Ainda não sabemos o lugar",
                Dados.curraisIdentidadeConfirmada ? "Contar o nome: Nair Pereira" : "Ainda não sabemos o nome",
                "Lembrar Bento", "Guardar os nomes e descansar" },
            new Action[] { () => Iniciar(RelembrarAssuntoFinal(1, Dados.curraisDestinoConfirmado ? lugarEncerramento : faltaLugarEncerramento)),
                () => Iniciar(RelembrarAssuntoFinal(2, Dados.curraisIdentidadeConfirmada ? nomeEncerramento : faltaNomeEncerramento)),
                () => Iniciar(RelembrarAssuntoFinal(4, bentoEncerramento)), () => Iniciar(FecharConversaFinal()) });
    }

    private IEnumerator RelembrarAssuntoFinal(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisAssuntosEncerramento |= bit;
        Salvar(); MenuNomesFinais();
    }

    private IEnumerator FecharConversaFinal()
    {
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
