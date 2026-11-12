using System;
using System.Collections;
using UnityEngine;

// Etapa 4 do mesmo componente EntradaCurrais. Não adicionar outro controlador à cena.
public partial class EntradaCurrais
{
    [Header("Etapa 4 - posto, quarto e abrigo")]
    [SerializeField] private GameObject setorPosto, quartoInterior, portaQuartoFechada, papelPosto;
    [SerializeField] private Collider2D areaPostoEscolha, areaPortaQuarto, areaFamiliaAbrigo, areaEsteiraNoite;
    [SerializeField] private Transform raimundoNoPosto, raimundoSentado, raimundoOutroSetor, mariNoite, maeNoite, paiNoite, luziaNoite, baleiaCarinho;
    [SerializeField] private GameObject panoAlca, curativoRaimundo, curativoPai, lacoNaMao, maoNaRoupa, cabacaNoite;
    [SerializeField] private SpriteRenderer[] cenarioNoturno;
    [SerializeField] private Color tomNoite = new Color(.42f, .48f, .65f, 1f);
    [SerializeField] private Fala[] postoLuzia, postoAjuda, postoCopia, postoRepetido, quartoVoz, quartoFalas;
    [SerializeField] private Fala[] familiaInicio, familiaDestino, familiaIdentidade, familiaSemPista, familiaFim;
    [SerializeField] private Fala[] refeicaoTudo, refeicaoBaleia, refeicaoIrma;
    [SerializeField] private Fala[] noitePromessa, noiteTranquilizou, noiteSilencio, noiteInicio, noiteContar, noitePerto;
    [SerializeField] private string[] opcoesPosto, opcoesAbrigo, opcoesNoite, objetivosPosto;
    [SerializeField, TextArea(2, 5)] private string perguntaPosto, perguntaNoite;
    private Color[] coresAntesDaNoite;
    private bool noiteEmCurso;

    private bool PostoDisponivel => Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 6;

    private bool ReferenciasPostoValidas()
    {
        UnityEngine.Object[] referencias = { setorPosto, quartoInterior, portaQuartoFechada, papelPosto,
            areaPostoEscolha, areaPortaQuarto, areaFamiliaAbrigo, areaEsteiraNoite, raimundoNoPosto,
            raimundoSentado, raimundoOutroSetor, mariNoite, maeNoite, paiNoite, luziaNoite, baleiaCarinho, panoAlca,
            curativoRaimundo, curativoPai, lacoNaMao, maoNaRoupa, cabacaNoite };
        string[] nomes = { nameof(setorPosto), nameof(quartoInterior), nameof(portaQuartoFechada), nameof(papelPosto),
            nameof(areaPostoEscolha), nameof(areaPortaQuarto), nameof(areaFamiliaAbrigo), nameof(areaEsteiraNoite),
            nameof(raimundoNoPosto), nameof(raimundoSentado), nameof(raimundoOutroSetor), nameof(mariNoite),
            nameof(maeNoite), nameof(paiNoite), nameof(luziaNoite), nameof(baleiaCarinho), nameof(panoAlca), nameof(curativoRaimundo), nameof(curativoPai),
            nameof(lacoNaMao), nameof(maoNaRoupa), nameof(cabacaNoite) };
        bool valido = true;
        for (int i = 0; i < referencias.Length; i++) valido &= ConferirReferencia(referencias[i], nomes[i]);
        Fala[][] sequencias = { postoLuzia, postoAjuda, postoCopia, postoRepetido, quartoVoz, quartoFalas,
            familiaInicio, familiaDestino, familiaIdentidade, familiaSemPista, familiaFim, refeicaoTudo,
            refeicaoBaleia, refeicaoIrma, noitePromessa, noiteTranquilizou, noiteSilencio, noiteInicio, noiteContar, noitePerto };
        foreach (Fala[] falas in sequencias) valido &= FalasValidas(falas);
        if (opcoesPosto == null || opcoesPosto.Length != 3 || opcoesAbrigo == null || opcoesAbrigo.Length != 3 ||
            opcoesNoite == null || opcoesNoite.Length != 2 || objetivosPosto == null || objetivosPosto.Length != 5 ||
            string.IsNullOrWhiteSpace(perguntaPosto) || string.IsNullOrWhiteSpace(perguntaNoite) ||
            cenarioNoturno == null || cenarioNoturno.Length == 0) valido = false;
        else foreach (SpriteRenderer desenho in cenarioNoturno) valido &= ConferirReferencia(desenho, nameof(cenarioNoturno));
        return valido;
    }

