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
        bool ativa = PostoDisponivel && Dados.curraisEtapaPosto == 4 && Dados.curraisEtapaConsulta == 0;
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
        baldeRaimundoCenario.SetActive(pegar || (manha && Dados.curraisBaldeApoiado));
        panoAlca.SetActive(manha && Dados.curraisBaldeApoiado);
        areaBaldeRaimundo.enabled = pegar;
        diarioNaNoite.SetActive(noiteEmCurso || (Dados.curraisEtapaPosto == 4 && Dados.curraisEtapaConsulta == 0));
    }

    private IEnumerator ConversarFamilia(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisPerguntasFamilia |= bit;
        Salvar(); MenuAbrigo();
    }

    private void MenuLembrancasFamilia()
    {
        bool refeicao = Dados.secaFoodChoice >= 1 && Dados.secaFoodChoice <= 3;
        string[] rotulos = refeicao ? new[] { "Repetir as notícias", "Lembrar da refeição", "O caderno", "Voltar" }
            : new[] { "Repetir as notícias", "O caderno", "Voltar" };
        Action noticias = () => Iniciar(ReverNoticiasFamilia());
        Action caderno = () => Iniciar(ConversarFamilia(4, familiaCaderno));
        Action[] acoes = refeicao ? new Action[] { noticias, () => Iniciar(RelembrarRefeicao()), caderno, MenuAbrigo }
            : new Action[] { noticias, caderno, MenuAbrigo };
        MostrarOpcoes("Família", "O que você quer conversar?", rotulos, acoes);
    }

    private IEnumerator ReverNoticiasFamilia()
    {
        yield return NoticiasDaBusca();
        MenuLembrancasFamilia();
    }

    private void MenuRaimundoManha()
    {
        MostrarOpcoes("Raimundo", Dados.curraisBaldeApoiado ? "Obrigado pela ajuda, Josefa. Vamos guardar o nome com cuidado."
            : "A alça do balde continua cortando minha mão.",
            new[] { "O que descobriu da mulher?", "Posso proteger a alça?", "Devolver o balde protegido", "Sair" },
            new Action[] { () => Iniciar(ConversarRaimundoManha()), () => Iniciar(PedirBaldeRaimundo()),
                () => Iniciar(DevolverBaldeRaimundo()), FecharMenu });
    }

    private IEnumerator ConversarRaimundoManha()
    {
        // A informação depende de perguntar, não de uma recompensa moral por ajudar.
        Dados.curraisIdentidadeConfirmada = true;
        GameProgress.Instance.UnlockDiaryPage("currais-nair");
        Salvar();
        yield return Dizer(nomeRaimundoFalas);
        MenuRaimundoManha();
    }

    private IEnumerator PedirBaldeRaimundo()
    {
        if (Dados.curraisBaldeApoiado)
        { yield return Dizer(new[] { new Fala { personagem = "Raimundo", texto = "A alça já tá protegida. Obrigado." } }); yield break; }
        yield return Dizer(pedidoBaldeFalas);
        Dados.curraisBaldeSolicitado = true;
        AtualizarFamiliaAlternativa(); Salvar();
        objetivoTexto.text = string.Empty;
    }

    private IEnumerator PegarBaldeRaimundo()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 1 || !Dados.curraisBaldeSolicitado || Dados.curraisBaldeRecolhido || Dados.curraisBaldeApoiado) yield break;
        yield return ExaminarOuColetar(baldeRaimundoItem);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisBaldeRecolhido = true;
        InventoryManager.Instance.AddItem(baldeRaimundoItem);
        AtualizarFamiliaAlternativa(); Salvar();

    }

    private IEnumerator DevolverBaldeRaimundo()
    {
        if (Dados.curraisBaldeApoiado)
        { yield return Dizer(baldeDevolvidoFalas); yield break; }
        if (!Dados.curraisBaldeRecolhido || !TemItem(baldeProtegidoItem))
        {
            yield return Dizer(baldeAindaSemPano);
            MenuRaimundoManha();
            yield break;
        }
        Dados.curraisBaldeApoiado = true;
        InventoryManager.Instance.RemoveItem(baldeProtegidoItem);
        Dados.curraisIdentidadeConfirmada = true;
        GameProgress.Instance.UnlockDiaryPage("currais-nair");
        AtualizarFamiliaAlternativa(); Salvar();
        yield return Dizer(baldeDevolvidoFalas);
        yield return Dizer(nomeRaimundoFalas);
    }
}
