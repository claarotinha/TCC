using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashManager : MonoBehaviour
{
    private const string IntroWatchedKey = "Currais.CurraisOpeningWatched";

    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipIndicator;

    [Serializable]
    private class CreditoAbertura
    {
        public CanvasGroup painel;
        [Range(0f, 1f)] public float inicio;
        [Range(0f, 1f)] public float fim;
    }

    [Header("Vídeo e créditos editáveis na Hierarchy")]
    [SerializeField] private RawImage imagemVideo;
    [SerializeField] private CreditoAbertura[] creditos;
    [SerializeField, Min(0f)] private float tempoFadeCreditos = 0.6f;

    private TMP_Text textoPular;
    private bool mostrouAviso;
    private bool canSkip;
    private bool leaving;

    private void Awake()
    {
        // Assistir à introdução é uma preferência; não depende dos checkpoints.
        canSkip = PlayerPrefs.GetInt(IntroWatchedKey, 0) == 1;
        if (skipIndicator != null)
        {
            textoPular = skipIndicator.GetComponentInChildren<TMP_Text>(true);
            skipIndicator.SetActive(false);
        }

        if (videoPlayer == null)
        {
            Debug.LogError("SplashManager: VideoPlayer não configurado.", this);
            enabled = false;
            return;
        }

        OcultarCreditos();
        videoPlayer.isLooping = false;
        // A textura pertence ao player desta cena; não depende de Camera.main
        // durante a troca síncrona Boot -> Intro -> Splash.
        videoPlayer.Stop();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.targetTexture = null;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.sendFrameReadyEvents = true;
        if (imagemVideo != null)
        {
            imagemVideo.texture = null;
            imagemVideo.uvRect = new Rect(0f, 0f, 1f, 1f);
            imagemVideo.enabled = false;
        }
        videoPlayer.frameReady += PrimeiroQuadro;
        videoPlayer.prepareCompleted += VideoPreparado;
        videoPlayer.errorReceived += ErroVideo;
        videoPlayer.loopPointReached += FimDoVideo;
    }

    private IEnumerator Start()
    {
        yield return null;
        if (videoPlayer == null) yield break;
        if (videoPlayer.isPrepared) VideoPreparado(videoPlayer);
        else videoPlayer.Prepare();
    }

    private void VideoPreparado(VideoPlayer player)
    {
        if (leaving || player != videoPlayer) return;
        player.Play();
    }

    private void PrimeiroQuadro(VideoPlayer player, long quadro)
    {
        if (leaving || player != videoPlayer || imagemVideo == null || player.texture == null) return;
        imagemVideo.texture = player.texture;
        imagemVideo.enabled = true;
        RectTransform area = imagemVideo.rectTransform.parent as RectTransform;
        if (area != null && player.height > 0)
        {
            float aspecto = (float)player.width / player.height;
            float largura = Mathf.Min(area.rect.width, area.rect.height * aspecto);
            imagemVideo.rectTransform.anchorMin = imagemVideo.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imagemVideo.rectTransform.anchoredPosition = Vector2.zero;
            imagemVideo.rectTransform.sizeDelta = new Vector2(largura, largura / aspecto);
        }
        if (canSkip && !mostrouAviso)
        {
            mostrouAviso = true;
            StartCoroutine(PulsarAviso());
        }
    }

    private IEnumerator PulsarAviso()
    {
        if (skipIndicator == null || textoPular == null) yield break;
        skipIndicator.SetActive(true);
        float tempo = 0f;
        while (tempo < 2f && !leaving)
        {
            tempo += Time.unscaledDeltaTime;
            // Dois pulsos suaves, começando e terminando transparentes.
            textoPular.alpha = Mathf.Pow(Mathf.Sin(Mathf.Clamp01(tempo / 2f) * Mathf.PI * 2f), 2f);
            yield return null;
        }
        textoPular.alpha = 0f;
        skipIndicator.SetActive(false);
    }

    private void ErroVideo(VideoPlayer player, string mensagem)
    {
        if (leaving || player != videoPlayer) return;
        Debug.LogWarning("Não foi possível reproduzir a abertura: " + mensagem, this);
        GoToMenu();
    }

    private void OcultarCreditos()
    {
        if (creditos == null) return;
        foreach (CreditoAbertura credito in creditos)
        {
            if (credito == null || credito.painel == null) continue;
            credito.painel.alpha = 0f;
            credito.painel.interactable = false;
            credito.painel.blocksRaycasts = false;
        }
    }

    private void AtualizarCreditos()
    {
        if (videoPlayer == null || creditos == null || !videoPlayer.isPrepared ||
            videoPlayer.length <= 0d) return;
        float tempo = (float)videoPlayer.time;
        float duracao = (float)videoPlayer.length;
        foreach (CreditoAbertura credito in creditos)
        {
            if (credito == null || credito.painel == null) continue;
            float inicio = Mathf.Clamp01(credito.inicio) * duracao;
            float fim = Mathf.Clamp01(credito.fim) * duracao;
            float alpha = 0f;
            if (fim > inicio && tempo >= inicio && tempo <= fim)
            {
                float fade = Mathf.Min(tempoFadeCreditos, (fim - inicio) * 0.5f);
                alpha = fade <= 0f ? 1f : Mathf.Min(
                    Mathf.Clamp01((tempo - inicio) / fade),
                    Mathf.Clamp01((fim - tempo) / fade));
            }
            credito.painel.alpha = alpha;
        }
    }

    private void Update()
    {
        if (!leaving) AtualizarCreditos();
        if (canSkip && !leaving && Input.GetKeyDown(KeyCode.Escape))
            PularVideo();
    }

    private void FimDoVideo(VideoPlayer vp)
    {
        if (leaving || vp != videoPlayer) return;

        // Só libera ESC para a próxima exibição depois de assistir até o fim.
        PlayerPrefs.SetInt(IntroWatchedKey, 1);
        PlayerPrefs.Save();
        GoToMenu();
    }

    private void PularVideo()
    {
        if (canSkip)
            GoToMenu();
    }

    private void GoToMenu()
    {
        if (leaving) return;
        leaving = true;
        OcultarCreditos();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("MenuPrincipal");
        else
            SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.frameReady -= PrimeiroQuadro;
            videoPlayer.loopPointReached -= FimDoVideo;
            videoPlayer.prepareCompleted -= VideoPreparado;
            videoPlayer.errorReceived -= ErroVideo;
        }
    }
}
