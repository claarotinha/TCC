using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cena 21: encerramento do passado, sem carregar o epílogo ou escolher um final.
public partial class EntradaCurrais
{
    [Header("Etapa 6 - as páginas seguintes")]
    [SerializeField] private GameObject planoMemoria, folhaMemoria, lacoMemoria, documentoMemoria, maoMemoria;
    [SerializeField] private SpriteRenderer quartoMemoria;
    [SerializeField] private Collider2D areaPaginasMemoria;
    [SerializeField] private Transform mariPlanoMemoria, cameraPlanoMemoria;
    [SerializeField] private Fala[] paginasMemoria;
    [SerializeField] private Fala pensamentoMemoria, chamadoPresente;
    [SerializeField] private string[] idsPaginasMemoria, opcoesFimMemoria;
    [SerializeField] private string objetivoPaginasMemoria, objetivoFimMemoria, tituloFimMemoria;
    [SerializeField, Min(1f)] private float duracaoPaginaMemoria = 8f;
    [SerializeField, Min(.1f)] private float leituraMinimaMemoria = 2.5f;
    [SerializeField, Min(.1f)] private float duracaoCorteMemoria = 1.5f;
    private bool planoMemoriaAtivo, leituraPaginaMemoria, paginaMemoriaPodeAvancar;
    private readonly Dictionary<Animator, bool> animadoresMemoria = new Dictionary<Animator, bool>();

    private bool MemoriaDisponivel => ConsultaDisponivel && Dados.curraisEtapaConsulta >= 4;

    private bool ReferenciasMemoriaValidas()
    {
        if (areaPaginasMemoria == null) areaPaginasMemoria = areaEsteiraNoite;
        UnityEngine.Object[] referencias = { planoMemoria, folhaMemoria, lacoMemoria, documentoMemoria,
            maoMemoria, quartoMemoria, areaPaginasMemoria, mariPlanoMemoria, cameraPlanoMemoria };
        string[] nomes = { nameof(planoMemoria), nameof(folhaMemoria), nameof(lacoMemoria), nameof(documentoMemoria),
            nameof(maoMemoria), nameof(quartoMemoria), nameof(areaPaginasMemoria), nameof(mariPlanoMemoria), nameof(cameraPlanoMemoria) };
        bool valido = true;
        for (int i = 0; i < referencias.Length; i++) valido &= ConferirReferencia(referencias[i], nomes[i]);
        valido &= FalasValidas(paginasMemoria) && paginasMemoria.Length == 3 &&
            FalaValida(pensamentoMemoria) && FalaValida(chamadoPresente);
        valido &= idsPaginasMemoria != null && idsPaginasMemoria.Length == 3 &&
            opcoesFimMemoria != null && opcoesFimMemoria.Length == 4;
        valido &= !string.IsNullOrWhiteSpace(objetivoPaginasMemoria) &&
            !string.IsNullOrWhiteSpace(objetivoFimMemoria) && !string.IsNullOrWhiteSpace(tituloFimMemoria);
        return valido;
    }

    private string ObjetivoMemoria() => Dados.ato2Concluido ? objetivoFimMemoria : objetivoPaginasMemoria;

    private Collider2D[] AlvosMemoria() => new[] { areaRetornoConsulta, areaPaginasMemoria };

    private void AtualizarMemoria()
    {
        planoMemoria.SetActive(planoMemoriaAtivo);
        if (!MemoriaDisponivel) return;
        objetivoTexto.text = ObjetivoMemoria();
        areaPaginasMemoria.enabled = true;
        lacoMemoria.SetActive(planoMemoriaAtivo && Dados.curraisLacoGuardado);
        documentoMemoria.SetActive(planoMemoriaAtivo && !Dados.curraisLacoGuardado);
    }

