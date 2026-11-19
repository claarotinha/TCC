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

    private bool PaisNoFuneral => Dados.curraisEscolhaFuneral == 2 && !VoltouDoQuarto;
    private bool PostoDisponivel => Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 6;

    private bool ReferenciasPostoValidas()
    {
        // A lista só aplica cor aos desenhos presentes. Remover um objeto do cenário
        // não deve deixar um slot vazio impedindo a inicialização de toda a história.
        if (cenarioNoturno != null)
            cenarioNoturno = Array.FindAll(cenarioNoturno, desenho => desenho != null);
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
            opcoesNoite == null || opcoesNoite.Length != 3 || objetivosPosto == null || objetivosPosto.Length != 5 ||
            string.IsNullOrWhiteSpace(perguntaPosto) || string.IsNullOrWhiteSpace(perguntaNoite) ||
            cenarioNoturno == null || cenarioNoturno.Length == 0) valido = false;
        else foreach (SpriteRenderer desenho in cenarioNoturno) valido &= ConferirReferencia(desenho, nameof(cenarioNoturno));
        return valido && ReferenciasDespedidaValidas();
    }

    private string ObjetivoPosto() => string.Empty;

    private Collider2D[] AlvosPosto() => ComAlvosDetalhadosDoPosto(ConsultaDisponivel ? AlvosConsulta() :
        new[] { areaPostoEscolha, areaPortaQuarto, areaFamiliaAbrigo, areaEsteiraNoite });

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
                cenarioNoturno[i].color = coresAntesDaNoite[i];
    }

    private void AtualizarPosto()
    {
        bool ativo = PostoDisponivel;
        setorPosto.SetActive(true);
        curativoPai.SetActive(false);
        lacoNaMao.SetActive(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4) && Dados.curraisLacoGuardado);
        maoNaRoupa.SetActive(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4) && !Dados.curraisLacoGuardado);
        cabacaNoite.SetActive(ativo && Dados.curraisEtapaPosto == 4);
        AplicarNoite(ativo && (noiteEmCurso || Dados.curraisEtapaPosto == 4));
        if (!ativo)
        {
            if (Dados.curraisBentoMorto)
                Posicionar(areaRaimundo.transform, raimundoNoPosto);
            AtualizarConsulta();
            return;
        }
        // Saves que pararam no posto ou no quarto retomam no reencontro com a família.
        if (Dados.curraisEscolhaPosto != 0 && Dados.curraisEtapaPosto <= 2)
        {
            Dados.curraisEtapaPosto = 2;
            if (!VoltouDoQuarto) RegistrarConversaEntrada("quarto-retorno-familia");
        }
        pai.gameObject.SetActive(!PaisNoFuneral);
        mae.gameObject.SetActive(!PaisNoFuneral);
        int etapa = Dados.curraisEtapaPosto;
        objetivoTexto.text = string.Empty;
        areaPostoEscolha.enabled = etapa == 0;
        areaFamiliaAbrigo.enabled = etapa >= 2 && !PaisNoFuneral;
        areaEsteiraNoite.enabled = etapa == 3;
        papelPosto.SetActive(Dados.curraisEscolhaPosto == 0);
        panoAlca.SetActive(Dados.curraisBaldeApoiado);
        AtualizarFamiliaAlternativa();
        curativoRaimundo.SetActive(Dados.curraisEscolhaPosto == 1);
        Posicionar(areaRaimundo.transform, raimundoNoPosto);
        areaRaimundo.transform.localRotation = Quaternion.identity;
        if (etapa >= 2 && VoltouDoQuarto && (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            bool duranteNoite = noiteEmCurso || etapa == 4;
            Posicionar(luzia.transform, duranteNoite ? luziaNoite : luziaRepouso);
            Posicionar(pai, duranteNoite ? paiNoite : paiAbrigo);
            Posicionar(mae, (noiteEmCurso || etapa == 4) ? maeNoite : maeAbrigo);
        }
        AtualizarConsulta();
        AtualizarDetalhesPosto();
        AtualizarCompanhiaPosto();
        Physics2D.SyncTransforms();
    }

    private bool InteragirPosto(Vector2 ponto)
    {
        if (InteragirDetalhesDoPosto(ponto)) return true;
        if (ConsultaDisponivel) return InteragirConsulta(ponto);
        foreach (Collider2D area in AlvosPosto())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            if (area == areaPostoEscolha) ClicarRaimundoPosto();
            else if (area == areaFamiliaAbrigo)
            {
                if (Dados.curraisEtapaPosto == 2) MenuNoticiasPosto();
                else Iniciar(AbrirConversaFamilia());
            }
            else if (area == areaEsteiraNoite) MenuDescansoFamilia();
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
        yield return EscolherPostoLinear();
    }

    private IEnumerator InvestigarQuarto()
    {
        yield return InvestigarQuartoLinear();
    }

    private IEnumerator NoticiasDaBusca()
    {
        yield return NoticiasPostoLinear();
    }

    private IEnumerator ContarAosPais()
    {
        yield return FinalizarBuscaNoPosto();
    }

    private void MenuAbrigo()
    {
        MenuFamiliaLinear();
    }

    private IEnumerator RelembrarRefeicao()
    {
        if (!TemLembrancaRefeicao) yield break;
        // A lembrança não teleporta Baleia nem altera o enquadramento.
        yield return Dizer(Dados.secaFoodChoice == 1 ? refeicaoTudo : Dados.secaFoodChoice == 2 ? refeicaoBaleia : refeicaoIrma);
        Dados.curraisRefeicaoRelembrada = true;
        Salvar(); MenuLembrancasFamilia();
    }

    private void MenuDescansoFamilia()
    {
        MostrarOpcoes("Mãe", string.Empty,
            new[] { "Ficar perto da mãe", "Conversar mais um pouco", "Sair" },
            new Action[] { () => Iniciar(PassarANoite()), MenuAbrigo, FecharMenu });
    }

    private IEnumerator PassarANoite()
    {
        yield return PassarNoiteLinear();
    }
}
