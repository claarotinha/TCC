using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class EntradaCurrais
{
    [Header("Sepultamento — composição editável")]
    [SerializeField] private Image[] companhiaSepultamento;
    [SerializeField] private SpriteRenderer[] origensCompanhiaSepultamento;
    [SerializeField] private GameObject cruzDeBento;

    [Header("Posto e quarto — estados editáveis")]
    [SerializeField] private Transform dobradicaPortaQuarto, papelPostoLeitura, raimundoApoiadoPosto;
    [SerializeField] private GameObject registroCopiado, baldesApoiadosPosto;
    [SerializeField] private SpriteRenderer cabacaComTrabalhadora;
    [SerializeField] private Collider2D areaMesaDocumento, areaTrabalhadoraQuarto, areaCaboQuarto, areaDoenteUm, areaDoenteDois;
    [SerializeField] private Fala[] postoSeguinte, registroRevisado, folhaRetirada, quartoRevisita;
    [SerializeField] private Fala[] quartoVoltarFamilia, quartoSemCabaca, caboQuartoFalas, doenteUmFalas, doenteDoisFalas;
    private Quaternion portaAbertaOriginal;
    private bool portaEmMovimento, emprestandoNoQuarto, sepultamentoEmCurso;

    private bool ReferenciasDespedidaValidas()
    {
        bool ok = companhiaSepultamento != null && origensCompanhiaSepultamento != null &&
            companhiaSepultamento.Length == 4 && origensCompanhiaSepultamento.Length == 4;
        if (ok) for (int i = 0; i < 4; i++)
            ok &= ConferirReferencia(companhiaSepultamento[i], nameof(companhiaSepultamento)) &
                ConferirReferencia(origensCompanhiaSepultamento[i], nameof(origensCompanhiaSepultamento));
        UnityEngine.Object[] refs = { cruzDeBento, dobradicaPortaQuarto, papelPostoLeitura, raimundoApoiadoPosto,
            registroCopiado, baldesApoiadosPosto, cabacaComTrabalhadora, areaMesaDocumento,
            areaTrabalhadoraQuarto, areaCaboQuarto, areaDoenteUm, areaDoenteDois };
        string[] nomes = { nameof(cruzDeBento), nameof(dobradicaPortaQuarto), nameof(papelPostoLeitura), nameof(raimundoApoiadoPosto),
            nameof(registroCopiado), nameof(baldesApoiadosPosto), nameof(cabacaComTrabalhadora), nameof(areaMesaDocumento),
            nameof(areaTrabalhadoraQuarto), nameof(areaCaboQuarto), nameof(areaDoenteUm), nameof(areaDoenteDois) };
        for (int i = 0; i < refs.Length; i++) ok &= ConferirReferencia(refs[i], nomes[i]);
        foreach (Fala[] falas in new[] { postoSeguinte, registroRevisado, folhaRetirada, quartoRevisita,
            quartoVoltarFamilia, quartoSemCabaca, caboQuartoFalas, doenteUmFalas, doenteDoisFalas }) ok &= FalasValidas(falas);
        return ok && ConferirReferencia(exameMesaPosto, nameof(exameMesaPosto)) &&
            ConferirReferencia(exameRaimundoPosto, nameof(exameRaimundoPosto)) &&
            ConferirReferencia(areaAnotacaoPosto, nameof(areaAnotacaoPosto)) &&
            ConferirReferencia(anotacaoPosto, nameof(anotacaoPosto));
    }

    private void PrepararDespedidaEQuarto()
    {
        portaAbertaOriginal = dobradicaPortaQuarto.localRotation;
        if (Dados.curraisEtapaPosto >= 2) Dados.curraisQuartoAberto = true;
        if (Dados.curraisCabacaEmprestadaQuarto)
        {
            DevolverCabacaDoQuarto();
            if (Dados.curraisEtapaPosto == 1 && Dados.curraisQuartoAberto)
            {
                Dados.curraisEtapaPosto = 2;
                GameProgress.Instance.UnlockDiaryPage("currais-quarto");
            }
            Salvar();
        }
        AtualizarDetalhesPosto();
    }

    private void AtualizarCompanhiaSepultamento()
    {
        if (!sepultamentoNaTela) return;
        for (int i = 0; i < companhiaSepultamento.Length; i++)
            companhiaSepultamento[i].sprite = origensCompanhiaSepultamento[i].sprite;
    }

    private IEnumerator BaixarBentoNoPlano()
    {
        RectTransform rect = corpoNoPlano.GetComponent<RectTransform>();
        Image imagem = corpoNoPlano.GetComponent<Image>();
        Vector2 origem = rect.anchoredPosition;
        Color cor = imagem.color;
        try
        {
            float tempo = 0f;
            while (tempo < 1f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    float t = Mathf.Clamp01(tempo);
                    rect.anchoredPosition = origem + Vector2.down * (55f * t);
                    imagem.color = new Color(cor.r, cor.g, cor.b, cor.a * (1f - t));
                }
                yield return null;
            }
            corpoNoPlano.SetActive(false);
            cruzDeBento.SetActive(true);
        }
        finally { rect.anchoredPosition = origem; imagem.color = cor; }
    }

    private Collider2D[] ComAlvosDetalhadosDoPosto(Collider2D[] alvos)
    {
        Collider2D[] extras = { areaMesaDocumento, areaTrabalhadoraQuarto, areaCaboQuarto,
            areaDoenteUm, areaDoenteDois, areaRaimundo, areaLuzia, areaPortaQuarto, areaAnotacaoPosto };
        Collider2D[] resultado = new Collider2D[alvos.Length + extras.Length];
        Array.Copy(alvos, resultado, alvos.Length);
        Array.Copy(extras, 0, resultado, alvos.Length, extras.Length);
        return resultado;
    }

    private void AtualizarDetalhesPosto()
    {
        bool ativo = PostoDisponivel;
        bool aberto = ativo && (Dados.curraisQuartoAberto || Dados.curraisEtapaPosto >= 2 || portaEmMovimento);
        if (!portaEmMovimento) dobradicaPortaQuarto.gameObject.SetActive(aberto);
        portaQuartoFechada.SetActive(ativo && !aberto);
        quartoInterior.SetActive(aberto);
        areaPortaQuarto.enabled = ativo && Dados.curraisEscolhaPosto != 0;
        areaMesaDocumento.enabled = ativo && !ConsultaDisponivel;
        areaTrabalhadoraQuarto.enabled = aberto;
        areaCaboQuarto.enabled = ativo && Dados.curraisEscolhaPosto != 0;
        areaDoenteUm.enabled = aberto;
        areaDoenteDois.enabled = aberto;
        registroCopiado.SetActive(ativo && PapelAvulsoPostoPendente);
        baldesApoiadosPosto.SetActive(ativo);
        AtualizarRecipientePosto();
        cabacaComTrabalhadora.gameObject.SetActive(aberto && Dados.curraisCabacaEmprestadaQuarto);
        if (ativo && !ConsultaDisponivel) areaRaimundo.enabled = true;
    }

    private bool InteragirDetalhesDoPosto(Vector2 ponto)
    {
        if (areaAnotacaoPosto.enabled && areaAnotacaoPosto.gameObject.activeInHierarchy && areaAnotacaoPosto.OverlapPoint(ponto))
        { Iniciar(RecolherAnotacaoPosto()); return true; }
        foreach (Collider2D alvo in new[] { areaTrabalhadoraQuarto, areaDoenteUm, areaDoenteDois,
            areaMesaDocumento, areaCaboQuarto, areaRaimundo, areaLuzia })
        {
            if (!alvo.enabled || !alvo.gameObject.activeInHierarchy || !alvo.OverlapPoint(ponto)) continue;
            if ((alvo == areaRaimundo || alvo == areaLuzia) && ConsultaDisponivel) return false;
            if (alvo == areaMesaDocumento) Iniciar(ExaminarMesaPosto());
            else if (alvo == areaRaimundo) ClicarRaimundoPosto();
            else if (alvo == areaTrabalhadoraQuarto) MenuTrabalhadoraDoQuarto();
            else if (alvo == areaDoenteUm) MenuAssuntoNPC("Mulher", "Você quer falar comigo?", "Como a senhora tá?", () => Dizer(doenteUmFalas));
            else if (alvo == areaDoenteDois) MenuAssuntoNPC("Homem", "Diga.", "O senhor viu uma menina?", () => Dizer(doenteDoisFalas));
            else if (alvo == areaCaboQuarto)
            {
                if (!Dados.curraisQuartoAberto && Dados.curraisEtapaPosto == 1) MenuAssuntoNPC("Cabo", "Tá procurando quem?", "Perguntar pelas pessoas do quarto", InvestigarQuarto);
                else MenuAssuntoNPC("Cabo", "O que foi?", "Perguntar pelo quarto", () => Dizer(caboQuartoFalas));
            }
            else if (Dados.curraisEscolhaPosto != 0 && Dados.curraisEtapaPosto >= 2)
                MenuAssuntoNPC("Luzia", "Tô ouvindo.", "Sobre o quarto", () => Dizer(quartoVoltarFamilia));
            return true;
        }
        return false;
    }

    private void MenuTrabalhadoraDoQuarto()
    {
        MostrarOpcoes("Trabalhadora", quartoRevisita[0].texto,
            new[] { "Minha irmã", "O que tá faltando?", "Sair" },
            new Action[] { () => Iniciar(Dizer(quartoRevisita)), () => Iniciar(Dizer(quartoSemCabaca)), FecharMenu });
    }

    private IEnumerator AbrirPortaDoQuarto()
    {
        portaEmMovimento = true;
        try
        {
            portaQuartoFechada.SetActive(false);
            quartoInterior.SetActive(true);
            dobradicaPortaQuarto.gameObject.SetActive(true);
            dobradicaPortaQuarto.localRotation = Quaternion.identity;
            float tempo = 0f;
            while (tempo < .65f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    dobradicaPortaQuarto.localRotation = Quaternion.Slerp(Quaternion.identity, portaAbertaOriginal,
                        Mathf.Clamp01(tempo / .65f));
                }
                yield return null;
            }
            Dados.curraisQuartoAberto = true;
            Salvar();
        }
        finally
        {
            portaEmMovimento = false;
            dobradicaPortaQuarto.localRotation = portaAbertaOriginal;
            if (!encerrando) AtualizarDetalhesPosto();
        }
    }

    private IEnumerator LerFolhaDoPosto()
    {
        Transform folha = papelPosto.transform;
        Vector3 origem = folha.position;
        try
        {
            yield return DeslocarFolhaRegistro(folha, papelPostoLeitura.position, .4f);
            yield return Esperar(.3f);
            yield return DeslocarFolhaRegistro(folha, origem, .3f);
        }
        finally { folha.position = origem; }
    }

    private IEnumerator ServirAguaNoQuarto()
    {
        Quaternion rotacao = cabacaComTrabalhadora.transform.localRotation;
        Color cor = cabacaComTrabalhadora.color;
        try
        {
            cabacaComTrabalhadora.color = new Color(.65f, .85f, 1f, 1f);
            float tempo = 0f;
            while (tempo < .9f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    cabacaComTrabalhadora.transform.localRotation = rotacao *
                        Quaternion.Euler(0f, 0f, Mathf.Sin(Mathf.Clamp01(tempo / .9f) * Mathf.PI) * 18f);
                }
                yield return null;
            }
        }
        finally
        {
            cabacaComTrabalhadora.transform.localRotation = rotacao;
            cabacaComTrabalhadora.color = cor;
        }
    }

    private void DevolverCabacaDoQuarto()
    {
        if (!Dados.curraisCabacaEmprestadaQuarto) return;
        // A flag registra uma unidade devida, mesmo se houver outra cabaça igual.
        Dados.curraisCabacaEmprestadaQuarto = false;
        Dados.curraisAjudouAtendimentoQuarto = true;
        InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        AtualizarDetalhesPosto();
    }

    private void EncerrarDespedidaEQuarto()
    {
        portaEmMovimento = false;
        emprestandoNoQuarto = false;
        sepultamentoEmCurso = false;
        if (!encerrando && GameProgress.Instance != null && Dados.curraisCabacaEmprestadaQuarto)
        {
            DevolverCabacaDoQuarto();
            Salvar();
        }
    }
}
