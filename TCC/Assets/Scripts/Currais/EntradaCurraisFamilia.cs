using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Família — assuntos que não alteram os finais")]
    [SerializeField] private Fala[] familiaPromessas, familiaTrabalho, familiaCaderno, noiteCaderno;
    [SerializeField] private GameObject diarioNaNoite;

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
            pedidoBaldeFalas, baldeDevolvidoFalas, nomeRaimundoFalas, papelApresentado }) ok &= FalasValidas(falas);
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
        bool manha = ConsultaDisponivel && Dados.curraisEtapaConsulta == 1;
        // A mesma pessoa e o mesmo collider da conversa inicial são reutilizados.
        if (manha) areaRaimundo.enabled = true;
        else if (Dados.curraisEtapaBusca > 0) areaRaimundo.enabled = false;
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
        objetivoTexto.text = "Pegue o balde junto de Raimundo. Combine com a tira de pano no inventário e devolva a ele.";
    }

    private IEnumerator PegarBaldeRaimundo()
    {
        if (!Dados.curraisBaldeSolicitado || Dados.curraisBaldeRecolhido) yield break;
        yield return ExaminarOuColetar(baldeRaimundoItem);
        if (!coletaDoCenarioConfirmada) yield break;
        if (!TemItem(baldeRaimundoItem) && !TemItem(baldeProtegidoItem)) InventoryManager.Instance.AddItem(baldeRaimundoItem);
        Dados.curraisBaldeRecolhido = true;
        AtualizarFamiliaAlternativa(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Zefinha", texto =
            "A tira que trouxe da lavagem pode proteger essa alça. Vou arrastar um item sobre o outro no inventário." } });
    }

    private IEnumerator DevolverBaldeRaimundo()
    {
        if (Dados.curraisBaldeApoiado)
        { yield return Dizer(baldeDevolvidoFalas); yield break; }
        if (!TemItem(baldeProtegidoItem))
        {
            yield return Dizer(new[] { new Fala { personagem = "Raimundo", texto =
                "Pegue o balde e combine com a tira de pano no inventário. Depois pode devolver aqui." } });
            yield break;
        }
        InventoryManager.Instance.RemoveItem(baldeProtegidoItem);
        Dados.curraisBaldeApoiado = true;
        Dados.curraisIdentidadeConfirmada = true;
        GameProgress.Instance.UnlockDiaryPage("currais-nair");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(baldeDevolvidoFalas);
        yield return Dizer(nomeRaimundoFalas);
    }
}
