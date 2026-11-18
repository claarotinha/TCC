using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cena 21: encerra o passado e preserva as pistas ao entrar no epílogo.
public partial class EntradaCurrais
{
    [Header("Etapa 6 - as páginas seguintes")]
    [SerializeField] private GameObject planoMemoria, folhaMemoria, lacoMemoria, documentoMemoria, maoMemoria;
    [SerializeField] private SpriteRenderer quartoMemoria;
    [SerializeField] private Collider2D areaPaginasMemoria;
    [SerializeField] private Transform mariPlanoMemoria, cameraPlanoMemoria;
    [SerializeField] private Fala[] paginasMemoria;
    [SerializeField] private Fala pensamentoMemoria, chamadoPresente;
    [SerializeField] private string[] idsPaginasMemoria;
    [SerializeField] private string objetivoPaginasMemoria;
    [SerializeField, Min(1f)] private float duracaoPaginaMemoria = 8f;
    [SerializeField, Min(.1f)] private float leituraMinimaMemoria = 2.5f;
    [SerializeField, Min(.1f)] private float duracaoCorteMemoria = 1.5f;
    [SerializeField] private string cenaEpilogo = "epilogo";
    private bool planoMemoriaAtivo, leituraPaginaMemoria, paginaMemoriaPodeAvancar;
    private bool composicaoMemoriaGuardada;
    private Vector3 mariAntesDaMemoria, cameraAntesDaMemoria;
    private float zoomAntesDaMemoria;
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
            imagensPassagem != null && imagensPassagem.Length == 3;
        if (imagensPassagem != null)
            foreach (GameObject imagem in imagensPassagem) valido &= ConferirReferencia(imagem, nameof(imagensPassagem));
        return valido;
    }

    private string ObjetivoMemoria() => string.Empty;

    private Collider2D[] AlvosMemoria() => new[] { areaRetornoConsulta, areaPaginasMemoria, areaMae, areaPaiAbrigo, areaLuzia };

    private void AtualizarMemoria()
    {
        planoMemoria.SetActive(planoMemoriaAtivo);
        if (!MemoriaDisponivel) return;
        objetivoTexto.text = string.Empty;
        areaPaginasMemoria.enabled = true;
        lacoMemoria.SetActive(planoMemoriaAtivo && Dados.curraisLacoGuardado);
        documentoMemoria.SetActive(planoMemoriaAtivo && (TemItem(itemCanhoto) || TemItem(anotacaoPosto)));
    }

    private bool InteragirMemoria(Vector2 ponto)
    {
        foreach (Collider2D area in AlvosMemoria())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;
            if (Dados.ato2Concluido) Iniciar(IrParaEpilogo());
            else if (area == areaPaginasMemoria) MenuEsteiraFinal();
            else MenuNomesFinais();
            return true;
        }
        return false;
    }

    private void PrepararPlanoMemoria()
    {
        if (!composicaoMemoriaGuardada)
        {
            mariAntesDaMemoria = mari.transform.position;
            cameraAntesDaMemoria = cameraCena.transform.position;
            zoomAntesDaMemoria = cameraCena.orthographicSize;
            composicaoMemoriaGuardada = true;
        }
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

    private IEnumerator ExibirPaginasMemoria()
    {
        if (!MemoriaDisponivel) yield break;
        Dados.curraisMemoriaIniciada = true;
        Salvar();
        yield return Escurecer(1f);
        PrepararPlanoMemoria();
        yield return Escurecer(0f);
        int inicio = Mathf.Clamp(Dados.curraisPaginaMemoria, 0, 3);
        for (int i = inicio; i < paginasMemoria.Length; i++)
        {
            yield return ChapterCard.Show("josefa-" + (12 + i).ToString("00"));
            MostrarPassagemMemoria(i);
            yield return LerPaginaMemoria(paginasMemoria[i]);
            Dados.curraisPaginaMemoria = i + 1;
            GameProgress.Instance.UnlockDiaryPage(idsPaginasMemoria[i]);
            Salvar();
        }
        MostrarPassagemMemoria(-1);
        maoMemoria.SetActive(true);
        yield return LerPaginaMemoria(pensamentoMemoria);
        yield return CorteParaPresente();
        Dados.ato2Concluido = true;
        GameProgress.Instance.UnlockDiaryPage("currais-memoria-encerrada");
        Salvar();
        yield return IrParaEpilogo();
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
            yield return IrParaEpilogo();
            yield break;
        }
        yield return ExibirPaginasMemoria();
    }

    private IEnumerator IrParaEpilogo()
    {
        if (!Dados.ato2Concluido)
        {
            objetivoTexto.text = string.Empty;
            yield break;
        }
        if (string.IsNullOrWhiteSpace(cenaEpilogo) || !Application.CanStreamedLevelBeLoaded(cenaEpilogo))
        {
            CorFade(0f);
            Debug.LogError("Currais: a cena do epílogo não está habilitada na build.", this);
            yield break;
        }
        yield return Escurecer(1f);
        // Não altera destino, identidade, inventário ou escolhas anteriores.
        GameSaveSystem sistema = GameProgress.Instance.GetComponent<GameSaveSystem>();
        if (sistema != null && Dados.CanSave && !GameSaveSystem.IsRestoring)
            sistema.TrySaveCheckpoint(cenaEpilogo);
        SceneManager.LoadScene(cenaEpilogo);
    }

    private void RestaurarApresentacaoMemoria()
    {
        leituraPaginaMemoria = false; paginaMemoriaPodeAvancar = false;
        planoMemoriaAtivo = false;
        MostrarPassagemMemoria(-1);
        foreach (KeyValuePair<Animator, bool> animador in animadoresMemoria)
            if (animador.Key != null) animador.Key.enabled = animador.Value;
        animadoresMemoria.Clear();
        if (planoMemoria != null) planoMemoria.SetActive(false);
        if (composicaoMemoriaGuardada)
        {
            if (mari != null) mari.transform.position = mariAntesDaMemoria;
            if (cameraCena != null)
            {
                cameraCena.transform.position = cameraAntesDaMemoria;
                cameraCena.orthographicSize = zoomAntesDaMemoria;
            }
            composicaoMemoriaGuardada = false;
            Physics2D.SyncTransforms();
        }
    }
}