    private string ObjetivoPosto() => ConsultaDisponivel ? ObjetivoConsulta() : objetivosPosto[Mathf.Clamp(Dados.curraisEtapaPosto, 0, 4)];

    private Collider2D[] AlvosPosto() => ConsultaDisponivel ? AlvosConsulta() : new[] { areaPostoEscolha, areaPortaQuarto, areaFamiliaAbrigo, areaEsteiraNoite };

    private void AplicarNoite(bool ativa)
    {
        if (coresAntesDaNoite == null)
        {
            coresAntesDaNoite = new Color[cenarioNoturno.Length];
            for (int i = 0; i < cenarioNoturno.Length; i++)
                if (cenarioNoturno[i] != null) coresAntesDaNoite[i] = cenarioNoturno[i].color;
        }
        for (int i = 0; i < cenarioNoturno.Length; i++)
            if (cenarioNoturno[i] != null)
                cenarioNoturno[i].color = ativa ? coresAntesDaNoite[i] * tomNoite : coresAntesDaNoite[i];
    }

    private void AtualizarPosto()
    {
        bool ativo = PostoDisponivel;
        setorPosto.SetActive(ativo);
        curativoPai.SetActive(ativo && Dados.curraisEtapaPosto >= 2);
        lacoNaMao.SetActive(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4) && Dados.curraisLacoGuardado);
        maoNaRoupa.SetActive(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4) && !Dados.curraisLacoGuardado);
        cabacaNoite.SetActive(ativo && Dados.curraisEtapaPosto == 4);
        AplicarNoite(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4));
        if (!ativo) { AtualizarConsulta(); return; }
        int etapa = Dados.curraisEtapaPosto;
        objetivoTexto.text = ObjetivoPosto();
        areaPostoEscolha.enabled = etapa == 0;
        areaPortaQuarto.enabled = etapa == 1;
        areaFamiliaAbrigo.enabled = etapa >= 2;
        areaEsteiraNoite.enabled = etapa == 3;
        papelPosto.SetActive(Dados.curraisEscolhaPosto == 0);
        quartoInterior.SetActive(etapa >= 2);
        portaQuartoFechada.SetActive(etapa < 2);
        panoAlca.SetActive(Dados.curraisBaldeApoiado);
        AtualizarFamiliaAlternativa();
        curativoRaimundo.SetActive(Dados.curraisEscolhaPosto != 0);
        Posicionar(areaRaimundo.transform, etapa == 0 ? raimundoNoPosto :
            Dados.curraisEscolhaPosto == 1 ? raimundoSentado : raimundoOutroSetor);
        areaRaimundo.transform.localRotation = Quaternion.identity;
        if (etapa >= 2 && (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            bool duranteNoite = noiteEmCurso || etapa == 4;
            Posicionar(luzia.transform, duranteNoite ? luziaNoite : luziaRepouso);
            Posicionar(pai, duranteNoite ? paiNoite : paiAbrigo);
            Posicionar(mae, (noiteEmCurso || etapa == 4) ? maeNoite : maeAbrigo);
        }
        AtualizarConsulta();
        Physics2D.SyncTransforms();
    }

    private bool InteragirPosto(Vector2 ponto)
    {
        if (ConsultaDisponivel) return InteragirConsulta(ponto);
        foreach (Collider2D area in AlvosPosto())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;
            if (Mathf.Abs(mari.transform.position.x - area.bounds.center.x) > alcanceInteracao)
            { objetivoTexto.text = "Aproxime-se para interagir. " + ObjetivoPosto(); return true; }
            if (area == areaPostoEscolha) Iniciar(EscolherNoPosto());
            else if (area == areaPortaQuarto) Iniciar(InvestigarQuarto());
            else if (area == areaFamiliaAbrigo)
            {
                if (Dados.curraisEtapaPosto == 2) Iniciar(ContarAosPais());
                else MenuAbrigo();
            }
            else if (area == areaEsteiraNoite) Iniciar(PassarANoite());
            return true;
        }
        return false;
    }

    private void AvancarPosto(int etapa)
    {
        Dados.curraisEtapaPosto = etapa;
        AtualizarEstadoBusca();
        Salvar();
    }

    private IEnumerator EscolherNoPosto()
    {
        if (Dados.curraisEscolhaPosto != 0) yield break;
        yield return Dizer(postoLuzia);
        yield return Escolher("Zefinha", perguntaPosto, opcoesPosto);
        int opcao = escolhaRecebida;
        if (opcao == 2) yield break; // Sair adia a decisão sem registrar uma escolha.
        bool destinoAnterior = Dados.curraisDestinoConfirmado;
        if (opcao == 0)
        {
            yield return Dizer(postoAjuda);
        }
        else
        {
            yield return Dizer(postoCopia);
            if (destinoAnterior) yield return Dizer(postoRepetido);
        }
        // A decisão e a etapa são registradas juntas após o encerramento da conversa.
        Dados.curraisEscolhaPosto = opcao + 1;
        if (opcao == 1) Dados.curraisDestinoConfirmado = true;
        Dados.curraisEtapaPosto = 1;
        if (opcao == 1) GameProgress.Instance.UnlockDiaryPage("currais-copia");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator InvestigarQuarto()
    {
        yield return Dizer(quartoVoz);
        yield return Escurecer(1f);
        portaQuartoFechada.SetActive(false);
        quartoInterior.SetActive(true);
        yield return Escurecer(0f);
        yield return Esperar(tempoPlano);
        yield return Dizer(quartoFalas);
        yield return AjudarNoQuarto();
        Dados.curraisEtapaPosto = 2;
        GameProgress.Instance.UnlockDiaryPage("currais-quarto");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator NoticiasDaBusca()
    {
        yield return Dizer(familiaInicio);
        if (Dados.curraisDestinoConfirmado) yield return Dizer(familiaDestino);
        if (Dados.curraisIdentidadeConfirmada) yield return Dizer(familiaIdentidade);
        if (!Dados.curraisDestinoConfirmado && !Dados.curraisIdentidadeConfirmada) yield return Dizer(familiaSemPista);
        yield return Dizer(familiaFim);
    }

    private IEnumerator ContarAosPais()
    {
        yield return NoticiasDaBusca();
        Dados.curraisEtapaPosto = 3;
        GameProgress.Instance.UnlockDiaryPage("currais-pais");
        AtualizarEstadoBusca(); Salvar();
        MenuAbrigo();
    }

    private void MenuAbrigo()
    {
        MostrarOpcoes("Família", "Podemos conversar um pouco antes de tentar descansar.",
            new[] { "O que prometeram?", "O trabalho e Bento", "Nossa família e o caderno", "Sair" },
            new Action[] { () => Iniciar(ConversarFamilia(1, familiaPromessas)),
                () => Iniciar(ConversarFamilia(2, familiaTrabalho)), MenuLembrancasFamilia, FecharMenu });
    }

    private IEnumerator RelembrarRefeicao()
    {
        if (Dados.secaFoodChoice == 2) Posicionar(baleia, baleiaCarinho);
        yield return Dizer(Dados.secaFoodChoice == 1 ? refeicaoTudo : Dados.secaFoodChoice == 2 ? refeicaoBaleia : refeicaoIrma);
        Dados.curraisRefeicaoRelembrada = true;
        Salvar(); MenuAbrigo();
    }

    private IEnumerator PassarANoite()
    {
        yield return Escurecer(1f);
        Posicionar(mari.transform, mariNoite);
        Posicionar(mae, maeNoite);
        noiteEmCurso = true;
        AtualizarPosto();
        yield return Escurecer(0f);
        int promessa = Dados.estradaSegundaEscolhaNoturna;
        if (promessa >= 1 && promessa <= 3)
            yield return Dizer(promessa == 1 ? noitePromessa : promessa == 2 ? noiteTranquilizou : noiteSilencio);
        yield return Dizer(noiteCaderno);
        yield return Dizer(noiteInicio);
        yield return Escolher("Zefinha", perguntaNoite, opcoesNoite);
        int opcao = escolhaRecebida;
        yield return Dizer(opcao == 0 ? noiteContar : noitePerto);
        Dados.curraisEscolhaNoite = opcao + 1;
        Dados.curraisEtapaPosto = 4;
        noiteEmCurso = false;
        GameProgress.Instance.UnlockDiaryPage("currais-noite");
        AtualizarEstadoBusca(); Salvar();
    }
}
