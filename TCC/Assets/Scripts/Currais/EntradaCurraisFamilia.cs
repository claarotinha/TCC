using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Família — assuntos que não alteram os finais")]
    [SerializeField] private Fala[] familiaPromessas, familiaTrabalho, familiaCaderno, noiteCaderno;
    [SerializeField] private GameObject diarioNaNoite;
    [Header("Noite — poses opcionais da autora na Hierarchy")]
    [SerializeField] private SpriteRenderer[] personagensNoAbrigo, posesDeRepouso;
    [SerializeField] private Fala[] amanhecerFamilia, baldeAindaSemPano;
    private bool[] visibilidadeAntesDoRepouso;
    private bool repousoAplicado;

    private void AtualizarRepousoFamilia()
    {
        bool ativa = PostoDisponivel && (noiteEmCurso || Dados.curraisEtapaPosto == 4) && Dados.curraisEtapaConsulta == 0;
        if (repousoAplicado == ativa) return;
        repousoAplicado = ativa;
        if (personagensNoAbrigo == null || posesDeRepouso == null) return;
        if (ativa) visibilidadeAntesDoRepouso = new bool[personagensNoAbrigo.Length];
        for (int i = 0; i < personagensNoAbrigo.Length && i < posesDeRepouso.Length; i++)
        {
            SpriteRenderer original = personagensNoAbrigo[i], pose = posesDeRepouso[i];
            if (original == null || pose == null) continue;
            if (ativa)
            {
                visibilidadeAntesDoRepouso[i] = original.enabled;
                // Slot vazio preserva o ciclo da autora. Não gira idle para fingir sono.
                bool temPose = pose.sprite != null;
                pose.gameObject.SetActive(temPose);
                if (temPose) original.enabled = false;
            }
            else
            {
                pose.gameObject.SetActive(false);
                if (visibilidadeAntesDoRepouso != null && i < visibilidadeAntesDoRepouso.Length)
                    original.enabled = visibilidadeAntesDoRepouso[i];
            }
        }
    }

    private void EncerrarNoiteFamilia()
    {
        noiteEmCurso = false;
        // A restauração visual também ocorre ao desativar o componente ou interromper uma fala.
        if (repousoAplicado)
        {
            repousoAplicado = false;
            for (int i = 0; i < personagensNoAbrigo.Length && i < posesDeRepouso.Length; i++)
            {
                if (posesDeRepouso[i] != null) posesDeRepouso[i].gameObject.SetActive(false);
                if (personagensNoAbrigo[i] != null && visibilidadeAntesDoRepouso != null && i < visibilidadeAntesDoRepouso.Length)
                    personagensNoAbrigo[i].enabled = visibilidadeAntesDoRepouso[i];
            }
        }
        AplicarNoite(false);
    }

    [Header("Raimundo — balde e alça protegida")]
    [SerializeField] private ItemData baldeRaimundoItem, baldeProtegidoItem;
    [SerializeField] private ItemCombination combinacaoBalde;
    [SerializeField] private Collider2D areaBaldeRaimundo;
    [SerializeField] private GameObject baldeRaimundoCenario;
    [SerializeField] private Fala[] pedidoBaldeFalas, baldeDevolvidoFalas, nomeRaimundoFalas, papelApresentado;

    private bool ReferenciasFamiliaAlternativaValidas()
    {
        bool ok = ConferirReferencia(diarioNaNoite, nameof(diarioNaNoite)) &
            ConferirReferencia(baldeRaimundoItem, nameof(baldeRaimundoItem)) &
            ConferirReferencia(baldeProtegidoItem, nameof(baldeProtegidoItem)) &
            ConferirReferencia(combinacaoBalde, nameof(combinacaoBalde)) &
            ConferirReferencia(areaBaldeRaimundo, nameof(areaBaldeRaimundo)) &
            ConferirReferencia(baldeRaimundoCenario, nameof(baldeRaimundoCenario));
        foreach (Fala[] falas in new[] { familiaPromessas, familiaTrabalho, familiaCaderno, noiteCaderno,
            pedidoBaldeFalas, baldeDevolvidoFalas, nomeRaimundoFalas, papelApresentado, amanhecerFamilia, baldeAindaSemPano }) ok &= FalasValidas(falas);
        ok &= personagensNoAbrigo != null && posesDeRepouso != null &&
            personagensNoAbrigo.Length == 4 && posesDeRepouso.Length == 4;
        if (ok)
            for (int i = 0; i < 4; i++)
                ok &= ConferirReferencia(personagensNoAbrigo[i], "personagensNoAbrigo[" + i + "]") &
                    ConferirReferencia(posesDeRepouso[i], "posesDeRepouso[" + i + "]");
        return ok && combinacaoBalde.itemA == baldeRaimundoItem &&
            combinacaoBalde.itemB == tiraPanoLavagem && combinacaoBalde.result == baldeProtegidoItem;
    }

    private void PrepararFamiliaAlternativa()
    {
        CombinationPrompt prompt = InventoryTabController.Instance != null
            ? InventoryTabController.Instance.GetComponent<CombinationPrompt>() : null;
        if (prompt != null) prompt.RegisterRecipe(combinacaoBalde);
        // Repara apenas o empréstimo interrompido, respeitando a combinação já feita.
        if (Dados.curraisBaldeRecolhido && !Dados.curraisBaldeApoiado &&
            !TemItem(baldeRaimundoItem) && !TemItem(baldeProtegidoItem))
            InventoryManager.Instance.AddItem(baldeRaimundoItem);
        AtualizarFamiliaAlternativa();
    }

    private void AtualizarFamiliaAlternativa()
    {
        AtualizarRepousoFamilia();
        bool manha = ConsultaDisponivel && Dados.curraisEtapaConsulta == 1;
        // A mesma pessoa e o mesmo collider da conversa inicial são reutilizados.
        if (manha) areaRaimundo.enabled = true;
        else if (Dados.curraisEtapaBusca > 0) areaRaimundo.enabled = PrimeiraBuscaEmCurso;
        bool pegar = manha && Dados.curraisBaldeSolicitado && !Dados.curraisBaldeRecolhido && !Dados.curraisBaldeApoiado;
        baldeRaimundoCenario.SetActive(pegar || (ConsultaDisponivel && Dados.curraisBaldeApoiado));
        panoAlca.SetActive(PostoDisponivel && Dados.curraisBaldeApoiado);
        areaBaldeRaimundo.enabled = pegar;
        diarioNaNoite.SetActive(TemCadernoNaNoite &&
            (noiteEmCurso || (Dados.curraisEtapaPosto == 4 && Dados.curraisEtapaConsulta == 0)));
    }

    private IEnumerator ConversarFamilia(int bit, Fala[] falas)
    {
        if ((Dados.curraisPerguntasFamilia & bit) != 0) yield break;
        yield return Dizer(falas);
        Dados.curraisPerguntasFamilia |= bit;
        Salvar(); MenuCampoFamilia();
    }

    private void MenuLembrancasFamilia()
    {
        MenuCasaFamilia();
    }

    private void MenuRaimundoManha()
    {
        Iniciar(AbrirRaimundoManha());
    }

    private IEnumerator ConversarRaimundoManha()
    {
        yield return ConfirmarNomeRaimundo();
        MenuRaimundoLinear();
    }

    private IEnumerator PedirBaldeRaimundo()
    {
        yield return SolicitarBaldeLinear();
    }

    private IEnumerator PegarBaldeRaimundo()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 1 || !Dados.curraisBaldeSolicitado || Dados.curraisBaldeRecolhido || Dados.curraisBaldeApoiado) yield break;
        yield return ExaminarOuColetar(baldeRaimundoItem);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisBaldeRecolhido = true;
        AtualizarFamiliaAlternativa(); Salvar();

    }

    private IEnumerator DevolverBaldeRaimundo()
    {
        yield return DevolverBaldeLinear();
    }
}