    private bool InteragirMemoria(Vector2 ponto)
    {
        foreach (Collider2D area in AlvosMemoria())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;
            if (Mathf.Abs(mari.transform.position.x - area.bounds.center.x) > alcanceInteracao)
            { objetivoTexto.text = "Aproxime-se para interagir. " + ObjetivoMemoria(); return true; }
            if (area == areaPaginasMemoria)
            {
                if (Dados.ato2Concluido) AbrirMenuFimMemoria();
                else Iniciar(ExibirPaginasMemoria(false));
            }
            else MostrarOpcoes("Família", InformacoesConsulta(), opcoesRetornoConsulta, new Action[] {
                () => Iniciar(RelatarInformacoes()), FecharMenu });
            return true;
        }
        return false;
    }

    private void PrepararPlanoMemoria()
    {
        planoMemoriaAtivo = true;
        mari.enabled = false;
        seguirCamera.enabled = false;
        Posicionar(mari.transform, mariPlanoMemoria);
        cameraCena.transform.position = cameraPlanoMemoria.position;
        Transform[] pessoas = { mari.transform, pai, mae, baleia, luzia.transform };
        foreach (Transform pessoa in pessoas)
            foreach (Animator animador in pessoa.GetComponentsInChildren<Animator>(true))
                if (!animadoresMemoria.ContainsKey(animador))
                {
                    animadoresMemoria[animador] = animador.enabled;
                    animador.enabled = false;
                }
        maoMemoria.SetActive(false);
        quartoMemoria.gameObject.SetActive(false);
        DefinirAlphaQuarto(0f);
        AtualizarMemoria();
    }

    private static bool InventarioAbertoMemoria() =>
        InventoryTabController.Instance != null && InventoryTabController.Instance.IsOpen;

    private static bool TempoMemoriaLiberado() =>
        !PauseHelper.BlockInput() && Time.timeScale > 0f && !InventarioAbertoMemoria();

    private IEnumerator LerPaginaMemoria(Fala fala)
    {
        leituraPaginaMemoria = true;
        paginaMemoriaPodeAvancar = false;
        dialogando = true; aguardandoClique = true;
        escolhas.SetActive(false); AbrirPainelDialogo();
        nomeTexto.text = fala.personagem; falaTexto.text = fala.texto;
        if (retrato != null) retrato.enabled = false;
        quadroBloqueado = Time.frameCount;
        float tempo = 0f;
        try
        {
            float duracao = Mathf.Max(duracaoPaginaMemoria, leituraMinimaMemoria);
            while (tempo < duracao && aguardandoClique)
            {
                if (TempoMemoriaLiberado())
                {
                    tempo += Time.deltaTime;
                    paginaMemoriaPodeAvancar = tempo >= leituraMinimaMemoria;
                }
                yield return null;
            }
        }
        finally
        {
            leituraPaginaMemoria = false; paginaMemoriaPodeAvancar = false;
            FecharDialogo();
        }
    }

    private IEnumerator ExibirPaginasMemoria(bool rever)
    {
        if (!rever)
        {
            Dados.curraisMemoriaIniciada = true;
            Salvar();
        }
        yield return Escurecer(1f);
        PrepararPlanoMemoria();
        yield return Escurecer(0f);
        int inicio = rever ? 0 : Mathf.Clamp(Dados.curraisPaginaMemoria, 0, 3);
        for (int i = inicio; i < paginasMemoria.Length; i++)
        {
            yield return LerPaginaMemoria(paginasMemoria[i]);
            if (!rever)
            {
                Dados.curraisPaginaMemoria = i + 1;
                GameProgress.Instance.UnlockDiaryPage(idsPaginasMemoria[i]);
                Salvar();
            }
        }
        maoMemoria.SetActive(true);
        yield return LerPaginaMemoria(pensamentoMemoria);
        yield return CorteParaPresente();
        if (!rever)
        {
            Dados.ato2Concluido = true;
            GameProgress.Instance.UnlockDiaryPage("currais-memoria-encerrada");
            Salvar(); // O checkpoint continua em Currais, sem entrar no epílogo.
        }
        AbrirMenuFimMemoria();
    }

    private IEnumerator CorteParaPresente()
    {
        quartoMemoria.gameObject.SetActive(true);
        float tempo = 0f;
        while (tempo < duracaoCorteMemoria)
        {
            if (TempoMemoriaLiberado())
            {
                tempo += Time.deltaTime;
                DefinirAlphaQuarto(Mathf.Clamp01(tempo / duracaoCorteMemoria));
            }
            yield return null;
        }
        DefinirAlphaQuarto(1f);
        yield return LerPaginaMemoria(chamadoPresente);
    }

    private void DefinirAlphaQuarto(float alpha)
    {
        Color cor = quartoMemoria.color;
        cor.a = alpha;
        quartoMemoria.color = cor;
    }

    private IEnumerator RetomarMemoria()
    {
        if (Dados.ato2Concluido)
        {
            PrepararPlanoMemoria();
            quartoMemoria.gameObject.SetActive(true); DefinirAlphaQuarto(1f);
            AbrirMenuFimMemoria();
            yield break;
        }
        yield return ExibirPaginasMemoria(false);
    }

    private void AbrirMenuFimMemoria()
    {
        MostrarOpcoes(tituloFimMemoria, InformacoesConsulta(), opcoesFimMemoria, new Action[] {
            () => Iniciar(ExibirPaginasMemoria(true)),
            () => Iniciar(RevisarPistasMemoria()),
            VoltarAoAbrigoMemoria,
            VoltarAoMenuMemoria });
    }

    private IEnumerator RevisarPistasMemoria()
    {
        yield return Dizer(new[] { new Fala { personagem = "Memória", texto = InformacoesConsulta() } });
        AbrirMenuFimMemoria();
    }

    private void VoltarAoAbrigoMemoria()
    {
        RestaurarApresentacaoMemoria();
        FecharDialogo();
        AtualizarEstadoBusca();
        LiberarMari();
    }

    private void VoltarAoMenuMemoria()
    {
        Salvar();
        UniversalPauseManager pausa = UniversalPauseManager.Instance;
        if (pausa != null) pausa.GoToMainMenu();
        else
        {
            Debug.LogError("Currais: o sistema de pausa existente não está disponível para voltar ao menu.", this);
            VoltarAoAbrigoMemoria();
        }
    }

    private void RestaurarApresentacaoMemoria()
    {
        leituraPaginaMemoria = false; paginaMemoriaPodeAvancar = false;
        planoMemoriaAtivo = false;
        foreach (KeyValuePair<Animator, bool> animador in animadoresMemoria)
            if (animador.Key != null) animador.Key.enabled = animador.Value;
        animadoresMemoria.Clear();
        if (planoMemoria != null) planoMemoria.SetActive(false);
    }
}
