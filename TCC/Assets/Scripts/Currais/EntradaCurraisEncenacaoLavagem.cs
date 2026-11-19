using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Lavagem e lembranças — elementos editáveis")]
    [SerializeField] private Collider2D areaGuardaRonda;
    [SerializeField] private SpriteRenderer panoEmLavagem;
    [SerializeField] private Transform panoMergulhoLavagem, luziaRetornoLembranca, inicioLembranca;
    [SerializeField] private GameObject varalPanosUso;
    [SerializeField] private Fala[] lavadeiraEsperaLote, lavadeiraIrma, raimundoRetorno, lavagemFaltaUso, lavagemFaltaAtendimento;
    [SerializeField] private Fala[] lavagemConcluida, lavagemUsoRecebido, lavagemAtendimentoRecebido;
    [SerializeField] private Fala[] luziaAntesRaimundo, luziaAntesLavagem, luziaNaPassagem;
    [SerializeField] private Fala[] luziaDepoisPassagem, guardaPassagemFalas, lembrancaSemLenda;
    private bool lembrancaEmCurso;

    private bool PrimeiraBuscaEmCurso => Dados.curraisEtapaEntrada == 4 &&
        Dados.curraisPanelaConcluida && !PostoDisponivel && Dados.curraisEtapaBusca <= 5;

    private bool ReferenciasEncenacaoLavagemValidas()
    {
        UnityEngine.Object[] referencias = { areaGuardaRonda, panoEmLavagem,
            panoMergulhoLavagem, luziaRetornoLembranca, inicioLembranca, varalPanosUso };
        string[] nomes = { nameof(areaGuardaRonda), nameof(panoEmLavagem),
            nameof(panoMergulhoLavagem), nameof(luziaRetornoLembranca), nameof(inicioLembranca), nameof(varalPanosUso) };
        bool ok = true;
        for (int i = 0; i < referencias.Length; i++) ok &= ConferirReferencia(referencias[i], nomes[i]);
        foreach (Fala[] falas in new[] { lavadeiraEsperaLote, lavadeiraIrma, raimundoRetorno, lavagemFaltaUso, lavagemFaltaAtendimento,
            lavagemConcluida, lavagemUsoRecebido, lavagemAtendimentoRecebido, luziaAntesRaimundo,
            luziaAntesLavagem, luziaNaPassagem, luziaDepoisPassagem, guardaPassagemFalas, lembrancaSemLenda })
            ok &= FalasValidas(falas);
        return ok;
    }

    private Collider2D[] AlvosLavagemEPassagem()
    {
        return new[] { areaPanosUso, areaPanosAtendimento, areaPanoLavagem,
            areaRaimundo, areaLavadeira, areaRecebimentoLavagem, areaLuzia, areaGuardaRonda, areaLembranca,
            areaAguaPatio, areaPanosPatio, areaSocorroBento, areaCabacaSocorro,
            areaPatio, areaPanoAtendimento };
    }

    private void ConversarLuziaNaBusca()
    {
        int etapa = Dados.curraisEtapaBusca;
        if (etapa == 0)
        {
            if (PodeConversarRaimundo && !ViuConversaEntrada("raimundo-aproximacao"))
                Iniciar(ComentarRaimundo());
            return;
        }
        if (etapa == 1 || etapa == 2 || etapa == 3) return;
        if (etapa == 4 || etapa == 5) { ConversarLuziaNoPatio(); return; }
        Fala[] falas = etapa == 0 ? luziaAntesRaimundo :
            etapa == 1 ? luziaAntesLavagem :
            etapa == 2 ? EstadoSeparacaoLavagem() :
            etapa == 3 ? luziaNaPassagem : luziaDepoisPassagem;
        MenuAssuntoNPC("Luzia", "Pode falar, Josefa.", "O que aconteceu por aqui?", () => Dizer(falas), "Sobre Mariquinha", () => Dizer(luziaIrma));
    }

    private Fala[] EstadoSeparacaoLavagem()
    {
        int separados = Dados.curraisPanosSeparados & 3;
        return separados == 3 ? lavagemConcluida : separados == 1
            ? lavagemFaltaAtendimento : separados == 2 ? lavagemFaltaUso : instrucoesLavagem;
    }

    private void MenuLavadeiraNaBusca() => Iniciar(AbrirLavadeiraLinear());

    private void MenuPilhaLavagem(bool atendimento) => Iniciar(RemendarPanoLavagem());

    private void AtualizarDetalhesLavagem()
    {
        bool uso = (Dados.curraisPanosSeparados & 1) != 0;
        bool atendimento = (Dados.curraisPanosSeparados & 2) != 0;
        // Os objetos permanecem editáveis; muda somente a visibilidade dos estados.
        areaPanosUso.GetComponent<SpriteRenderer>().enabled = !uso;
        areaPanosAtendimento.GetComponent<SpriteRenderer>().enabled = !atendimento;
        varalPanosUso.SetActive(Dados.curraisPanelaConcluida && uso);
        areaPanoLavagem.gameObject.SetActive(Dados.curraisPanelaConcluida &&
            Dados.curraisEtapaBusca == 2 && RetalhoAutorizado && !Dados.curraisTiraPanoRecolhida);
        areaGuardaRonda.enabled = PrimeiraBuscaEmCurso && Dados.curraisEtapaBusca >= 3;
    }

    private IEnumerator LavarPecaLavagem(bool atendimento)
    {
        AudioManager.Play("lavagem", 0.65f);
        Transform peca = panoEmLavagem.transform;
        Vector3 origem = peca.position, escala = peca.localScale;
        Quaternion rotacao = peca.localRotation;
        Sprite desenho = panoEmLavagem.sprite;
        Color cor = panoEmLavagem.color;
        try
        {
            panoEmLavagem.sprite = atendimento ? panoAtendimentoLavagem.icon : panoUsoLavagem.icon;
            if (desenho != null && panoEmLavagem.sprite != null && panoEmLavagem.sprite.bounds.size.y > 0f)
                peca.localScale = escala * (desenho.bounds.size.y / panoEmLavagem.sprite.bounds.size.y);
            panoEmLavagem.color = atendimento ? new Color(.7f, .45f, .4f, 1f) : Color.white;
            panoEmLavagem.gameObject.SetActive(true);
            float tempo = 0f;
            while (tempo < 1f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    float t = Mathf.Clamp01(tempo);
                    peca.position = Vector3.Lerp(origem, panoMergulhoLavagem.position, Mathf.Sin(t * Mathf.PI));
                    peca.localRotation = rotacao * Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * 8f);
                }
                yield return null;
            }
        }
        finally
        {
            peca.position = origem; peca.localScale = escala; peca.localRotation = rotacao;
            panoEmLavagem.sprite = desenho; panoEmLavagem.color = cor;
            panoEmLavagem.gameObject.SetActive(false);
        }
    }

    private IEnumerator MostrarPercepcaoGradual(GameObject alvo)
    {
        SpriteRenderer desenho = alvo.GetComponent<SpriteRenderer>();
        if (desenho == null) { alvo.SetActive(true); yield break; }
        Color original = desenho.color;
        try
        {
            desenho.color = new Color(original.r, original.g, original.b, 0f);
            alvo.SetActive(true);
            float tempo = 0f;
            while (tempo < .35f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    desenho.color = new Color(original.r, original.g, original.b,
                        original.a * Mathf.Clamp01(tempo / .35f));
                }
                yield return null;
            }
        }
        finally { desenho.color = original; }
    }

    private IEnumerator OrientarAcompanhanteNaLembranca()
    {
        // A acompanhante já está junto de Josefa. Não busca Luzia em outra parte da cena.
        Transform acompanhante = mae;
        OlharParaMari(acompanhante);
        AnimarEscolta(acompanhante, false);
        yield break;
    }
    private void EncerrarLavagemELembranca()
    {
        lembrancaEmCurso = false;
        if (panoEmLavagem != null) panoEmLavagem.gameObject.SetActive(false);
        foreach (GameObject imagem in new[] { sombraGalho, pessoaNaCerca, reflexoIrma, reflexoZefinha, sinalGuarda })
            if (imagem != null) imagem.SetActive(false);
    }

}

